using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Resources;
using System.Diagnostics;
using Exo.Exoget.Model.Media;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace Exo.Exoget.Model.User
{
    public partial class UserManager
    {
        public OperationResponseInfo AddMediaComment(uint mediaId, ushort mediaSkey, uint userId, uint parentId, MediaCommentInfo comment)
        {
            // not ratings
            if (comment.Rating == 0)
            {
                Validator<MediaCommentInfo> validator = ValidationFactory.CreateValidator<MediaCommentInfo>();
                ValidationResults results = validator.Validate(comment);

                if (!results.IsValid)
                    return new OperationResponseInfo(OperationStatus.UnknownReference, results.FirstResult.Message);
            }

            OperationStatus status;

            try
            {
                using (MySqlCommand command = new MySqlCommand("addUserMediaComment", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("?mediaId", mediaId);
                    command.Parameters.AddWithValue("?mediaSkey", mediaSkey);
                    command.Parameters.AddWithValue("?userId", userId);
                    command.Parameters.AddWithValue("?parent", parentId == 0 ? null : parentId.ToString());
                    command.Parameters.AddWithValue("?rating", comment.Rating);
                    command.Parameters.AddWithValue("?title", comment.Title);
                    command.Parameters.AddWithValue("?body", comment.Body);

                    command.Parameters.Add("?responseStatus", MySqlDbType.Byte);
                    command.Parameters["?responseStatus"].Direction = ParameterDirection.Output;

                    command.ExecuteNonQuery();

                    status = (OperationStatus)Convert.ToByte(command.Parameters["?responseStatus"].Value);
                }

                /*
                if (parentId != 0)
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT ", conn))
                    {
                        // if parent comment's user has enabled an email when a reply to there comment is added => email them
                    }
                }
                */ 
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);

                status = OperationStatus.UnknownError;
            }

            switch (status)
            {
                case OperationStatus.Success:
                    {
                        // remove from cache
                        MediaManager.mediaCache.Remove(mediaId);

                        return new OperationResponseInfo(status, UserResources.CommentAddSuccess);
                    }

                case OperationStatus.IntervalNotElapsed:
                    return new OperationResponseInfo(status, UserResources.CommentAddIntervalNotElapsed);

                case OperationStatus.QuotaExceeded:
                    return new OperationResponseInfo(status, UserResources.CommentAddQuotaExceeded);

                case OperationStatus.Duplicate:
                    return new OperationResponseInfo(status, UserResources.CommentAddDuplicate);

                default:
                    return (OperationResponseInfo)status;
            }
        }

        /// <summary>
        /// Adds a media rating
        /// </summary>
        public OperationResponseInfo AddMediaRating(uint userId, uint mediaId, ushort mediaSkey, byte rating)
        {
            MediaCommentInfo comment = new MediaCommentInfo()
            {
                Rating = rating
            };

            return AddMediaComment(mediaId, mediaSkey, userId, 0, comment);
        }

        /// <summary>
        /// Adds a media rating using the current authenticated user
        /// </summary>
        public OperationResponseInfo AddMediaRating(uint mediaId, ushort mediaSkey, byte rating)
        {
            if (!System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
                return (OperationResponseInfo)OperationStatus.NotAuthenticated;

            else
            {
                Debug.Assert(Thread.CurrentPrincipal.Identity is UserIdentity);

                return AddMediaRating(((UserIdentity)Thread.CurrentPrincipal.Identity).Id, mediaId, mediaSkey, rating);
            }
        }

        /// <summary>
        /// Adds a media comment using the current authenticated user
        /// </summary>
        public OperationResponseInfo AddMediaComment(uint mediaId, ushort mediaSkey, uint parentId, MediaCommentInfo comment)
        {
            if (!System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
                return (OperationResponseInfo)OperationStatus.NotAuthenticated;

            else
            {
                Debug.Assert(Thread.CurrentPrincipal.Identity is UserIdentity);

                return AddMediaComment(mediaId, mediaSkey, ((UserIdentity)Thread.CurrentPrincipal.Identity).Id, parentId, comment);
            }
        }

        public void RemoveComment(uint commentId)
        {
            using (MySqlCommand command = new MySqlCommand("DELETE FROM mediacomments WHERE id = " + commentId, conn))
            {
                command.ExecuteNonQuery();
            }
        }

        public OperationResponseInfo AddMediaCommentRating(uint mediaId, uint commentId, CommentRating rating, uint userId)
        {
            OperationStatus status;

            try
            {
                using (MySqlCommand command = new MySqlCommand("addUserMediaCommentRating", conn))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("?userId", userId);
                    command.Parameters.AddWithValue("?commentId", commentId);
                    command.Parameters.AddWithValue("?rating", (byte)rating);
                    command.Parameters.Add("?responseStatus", MySqlDbType.Byte);

                    command.ExecuteNonQuery();

                    status = (OperationStatus)Convert.ToByte(command.Parameters["?responseStatus"].Value);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);

                status = OperationStatus.UnknownError;
            }

            if (status == OperationStatus.Success)
            {
                // remove from cache
                MediaManager.mediaCache.Remove(mediaId);
            }
            
            if (status == OperationStatus.Duplicate)
                return new OperationResponseInfo(status, UserResources.CommentRatingDuplicate);

            else
                return (OperationResponseInfo)status;
        }

        public OperationResponseInfo AddMediaCommentRating(uint mediaId, uint id, CommentRating rating)
        {
            if (!System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
                return (OperationResponseInfo)OperationStatus.NotAuthenticated;

            else
            {
                Debug.Assert(Thread.CurrentPrincipal.Identity is UserIdentity);

                return AddMediaCommentRating(mediaId, id, rating, ((UserIdentity)System.Threading.Thread.CurrentPrincipal.Identity).Id);
            }
        }
    }
}