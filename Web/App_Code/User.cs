using System;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using Exo.Exoget.Model.User;
using MySql.Data.MySqlClient;
using Exo.Exoget.Model;
using System.Configuration;
using System.Diagnostics;
using Exo.Exoget.Model.Media;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Exo.Exoget.Model.Search;

[WebService(Namespace = "http://exoget.com/services/user")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class User : System.Web.Services.WebService
{
    private readonly MySqlConnection conn;
    private readonly UserManager user;

    public User()
    {
        conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["exoget"].ConnectionString);
        conn.Open();

        user = new UserManager(conn);
    }

    protected override void Dispose(bool disposing)
    {
        conn.Dispose();

        base.Dispose(disposing);
    }

    [WebMethod]
    public OperationResponseInfo AddMediaFavorite(uint mediaId, ushort mediaSkey)
    {
        return user.AddMediaFavorite(mediaId, mediaSkey);
    }

    [WebMethod]
    public OperationResponseInfo AddMediaRating(uint mediaId, ushort mediaSkey, byte rating)
    {
        return user.AddMediaRating(mediaId, mediaSkey, rating);
    }

    [WebMethod]
    public OperationResponseInfo AddMediaComment(uint mediaId, ushort mediaSkey, uint parentId, string title, string body)
    {
        MediaCommentInfo comment = new MediaCommentInfo
        {
            Title = String.IsNullOrEmpty(title) ? null : title,
            Body = String.IsNullOrEmpty(body) ? null : body
        };

        return (OperationResponseInfo)user.AddMediaComment(mediaId, mediaSkey, parentId, comment);
    }

    [WebMethod]
    public OperationResponseInfo AddMediaCommentRating(uint mediaId, uint commentId, CommentRating rating)
    {
        return user.AddMediaCommentRating(mediaId, commentId, rating);
    }
}

