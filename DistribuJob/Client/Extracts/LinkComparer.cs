using System;
using System.Collections.Generic;
using System.Diagnostics;
using DistribuJob.Client.Extracts.Links;
using Exo.Extensions;

namespace DistribuJob.Client.Extracts
{
    // add support to compare partially uri based link text

    class LinkComparer : IComparer<Link>
    {
        #region IComparer<Link> Members

        public int Compare(Link x, Link y)
        {
            string[] xWords = x.TextWords, yWords = y.TextWords;

            bool
                xIsEmpty = xWords == null || (xWords.Length == 1 && xWords[0] == String.Empty),
                yIsEmpty = yWords == null || (yWords.Length == 1 && yWords[0] == String.Empty);

            if (xIsEmpty || yIsEmpty)
            {
                if (xIsEmpty && !yIsEmpty)
                    return 1;

                else if (yIsEmpty && !xIsEmpty)
                    return -1;

                else
                    return 0;
            }

            if (xWords.Length > yWords.Length && Exo.Array.Contains<string>(yWords, xWords))
                yWords = Exo.Array.RemoveArrayElements<string>(yWords, xWords);

            else if (yWords.Length > xWords.Length && Exo.Array.Contains<string>(xWords, yWords))
                xWords = Exo.Array.RemoveArrayElements<string>(xWords, yWords);

            return ScoreAmbiguous(yWords) - ScoreAmbiguous(xWords);
        }

        #endregion

        private int ScoreAmbiguous(string[] words)
        {
            try
            {
                if (words.Length == 0)
                    return -1;

                string[] wordsUnambiguous = words.RemovePartialAmbiguousWords();

                if (wordsUnambiguous.Length == 0)
                    return -1;

                double proportionAmbiguous = 1d - (1d / (words.Length / wordsUnambiguous.Length));

                return -1 * (int)(proportionAmbiguous * 100);
            }
            catch
            {
                Debug.Write("ScoreAmbiguous failed, words.Length, " + String.Join(",", words));

                return -1;
            }
        }
    }
}
