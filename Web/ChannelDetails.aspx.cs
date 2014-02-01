using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using Exo.Exoget.Model.Feed;
using Exo.Exoget.Model.Search;
using System.Collections.Generic;
using Exo.Exoget.Web.Controls;

public partial class ChannelDetails : CommonPage
{
    protected FeedInfo feed;

    public ChannelDetails()
    {
        Load += new EventHandler(SetTitle);
        Load += new EventHandler(ShowLatestEpisodes);
    }

    private void SetTitle(object sender, EventArgs e)
    {
        Page.Title = String.Format("{0} - exoGet Channels", Feed.Title);

        if (Feed.Description != null)
            Header.Controls.Add(new HtmlMeta { Name = "description", Content = Feed.Description });

        HtmlLink feedLink = new HtmlLink
        {
            Href = Feed.Url
        };

        feedLink.Attributes["rel"] = "alternate";
        feedLink.Attributes["type"] = "application/rss+xml";
        feedLink.Attributes["title"] = Feed.Title;

        Header.Controls.Add(feedLink);
    }

    private void ShowLatestEpisodes(object sender, EventArgs e)
    {
        SqlMediaResult sqlMediaResult = new SqlMediaResult
        {
            Sql = String.Format("SELECT SQL_CALC_FOUND_ROWS id FROM media WHERE feedId = {0} ORDER BY id DESC", Feed.Id),
            MaxPageNumber = 10,
            FirstPagingUriFormat = String.Format("/channel/{0}_{1}", Id, SKey),
            PagingUriFormat = String.Format("/channel/{0}_{1}/episodes/latest/page{{0}}", Id, SKey)
        };

        mediaResultHolder.Controls.Add(sqlMediaResult);
    }

    protected FeedInfo Feed
    {
        get { return feed ?? (feed = new FeedManager(DatabaseConnection).GetFeed(Id, SKey, FeedManager.FeedInfoTypes.DocumentLookups | FeedManager.FeedInfoTypes.Properties)); }
    }
}
