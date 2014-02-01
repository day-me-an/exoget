using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DistribuJob.Client.Extracts.Links;
using Exo.Exoget.Model.Search;
using Exo.Extensions;
using Exo.Misc;
using Exo.Web;
using Lucene.Net.Documents;
using MySql.Data.MySqlClient;
using Exo.Exoget.Model.Media;
using System.Globalization;

namespace DistribuJob.Indexer
{
    public partial class Indexer
    {
        /// <summary>
        /// Maximum length of a Page Link's description, avoids garbage text in the mediawordindex table
        /// </summary>
        const int MaxPageLinkDescriptionLength = 50;

        private readonly uint mediaId;
        private uint serverId, mediaSourceDocId, mediaImageDocId, mediaImageId;
        private DocumentType mediaDocType, mediaSourceDocType;
        private MediaType mediaType;

        private readonly StringBuilder sql;
        private readonly MySqlConnection conn;

        private bool hasMediaAuthorProperty = false;
        private uint feedImageDocId = 0;
        private uint mediaFeedId = 0;

        private readonly HashSet<SentenceInfo> sentenceSet = new HashSet<SentenceInfo>();
        private readonly HashSet<WordInfo> wordSet = new HashSet<WordInfo>();
        private readonly HashSet<string> keywordSet = new HashSet<string>();

        public Indexer(uint mediaId, StringBuilder insertSql, MySqlConnection conn)
        {
            this.mediaId = mediaId;
            this.conn = conn;
            this.sql = insertSql;
        }

        public bool Index()
        {
            uint docId, sourceDocId, imageDocId, mediaDuration, mediaBitrate;
            string docPath, title, desc;
            DocumentFormat docFormat;
            bool hasSpamDesc = false;

            using (MySqlCommand command = new MySqlCommand(
@"SELECT
media.docId,
docs.serverId,
docs.type,
docs.format,
docs.path,
media.sourceDocId,
media.imageDocId,
media.mediaType,
media.duration,
media.bitrate,
media.title,
media.description,
media.transcript

FROM media
INNER JOIN docs ON media.docId = docs.id
WHERE media.id = " + mediaId,
                conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    command.Dispose();
                    reader.Dispose();

                    return false;
                }

                docId = reader.GetUInt32(0);
                serverId = reader.GetUInt32(1);
                mediaDocType = (DocumentType)reader.GetByte(2);
                docFormat = (DocumentFormat)reader.GetByte(3);
                docPath = reader[4] as string;
                sourceDocId = !reader.IsDBNull(5) ? reader.GetUInt32(5) : 0;
                imageDocId = !reader.IsDBNull(6) ? reader.GetUInt32(6) : 0;
                mediaType = (MediaType)reader.GetByte(7);
                mediaDuration = reader.IsDBNull(8) ? 0 : reader.GetUInt32(8);
                mediaBitrate = reader.IsDBNull(9) ? 0 : reader.GetUInt32(9);
                title = reader[10] as string;

                desc = reader[11] as string;

                if (desc != null)
                {
                    hasSpamDesc =
                        desc.Contains("race rap rock sex bj blowjob sperm kiss snog dungeon she show soccer song spears tits")
                        || desc.Contains("superbowl xli commercial prince halftimefunny comedy cool")
                        || desc.Contains("Great Awesome Wonderful Japanese Karate Boxing Fight Tae Kwon Do Phillipines");
                }

                AddWord(title, IndexPropertyType.Title, DocumentType.Media);

                if (!hasSpamDesc)
                    AddWord(desc, IndexPropertyType.Description, DocumentType.Media);

                AddWord(reader[12] as string, IndexPropertyType.Transcript, DocumentType.Media);
            }

            IndexMediaProperties(mediaId);

            bool noTitle = Convert.IsDBNull(title) || title == null;

            if ((!hasMediaAuthorProperty || noTitle) && docPath.IndexOf('?') == -1)
            {
                Uri docPathUri = new Uri(docPath, UriKind.RelativeOrAbsolute);

                string name;
                string docPathUriStr = docPathUri.ToString().Trim('/');

                if (docPathUriStr.LastIndexOf('/') != -1)
                    name = docPathUriStr.Substring(docPathUriStr.LastIndexOf('/') + 1);

                else
                    name = docPathUriStr;

                name = HttpUtility.UrlDecode(name);

                string docFilename;

                if (name.LastIndexOf('.') != -1)
                    docFilename = name.Substring(0, name.LastIndexOf('.')).Clean(StringCleanOptions.RemoveWhitespaceAndLines | StringCleanOptions.DecodeHtmlEntities);

                else
                    docFilename = name.Clean(StringCleanOptions.RemoveWhitespaceAndLines | StringCleanOptions.DecodeHtmlEntities);

                if (docFilename != null)
                {
                    string[] docFilenameParts = docFilename.Split('-');

                    if (!hasMediaAuthorProperty && docFilenameParts.Length >= 2)
                    {
                        int firstHyphenIndex = docFilename.IndexOf('-');
                        string author = docFilename.Substring(0, firstHyphenIndex);

                        AddWord(author, IndexWordType.MediaFilenameAuthor);

                        if (noTitle)
                        {
                            int hyphenPos = docFilename.IndexOf('-', firstHyphenIndex);

                            if (hyphenPos != docFilename.Length - 1)
                            {
                                string filenameTitle = docFilename.Substring(hyphenPos + 1);
                                sentenceSet.Add(new SentenceInfo(filenameTitle, IndexWordType.MediaFilenameTitle));
                            }
                        }
                    }

                    if (noTitle && docFilename.DigitProportion() < 0.5)
                    {
                        AddWord(docFilename, IndexWordType.MediaFilename);
                        noTitle = false;
                    }
                }
            }

            LinkInfo[] links = IndexLinks(docId);
            IndexLinkSourceDoc(links);

            if (links.Length > 0)
            {
                if (!hasMediaAuthorProperty)
                {
                    foreach (LinkInfo link in links)
                    {
                        if (link.Text == null || (link.Type != LinkType.Page && link.Type != LinkType.Feed))
                            continue;

                        string[] linkTextParts = link.Text.Split('-');

                        if (linkTextParts.Length != 2)
                            continue;

                        string author = linkTextParts[0].Trim();

                        if (!String.IsNullOrEmpty(author))
                            AddWord(author, IndexWordType.MediaLinkTextAuthor);
                    }
                }

                if (mediaImageDocId == 0)
                {
                    LinkInfo[] imageComparerLinks = links;
                    LinkImageComparer linkImageComparer = new LinkImageComparer();
                    Array.Sort<LinkInfo>(imageComparerLinks, linkImageComparer);

                    if (linkImageComparer.HasImage)
                        mediaImageDocId = links[0].ImageDocId;

                    else if (feedImageDocId > 0)
                        mediaImageDocId = feedImageDocId;
                }

                LinkInfo[] sourceDocLinkComparerLinks = links;
                SourceDocLinkComparer sourceDocLinkComparer = new SourceDocLinkComparer();
                Array.Sort<LinkInfo>(sourceDocLinkComparerLinks, sourceDocLinkComparer);
                mediaSourceDocId = sourceDocLinkComparerLinks[0].SourceDocId;
                this.mediaSourceDocType = sourceDocLinkComparerLinks[0].SourceDocType;

                sql.AppendLine("UPDATE media SET sourceDocId = {1} WHERE id = {0};",
                    mediaId,
                    mediaSourceDocId
                    );

                if (mediaFeedId > 0)
                {
                    sql.AppendLine("UPDATE media SET feedId = {1} WHERE id = {0};",
                        mediaId,
                        mediaFeedId
                        );
                }
            }

            if (mediaImageId == 0 && mediaImageDocId > 0)
            {
                using (MySqlCommand command = new MySqlCommand("SELECT id FROM images WHERE docId=" + mediaImageDocId, conn))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        mediaImageId = reader.GetUInt32(0);
                }
            }

            if (mediaImageId > 0)
            {
                sql.AppendLine("UPDATE media SET imageId = {0} WHERE id = {1};",
                    mediaImageId,
                    mediaId
                    );
            }
            else if (mediaImageDocId > 0)
            {
                sql.AppendLine("UPDATE media SET imageDocId = {0} WHERE id = {1};",
                    mediaImageDocId,
                    mediaId
                    );
            }

            if (mediaDocType != DocumentType.ArtificalMedia && sentenceSet.Count > 0)
            {
                SentenceInfo[]
                    titleSentences = sentenceSet.ToArray(),
                    descSentences = sentenceSet.ToArray(),
                    authorSentences = sentenceSet.ToArray();

                SentenceComparer
                    titleSentenceComparer = new SentenceComparer(SentenceComparerType.Title),
                    descSentenceComparer = new SentenceComparer(SentenceComparerType.Description),
                    authorSentenceComparer = new SentenceComparer(SentenceComparerType.Author);

                Array.Sort<SentenceInfo>(titleSentences, titleSentenceComparer);

                if (titleSentenceComparer.HasSentencesOfType)
                {
                    SentenceInfo titleSentence = titleSentences[0];

                    if (titleSentence.Type != IndexWordType.MediaTitle
                        && titleSentence.Type != IndexWordType.ArtificialMediaLinkText)
                    {
                        string indexTitle = titleSentence.Sentence;

                        if (mediaFeedId == 0 && (titleSentence.Type == IndexWordType.MediaFilenameTitle
                            || titleSentence.Type == IndexWordType.MediaFilename
                            || titleSentence.Type == IndexWordType.PageLinkText))
                        {
                            indexTitle = indexTitle.Sentenize(TokenizeOptions.All);
                        }

                        indexTitle = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(indexTitle);

                        sql.AppendLine("UPDATE media SET indexTitle = {1} WHERE id = {0};",
                            mediaId,
                            indexTitle.SqlEscape()
                            );

                        descSentenceComparer.ExludeWordType = titleSentences[0].Type;
                    }
                }

                if (!hasSpamDesc)
                {
                    Array.Sort<SentenceInfo>(descSentences, descSentenceComparer);

                    if (descSentenceComparer.HasSentencesOfType)
                    {
                        SentenceInfo descSentence = descSentences[0];

                        if (descSentence.Type != descSentenceComparer.ExludeWordType
                            && descSentence.Type != IndexWordType.MediaDescription
                            && descSentence.Type != IndexWordType.ArtificialMediaLinkDescription)
                        {
                            sql.AppendLine("UPDATE media SET indexDescription = {1} WHERE id = {0};",
                                mediaId,
                                descSentence.Sentence.SqlEscape()
                                );
                        }
                    }
                }
                else
                {
                    sql.AppendLine("UPDATE media SET description = NULL, indexDescription = NULL WHERE id = {0};",
                        mediaId);
                }

                Array.Sort<SentenceInfo>(authorSentences, authorSentenceComparer);

                if (authorSentenceComparer.HasSentencesOfType)
                {
                    sql.AppendLine("INSERT IGNORE INTO mediapropmap (mediaId, propId) VALUES ({0}, getPropId({1}, {2}));",
                        mediaId,
                        (byte)IndexPropertyType.Author,
                        authorSentences[0].Sentence.SqlEscape()
                        );
                }
            }

            if (keywordSet.Count > 0)
            {
                string keywords = String.Join(",", Array.ConvertAll<string, string>(keywordSet.ToArray(),
                    delegate(string keyword)
                    {
                        return String.Format("({0}, getPropId({1}, {2}))",
                            mediaId,
                            (byte)IndexPropertyType.Keyword,
                            keyword.SqlEscape()
                            );
                    }
                    ));

                sql.AppendLine("INSERT IGNORE INTO mediapropmap (mediaId, propId) VALUES " + keywords + ";");
            }

            // group by sentence type
            SentenceInfo[] sortedSentences = sentenceSet.ToArray();
            Array.Sort<SentenceInfo>(sortedSentences, new SentenceTypeComparer());

            Document doc = new Document();
            IndexWordType currentSentenceType = IndexWordType.None;
            StringBuilder currentFieldValueSb = new StringBuilder();

            Field field;
            float boost;

            Export export = delegate()
            {
                if (currentSentenceType != IndexWordType.None && currentFieldValueSb.Length > 0)
                {
                    currentFieldValueSb = currentFieldValueSb.Remove(currentFieldValueSb.Length - 1, 1);

                    if (currentFieldValueSb.Length > 0)
                    {
                        field = new Field(currentSentenceType.ToString().ToLower(),
                            currentFieldValueSb.ToString(),
                            Field.Store.NO,
                            Field.Index.TOKENIZED,
                            Field.TermVector.WITH_POSITIONS_OFFSETS);

                        if (TryGetBoost(currentSentenceType, out boost))
                            field.SetBoost(boost);

                        doc.Add(field);

                        currentFieldValueSb.Length = 0;
                    }
                }
            };

            foreach (SentenceInfo sentence in sortedSentences)
            {
                if (currentSentenceType != sentence.Type)
                {
                    export();
                    currentSentenceType = sentence.Type;
                }

                string[] words = sentence.Words;

                if (words != null)
                {
                    currentFieldValueSb.Append(String.Join(" ", words));
                    currentFieldValueSb.Append(' ');
                }
            }

            export();

            doc.Add(new Field("id", mediaId.ToString(), Field.Store.YES, Field.Index.UN_TOKENIZED));
            doc.Add(new Field("type", ((byte)mediaType).ToString(), Field.Store.NO, Field.Index.UN_TOKENIZED));
            doc.Add(new Field("format", ((byte)docFormat).ToString(), Field.Store.NO, Field.Index.UN_TOKENIZED));

            float docBoost = 1;

            if (mediaDuration > 0)
            {
                float durationType = GetDurationType(mediaDuration);

                doc.Add(new Field("duration", durationType.ToString(), Field.Store.NO, Field.Index.UN_TOKENIZED));

                if (durationType > 0)
                    docBoost += durationType * 0.1f;
            }

            if (mediaBitrate > 0)
            {
                byte qualityType = (byte)MediaInfo.GetBitrateQuality(mediaBitrate);

                doc.Add(new Field("quality", qualityType.ToString(), Field.Store.NO, Field.Index.UN_TOKENIZED));

                if (qualityType > 0)
                    docBoost += qualityType * 0.1f;
            }

            if (this.mediaSourceDocType == DocumentType.Feed)
            {

            }

            if (docBoost > 1)
                doc.SetBoost(docBoost);

            Program.indexWriter.AddDocument(doc);

            return true;
        }

        delegate void Export();

        private bool TryGetBoost(IndexWordType wordType, out float boost)
        {
            switch (wordType)
            {
                case IndexWordType.MediaAuthor:
                    {
                        if (mediaType == MediaType.Audio)
                            boost = 1.7f;

                        else
                            goto default;

                        break;
                    }

                case IndexWordType.MediaTitle:
                    boost = 1.6f;
                    break;

                case IndexWordType.MediaFilenameAuthor:
                    boost = 1.5f;
                    break;

                case IndexWordType.MediaFilenameTitle:
                    boost = 1.4f;
                    break;

                case IndexWordType.MediaFilename:
                    boost = 1.3f;
                    break;

                case IndexWordType.MediaAlbum:
                    boost = 1.2f;
                    break;

                case IndexWordType.MediaGenre:
                    boost = 1.1f;
                    break;

                default:
                    boost = 1;
                    return false;
            }

            return true;
        }

        private byte GetDurationType(uint duration)
        {
            // 1:25
            if (duration <= 95)
                return 0;
                
            // perfect
            else if (duration < (uint)TimeSpan.FromMinutes(5).TotalSeconds)
                return 10;

            else if (duration < (uint)TimeSpan.FromMinutes(10).TotalSeconds)
                return 1;

            else if (duration < (uint)TimeSpan.FromMinutes(30).TotalSeconds)
                return 2;

            else if (duration < (uint)TimeSpan.FromMinutes(30).TotalSeconds)
                return 3;

            else
                return 0;
        }

        private void IndexLinkSourceDoc(LinkInfo[] links)
        {
            foreach (LinkInfo link in links)
            {
                switch (link.SourceDocType)
                {
                    case DocumentType.Page:
                        {
                            IndexPage(link.SourceDocId);

                            if (mediaDocType == DocumentType.ArtificalMedia)
                            {
                                using (MySqlCommand command = new MySqlCommand(@"SELECT imageDoc FROM links WHERE targetDoc = " + link.SourceDocId + " AND imageDoc IS NOT NULL ORDER BY id ASC LIMIT 1", conn))
                                using (MySqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.Read() && !reader.IsDBNull(0))
                                        mediaImageDocId = reader.GetUInt32(0);
                                }
                            }

                            break;
                        }

                    case DocumentType.Feed:
                        {
                            mediaFeedId = IndexFeed(link.SourceDocId);

                            break;
                        }

                    default:
                        {
                            if (link.Type == LinkType.Feed)
                                mediaFeedId = IndexFeed(link.SourceDocId);

                            break;
                        }
                }
            }
        }

        private void IndexMediaProperties(uint mediaId)
        {
            using (MySqlCommand command = new MySqlCommand(
@"SELECT props.type, props.value
FROM props
INNER JOIN mediapropmap
ON props.id = mediapropmap.propId AND mediapropmap.mediaId = " + mediaId,
                conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    IndexPropertyType propertyType = (IndexPropertyType)reader.GetByte(0);

                    if (propertyType == IndexPropertyType.Pubdate)
                        continue;

                    if (propertyType == IndexPropertyType.Author)
                        hasMediaAuthorProperty = true;

                    AddWord(reader.GetString(1), propertyType, DocumentType.Media);
                }
            }
        }

        private void IndexPage(uint docId)
        {
            if (mediaDocType == DocumentType.ArtificalMedia)
                return;

            using (MySqlCommand command = new MySqlCommand(
@"SELECT title, heading
FROM pages
WHERE docId = " + docId,
                conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    AddWord(reader[0] as string, IndexPropertyType.Title, DocumentType.Page);
                    AddWord(reader[1] as string, IndexPropertyType.Description, DocumentType.Page);
                }
                else
                {
                    // trace a reference to this page was ivalid
                }
            }
        }

        private uint IndexFeed(uint docId)
        {
            uint id = 0;

            using (MySqlCommand command = new MySqlCommand(
@"SELECT id, imageDocId, imageId, title, description
FROM feeds
WHERE docId = " + docId,
                conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    id = reader.GetUInt32(0);
                    feedImageDocId = !reader.IsDBNull(1) ? reader.GetUInt32(1) : 0;
                    mediaImageId = !reader.IsDBNull(2) ? reader.GetUInt32(2) : 0;

                    AddWord(reader[3] as string, IndexPropertyType.Title, DocumentType.Feed);
                    AddWord(reader[4] as string, IndexPropertyType.Description, DocumentType.Feed);
                }
                else
                {
                    // trace a reference to this feed was invalid
                }
            }

            if (id > 0)
            {
                using (MySqlCommand command = new MySqlCommand(
@"SELECT feedpropmap.feedId, props.type, props.value
FROM props
INNER JOIN feedpropmap ON props.id = feedpropmap.propId AND feedpropmap.feedId = " + id,
                    conn))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AddWord(reader.GetString(2), (IndexPropertyType)reader.GetByte(1), DocumentType.Feed);
                    }
                }
            }

            return id;
        }

        private LinkInfo[] IndexLinks(uint docId)
        {
            Dictionary<uint, LinkInfo> links = new Dictionary<uint, LinkInfo>();

            StringBuilder linkIds = new StringBuilder();

            using (MySqlCommand command = new MySqlCommand(
@"SELECT links.id, links.type, links.sourceDoc, docs.type, links.imageDoc, links.text, links.description, links.ambiguous
FROM links
INNER JOIN docs ON links.sourceDoc = docs.id
WHERE targetDoc=" + docId,
                conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    LinkInfo link = new LinkInfo();

                    link.Id = reader.GetUInt32(0);
                    link.Type = (LinkType)reader.GetByte(1);
                    link.SourceDocId = reader.GetUInt32(2);
                    link.SourceDocType = (DocumentType)reader.GetByte(3);
                    link.ImageDocId = !reader.IsDBNull(4) ? reader.GetUInt32(4) : 0;
                    link.Text = reader[5] as string;
                    link.Description = reader[6] as string;
                    link.IsAmbiguous = reader.GetBoolean(7);

                    links[link.Id] = link;

                    linkIds.Append(link.Id);
                    linkIds.Append(',');

                    if (link.Type != LinkType.ArtificialMedia)
                    {
                        if (!link.IsAmbiguous)
                            AddWord(link.Text, IndexPropertyType.Title, link.Type);

                        if (link.Type != LinkType.Page || (link.Description != null && link.IsAmbiguous && link.Description.Length <= MaxPageLinkDescriptionLength))
                            AddWord(link.Description, IndexPropertyType.Description, link.Type);
                    }
                }
            }

            if (links.Count > 0)
            {
                linkIds = linkIds.Remove(linkIds.Length - 1, 1);

                using (MySqlCommand command = new MySqlCommand(
@"SELECT linkpropmap.linkId, props.type, props.value
FROM props
INNER JOIN linkpropmap ON props.id = linkpropmap.propId AND linkpropmap.linkId IN (" + linkIds + ")",
                    conn))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        uint linkId = reader.GetUInt32(0);
                        IndexPropertyType propType = (IndexPropertyType)reader.GetByte(1);
                        string propValue = reader.GetString(2);

                        if (propType != IndexPropertyType.Pubdate)
                            AddWord(propValue, propType, links[linkId].SourceDocType);

                        else
                        {
                            sql.AppendFormat("INSERT IGNORE INTO mediapropmap (mediaId,propId) VALUES ({0},getPropId({1},{2}));" + Environment.NewLine,
                                mediaId,
                                (byte)propType,
                                propValue.SqlEscape());
                        }
                    }
                }
            }

            LinkInfo[] linkArr = new LinkInfo[links.Count];
            links.Values.CopyTo(linkArr, 0);

            return linkArr;
        }
    }
}