using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Exo.Exoget.Model.User;

namespace Exo.Exoget.Model.Service
{
    [ServiceContract(Namespace="")]
    public interface IUser
    {
        [OperationContract]
        OperationResponseInfo AddMediaComment(uint mediaId, uint parentId, byte rating, string title, string body);

        [OperationContract]
        OperationResponseInfo AddMediaCommentRating(uint id, CommentRating rating);

        [OperationContract]
        OperationResponseInfo AddMediaFavorite(uint id, ushort skey);
    }
}