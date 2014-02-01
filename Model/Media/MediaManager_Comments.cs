using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using Exo.Exoget.Model.User;

namespace Exo.Exoget.Model.Media
{
    public partial class MediaManager
    {
        public void GetComments(MediaInfo media)
        {
            int rateSum = 0;
            int count = 0;

            Dictionary<uint, MediaCommentInfo> parentComments = new Dictionary<uint, MediaCommentInfo>();
            Dictionary<uint, byte> userIdToRating = new Dictionary<uint, byte>();

            using (MySqlCommand command = new MySqlCommand(
@"SELECT
usermediacomments.id,
usermediacomments.parentId,
usermediacomments.rating,
usermediacomments.title,
usermediacomments.body,
usermediacomments.approve,
usermediacomments.disapprove, 
usermediacomments.modified,
users.username,
users.id

FROM usermediacomments
INNER JOIN users ON usermediacomments.userId = users.id
WHERE usermediacomments.mediaId = " + media.Id +
" ORDER BY usermediacomments.parentId ASC", conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    byte rating = reader.GetByte(2);

                    if (rating > 0)
                    {
                        rateSum += rating;
                        count++;

                        userIdToRating[reader.GetUInt32(9)] = rating;
                    }

                    if (reader.IsDBNull(4))
                        continue;

                    MediaCommentInfo comment = new MediaCommentInfo
                    {
                        Id = reader.GetUInt32(0),
                        Title = !reader.IsDBNull(3) ? reader.GetString(3) : null,
                        Body = reader.GetString(4),
                        Approve = reader.GetUInt16(5),
                        Disapprove = reader.GetUInt16(6),
                        Modified = reader.GetDateTime(7)
                    };

                    comment.User = new UserInfo(reader.GetUInt32(9));
                    comment.User.Username = reader.GetString(8);

                    if (!reader.IsDBNull(1))
                    {
                        parentComments[reader.GetUInt32(1)].Children.Add(comment);
                        comment.Parent = parentComments[reader.GetUInt32(1)];
                    }

                    parentComments.Add(comment.Id, comment);
                }
            }

            if (count > 0)
            {
                List<MediaCommentInfo> comments = new List<MediaCommentInfo>();

                foreach (KeyValuePair<uint, MediaCommentInfo> pair in parentComments)
                {
                    byte rating;

                    if (userIdToRating.TryGetValue(pair.Value.User.Id, out rating))
                    {
                        pair.Value.Rating = rating;
                        userIdToRating.Remove(pair.Value.User.Id);
                    }

                    if (!pair.Value.HasParent)
                        comments.Add(pair.Value);
                }

                media.Rating = rateSum / count;
                media.Comments = comments.ToArray();
            }
        }
    }
}
