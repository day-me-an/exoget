using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace Exo.Exoget.Model.User
{
    public class MediaCommentInfo : CommentInfo
    {
        private uint mediaId;
        private ushort approve, disapprove;
        private byte rating;

        public uint MediaId
        {
            get { return mediaId; }
            set { mediaId = value; }
        }

        public byte Rating
        {
            get { return rating; }
            set { rating = value; }
        }

        /// <summary>
        /// The rating expressed as a percentage
        /// </summary>
        public double RatingPercentage
        {
            get { return Math.Round(rating * 20d); }
        }

        public ushort Approve
        {
            get { return approve; }
            set { approve = value; }
        }

        public ushort Disapprove
        {
            get { return disapprove; }
            set { disapprove = value; }
        }
    }
}
