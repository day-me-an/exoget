using System;
using System.Collections.Generic;
using System.Text;
using DistribuJob.Client.Processors.Html.Lines;

namespace DistribuJob.Client.Processors.Html
{
    class ImageLineComparer : IComparer<ObjectLine>
    {
        #region IComparer<ImageLine> Members

        public int Compare(ObjectLine x, ObjectLine y)
        {
            return Score(y) - Score(x);
        }

        #endregion

        private int Score(ObjectLine imageLine)
        {
            return imageLine.Width + imageLine.Height + imageLine.TargetUri.ToString().Length;
        }
    }
}