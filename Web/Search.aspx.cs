using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using Exo.Exoget.Model.Search;
using Exo.Web;
using Exo.Web.Controls.Cache;
using MySql.Data.MySqlClient;

public partial class Search : CommonPage
{
    private static readonly AntiScrape antiScrape;
    private static readonly BufferedInsertOrIncrement<string> searchCounter;

    private static readonly CachedUrlsElement audioExoCacheControlConfig, videoExoCacheControlConfig;

    private string query, htmlEncodedQuery;

    static Search()
    {
        antiScrape = new AntiScrape("Search.aspx", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30), TimeSpan.FromDays(1), 100);

        IDbConnection conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["exoget"].ConnectionString);
        conn.Open();
        searchCounter = new BufferedInsertOrIncrement<string>("searchhistory", new string[] { "scope", "options", "query" }, "count", TimeSpan.FromMinutes(1), conn);

        IEnumerable<CachedUrlsElement> cachedUrlElements = ((ExoCacheSettingsSection)WebConfigurationManager.GetWebApplicationSection("exoCacheControl")).CachedUrls.Cast<CachedUrlsElement>();

        audioExoCacheControlConfig =
            (from cachedUrlElement in cachedUrlElements where cachedUrlElement.Name == "audioSearch" select cachedUrlElement).First();

        videoExoCacheControlConfig =
            (from cachedUrlElement in cachedUrlElements where cachedUrlElement.Name == "videoSearch" select cachedUrlElement).First();
    }

    public Search()
    {
        PreLoad += Antiscrape;
        PreLoad += ValidateQueryStrings;
        PreLoad += SearchCount;
        PreLoad += SetTitle;
        PreLoad += AddExoCacheControl;
    }

    private void AddExoCacheControl(object sender, EventArgs e)
    {
        ExoCacheControl exoCacheControl = new ExoCacheControl()
        {
            CurrentCacheSettings = Scope == SearchScope.Audio ? audioExoCacheControlConfig : videoExoCacheControlConfig
        };

        Control searchControl = LoadControl("~/UserControls/Search.ascx");
        exoCacheControl.Controls.Add(searchControl);

        exoCacheControlHolder.Controls.Add(exoCacheControl);
    }

    private void Antiscrape(object sender, EventArgs e)
    {
        IPAddress userIp = IPAddress.Parse(Request.UserHostAddress);

        if (!antiScrape.ValidateRequest(userIp))
        {
            Response.StatusCode = 503;
            Response.ContentType = "text/plain";
            Response.Write(ConfigurationSettings.AppSettings["AntiscrapeMessage"]);

            CloseDatabaseConnection();
            Response.End();
        }
    }

    private void ValidateQueryStrings(object sender, EventArgs e)
    {
        if (Request.QueryString["query"] == null)
        {
            Response.StatusCode = 404;
            CloseDatabaseConnection();
            Response.End();
        }
    }

    private void SearchCount(object sender, EventArgs e)
    {
        if (Request.QueryString["page"] == null)
            searchCounter.Insert((byte)Scope, (uint)CurrentSearchOptions, Query);
    }

    private void SetTitle(object sender, EventArgs e)
    {
        string format;

        if (Scope == SearchScope.Video)
            format = "{0} Videos";

        else
            format = "{0} MP3 Download";
		
		string query = Query;
		string field = Request.QueryString["field"];
		
		if(field != null)
		{
			query = query.Replace(field, "");
			query = query.Replace(":", "");
			query = query.Replace("\"", "");
		}
		
        Title = String.Format(format, HttpUtility.HtmlEncode(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(query)));
    }

    protected string Query
    {
        get
        {
            if (query == null)
            {
                query = Request.QueryString["query"].Trim();

                if (Request.QueryString["rewritten"] != null)
                {
                    if (query[0] == '-')
                    {
                        query = query.Substring(1);
                        query = query.Insert(0, "_____");
                    }

                    query = query.Replace("---", "_____");
                    query = query.Replace('-', ' ');
                    query = query.Replace("_____", " -");
                }
            }

            return query;
        }
    }

    protected string HtmlEncodedQuery
    {
        get { return htmlEncodedQuery ?? (htmlEncodedQuery = Server.HtmlEncode(Query)); }
    }

    protected SearchScope Scope
    {
        get
        {
            SearchScope scope = SearchScope.Audio;

            if (Request.QueryString["scope"] == "2")
                scope = SearchScope.Video;

            return scope;
        }
    }
}
