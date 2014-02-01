using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Exo.Exoget.Model.Search;
using System.Text;

namespace Exo.Exoget.Web.Controls
{
    public partial class SearchForm : System.Web.UI.UserControl
    {
        protected enum SearchScope : byte
        {
            Audio,
            Video
        }

        private SearchScope searchScope;
        private string query;

        public SearchForm()
        {
            Load += Page_Load;
        }

        private void Page_Load(object sender, EventArgs e)
        {
            string scopeQuery;

            if (Request.QueryString["scope"] != null)
            {
                scopeQuery = Request.QueryString["scope"];

                switch (scopeQuery)
                {
                    default:
                    case "1":
                        searchScope = SearchScope.Audio;
                        break;

                    case "2":
                        searchScope = SearchScope.Video;
                        break;
                }
            }
            else if (Request.Path.EndsWith("MediaDetails.aspx"))
            {
                switch (Request.QueryString["type"])
                {
                    case "1":
                    default:
                        searchScope = SearchScope.Audio;
                        break;

                    case "2":
                        searchScope = SearchScope.Video;
                        break;
                }
            }
            else if (Request.Path.EndsWith("Videos.aspx"))
                searchScope = SearchScope.Video;

            else
                searchScope = SearchScope.Audio;
        }

        protected SearchScope Scope
        {
            get { return searchScope; }
        }

        protected string Query
        {
            get
            {
                if (query == null)
                {
                    if (Request.QueryString["query"] != null)
                        query = Request.QueryString["query"];

                    else
                        query = String.Empty;

                    if (Request.QueryString["rewritten"] != null)
                    {
						if(query[0] == '-')
						{
                            query = query.Substring(1);
                            query = query.Insert(0, "_____");
						}					
					
                        query = query.Replace("---", "_____");
                        query = query.Replace('-', ' ');
                        query = query.Replace("_____", " -");
                    }

                    query = HttpUtility.HtmlEncode(query);
                }

                return query;
            }
        }
    }
}