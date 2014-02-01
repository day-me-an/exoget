using System;
using System.Collections.Generic;
using System.Text;
using Exo.Exoget.Model.User;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Exo.Exoget.Model.User
{
    public enum CommentStatus : byte
    {
        None = 0,
        Success,
        IntervalTooLow,
        LimitPerTimeFrameExceeded,
    }

    public enum CommentRating : byte
    {
        None = 0,
        Approve = 1,
        Disapprove = 2
    }

    public abstract class CommentInfo
    {
        private uint id, userId;
        private UserInfo user;
        private CommentInfo parent;
        private List<CommentInfo> children;
        private string title, body;
        private DateTime modified;

        public uint Id
        {
            get { return id; }
            set { id = value; }
        }

        public uint ParentId
        {
            get { return parent.id; }
        }

        public CommentInfo Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public bool HasParent
        {
            get { return parent != null; }
        }

        public List<CommentInfo> Children
        {
            get { return children ?? (children = new List<CommentInfo>()); }
        }

        public uint UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        public UserInfo User
        {
            get { return user; }
            set { user = value; }
        }

        [StringLengthValidator(1, 255)]
        [IgnoreNulls]
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        [StringLengthValidator(1, 1024)]
        public string Body
        {
            get { return body; }
            set { body = value; }
        }

        public DateTime Modified
        {
            get { return modified; }
            set { modified = value; }
        }

        public string ModifiedTimeDifferenceDescription
        {
            get { return Exo.Misc.ExoUtil.GetTimeDifferenceDescription(modified); }
        }
    }
}
