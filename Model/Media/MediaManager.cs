using System;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;
using Exo.Exoget.Model.Search;
using System.Text;
using System.Data.Common;
using Exo.Web;
using System.Diagnostics;
using Exo.Collections;
using Exo.Exoget.Model.Feed;

namespace Exo.Exoget.Model.Media
{
    [Flags]
    public enum MediaInfoTypes : byte
    {
        None = 0,
        DocumentLookups = 1,
        Properties = 2,
        Comments = 4
    }

    public partial class MediaManager
    {
        public static readonly Cache<uint, MediaInfo> mediaCache;
        
        private readonly MySqlConnection conn;

        static MediaManager()
        {
            mediaCache = new Cache<uint, MediaInfo>(Properties.Settings.Default.MediaCacheExpire, Properties.Settings.Default.MediaCacheCapacity);
        }

        public MediaManager(MySqlConnection conn)
        {
            this.conn = conn;
        }

        public IList<MediaInfo> GetMedia(uint[] ids, ushort[] skeys, MediaInfoTypes infoTypes)
        {
            if ((skeys != null && ids.Length != skeys.Length) || ids == null)
                throw new ArgumentException();

            else if (ids.Length == 0)
                return null;

            List<MediaInfo> cachedMedias = null;

            uint[] nonCachedIds = new uint[ids.Length];
            ushort[] nonCachedSkeys = null;

            if (skeys != null)
                nonCachedSkeys = new ushort[ids.Length];

            int nonCachedIndex = 0;

            for (int i = 0; i < ids.Length; i++)
            {
                MediaInfo media;

                if (!mediaCache.TryGetValue(ids[i], out media))
                {
                    nonCachedIds[nonCachedIndex] = ids[i];

                    if (skeys != null)
                        nonCachedSkeys[nonCachedIndex] = skeys[i];

                    nonCachedIndex++;
                }
                else if (skeys == null || media.SKey == skeys[i])
                {
                    (cachedMedias ?? (cachedMedias = new List<MediaInfo>(10))).Add(media);
                }
            }

            if (nonCachedIndex == 0)
            {
                return cachedMedias;
            }
            else if (nonCachedIds.Length != ids.Length)
            {
                System.Array.Resize<uint>(ref nonCachedIds, nonCachedIndex + 1);

                if (skeys != null)
                    System.Array.Resize<ushort>(ref nonCachedSkeys, nonCachedIndex + 1);
            }

            List<MediaInfo> medias = new List<MediaInfo>(nonCachedIndex);

            StringBuilder sql = new StringBuilder();

            sql.Append("SELECT * FROM fullmedia WHERE id IN (");

            string idsStr = String.Join(",", System.Array.ConvertAll<uint, string>(nonCachedIds, Convert.ToString));

            sql.Append(idsStr);

            if (nonCachedSkeys != null)
            {
                string skeysStr = String.Join(",", System.Array.ConvertAll<ushort, string>(nonCachedSkeys, Convert.ToString));

                sql.Append(") AND skey IN (");
                sql.Append(skeysStr);
            }

            sql.Append(") ORDER BY FIELD(id,");
            sql.Append(idsStr);
            sql.Append(");");

            if ((infoTypes & MediaInfoTypes.Properties) == MediaInfoTypes.Properties)
            {
                sql.Append(@"SELECT mediapropmap.mediaId, props.id, props.type, props.value FROM props INNER JOIN mediapropmap ON props.id = mediapropmap.propId AND mediapropmap.mediaId IN (");
                sql.Append(idsStr);
                sql.Append(") ORDER BY FIELD(mediapropmap.mediaId,");
                sql.Append(idsStr);
                sql.Append(");");
            }

            using (MySqlCommand command = new MySqlCommand(sql.ToString(), conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    MediaInfo media = new MediaInfo()
                    {
                        Id = reader.GetUInt32("id"),
                        SKey = reader.GetUInt16("skey"),

                        Type = (MediaType)Convert.ToByte(reader.GetByte("mediaType")),
                        DocType = (DocumentType)Convert.ToByte(reader.GetByte("docType")),
                        DocFormat = (DocumentFormat)Convert.ToByte(reader.GetByte("docFormat")),

                        MediaUrl = reader.GetString("mediaUrl"),
                        SourceUrl = reader["sourceUrl"] as string,
                        ImageUrl = reader["imageUrl"] as string,

                        MediaFileSize = reader.GetUInt32("size"),

                        // title *should* never be null, but can currently be if something went wrong in Indexer
                        Title = reader["title"] as string,
                        Description = reader["desc"] as string,

                        Bitrate = reader.GetUInt32("bitrate"),
                        Duration = reader.GetUInt32("duration"),
                        Views = reader.GetUInt32("viewCount")
                    };

                    if (reader["feedId"] != DBNull.Value)
                    {
                        media.Feed = new FeedInfo()
                        {
                            Id = reader.GetUInt32("feedId"),
                            SKey = reader.GetUInt16("feedSkey"),
                            Title = reader.GetString("feedTitle")
                        };
                    }

                    medias.Add(media);
                }

                if ((infoTypes & MediaInfoTypes.Properties) == MediaInfoTypes.Properties
                    && medias.Count > 0
                    && reader.NextResult())
                {
                    List<IndexPropertyInfo> properties = new List<IndexPropertyInfo>();
                    uint currentMediaId = 0;
                    int currentMediaIndex = 0;

                    while (reader.Read())
                    {
                        uint mediaId = reader.GetUInt32(0);
                        uint propId = reader.GetUInt32(1);
                        IndexPropertyType propType = (IndexPropertyType)reader.GetByte(2);
                        string propValue = reader.GetString(3);

                        if (mediaId != currentMediaId)
                        {
                            if (currentMediaId != 0)
                            {
                                currentMediaIndex = SkipMissingMediaIndex(medias, currentMediaId, currentMediaIndex);

                                medias[currentMediaIndex].Properties = properties.ToArray();
                                currentMediaIndex++;
                                properties.Clear();
                            }

                            currentMediaId = mediaId;
                        }

                        properties.Add(new IndexPropertyInfo(propId, propType, propValue));
                    }

                    currentMediaIndex = SkipMissingMediaIndex(medias, currentMediaId, currentMediaIndex);

                    if (currentMediaIndex < medias.Count)
                        medias[currentMediaIndex].Properties = properties.ToArray();
                }
            }

            if ((infoTypes & MediaInfoTypes.Comments) != 0)
            {
                foreach (MediaInfo media in medias)
                {
                    GetComments(media);
                    
                    // only add to cache for full MediaDetails
                    mediaCache[media.Id] = media;
                }
            }

            if (cachedMedias != null)
            {
                medias.AddRange(cachedMedias);

                if (medias.Count > 1)
                {
                    List<MediaInfo> tmpMedias = new List<MediaInfo>(medias.Count);

                    foreach (uint id in ids)
                    {
                        foreach (MediaInfo media in medias)
                        {
                            if (media.Id == id)
                            {
                                tmpMedias.Add(media);
                                break;
                            }
                        }
                    }

                    medias = tmpMedias;
                }
            }

            return medias;
        }

        private static int SkipMissingMediaIndex(List<MediaInfo> medias, uint currentMediaId, int currentMediaIndex)
        {
            if (medias[currentMediaIndex].Id != currentMediaId)
            {
                for (int i = currentMediaIndex; i < medias.Count; i++)
                {
                    if (medias[i].Id == currentMediaId)
                        break;

                    else
                        currentMediaIndex++;
                }
            }

            return currentMediaIndex;
        }

        public IList<MediaInfo> GetMedia(uint[] ids, MediaInfoTypes infoTypes)
        {
            return GetMedia(ids, null, infoTypes);
        }

        public MediaInfo GetMedia(uint id, ushort skey, MediaInfoTypes infoTypes)
        {
            IList<MediaInfo> medias = GetMedia(new uint[] { id }, new ushort[] { skey }, infoTypes);

            return medias.Count > 0 ? medias[0] : null;
        }

        /// <summary>
        /// Gets a media result by an SQL
        /// </summary>
        public IList<MediaInfo> GetMedia(string sql, MediaInfoTypes infoTypes)
        {
            List<uint> mediaIds = new List<uint>();

            using (MySqlCommand command = new MySqlCommand(sql, conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                    mediaIds.Add(reader.GetUInt32(0));

                /*reader.NextResult();

                if (reader.Read())
                    result.ResultsFoundCount = reader.GetUInt32(0);*/
            }

            return GetMedia(mediaIds.ToArray(), infoTypes);
        }

        private IndexPropertyInfo[] GetProperties(uint mediaId)
        {
            List<IndexPropertyInfo> properties = new List<IndexPropertyInfo>();

            using (MySqlCommand command = new MySqlCommand("SELECT props.id, props.type, props.value FROM props INNER JOIN mediapropmap ON props.id = mediapropmap.propId AND mediapropmap.mediaId = " + mediaId, conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                    properties.Add(new IndexPropertyInfo(reader.GetUInt32(0), (IndexPropertyType)reader.GetByte(1), reader.GetString(2)));
            }

            return properties.ToArray();
        }
    }
}