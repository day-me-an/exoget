using System;
using System.Collections.Generic;
using System.Text;
using Exo.Extensions;

namespace DistribuJob.Client.Processors.Html.Lines
{
    class DescriptionComparer : IComparer<Line>
    {
        private AmbiguousWordType ambiguousWordType;

        public DescriptionComparer(AmbiguousWordType ambiguousWordType)
        {
            this.ambiguousWordType = ambiguousWordType;
        }

        public DescriptionComparer()
            : this(AmbiguousWordType.All)
        {
        }

        #region IComparer<Line> Members

        public int Compare(Line x, Line y)
        {
            return y.Words.RemoveAmbiguousWords(ambiguousWordType).Length - x.Words.RemoveAmbiguousWords(ambiguousWordType).Length;
        }

        #endregion
    }
}
