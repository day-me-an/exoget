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
using Exo.Exoget.Model.User;
using Exo.Exoget.Web.Controls;

public partial class UserProfile : CommonPage
{
    private UserInfo user;

    public UserProfile()
    {
        PreLoad += Pre_Load;
    }

    protected void Pre_Load(object sender, EventArgs e)
    {
        if (User == null)
        {
            DisplayFlash("Sorry, that user does not exist.");
            content.Enabled = false;
            Response.StatusCode = 404;
            return;
        }

        string sql = null;
        string uriName = null;

        switch (CurrentView)
        {
            default:
            case "Favorite":
                {
                    Page.Title = String.Format("Favorites - {0} - exoGet", User.Username);
                    sectionLiteral.Text = "Favorites";
                    uriName = "favs";

                    if (!User.FavoritesPublic && (ExoUser == null || ExoUser.Id != User.Id))
                    {
                        DisplayFlash("Sorry, this user has disabled public access to there favorites.");

                        return;
                    }

                    sql = String.Format("SELECT SQL_CALC_FOUND_ROWS mediaId FROM usermediafavorites WHERE userId={0} ORDER BY id DESC", User.Id);
                    break;
                }

            case "History":
                {
                    Page.Title = String.Format("History - {0} - exoGet", User.Username);
                    sectionLiteral.Text = "History";
                    uriName= "history";

                    if (!User.HistoryPublic && (ExoUser == null || ExoUser.Id != User.Id))
                    {
                        DisplayFlash("Sorry, this user has disabled public access to there viewing history.");

                        return;
                    }

                    sql = String.Format("SELECT SQL_CALC_FOUND_ROWS mediaId FROM usermediahistory WHERE userId={0} ORDER BY id DESC", User.Id);
                    break;
                }

            case "Rated":
                {
                    Page.Title = String.Format("Rated - {0} - exoGet", User.Username);
                    sectionLiteral.Text = "Rated";
                    uriName = "rated";

                    if (!User.RatedPublic && (ExoUser == null || ExoUser.Id != User.Id))
                    {
                        DisplayFlash("Sorry, this user has disabled public access to there rating history.");

                        return;
                    }

                    sql = String.Format("SELECT DISTINCT SQL_CALC_FOUND_ROWS mediaId FROM usermediacomments WHERE userId={0} ORDER BY id DESC", User.Id);
                    break;
                }
        }

        if (sql != null)
        {
            SqlMediaResult sqlMediaResult = new SqlMediaResult
            {
                Sql = sql,
                MaxPageNumber = 1000,
                FirstPagingUriFormat = String.Format("/user/{0}/{1}", User.Username, uriName),
                PagingUriFormat = String.Format("/user/{0}/{1}/page{{0}}", User.Username, uriName)
            };

            mediaResultHolder.Controls.Add(sqlMediaResult);
        }
    }

    private void DisplayFlash(string contents)
    {
        tipsPlaceHolder.Controls.Add(new LiteralControl("<em class=\"flash\">" + contents + "</em>"));
    }

    protected string CurrentView
    {
        get { return Request.QueryString["view"]; }
    }

    protected new UserInfo User
    {
        get
        {
            if (user == null && Request.QueryString["username"] != null)
                user = new UserManager(DatabaseConnection).GetUser(Request.QueryString["username"]);

            return user;
        }
    }
}
