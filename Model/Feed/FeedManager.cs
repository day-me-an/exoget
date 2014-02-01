using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Exo.Web.Feed;
using Exo.Exoget.Model.Search;

namespace Exo.Exoget.Model.Feed
{
    public class FeedManager
    {
        private MySqlConnection conn;

        public FeedManager(MySqlConnection conn)
        {
            this.conn = conn;
        }

        [Flags]
        public enum FeedInfoTypes : byte
        {
            None = 0,
            DocumentLookups = 1,
            Properties = 2,
            //Episodes = 4
        }

        public IList<FeedInfo> GetFeed(uint[] ids, ushort[] skeys, FeedInfoTypes infoTypes)
        {
            if ((skeys != null && ids.Length != skeys.Length) || ids == null)
                throw new ArgumentException();

            else if (ids.Length == 0)
                return null;

            string idsStr = String.Join(",", System.Array.ConvertAll<uint, string>(ids, Convert.ToString));

            StringBuilder sql = new StringBuilder();

            #region Feed
            sql.Append("SELECT feeds.id, feeds.type, feeds.imageId, feeds.title, feeds.description");

            if ((infoTypes & FeedInfoTypes.DocumentLookups) == FeedInfoTypes.DocumentLookups)
            {
                sql.Append(@",fulluridocs.uri, imageview.imageUrl FROM feeds
INNER JOIN fulluridocs ON feeds.docId=fulluridocs.id
LEFT JOIN imageview ON feeds.imageId = imageview.id ");
            }
            else
            {
                sql.Append(" FROM feeds ");
            }

            sql.Append("WHERE feeds.id IN (");
            sql.Append(idsStr);
            sql.Append(')');

            if (skeys != null)
            {
                string skeysStr = String.Join(",", System.Array.ConvertAll<ushort, string>(skeys, Convert.ToString));

                sql.Append(" AND feeds.skey IN (");
                sql.Append(skeysStr);
                sql.Append(')');
            }

            sql.Append(" ORDER BY FIELD(feeds.id,");
            sql.Append(idsStr);
            sql.Append(");");
            #endregion

            #region Properties
            if ((infoTypes & FeedInfoTypes.Properties) == FeedInfoTypes.Properties)
            {
                sql.Append("SELECT feedpropmap.feedId, props.id, props.type, props.value FROM feedpropmap INNER JOIN props ON feedpropmap.propId = props.id WHERE feedpropmap.feedId IN (");
                sql.Append(idsStr);

                sql.Append(") ORDER BY FIELD(feedpropmap.feedId,");
                sql.Append(idsStr);
                sql.Append(");");
            }
            #endregion

            List<FeedInfo> feeds = new List<FeedInfo>();

            using (MySqlCommand command = new MySqlCommand(sql.ToString(), conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    FeedInfo feed = new FeedInfo()
                    {
                        Id = reader.GetUInt32(0),
                        Type = (FeedType)reader.GetByte(1),
                        ImageId = reader.IsDBNull(2) ? 0 : reader.GetUInt32(2),
                        Title = reader[3] as string,
                        Description = reader[4] as string
                    };

                    if ((infoTypes & FeedInfoTypes.DocumentLookups) == FeedInfoTypes.DocumentLookups)
                    {
                        feed.Url = reader.GetString(5);
                        feed.ImageUrl = reader[6] as string;
                    }

                    feeds.Add(feed);
                }

                if ((infoTypes & FeedInfoTypes.Properties) == FeedInfoTypes.Properties && reader.NextResult())
                {
                    List<IndexPropertyInfo> properties = new List<IndexPropertyInfo>();
                    uint currentFeedId = 0;
                    int currentFeedIndex = 0;

                    while (reader.Read())
                    {
                        uint feedId = reader.GetUInt32(0);
                        uint propId = reader.GetUInt32(1);
                        IndexPropertyType propType = (IndexPropertyType)reader.GetByte(2);
                        string propValue = reader.GetString(3);

                        if (feedId != currentFeedId)
                        {
                            if (currentFeedId != 0)
                            {
                                currentFeedIndex = SkipMissingFeedIndex(feeds, currentFeedId, currentFeedIndex);

                                feeds[currentFeedIndex].Properties = properties.ToArray();
                                currentFeedIndex++;
                                properties.Clear();
                            }

                            currentFeedId = feedId;
                        }

                        properties.Add(new IndexPropertyInfo(propId, propType, propValue));
                    }

                    currentFeedIndex = SkipMissingFeedIndex(feeds, currentFeedId, currentFeedIndex);

                    if (currentFeedIndex < feeds.Count)
                        feeds[currentFeedIndex].Properties = properties.ToArray();
                }
            }

            return feeds;
        }

        private static int SkipMissingFeedIndex(List<FeedInfo> feeds, uint currentFeedId, int currentFeedIndex)
        {
            if (feeds[currentFeedIndex].Id != currentFeedId)
            {
                for (int i = currentFeedIndex; i < feeds.Count; i++)
                {
                    if (feeds[i].Id == currentFeedId)
                        break;

                    else
                        currentFeedIndex++;
                }
            }

            return currentFeedIndex;
        }

        public IList<FeedInfo> GetFeed(uint[] ids, FeedInfoTypes infoTypes)
        {
            return GetFeed(ids, null, infoTypes);
        }

        public FeedInfo GetFeed(uint id, ushort skey, FeedInfoTypes infoTypes)
        {
            IList<FeedInfo> feeds = GetFeed(new uint[] { id }, new ushort[] { skey }, infoTypes);

            return feeds.Count > 0 ? feeds[0] : null;
        }

        public FeedInfo GetFeed(uint id, FeedInfoTypes infoTypes)
        {
            IList<FeedInfo> feeds = GetFeed(new uint[] { id }, null, infoTypes);

            return feeds.Count > 0 ? feeds[0] : null;
        }

        public IList<FeedInfo> GetFeed(string sql, FeedInfoTypes infoTypes)
        {
            List<uint> feedIds = new List<uint>();

            using (MySqlCommand command = new MySqlCommand(sql, conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                    feedIds.Add(reader.GetUInt32(0));
            }

            return GetFeed(feedIds.ToArray(), infoTypes);
        }
    }
}
