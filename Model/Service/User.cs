using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exo.Exoget.Model.User;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.ServiceModel;

namespace Exo.Exoget.Model.Service
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class User : IUser, IDisposable
    {
        private readonly MySqlConnection conn;
        private readonly UserManager user;

        public User()
        {
            conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["exoget"].ConnectionString);
            conn.Open();

            user = new UserManager(conn);
        }

        #region IUser Members

        public OperationResponseInfo AddMediaFavorite(uint id, ushort skey)
        {
            return null;//return user.AddMediaFavorite(id, skey);
        }

        public OperationResponseInfo AddMediaComment(uint mediaId, ushort mediaSkey, uint parentId, byte rating, string title, string body)
        {
            MediaCommentInfo comment = new MediaCommentInfo();
            comment.Rating = rating;
            comment.Title = title;
            comment.Body = body;

            Validator<MediaCommentInfo> validator = ValidationFactory.CreateValidator<MediaCommentInfo>();
            ValidationResults results = validator.Validate(comment);

            if (results.IsValid)
                return user.AddMediaComment(mediaId,mediaSkey, parentId, comment);

            else
                return new OperationResponseInfo(OperationStatus.NotValid, results.FirstResult.Message);
        }

        public OperationResponseInfo AddMediaCommentRating(uint id, CommentRating rating)
        {
            return user.AddMediaCommentRating(id, rating);
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            conn.Dispose();
        }

        #endregion
    }
}
