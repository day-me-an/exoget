using System;
using System.Collections.Generic;
using System.Text;
using DistribuJob.Client.Processors.Html.Lines;
using HtmlAgilityPack;
using DistribuJob.Client.Net.Policies;
using C5;
using Exo.Misc;
using System.Text.RegularExpressions;

namespace DistribuJob.Client.Processors.Html
{
    class SingleEmbedExtractor
    {
        private static Regex titleSegmentRegex = new Regex("\\s[^a-z0-9\\s'!?,]+\\s", RegexOptions.IgnoreCase);
        private static Regex durationRegex = new Regex("([0-9]*)(:|\\.| ?h(ours?)? )?([0-9]*)(:|\\.| ?m(in(s|utes?)?)? )([0-9]*)( ?s(ecs?(onds?)?)?)?", RegexOptions.IgnoreCase);

        private readonly HtmlExtractor htmlExtractor;
        private readonly int titleStartPos,
            titleUpwards,
            titleDownwards,
            descriptionStartPos,
            descriptionUpwards,
            descriptionDownwards,
            durationStartPos,
            durationUpwards,
            durationDownwards;

        private string title = "";
        private string[] titleWords;

        public SingleEmbedExtractor(HtmlExtractor htmlExtractor,
            int titleStartPos,
            int titleUpwards,
            int titleDownwards,
            int descriptionStartPos,
            int descriptionUpwards,
            int descriptionDownwards,
            int durationStartPos,
            int durationUpwards,
            int durationDownwards)
        {
            this.htmlExtractor = htmlExtractor;
            this.titleStartPos = titleStartPos;
            this.titleUpwards = titleUpwards;
            this.titleDownwards = titleDownwards;
            this.descriptionStartPos = descriptionStartPos;
            this.descriptionUpwards = descriptionUpwards;
            this.descriptionDownwards = descriptionDownwards;
            this.durationStartPos = durationStartPos;
            this.durationUpwards = durationUpwards;
            this.durationDownwards = durationDownwards;
        }

        /*public void ExtractTitlePositionOnPage(List<Line> lines)
        {
            List<Line> titleClueSorted = lines;
            titleClueSorted.Sort(new DescriptionBasedOnCluesComparer(text));

            Console.WriteLine("top: " + titleClueSorted[titleClueSorted.Count - 1].ToString());
            Console.WriteLine("bottom: " + titleClueSorted[0].ToString());
        }*/

        public void extractTitle()
        {
            HtmlNodeCollection titleNodes = htmlExtractor.document.DocumentNode.SelectNodes("//title");
            string[] strTitleSegments = titleSegmentRegex.Split(titleNodes[titleNodes.Count - 1].InnerText);
            Sentence[] titleSegments = new Sentence[strTitleSegments.Length];

            for (int i = 0; i < strTitleSegments.Length; i++)
                titleSegments[i] = new Sentence(strTitleSegments[i]);

            UriPolicy titleSegmentUriPolicy = htmlExtractor.currentJob.UriPolicyTitleSegmentHash;

            if (titleSegmentUriPolicy != null)
            {
                List<Sentence> uniqueSegments = new List<Sentence>(8);

                foreach (Sentence titleSegment in titleSegments)
                    if (Array.IndexOf(titleSegmentUriPolicy.IntArrayValue, titleSegment.GetHashCode()) == -1)
                        uniqueSegments.Add(titleSegment);

                List<string> titleWords = new List<string>(uniqueSegments.Count);

                foreach (Sentence uniqueSegment in uniqueSegments)
                {
                    titleWords.AddRange(uniqueSegment.Words);
                    title += uniqueSegment + " ";
                }

                title.TrimEnd();

            }
            else if (titleSegments.Length == 1)
            {
                title = titleSegments[0].ToString();
                titleWords = titleSegments[0].Words;

            }
            else if (htmlExtractor.currentJob.UriPolicyMetaInfo != null)
            {
                Line[] possibleTitleContainingLines = htmlExtractor.GetDescriptiveTextSample(htmlExtractor.textLines,
                        titleStartPos,
                        titleUpwards,
                        titleDownwards);

                double[] lineMatches = new double[possibleTitleContainingLines.Length];
                double[] currentMatch = new double[titleSegments.Length];

                for (int i = 0; i < possibleTitleContainingLines.Length; i++)
                {
                    for (int j = 0; j < titleSegments.Length; j++)
                    {
                        currentMatch[j] = TextUtil.MatchCount(titleSegments[j].Words,
                                possibleTitleContainingLines[i].Words);
                    }

                    double total = 0;
                    for (int j = 0; j < currentMatch.Length; j++)
                    {
                        total += currentMatch[j];
                    }

                    lineMatches[i] = total / currentMatch.Length;
                }

                double[] sortedLineMatches = (double[])lineMatches.Clone();
                Array.Sort(sortedLineMatches);

                int topTitleContainingLine = Array.IndexOf(lineMatches, sortedLineMatches[sortedLineMatches.Length - 1]);

                title = possibleTitleContainingLines[topTitleContainingLine].Node.InnerText;
                titleWords = possibleTitleContainingLines[topTitleContainingLine].Words;

            }
            else
            {
                List<Sentence> titleSegmentsList = new List<Sentence>();
                titleSegmentsList.Sort(new TitleComparer());

                title = titleSegmentsList[0].ToString();
                titleWords = titleSegmentsList[0].Words;
            }

            Console.WriteLine("title is: " + title);
        }

        public void extractDescription()
        {
            Line[] titleDescriptionSortedLines = htmlExtractor.GetDescriptiveTextSample(htmlExtractor.textLines,
                descriptionStartPos,
                descriptionUpwards,
                descriptionDownwards,
                0.6);

            Array.Sort(titleDescriptionSortedLines, new DescriptionBasedOnCluesComparer(titleWords));

            Console.WriteLine("Description is: " + titleDescriptionSortedLines[0].Text);
        }

        public void extractDuration()
        {
            Line[] durationLines = htmlExtractor.GetDescriptiveTextSample(htmlExtractor.textLines,
                durationStartPos,
                durationUpwards,
                durationDownwards,
                0,
                0.3);

            foreach (Line textLine in htmlExtractor.textLines)
            {
                //string durationLine = null;
                MatchCollection matches = durationRegex.Matches(textLine.Text);

                foreach (Match match in matches)
                    Console.WriteLine("match at " + textLine.LineIndex + ": " + match);
            }
        }

        public void extractTags()
        {
            HtmlNode tagsContainingNode = htmlExtractor.document.DocumentNode.SelectSingleNode("//*[@id='vidTagsBegin']//a");

            foreach (HtmlNode tagNode in tagsContainingNode.ChildNodes)
            {
                Console.WriteLine("got tag: " + tagNode.InnerText);
            }
        }

        public void extractAuthor()
        {
            //: //*[@id='userInfoDiv']//a
        }
    }
}
