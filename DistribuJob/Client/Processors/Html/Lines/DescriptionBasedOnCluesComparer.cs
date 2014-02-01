using System;
using System.Collections.Generic;
using System.Text;
using Exo.Collections;
using Exo.Extensions;

namespace DistribuJob.Client.Processors.Html.Lines
{
    class DescriptionBasedOnCluesComparer : IComparer<Line>
    {
        private readonly HashSet<string> clues = new HashSet<string>();

        public DescriptionBasedOnCluesComparer(string[] clues)
        {
            this.clues.AddRange(clues);
        }

        public DescriptionBasedOnCluesComparer(Line[] clues)
        {
            foreach (Line clue in clues)
                this.clues.AddRange(clue.Words);
        }

        #region IComparer<Line> Members

        public int Compare(Line x, Line y)
        {
            int xClueMatches = 0, yClueMatches = 0;

            foreach (string xWord in x.Words)
                if (clues.Contains(xWord))
                    xClueMatches++;

            foreach (string yWord in y.Words)
                if (clues.Contains(yWord))
                    yClueMatches++;

            int matchDiff = yClueMatches - xClueMatches;

            if (x.Words != y.Words && Exo.Array.StartsWith(x.Words, y.Words))
                return matchDiff - 1;

            else if (x.Words != y.Words && Exo.Array.StartsWith(y.Words, x.Words))
                return matchDiff + 1;

            else
                return matchDiff;
        }

        #endregion
    }
}
