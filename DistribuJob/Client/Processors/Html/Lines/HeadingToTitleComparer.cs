using System;
using System.Collections.Generic;
using System.Text;
using Exo.Misc;

namespace DistribuJob.Client.Processors.Html.Lines
{
    class HeadingToTitleComparer : IComparer<Line>
    {
        const double SufficientMatch = 0.6;

        private readonly string[] titleWords;
        private double maxMatch;

        public HeadingToTitleComparer(string[] titleWords)
        {
            this.titleWords = titleWords;
        }

        #region IComparer<Line> Members

        public int Compare(Line x, Line y)
        {
            return Score(y) - Score(x);
        }

        #endregion

        private int Score(Line headingLine)
        {
            int score = 0;

            double titleWordsMatchAmount = TextUtil.MatchAmount2(headingLine.Words, titleWords);

            if (titleWordsMatchAmount > maxMatch)
                    maxMatch = titleWordsMatchAmount;

            score += (int)Math.Round(titleWordsMatchAmount * 100d);

            // body too long, give penalty
            if (headingLine.Words.Length > titleWords.Length + Math.Round(titleWords.Length * 0.3d))
                score -= headingLine.Words.Length - (titleWords.Length + (int)Math.Round(titleWords.Length * 0.3d));

            // body too short, give penalty
            if (headingLine.Words.Length < titleWords.Length - Math.Round(titleWords.Length * 0.3d))
                score -= headingLine.Words.Length - titleWords.Length - (int)Math.Round(titleWords.Length * 0.3d);

            return score;
        }

        public bool HasSufficientMatch
        {
            get { return maxMatch >= SufficientMatch; }
        }
    }
}
