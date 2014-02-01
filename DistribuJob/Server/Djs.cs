using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using DistribuJob.Client;
using DistribuJob.Client.Extracts;
using DistribuJob.Client.Extracts.Links;
using DistribuJob.Client.Net.Policies;
using Exo.Collections;
using Exo.Extensions;
using Exo.Misc;
using Exo.Web;
using MySql.Data.MySqlClient;

namespace DistribuJob.Server
{
    public partial class Djs : MarshalByRefObject
    {
        private static readonly Cache<int, uint> serverIdCache = new Cache<int, uint>(TimeSpan.FromHours(24), 40000, TimeSpan.FromMinutes(5));
        private static readonly Random rand = new Random();

        private static object importLock = new object();

        public Djs()
        {
        }

        private MySqlConnection DatabaseConnection
        {
            get
            {
                MySqlConnection dbConn = new MySqlConnection(ConfigurationManager.ConnectionStrings["exoget"].ConnectionString);
                dbConn.Open();

                return dbConn;
            }
        }

        private void CloseDatabaseConnection(MySqlConnection dbConn)
        {
            if (dbConn != null)
                dbConn.Dispose();
        }

        public Job[] Import(int count, bool lockResults)
        {
            Job[] jobs = new Job[count];

            lock (importLock)
            {
                using (MySqlConnection dbConn = DatabaseConnection)
                using (MySqlCommand command = new MySqlCommand("CALL getJob(" + count + ", " + Convert.ToByte(lockResults) + ");", dbConn))
                {
                    command.CommandTimeout = 20000;

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        for (int i = 0; i < count && jobs != null; i++)
                        {
                            if (!reader.Read())
                            {
                                if (i == 0)
                                    jobs = null;

                                else
                                    Array.Resize(ref jobs, i);

                                break;
                            }

                            jobs[i] = new Job(reader.GetUInt32(0));

                            jobs[i].ServerId = reader.GetUInt32(1);
                            jobs[i].Path = reader.GetString(2);

                            if (!reader.IsDBNull(3))
                                jobs[i].LastModified = reader.GetUInt32(3);
                        }
                    }
                }
            }

            return jobs;
        }

        public Job[] Import(int count)
        {
            return Import(count, true);
        }

        public void Export(Job[] jobs)
        {
            StringBuilder batchSql = new StringBuilder();

            using (MySqlConnection dbConn = DatabaseConnection)
            {
                foreach (Job job in jobs)
                {
                    if (job == null)
                        continue;

                    StringBuilder sql = new StringBuilder("SET AUTOCOMMIT=0;SET FOREIGN_KEY_CHECKS=0;");
                    sql.AppendLine();

                    if (job.HasBeenRedirected)
                        sql.AppendLine("# redirected");

                    sql.AppendFormat("SET @redirectsToExistingDocument = NULL; CALL updateJob({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, @redirectsToExistingDocument);" + Environment.NewLine,
                        job.Id,
                        job.ServerId,
                        job.Path.SqlEscape(),
                        ((int)job.Type).ToString(),
                        ((int)job.Format).ToString(),
                        job.ContentLength > 0 ? job.ContentLength.ToString() : "NULL",
                        job.LastModified.ToString(),
                        ((int)job.FetchStatus).ToString()
                        );

                    if (job.Type > 0 && job.Extract != null)
                    {
                        // images call directly, nothing is written
                        if (job.Type != DocumentType.Image)
                            sql.AppendLine("IF !@redirectsToExistingDocument THEN");

                        #region Links
                        if (job.Extract.Links != null)
                        {
                            foreach (Link link in job.Extract.Links)
                            {
                                string
                                    sourceDocId,
                                    targetDocId,
                                    imageDocId;

                                if (link is AbsoluteSourceLink)
                                {
                                    AbsoluteSourceLink absoluteSourceLink = (AbsoluteSourceLink)link;

                                    if (!TryAppendExtractDocIdSql(absoluteSourceLink.SourceUri,
                                        absoluteSourceLink.SourceFormat,
                                        absoluteSourceLink.SourceType,
                                        job.ServerId,
                                        sql,
                                        out sourceDocId))
                                    {
                                        continue;
                                    }
                                }
                                else
                                    sourceDocId = job.Id.ToString();

                                if (!TryAppendExtractDocIdSql(link.TargetUri, link.TargetFormat, link.TargetType, job.ServerId, sql, out targetDocId))
                                    continue;

                                bool hasImage;

                                if (link.HasImage
                                    && TryAppendExtractDocIdSql(link.ImageUri, link.ImageFormat, DocumentType.Image, job.ServerId, sql, out imageDocId))
                                {
                                    hasImage = true;
                                }
                                else
                                {
                                    imageDocId = null;
                                    hasImage = false;
                                }

                                sql.Append("SET @linkId = NULL;");

                                sql.AppendFormat(
                                    "CALL addLink({0}, {1}, {2}, {3}, {4}, {5}, {6}, @linkId);" + Environment.NewLine,
                                    (byte)link.Type,
                                    sourceDocId,
                                    targetDocId,
                                    hasImage ? imageDocId : "NULL",
                                    link.Text.SqlEscape(),
                                    link.Description.SqlEscape(),
                                    link.IsAmbiguous
                                    );

                                AddLinkProperties("@linkId", link, sql);

                                if (link.Type == LinkType.ArtificialMedia)
                                {
                                    MediaExtract mediaExtract = ((ArtificialMediaLink)link).MediaExtract;
                                    ExportMedia(mediaExtract, targetDocId, sql, dbConn);
                                }
                            }
                        }
                        #endregion

                        switch (job.Type)
                        {
                            case DocumentType.Page:
                                ExportPage(job, sql);
                                break;

                            case DocumentType.Feed:
                                ExportFeed(job, sql);
                                break;

                            case DocumentType.Media:
                                ExportMedia(job.MediaExtract, job.Id, sql, dbConn);
                                break;

                            case DocumentType.MediaPlaylist:
                                ExportMediaPlaylist(job, sql);
                                break;

                            case DocumentType.Image:
                                ExportImage(job, sql, dbConn);
                                break;
                        }

                        // images call directly, nothing is written
                        if (job.Type != DocumentType.Image)
                            sql.AppendLine("END IF;");
                    }

                    sql.AppendLine("COMMIT;");
                    sql.AppendLine();

                    batchSql.AppendLine("DELIMITER $$");
                    batchSql.AppendLine("DROP PROCEDURE IF EXISTS `exoget`.`_importJob`$$");
                    batchSql.AppendLine("CREATE PROCEDURE `_importJob`()");
                    batchSql.AppendLine("BEGIN");

                    batchSql.Append(sql.ToString());

                    batchSql.AppendLine("END$$");
                    batchSql.AppendLine("DELIMITER ;");
                    batchSql.AppendLine();
                    batchSql.AppendLine();
                    batchSql.AppendLine("CALL _importJob();");
                    batchSql.AppendLine();
                    batchSql.AppendLine();
                }
            }

            File.WriteAllText(@"sql\" + DateTime.Now.Ticks + ".sql", batchSql.ToString(), new UTF8Encoding(false));
        }

        private void ExportPage(Job job, StringBuilder sql)
        {
            sql.AppendFormat("CALL addPage({0}, {1}, {2}, {3}, {4});",
                job.Id,
                job.PageExtract.Title.SqlEscape(),
                job.PageExtract.Heading.SqlEscape(),
                job.PageExtract.Description.SqlEscape(),
                job.PageExtract.HasFrameset
                );

            sql.AppendLine();
        }

        private void ExportMediaPlaylist(Job job, StringBuilder sql)
        {
            sql.AppendFormat("SET @playlistId = NULL; CALL addPlaylist({0}, {1}, {2}, @playlistId);",
                job.Id,
                job.MediaPlaylistExtract.Title.SqlEscape(),
                job.MediaPlaylistExtract.Description.SqlEscape()
                );

            sql.AppendLine();

            AddPlaylistProperties("@playlistId", job.MediaPlaylistExtract, sql);
        }

        private void ExportFeed(Job job, StringBuilder sql)
        {
            string imageDocId;
            DocumentFormatInfo imageFormatInfo;
            bool hasImage;

            if (job.FeedExtract.Channel.Image != null
                && UriUtil.TryGetFormatInfoFromUri(job.FeedExtract.Channel.Image.Uri, out imageFormatInfo)
                && TryAppendExtractDocIdSql(job.FeedExtract.Channel.Image.Uri, imageFormatInfo.Format, imageFormatInfo.Type, job.ServerId, sql, out imageDocId))
            {
                hasImage = true;
            }
            else
            {
                imageDocId = null;
                hasImage = false;
            }

            sql.Append("SET @feedId = NULL; ");

            sql.AppendFormat("CALL addFeed({0}, {1}, {2}, {3}, {4}, @feedId);" + Environment.NewLine,
                (int)job.FeedExtract.Type,
                job.Id,
                hasImage ? imageDocId : "NULL",
                job.FeedExtract.Channel.Title.SqlEscape(),
                job.FeedExtract.Channel.Description.SqlEscape()
                );

            AddFeedProperties("@feedId", job.FeedExtract, sql);
        }

        private void ExportMedia(MediaExtract mediaExtract, object docId, StringBuilder sql, MySqlConnection conn)
        {
            uint? imageId;

            if (mediaExtract.HasImage)
                AddImage(mediaExtract.Image, null, conn, out imageId);

            else
                imageId = null;

            sql.AppendFormat("SET @mediaId = NULL; CALL addMedia({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, @mediaId);" + Environment.NewLine,
                docId,
                imageId != null ? imageId.ToString() : "NULL",
                (int)mediaExtract.MediaType,
                mediaExtract.Bitrate > 0 ? mediaExtract.Bitrate.ToString() : "NULL",
                mediaExtract.Duration > 0 ? mediaExtract.Duration.ToString() : "NULL",
                mediaExtract.Title.SqlEscape(),
                mediaExtract.Description.SqlEscape(),
                mediaExtract.Transcript.SqlEscape(),
                mediaExtract.Width > 0 ? mediaExtract.Width.ToString() : "NULL",
                mediaExtract.Height > 0 ? mediaExtract.Height.ToString() : "NULL"
                );

            AddMediaProperties("@mediaId", mediaExtract, sql);
        }

        private void ExportImage(Job job, StringBuilder sql, MySqlConnection conn)
        {
            uint? imageId;

            AddImage(job.ImageExtract.Image, job.Id, conn, out imageId);
        }

        private void AddImage(ImageInfo image, uint? docId, MySqlConnection conn, out uint? imageId)
        {
            using (MySqlCommand command = new MySqlCommand("addImage", conn))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("?docId", docId);

                if (image.HasDimensions)
                {
                    command.Parameters.AddWithValue("?originalWidth", image.OriginalWidth);
                    command.Parameters.AddWithValue("?originalHeight", image.OriginalHeight);
                }

                command.Parameters.Add("?imageId", MySqlDbType.UInt32);
                command.Parameters.Add("?distributedFolderId", MySqlDbType.UInt32);

                command.ExecuteNonQuery();

                string distributedDirectory = @"c:\Inetpub\exogetwwwroot\distdir\" + command.Parameters["?distributedFolderId"].Value;

                if (!Directory.Exists(distributedDirectory))
                    Directory.CreateDirectory(distributedDirectory);

                char[] imageIdChars = command.Parameters["?imageId"].Value.ToString().ToCharArray();
                Array.Reverse(imageIdChars);

                File.WriteAllBytes(distributedDirectory + @"\" + new string(imageIdChars) + ".jpg",
                    image.ImageBytes);

                imageId = (uint?)command.Parameters["?imageId"].Value;
            }
        }

        private bool TryAppendExtractDocIdSql(Uri uri,
            DocumentFormat format,
            DocumentType type,
            uint relativeServerId,
            StringBuilder sql,
            out string docIdSqlVariableName)
        {
            if (uri == null)
                throw new ArgumentNullException();

            uint serverId;
            string path;

            if (!uri.IsAbsoluteUri)
            {
                serverId = relativeServerId;
                path = uri.ToString().TrimStart('/');
            }
            else if (TryGetServerId(uri, out serverId))
            {
                path = uri.PathAndQuery.TrimStart('/');
            }
            else
            {
                docIdSqlVariableName = null;
                return false;
            }
            
            docIdSqlVariableName = "@" + (DateTime.Now.Ticks + rand.Next()).ToString();

            sql.AppendLine
                (
                "SET {0} = getDocId({1}, {2}, {3}, {4});",

                docIdSqlVariableName,
                serverId,
                path.SqlEscape(),
                type > 0 ? ((int)type).ToString() : "0",
                format > 0 ? ((int)format).ToString() : "0"
                );

            return true;
        }

        [Obsolete]
        private uint GetDomainId(string domain, MySqlConnection dbConn)
        {
            uint currParent = 0;
            string[] domainSegments = domain.Split('.');

            object currentParentObject;
            byte octet;

            for (int i = domainSegments.Length - 1; i != -1; i--)
            {
                if (domainSegments[i].Length > 255)
                    throw new InvalidOperationException("\"" + domainSegments[i] + "\" cannot be more than 255 chars");

                using (MySqlCommand command = new MySqlCommand("SELECT id FROM domains WHERE name = '" + domainSegments[i] + "'" + (currParent != 0 ? " AND parent = " + currParent : ""), dbConn))
                {
                    currentParentObject = command.ExecuteScalar();

                    if (currentParentObject != null)
                        currParent = Convert.ToUInt32(command.ExecuteScalar());

                    else if (currParent != 0 || (Char.IsNumber(domainSegments[i][0]) && Byte.TryParse(domainSegments[i], out octet)))
                    {
                        using (MySqlCommand command2 = new MySqlCommand("INSERT INTO domains (name, parent) VALUES ('" + domainSegments[i] + "', " + (currParent != 0 ? currParent.ToString() : "NULL") + ") ON DUPLICATE KEY UPDATE domains.id = LAST_INSERT_ID(domains.id)", dbConn))
                        {
                            command2.ExecuteNonQuery();
                            currParent = (uint)command2.LastInsertedId;
                        }
                    }
                    else
                        throw new InvalidOperationException("\"" + domainSegments[i] + "\" is not a valid TLD in the domains table.");
                }
            }

            return currParent;
        }

        private bool TryGetDomainId(string domain, out uint domainId, MySqlConnection dbConn)
        {
            domainId = 0u;

            string[] domainSegments = domainSegments = domain.Split('.');

            if (domainSegments == null || domainSegments.Length <= 1)
            {
                Trace.TraceWarning("Domain \"{0}\" does not have any valid segments", domain);

                return false;
            }

            for (int i = domainSegments.Length - 1; i >= 0; i--)
            {
                if (domainSegments[i] == String.Empty || domainSegments[i].Length > 255)
                {
                    Trace.TraceWarning("Segment \"{0}\" of domain \"{1}\" is not a valid domain segment",
                        domainSegments[i],
                        domain);

                    return false;
                }

                domainSegments[i] = domainSegments[i].ToLower();

                using (MySqlCommand command = new MySqlCommand("SELECT id FROM domains WHERE name = '" + domainSegments[i] + "' AND parent " + (domainId != 0 ? "=" + domainId : "IS NULL"), dbConn))
                {
                    object currentParentObject = command.ExecuteScalar();
                    byte octet;

                    if (currentParentObject != null)
                        domainId = Convert.ToUInt32(command.ExecuteScalar());

                    else if (domainId != 0
                        || (domainSegments.Length == 4 && Char.IsNumber(domainSegments[i][0]) && Byte.TryParse(domainSegments[i], out octet)))
                    {
                        using (MySqlCommand command2 = new MySqlCommand("INSERT INTO domains (name, parent) VALUES ('" + domainSegments[i] + "', " + (domainId != 0 ? domainId.ToString() : "NULL") + ") ON DUPLICATE KEY UPDATE domains.id = LAST_INSERT_ID(domains.id)", dbConn))
                        {
                            command2.ExecuteNonQuery();
                            domainId = (uint)command2.LastInsertedId;
                        }
                    }
                    else
                    {
                        Trace.TraceWarning("Segment \"{0}\" of domain \"{1}\" is not a valid TLD",
                            domainSegments[i],
                            domain);

                        command.Dispose();

                        return false;
                    }
                }
            }

            return true;
        }

        [Obsolete]
        public uint GetDomainId(string domain)
        {
            uint domainId = 0;

            using (MySqlConnection dbConn = DatabaseConnection)
            {
                domainId = GetDomainId(domain, dbConn);
            }

            return domainId;
        }

        public bool TryGetDomainId(string domain, out uint domainId)
        {
            bool success;

            using (MySqlConnection dbConn = DatabaseConnection)
            {
                success = TryGetDomainId(domain, out domainId, dbConn);
            }

            return success;
        }

        private List<UriPolicy> GetUriPolicies(uint serverId, MySqlConnection dbConn)
        {
            List<UriPolicy> uriPolicies = new List<UriPolicy>();

            using (MySqlCommand command = new MySqlCommand("SELECT id, type, regex, value, expire FROM uripolicies WHERE serverId " + (serverId != 0 ? "= " + serverId : "IS NULL") + " AND locked = 0", dbConn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    UriPolicy uriPolicy;

                    bool expired;

                    if (!reader.IsDBNull(4))
                        expired = reader.GetDateTime(4) >= DateTime.Now;

                    else
                        expired = false;

                    UriPolicy.UriPolicyType type;

                    switch ((type = (UriPolicy.UriPolicyType)reader.GetByte(1)))
                    {
                        case UriPolicy.UriPolicyType.LINE_META_INFO:
                        case UriPolicy.UriPolicyType.LINE_META_TITLE_INFO:
                        case UriPolicy.UriPolicyType.LINE_META_DESCRIPTION_INFO:
                        case UriPolicy.UriPolicyType.LINE_META_DURATION_INFO:
                            {
                                MetaInfoUriPolicy metaInfoUriPolicy = new MetaInfoUriPolicy(reader.GetUInt32(0), type, reader.GetString(2));

                                string[] segments = reader.GetString(3).Split(',');

                                metaInfoUriPolicy.StartPos = Int32.Parse(segments[0]);
                                metaInfoUriPolicy.Upwards = Int32.Parse(segments[1]);
                                metaInfoUriPolicy.Downwards = Int32.Parse(segments[2]);

                                uriPolicy = metaInfoUriPolicy;

                                break;
                            }

                        case UriPolicy.UriPolicyType.LINKS_DISALLOW_URI_MATCHES:
                        case UriPolicy.UriPolicyType.LINKS_TEXT_MATCHES:
                        case UriPolicy.UriPolicyType.LINKS_PICTURE_URI_MATCHES:
                            {
                                uriPolicy = new RegexUriPolicy(reader.GetUInt32(0), type, reader.GetString(2));
                                uriPolicy.StringValue = reader.GetString(3);
                                break;
                            }

                        case UriPolicy.UriPolicyType.URI_VALUE_FORMAT_TARGET:
                            {
                                string[] segments = reader.GetString(3).Split(',');

                                uriPolicy = new ValueFromUriRegexUriPolicy(reader.GetUInt32(0), type, reader.GetString(2), segments[0]);
                                uriPolicy.StringValue = segments[1];

                                break;
                            }

                        case UriPolicy.UriPolicyType.DOM_META_TITLE_XPATH:
                        case UriPolicy.UriPolicyType.DOM_META_DESCRIPTION_XPATH:
                        case UriPolicy.UriPolicyType.DOM_META_TAGS_XPATH:
                        case UriPolicy.UriPolicyType.DOM_META_AUTHOR_XPATH:
                        case UriPolicy.UriPolicyType.DOM_META_TARGET_XPATH:
                        case UriPolicy.UriPolicyType.DOM_META_DURATION_XPATH:
                            {
                                uriPolicy = new XPathExpressionUriPolicy(reader.GetUInt32(0), type, reader.GetString(2));

                                if (!reader.IsDBNull(3))
                                    uriPolicy.Value = reader[3];

                                break;
                            }


                        default:
                            {
                                uriPolicy = new UriPolicy(reader.GetUInt32(0), type, reader.GetString(2));

                                if (!reader.IsDBNull(3))
                                    uriPolicy.Value = reader[3];

                                break;
                            }
                    }

                    if (uriPolicy != null)
                        uriPolicies.Add(uriPolicy);
                }
            }

            return uriPolicies;
        }

        public List<UriPolicy> GetUriPolicies(uint serverId)
        {
            List<UriPolicy> uriPolicies = new List<UriPolicy>();

            using (MySqlConnection dbConn = DatabaseConnection)
            {
                uriPolicies = GetUriPolicies(serverId, dbConn);
            }

            return uriPolicies;
        }

        private void SetUriPolicyValue(uint uriPolicyId, object value, MySqlConnection dbConn)
        {
            using (MySqlCommand command = new MySqlCommand("UPDATE uripolicies SET value = " + value.ToString().SqlEscape() + " WHERE id = " + uriPolicyId, dbConn))
            {
                command.ExecuteNonQuery();
            }
        }

        public void SetUriPolicyValue(uint uriPolicyId, object value)
        {
            using (MySqlConnection dbConn = DatabaseConnection)
            {
                SetUriPolicyValue(uriPolicyId, value, dbConn);
            }
        }

        public void SetServerStatus(uint serverId, DistribuJob.Client.Net.ServerStatus status)
        {
            using (MySqlConnection conn = DatabaseConnection)
            using (MySqlCommand command = new MySqlCommand("UPDATE servers SET status = " + (byte)status + " WHERE id = " + serverId, conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private uint GetServerId(Exo.Net.NetworkProtocol protocol, int port, uint domainId, MySqlConnection dbConn)
        {
            uint serverId;

            using (MySqlCommand command = new MySqlCommand("SELECT id FROM servers WHERE domainId = " + domainId + " AND protocol = " + (byte)protocol + " AND port = " + port, dbConn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                    serverId = reader.GetUInt32(0);

                else
                {
                    reader.Dispose();
                    command.Dispose();

                    using (MySqlCommand command2 = new MySqlCommand("INSERT INTO servers (domainId, port, protocol, status) VALUES(" + domainId + ", " + port + ", " + (byte)protocol + ", 1) ON DUPLICATE KEY UPDATE servers.id = LAST_INSERT_ID(servers.id)", dbConn))
                    {
                        command2.ExecuteNonQuery();
                        serverId = (uint)command2.LastInsertedId;
                    }
                }
            }

            return serverId;
        }

        public uint GetServerId(Exo.Net.NetworkProtocol protocol, int port, uint domainId)
        {
            uint serverId;

            using (MySqlConnection dbConn = DatabaseConnection)
            {
                serverId = GetServerId(protocol, port, domainId, dbConn);
            }

            return serverId;
        }

        [Obsolete]
        private uint GetServerId(Uri uri, MySqlConnection dbConn)
        {
            uint serverId;

            return TryGetServerId(uri, out serverId, dbConn) ? serverId : 0;
        }

        [Obsolete]
        public uint GetServerId(Uri uri)
        {
            using (MySqlConnection dbConn = DatabaseConnection)
            {
                return GetServerId(uri, dbConn);
            }
        }

        private bool TryGetServerId(Uri uri, out uint serverId, MySqlConnection dbConn)
        {
            int serverHash = uri.Scheme.GetHashCode() + uri.Authority.GetHashCode() + uri.Port.GetHashCode();

            if (!serverIdCache.TryGetValue(serverHash, out serverId))
            {
                uint domainId;

                if (!TryGetDomainId(uri.Host, out domainId, dbConn))
                    return false;

                serverId = GetServerId(Exo.Net.NetworkProtocol.Http, uri.Port, domainId, dbConn);
                serverIdCache[serverHash] = serverId;
            }

            return true;
        }

        public bool TryGetServerId(Uri uri, out uint serverId)
        {
            bool success;

            using (MySqlConnection dbConn = DatabaseConnection)
            {
                success = TryGetServerId(uri, out serverId, dbConn);
            }

            return success;
        }

        private DistribuJob.Client.Net.Server GetServer(uint serverId, MySqlConnection dbConn)
        {
            List<UriPolicy> uriPolicies = GetUriPolicies(serverId, dbConn);

            DistribuJob.Client.Net.Server server = new DistribuJob.Client.Net.Server(serverId, uriPolicies);

            using (MySqlCommand command = new MySqlCommand("SELECT getServerUri(id) FROM servers WHERE id = " + serverId, dbConn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read() && !reader.IsDBNull(0))
                    server.Uri = new Uri(reader.GetString(0), UriKind.Absolute);
            }

            return server;
        }

        public DistribuJob.Client.Net.Server GetServer(uint serverId)
        {
            DistribuJob.Client.Net.Server server;

            using (MySqlConnection dbConn = DatabaseConnection)
            {
                server = GetServer(serverId, dbConn);
            }

            return server;
        }

        public bool TryGetDocId(uint serverId, string path, out uint docId, MySqlConnection conn)
        {
            using (MySqlCommand command = new MySqlCommand("SELECT id FROM docs WHERE serverId = " + serverId + " AND path = " + path.SqlEscape(), conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read() && !reader.IsDBNull(0))
                {
                    docId = reader.GetUInt32(0);
                    return true;
                }
            }

            docId = 0;

            return false;
        }

        private StringBuilder CreateDocumentSql(IEnumerable<Uri> uris, DocumentType forceType, MySqlConnection dbConn)
        {
            StringBuilder sql = new StringBuilder("INSERT IGNORE INTO docs (serverId,path,type,format) VALUES ");

            foreach (Uri uri in uris)
            {
                uint serverId;

                if (uri == null || uri.PathAndQuery.TrimStart('/').Length > 255 || !TryGetServerId(uri, out serverId, dbConn))
                    continue;

                DocumentFormatInfo format;
                DocumentType type = DocumentType.None;

                bool hasFormat = UriUtil.TryGetFormatInfoFromUri(uri, out format);

                if (forceType != DocumentType.None)
                    type = forceType;

                sql.AppendFormat("({0},{1},{2},{3}),",
                    serverId,
                    uri.PathAndQuery.TrimStart('/').Clean(StringCleanOptions.DecodeHtmlEntities).SqlEscape(),
                    ((int)type).ToString(),
                    hasFormat ? ((int)format.Format).ToString() : "0"
                    );
            }

            sql[sql.Length - 1] = ';';

            return sql;
        }

        public StringBuilder CreateDocumentSql(IEnumerable<Uri> uris, DocumentType forceType)
        {
            StringBuilder sql;

            using (MySqlConnection dbConn = DatabaseConnection)
            {
                sql = CreateDocumentSql(uris, forceType, dbConn);
            }

            return sql;
        }

        public StringBuilder CreateDocumentSql(IEnumerable<Uri> uris)
        {
            return CreateDocumentSql(uris, DocumentType.None);
        }

        public void CreateDocument(IEnumerable<Uri> uris)
        {
            using (MySqlConnection dbConn = DatabaseConnection)
            {
                StringBuilder sql = CreateDocumentSql(uris, DocumentType.None, dbConn);

                using (MySqlCommand command = new MySqlCommand(sql.ToString(), dbConn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
