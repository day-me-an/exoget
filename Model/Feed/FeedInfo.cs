using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exo.Exoget.Model.Search;
using Exo.Web.Feed;

namespace Exo.Exoget.Model.Feed
{
    public class FeedInfo : PropertiesBase
    {
        private uint id, imageDocId, imageId;
        private ushort skey;
        private FeedType type;
        private string url, imageUrl, title, desc;

        public uint Id
        {
            get { return id; }
            set { id = value; }
        }

        public ushort SKey
        {
            get { return skey; }
            set { skey = value; }
        }

        public FeedType Type
        {
            get { return type; }
            set { type = value; }
        }

        public uint ImageDocId
        {
            get { return imageDocId; }
            set { imageDocId = value; }
        }

        public uint ImageId
        {
            get { return imageId; }
            set { imageId = value; }
        }

        public string ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value; }
        }

        public string ImagePath
        {
            get
            {
                if (imageUrl != null)
                    return imageUrl;

                else
                    return "/images/podcast.png";
            }
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public string Title
        {
            get { return title ?? "Untitled"; }
            set { title = value; }
        }

        public string Description
        {
            get { return desc; }
            set { desc = value; }
        }
    }
}
