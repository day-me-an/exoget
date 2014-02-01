using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using Exo.Exoget.Model.Media;
using Exo.Exoget.Model.Search;
using System.Diagnostics;
using System.Threading;

namespace Exo.Exoget.Model.User
{
    public partial class UserManager
    {
        public OperationResponseInfo AddMediaFavorite(uint userId, uint mediaId, ushort mediaSKey)
        {
            OperationStatus status;

            try
            {
                using (MySqlCommand command = new MySqlCommand("addUserMediaFavorite", conn))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("?userId", userId);
                    command.Parameters.AddWithValue("?mediaId", mediaId);
                    command.Parameters.AddWithValue("?mediaSKey", mediaSKey == 0 ? "NULL" : mediaSKey.ToString());
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

            if (status == OperationStatus.Duplicate)
                return new OperationResponseInfo(status, UserResources.FavoriteAddDuplicate);

            else
                return (OperationResponseInfo)status;
        }

        public OperationResponseInfo AddMediaFavorite(UserIdentity user, uint mediaId, ushort mediaSKey)
        {
            return AddMediaFavorite(user.Id, mediaId,  mediaSKey);
        }

        public OperationResponseInfo AddMediaFavorite(UserIdentity user, uint mediaId)
        {
            return AddMediaFavorite(user, mediaId, 0);
        }

        public OperationResponseInfo AddMediaFavorite(uint mediaId, ushort mediaSKey)
        {
            if (!System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
                return (OperationResponseInfo)OperationStatus.NotAuthenticated;

            else
            {
                Debug.Assert(Thread.CurrentPrincipal.Identity is UserIdentity);

                return AddMediaFavorite((UserIdentity)System.Threading.Thread.CurrentPrincipal.Identity, mediaId, mediaSKey);
            }
        }
    }
}
