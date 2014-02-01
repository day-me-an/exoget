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
using MySql.Data.MySqlClient;
using Exo.Exoget.Model.Media;
using System.Diagnostics;
using Exo.Exoget.Model;
using System.Net;
using Exo.Web;
using Exo.Exoget.Model.User;
using Exo.Exoget.Web.Controls;

public partial class MediaDetails : CommonPage
{
    private static readonly BufferedCounter<uint> bufferedCounter;
    private static readonly BufferedInsertOrIncrement<uint> bufferedUserHistoryCounter;
    private static readonly AntiScrape antiScrape;
    
    private MediaInfo media;

    static MediaDetails()
    {
        antiScrape = new AntiScrape("MediaDetails.aspx", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30), TimeSpan.FromDays(1), 100);

        IDbConnection conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["exoget"].ConnectionString);
        conn.Open();
        bufferedCounter = new BufferedCounter<uint>("media", "id", "viewCount", TimeSpan.FromMinutes(1), conn);

        IDbConnection conn2 = new MySqlConnection(ConfigurationManager.ConnectionStrings["exoget"].ConnectionString);
        conn2.Open();
        bufferedUserHistoryCounter = new BufferedInsertOrIncrement<uint>("usermediahistory", new string[] { "userId", "mediaId" }, "viewCount", TimeSpan.FromSeconds(5), conn2);
    }

    public MediaDetails()
    {
        PreLoad += new EventHandler(Antiscrape);
        PreLoad += new EventHandler(Missing);
        PreLoad += new EventHandler(SetTitle);

        Load += new EventHandler(ShowComments);

        Unload += new EventHandler(IncrementViews);
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

    private void Missing(object sender, EventArgs e)
    {
        if (Media == null)
        {
            Response.StatusCode = 404;
            CloseDatabaseConnection();
            Response.End();
        }
    }

    private void SetTitle(object sender, EventArgs e)
    {
        Page.Title = Media.Title + " - exoGet " + Media.Type;

        if (Media.Description != null)
            Header.Controls.Add(new HtmlMeta { Name = "description", Content = Media.Description });

        Header.Controls.Add(new LiteralControl("<script type=\"text/javascript\">isMediaDetails=true;currentMediaId=" + Id + ";currentMediaSkey=" + SKey + ";</script>"));
    }

    protected void ShowComments(object sender, EventArgs e)
    {
        if (Media.Comments != null)
        {
            CommentsPlaceholder.Controls.Add(new LiteralControl("<ul id=\"mediaComments\" class=\"comments\">"));

            foreach (CommentInfo commentInfo in Media.Comments)
            {
                MediaComment comment = (MediaComment)LoadControl("~/UserControls/MediaComment.ascx");
                comment.Comment = (MediaCommentInfo)commentInfo;

                CommentsPlaceholder.Controls.Add(comment);
            }

            CommentsPlaceholder.Controls.Add(new LiteralControl("</ul>"));
        }
    }

    private void IncrementViews(object sender, EventArgs e)
    {
        bufferedCounter.Increment(Id);

        if (User.Identity.IsAuthenticated)
            bufferedUserHistoryCounter.Insert(ExoUser.Id, Id);
    }

    protected MediaInfo Media
    {
        get
        {
            return media ?? (media = new MediaManager(DatabaseConnection).GetMedia(Id, SKey, MediaInfoTypes.Properties | MediaInfoTypes.Comments));
        }
    }
}