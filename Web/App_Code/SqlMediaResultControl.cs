using System;
using System.Collections.Generic;
using Exo.Exoget.Model.Media;
using Exo.Exoget.Model.Search;
using MySql.Data.MySqlClient;

namespace Exo.Exoget.Web.Controls
{
    public partial class SqlMediaResult : Exo.Exoget.Web.Controls.MediaResult
    {
        private string sql;

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (Result != null && Result.ResultsCount == 0)
                writer.Write("<em class=\"flash\">Sorry, no results found.</em>");

            else
                base.Render(writer);
        }

        public override ResultInfo Result
        {
            get
            {
                if (result == null && sql != null && PageNumber > 0)
                {
                    List<uint> mediaIds = new List<uint>(10);
                    result = new ResultInfo();

                    string fsql = String.Format("{0} LIMIT {1},{2};SELECT FOUND_ROWS();", sql, StartIndex, ResultsPerPage);

                    using (MySqlCommand command = new MySqlCommand(fsql, ((CommonPage)Page).DatabaseConnection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            mediaIds.Add(reader.GetUInt32(0));

                        reader.NextResult();

                        if (reader.Read())
                            result.ResultsFoundCount = reader.GetUInt32(0);
                    }

                    MediaManager mediaManager = new MediaManager(((CommonPage)Page).DatabaseConnection);
                    result.Medias = mediaManager.GetMedia(mediaIds.ToArray(), MediaInfoTypes.Properties);
                }

                return result;
            }

            set { throw new NotImplementedException(); }
        }

        public string Sql
        {
            get { return sql; }
            set { sql = value; }
        }
    }
}