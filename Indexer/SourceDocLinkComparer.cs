using System;
using System.Collections.Generic;
using System.Text;
using Exo.Web;
using DistribuJob.Client.Extracts.Links;

namespace DistribuJob.Indexer
{
    public class SourceDocLinkComparer:IComparer<LinkInfo>
    {
        #region IComparer<LinkInfo> Members

        public int Compare(LinkInfo x, LinkInfo y)
        {
            return Score(x) - Score(y);
        }

        #endregion

        private int Score(LinkInfo link)
        {
            switch (link.Type)
            {
                case LinkType.ArtificialMedia:
                    {
                        switch (link.SourceDocType)
                        {
                            case DocumentType.Page:
                                return 0;
                        }

                        break;
                    }

                case LinkType.FeedItemPageToMedia:
                    return 1;

                case LinkType.FeedChannelPageToFeed:
                    return 2;

                default:
                    {
                        switch (link.SourceDocType)
                        {
                            case DocumentType.Page:
                                return 3;

                            case DocumentType.Feed:
                                return 4;

                            case DocumentType.MediaPlaylist:
                                return 5;
                        }

                        break;
                    }
            }

            return Int32.MaxValue;
        }
    }
}
