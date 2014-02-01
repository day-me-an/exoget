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

namespace Exo.Exoget.Web.Controls
{
    public partial class MediaComment : System.Web.UI.UserControl
    {
        private MediaCommentInfo comment;

        public MediaComment()
        {
            Load += new EventHandler(ShowComments);
        }

        void ShowComments(object sender, EventArgs e)
        {
            if (Comment.Children != null && Comment.Children.Count > 0)
            {
                RepliesHolder.Controls.Add(new LiteralControl("<ul class=\"comments commentReplies\">"));

                foreach (CommentInfo childComment in Comment.Children)
                {
                    MediaComment commentControl = (MediaComment)LoadControl("~/UserControls/MediaComment.ascx");
                    commentControl.Comment = (MediaCommentInfo)childComment;

                    RepliesHolder.Controls.Add(commentControl);
                }

                RepliesHolder.Controls.Add(new LiteralControl("</ul>"));
            }
        }

        public MediaCommentInfo Comment
        {
            get { return comment; }
            set { comment = value; }
        }
    }
}