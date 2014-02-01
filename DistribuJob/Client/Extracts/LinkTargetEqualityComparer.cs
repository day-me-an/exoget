using System;
using System.Collections.Generic;
using System.Text;
using DistribuJob.Client.Extracts.Links;

namespace DistribuJob.Client.Extracts
{
    class LinkTargetEqualityComparer : IEqualityComparer<Link>
    {
        #region IEqualityComparer<Link> Members

        public bool Equals(Link x, Link y)
        {
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(Link obj)
        {
            return obj.linkLine.OriginalHrefHash;
        }

        #endregion
    }
}
