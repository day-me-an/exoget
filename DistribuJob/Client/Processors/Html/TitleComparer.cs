using System;
using System.Collections.Generic;
using System.Text;
using Exo.Misc;

namespace DistribuJob.Client.Processors.Html
{
    class TitleComparer:IComparer<Sentence>
    {
        #region IComparer<string> Members

        public int Compare(Sentence x, Sentence y)
        {
            return y.Words.Length - x.Words.Length;
        }

        #endregion
    }
}
