using System;
using System.Collections.Generic;
using System.Text;

namespace DistribuJob.Indexer
{
    public class SentenceTypeComparer:IComparer<SentenceInfo>
    {
        #region IComparer<SentenceInfo> Members

        public int Compare(SentenceInfo x, SentenceInfo y)
        {
            return (int)y.Type - (int)x.Type;
        }

        #endregion
    }
}
