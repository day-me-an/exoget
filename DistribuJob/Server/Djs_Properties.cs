using System;
using System.Collections.Generic;
using System.Text;
using Exo.Exoget.Model.Search;
using Exo.Misc;
using Exo.Extensions;
using DistribuJob.Client.Extracts;

namespace DistribuJob.Server
{
    public partial class Djs
    {
        private void AddToPropertyMap(object mapKeyId, IndexPropertiesBase properties, StringBuilder sql, string tableName, string mapKeyColumnName)
        {
            if (!properties.IndexProperties.GetEnumerator().MoveNext())
                return;

            else
                properties.IndexProperties.GetEnumerator().Reset();

            if (mapKeyId.ToString()[0] == '@')
                sql.AppendLine("IF " + mapKeyId + " IS NOT NULL THEN");

            sql.Append("INSERT IGNORE INTO " + tableName + " (" + mapKeyColumnName + ", propId) VALUES ");

            foreach (IndexPropertyInfo property in properties.IndexProperties)
            {
                sql.AppendFormat("({0}, getPropId({1}, {2})),",
                    mapKeyId,
                    (byte)property.Type,
                    property.Value.SqlEscape()
                    );
            }

            sql.Remove(sql.Length - 1, 1);
            sql.AppendLine(";");

            if (mapKeyId.ToString()[0] == '@')
                sql.AppendLine("END IF;");
        }

        private void AddLinkProperties(object linkId, IndexPropertiesBase properties, StringBuilder sql)
        {
            AddToPropertyMap(linkId, properties, sql, "linkpropmap", "linkId");
        }

        private void AddFeedProperties(object feedId, IndexPropertiesBase properties, StringBuilder sql)
        {
            AddToPropertyMap(feedId, properties, sql, "feedpropmap", "feedId");
        }

        private void AddPlaylistProperties(object playlistId, IndexPropertiesBase properties, StringBuilder sql)
        {
            AddToPropertyMap(playlistId, properties, sql, "playlistpropmap", "playlistId");
        }

        private void AddMediaProperties(object mediaId, IndexPropertiesBase properties, StringBuilder sql)
        {
            AddToPropertyMap(mediaId, properties, sql, "mediapropmap", "mediaId");
        }
    }
}
