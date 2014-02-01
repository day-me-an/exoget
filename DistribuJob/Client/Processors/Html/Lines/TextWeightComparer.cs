using System;
using System.Collections.Generic;
using System.Text;
using Exo.Misc;
using System.Xml;
using HtmlAgilityPack;
using System.Xml.XPath;
using System.Linq;

namespace DistribuJob.Client.Processors.Html.Lines
{
    class TextWeightComparer : IComparer<Line>
    {
        private const int ParagraphWeight = 1;
        private const int BoldWeight = 2;
        private const int FontWeightStart = 3;
        private const int FontWeightEnd = 74;
        private const int HeaderWeightStart = 75;
        private const int HeaderWeightEnd = 83;
        private const int TitleWeight = 84;

        private static readonly XPathExpression allFontElements = XPathExpression.Compile("//font");

        private readonly HtmlExtractor htmlExtractor;
        private int averageFontSize = -1;

        public TextWeightComparer(HtmlExtractor htmlExtractor)
        {
            this.htmlExtractor = htmlExtractor;
        }

        #region IComparer<Line> Members

        public int Compare(Line x, Line y)
        {
            return Score(y) - Score(x);
        }

        #endregion

        private int Score(Line line)
        {
            int score = 0;

            if (line.Node.InnerText == null)
                return score;

            else if (line.Node.Name == "title")
                score += TitleWeight;

            else if (line.Node.Name.Length == 2 && line.Node.Name[0] == 'h' && Char.IsNumber(line.Node.Name[1]) && line.Node.Name[1] > 0)
                score += (TitleWeight - (int)Char.GetNumericValue(line.Node.Name[1]));

            else if (line.Node.Name == "font")
            {
                int fontScore = ScoreFontElement(line.Node);

                if(fontScore > 0)
                    score = fontScore; ;
            }
            else
            {
                HtmlNodeParentEnumerator pne = new HtmlNodeParentEnumerator(line.Node);

                foreach (HtmlNode node in pne)
                {
                    int fontScore = ScoreFontElement(node);

                    if (fontScore > 0)
                        return fontScore;
                }

                if (score == 0 && (line.Node.Name == "strong" || line.Node.Name == "b"))
                    score += BoldWeight;
            }

            return score;
        }

        private int ScoreFontElement(HtmlNode node)
        {
            int fontSize = ParseFontElementSize(node);

            if (fontSize > 0
                && fontSize <= 72
                && fontSize != AverageFontSize)
            {
                return FontWeightStart + fontSize;
            }
            else
                return 0;
        }

        private int ParseFontElementSize(HtmlNode node)
        {
            if (node.Name == "font" && node.HasAttribute("size"))
            {
                string sizeAttributeValue = node.Attributes["size"].Value;

                if (sizeAttributeValue[0] == '+')
                    sizeAttributeValue = sizeAttributeValue.Substring(1);

                else if (sizeAttributeValue[0] == '-')
                    return 0;

                int fontSize;

                if (Int32.TryParse(sizeAttributeValue, out fontSize) && fontSize <= 72)
                    return fontSize;
            }

            return 0;
        }

        private int AverageFontSize
        {
            get
            {
                if (averageFontSize == -1)
                {
                    List<HtmlNode> fontElems = new List<HtmlNode>();

                    htmlExtractor.document.DocumentNode.Recurse(
                        delegate(HtmlNode node, ref bool stop)
                        {
                            if (node.Name == "font" && node.HasAttribute("size"))
                                fontElems.Add(node);
                        });

                    List<int> fontSizes = new List<int>();

                    foreach (HtmlNode fontElem in fontElems)
                    {
                        int fontSize = ParseFontElementSize(fontElem);

                        if (fontSize > 0)
                            fontSizes.Add(fontSize);
                    }

                    averageFontSize = Exo.Collections.Util.Mode<int>(fontSizes);
                }

                return averageFontSize;
            }
        }
    }
}