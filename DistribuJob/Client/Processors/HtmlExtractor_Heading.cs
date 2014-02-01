using System;
using DistribuJob.Client.Processors.Html.Lines;
using Exo.Extensions;

namespace DistribuJob.Client.Processors
{
    partial class HtmlExtractor
    {
        /// <summary>
        /// Filter out a title segment that matches 0.4 of the hostname with ambiguous body removed
        /// 
        /// Choose the header that is in one of the title segments
        /// 
        /// If no match of title, use top 0.3 heading that meets a specific proportion descriptiveness (needs a property adding to compare
        /// to indicate there hasn't been a good quality match)
        /// </summary>
        private string ExtractHeading()
        {
            if (currentJob.PageExtract.Title == null)
            {
                if (textWeightSortedLines.Length > 0)
                    return textWeightSortedLines[0].Text;

                else
                    return null;
            }

            string[]
                titleSegments = currentJob.PageExtract.Title.Split(new string[] { " - ", " | ", ">>", ":" }, StringSplitOptions.None),
                cleanTitleSegments = new string[titleSegments.Length];

            int cleanTitleSegmentsIndex = 0;

            foreach (string titleSegment in titleSegments)
            {
                string[] titleSegmentWords = titleSegment.RemoveAmbiguousWords(AmbiguousWordType.Language | AmbiguousWordType.Technical);

                int titleSegmentWordInHostCount = 0;

                foreach (string titleSegmentWord in titleSegmentWords)
                    if (currentJob.Server.Uri.Host.Contains(titleSegmentWord))
                        titleSegmentWordInHostCount++;

                if (titleSegmentWordInHostCount == 0 || 1d / (titleSegmentWords.Length / titleSegmentWordInHostCount) <= 0.3)
                    cleanTitleSegments[cleanTitleSegmentsIndex++] = titleSegment;
            }

            if (cleanTitleSegmentsIndex <= cleanTitleSegments.Length - 1)
                Array.Resize<string>(ref cleanTitleSegments, cleanTitleSegmentsIndex);

            string unambiguousTitle = String.Join(" ", cleanTitleSegments);
            string[] unambiguousTitleWords = unambiguousTitle.Tokenize();

            Line[] headings = (Line[])textWeightSortedLines.Clone();

            HeadingToTitleComparer headingToTitleComparer = new HeadingToTitleComparer(unambiguousTitleWords);

            Array.Sort(headings, headingToTitleComparer);

            if (headingToTitleComparer.HasSufficientMatch && headings[0].Text.Length <= 255)
                return headings[0].Text;

            else if (textWeightSortedLines.Length > 0 && textWeightSortedLines[0].Text.Length <= 255)
                return textWeightSortedLines[0].Text;

            else
                return null;
        }
    }
}
