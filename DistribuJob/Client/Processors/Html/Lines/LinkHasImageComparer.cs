using System;
using System.Collections.Generic;
using System.Text;

namespace DistribuJob.Client.Processors.Html.Lines
{
    class LinkHasImageComparer : IComparer<LinkLine>
    {
        #region IComparer<LinkLine> Members

        public int Compare(LinkLine x, LinkLine y)
        {
            return score(y) - score(x);
        }

        #endregion

        private int score(LinkLine obj)
        {
            if (obj.HasImageLineChildren)
                return 1;

            else
                return 0;
        }
    }
}
