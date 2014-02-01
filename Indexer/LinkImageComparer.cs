using System;
using System.Collections.Generic;
using System.Text;
using DistribuJob.Client.Extracts.Links;

namespace DistribuJob.Indexer
{
    public class LinkImageComparer : IComparer<LinkInfo>
    {
        private bool hasImage = false;

        #region IComparer<LinkInfo> Members

        public int Compare(LinkInfo x, LinkInfo y)
        {
            return Score(x) - Score(y);
        }

        #endregion

        private int Score(LinkInfo link)
        {
            if (link.ImageDocId != 0)
            {
                hasImage = true;

                switch (link.Type)
                {
                    case LinkType.ArtificialMedia:
                        return 0;

                    case LinkType.Feed:
                        return 1;

                    case LinkType.Page:
                        return 2;
                }
            }

            return Int32.MaxValue;
        }

        public bool HasImage
        {
            get { return hasImage; }
        }
    }
}
