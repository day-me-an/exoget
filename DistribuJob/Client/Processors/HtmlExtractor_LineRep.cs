using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistribuJob.Client.Processors.Html.Lines;
using HtmlAgilityPack;
using Exo.Misc;
using Exo.Extensions;

namespace DistribuJob.Client.Processors
{
    partial class HtmlExtractor
    {
        /// <summary>
        /// A set of block level element names
        /// </summary>
        private static readonly HashSet<string> blockLevelElements = new HashSet<string>
        {
          #region Block Level Elements
          "blockquote",
          "body",
          "br",
          "center",
          "dd",
          "dir",
          "div",
          "dl",
          "dt",
          "form",
          "h1",
          "h2",
          "h3",
          "h4",
          "h5",
          "h6",
          "head",
          "hr",
          "html",
          "isindex",
          "li",
          "menu",
          "noframes",
          "ol",
          "p",
          "pre",
          "td",
          "th",
          "title",
          "ul"
          #endregion
        };

        /// <summary>
        /// Comtains the line hashes of a UriPolicy.LineHash
        /// </summary>
        private HashSet<int> lineHashes = new HashSet<int>();

        /// <summary>
        /// Maps all link href hash codes to there LinkLine
        /// </summary>
        private Dictionary<int, LinkLine> linkNodeHashes;

        #region Text Lines
        private List<Line> allLines;
        private C5.HashSet<Line> textLines;
        private C5.HashSet<Line> headingLines;
        #endregion

        #region Sorted Text Lines
        private Line[] descriptivenessSortedLines;
        private Line[] textWeightSortedLines;
        #endregion

        #region Extended Lines
        private HashSet<LinkLine> linkLines;
        private List<ImageLine> imageLines;
        private HashSet<EmbedLine> embedLines;
        #endregion

        #region Uniqueness Scores
        private C5.HashBag<ObjectLine> imageLinesUniqueness;
        private C5.HashBag<ObjectLine> embedLinesUniqueness;
        #endregion

        /// <summary>
        /// The current text line that is being appended to
        /// </summary>
        private Line currentTextLine;

        private void CreateLineRep()
        {
            lineHashes = new HashSet<int>();
            linkNodeHashes = new Dictionary<int, LinkLine>();
          
            allLines = new List<Line>();
            textLines = new C5.HashSet<Line>();
            headingLines = new C5.HashSet<Line>();

            linkLines = new HashSet<LinkLine>();
            imageLines = new List<ImageLine>();
            embedLines = new HashSet<EmbedLine>();

            imageLinesUniqueness = new C5.HashBag<ObjectLine>();
            embedLinesUniqueness = new C5.HashBag<ObjectLine>();
          
            currentTextLine = null;
            
            if (currentJob.UriPolicyLineHash != null && currentJob.UriPolicyLineHash.HasValue)
                lineHashes.AddRange(currentJob.UriPolicyLineHash.IntArrayValue);

            RecurseNode(document.DocumentNode, false);

            foreach (ImageLine imageLine in imageLines)
            {
                HtmlNodeParentEnumerator npe = new HtmlNodeParentEnumerator(imageLine.Node);

                foreach (HtmlNode node in npe)
                {
                    if (node.Name != "a" || node.Attributes["href"] == null)
                        continue;

                    LinkLine linkLine;

                    if (linkNodeHashes.TryGetValue(node.Attributes["href"].Value.GetHashCode(), out linkLine)
                        && (imageLine.Width == 0 || imageLine.Width >= ImageLine.MinLinkImageWidth)
                        && (imageLine.Height == 0 || imageLine.Height >= ImageLine.MinLinkImageHeight)
                        && imageLinesUniqueness.ContainsCount(imageLine) <= ImageLine.MaxLinkRepeatingImage)
                    {
                        linkLine.ImageLineChildren.Add(imageLine);
                    }
                }
            }

            if (currentJob.UriPolicyLineHash != null && (!currentJob.UriPolicyLineHash.HasValue || currentJob.UriPolicyLineHash.IsExpired))
            {
                int[] hashes = new int[allLines.Count];

                for (int i = 0; i < hashes.Length; i++)
                    hashes[i] = allLines[i].GetHashCode();

                currentJob.UriPolicyLineHash.IntArrayValue = hashes;
                currentJob.UriPolicyLineHash.UpdateValue();
            }
        }

        private int HeadingElementScore(HtmlNode node)
        {
            switch (node.Name)
            {
                case "h1":
                    return 8;

                case "h2":
                    return 7;

                case "h3":
                    return 6;

                case "h4":
                    return 5;

                case "h5":
                    return 4;

                case "h6":
                    return 3;

                case "font":
                    return 2;

                case "strong":
                case "b":
                    return 1;

                default:
                    return 0;
            }
        }

        private void RecurseNode(HtmlNode currentNode, bool recursingLink)
        {
            if (currentNode.NodeType == HtmlNodeType.Comment)
                return;

            bool canRecurse = false, isLink = false;

            if (recursingLink && currentNode.Name != "img")
            {
                canRecurse = true;
                goto recurse;
            }

            switch (currentNode.Name)
            {
                case "meta":
                case "link":
                case "script":
                case "style":
                case "form": // should this be blocked?
                case "select":
                case "input":
                case "textarea":
                    return;

                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                case "font":
                case "strong":
                case "b":
                    {
                        string text = currentNode.InnerTextEscaped;

                        if (!String.IsNullOrEmpty(text))
                        {
                            Line headingLine = new Line(LineType.Text, allLines.Count, currentNode, text);
                            Line oldHeadingLine;

                            if (headingLines.UpdateOrAdd(headingLine, out oldHeadingLine)
                                && HeadingElementScore(headingLine.Node) < HeadingElementScore(oldHeadingLine.Node))
                            {
                                headingLines.Update(oldHeadingLine);
                            }
                        }

                        goto default;
                    }

                case "a":
                    {
                        isLink = true;
                        canRecurse = true;
                        AddLink(currentNode);
                        break;
                    }

                case "img":
                    {
                        AddImage(currentNode);
                        break;
                    }

                case "object":
                case "embed":
                case "bgsound":
                    {
                        AddObject(currentNode);
                        break;
                    }

                default:
                    {
                        if (currentNode.NodeType == HtmlNodeType.Text)
                            AppendTextToCurrentLine(currentNode);

                        else if (blockLevelElements.Contains(currentNode.Name))
                            AddBlockLevelLine();

                        canRecurse = true;

                        break;
                    }
            }

        recurse:
            if (canRecurse)
            {
                foreach (HtmlNode childNode in currentNode.ChildNodes)
                {
                    if (currentNode.Name == "a" && childNode.Name != "img")
                        continue;

                    RecurseNode(childNode, isLink);
                }
            }
        }

        private void AddLink(HtmlNode node)
        {
            if (node.Attributes["href"] == null || (node.Attributes["href"].Value.Length > 0 && node.Attributes["href"].Value[0] == '#'))
                return;

            Uri linkUri;

            string hrefAttr = node.Attributes["href"].Value;

          /*
             * YouTube hack 
             */
            if (hrefAttr.Contains("&mode=related&search="))
            {
                hrefAttr = hrefAttr.Substring(0, hrefAttr.Length - "&mode=related&search=".Length);
                node.Attributes["href"].Value = hrefAttr;
            }
          /*
             * end hack 
             */

            if (!Uri.TryCreate(hrefAttr, UriKind.RelativeOrAbsolute, out linkUri))
                return;

            try
            {
                linkUri.GetHashCode();
            }
            catch
            {
                return;
            }

            node.Attributes["href"].Value = linkUri.ToString();

            LinkLine linkLine = new LinkLine(allLines.Count, node, node.InnerTextEscaped, linkUri); // original hash*

            if (!lineHashes.Contains(linkLine.GetHashCode()))
            {
                linkLines.Add(linkLine);

                if (!linkNodeHashes.ContainsKey(node.Attributes["href"].Value.GetHashCode()))
                    linkNodeHashes.Add(node.Attributes["href"].Value.GetHashCode(), linkLine);

                linkNodeToLinkline[node] = linkLine;

                AddBlockLevelLine(linkLine);
            }
        }

        private void AddImage(HtmlNode node)
        {
            if (node.Attributes["src"] == null)
                return;

            Uri srcUri;

            if (!Uri.TryCreate(node.Attributes["src"].Value, UriKind.RelativeOrAbsolute, out srcUri))
                return;

            ImageLine imageLine = new ImageLine(allLines.Count, node, srcUri);

            if (node.HasAttribute("width"))
                imageLine.Width = ParseDimensionToPixels(node.Attributes["width"].Value, DimensionType.Width);

            if (node.HasAttribute("height"))
                imageLine.Height = ParseDimensionToPixels(node.Attributes["height"].Value, DimensionType.Height);

            if (node.HasAttribute("alt"))
                imageLine.AltText = node.Attributes["alt"].Value;

            if (node.HasAttribute("title"))
                imageLine.TitleText = node.Attributes["title"].Value;

            if (!lineHashes.Contains(imageLine.GetHashCode()))
            {
                imageLines.Add(imageLine);
                imageLinesUniqueness.Add(imageLine);

                AddBlockLevelLine(imageLine);
            }
        }

        private void AddObject(HtmlNode node)
        {
            string srcUriAttr = null, widthAttr = null, heightAttr = null, srcTypeAttr = null, flashVars = null;

            switch (node.Name)
            {
                case "object":
                    {
                        if (node.HasAttribute("srctype"))
                            srcTypeAttr = node.Attributes["srctype"].Value;

                        else if (node.HasAttribute("type"))
                            srcTypeAttr = node.Attributes["type"].Value;

                        if (node.HasAttribute("width"))
                            widthAttr = node.Attributes["width"].Value;

                        if (node.HasAttribute("height"))
                            heightAttr = node.Attributes["height"].Value;

                        if (node.HasAttribute("src"))
                            srcUriAttr = node.Attributes["src"].Value;

                        else if (node.HasAttribute("data"))
                            srcUriAttr = node.Attributes["data"].Value;

                        foreach (HtmlNode childNode in node.ChildNodes)
                        {
                            if (childNode.Name == "param" && childNode.HasAttribute("name") && childNode.HasAttribute("value"))
                            {
                                switch (childNode.Attributes["name"].Value.ToLower())
                                {
                                    case "src":
                                    case "movie":
                                    case "url":
                                    case "filename":
                                        srcUriAttr = childNode.Attributes["value"].Value;
                                        break;

                                    case "type":
                                    case "srctype":
                                        srcTypeAttr = childNode.Attributes["value"].Value;
                                        break;

                                    case "width":
                                        widthAttr = childNode.Attributes["value"].Value;
                                        break;

                                    case "height":
                                        heightAttr = childNode.Attributes["value"].Value;
                                        break;

                                    case "flashvars":
                                        flashVars = childNode.Attributes["value"].Value.TrimStart('?');
                                        break;
                                }
                            }
                            else if (childNode.Name == "embed" && childNode.HasAttribute("src") && childNode.Attributes["src"].Value == srcUriAttr)
                            {
                                if (childNode.HasAttribute("type"))
                                    srcTypeAttr = childNode.Attributes["type"].Value;

                                else if (childNode.HasAttribute("srctype"))
                                    srcTypeAttr = childNode.Attributes["srctype"].Value;

                                if (childNode.HasAttribute("width"))
                                    widthAttr = childNode.Attributes["width"].Value;

                                if (childNode.HasAttribute("height"))
                                    heightAttr = childNode.Attributes["height"].Value;

                                if (childNode.HasAttribute("flashvars"))
                                    flashVars = childNode.Attributes["flashvars"].Value.TrimStart('?');
                            }
                        }

                        break;
                    }

                case "embed":
                case "bgsound":
                    {
                        if (node.HasAttribute("type"))
                            srcTypeAttr = node.Attributes["type"].Value;

                        else if (node.HasAttribute("srctype"))
                            srcTypeAttr = node.Attributes["srctype"].Value;

                        if (node.HasAttribute("width"))
                            widthAttr = node.Attributes["width"].Value;

                        if (node.HasAttribute("height"))
                            heightAttr = node.Attributes["height"].Value;

                        if (node.HasAttribute("flashvars"))
                            flashVars = node.Attributes["flashvars"].Value.TrimStart('?');

                        break;
                    }
            }

            if (srcUriAttr == null)
                return;

            Uri srcUri;

            if (!Uri.TryCreate(srcUriAttr, UriKind.RelativeOrAbsolute, out srcUri))
                return;

            if (flashVars != null)
                srcUri = new Uri(srcUri.ToString() + (srcUri.ToString().IndexOf('?') == -1 ? "?" : "&") + flashVars, UriKind.RelativeOrAbsolute);

            EmbedLine embedLine = new EmbedLine(allLines.Count, node, srcUri);

            if (srcTypeAttr != null)
                embedLine.SpecifiedFormat = UriUtil.GetFormatFromMimetype(srcTypeAttr);

            if (widthAttr != null)
                embedLine.Width = ParseDimensionToPixels(widthAttr, DimensionType.Width);

            if (heightAttr != null)
                embedLine.Height = ParseDimensionToPixels(heightAttr, DimensionType.Height);

            if (!lineHashes.Contains(embedLine.GetHashCode()))
            {
                embedLines.Add(embedLine);
                embedLinesUniqueness.Add(embedLine);

                AddBlockLevelLine(embedLine);
            }
        }

        private void AppendTextToCurrentLine(HtmlNode node)
        {
            if (node.NodeType != HtmlNodeType.Text)
                throw new ArgumentException("node type must be HtmlNodeType.Text");

            if (node.InnerTextEscaped.Length == 0)
                return;

            if (currentTextLine == null)
                currentTextLine = new Line(LineType.Text, allLines.Count);

            currentTextLine.Text = node.InnerTextEscaped + " ";
        }

        private void AddBlockLevelLine(Line blockLevelLine)
        {
            if (currentTextLine != null)
            {
                currentTextLine.CommitText();
                allLines.Add(currentTextLine);
                textLines.Add(currentTextLine);
                currentTextLine = null;
            }

            if (blockLevelLine != null)
                allLines.Add(blockLevelLine);
        }

        private void AddBlockLevelLine()
        {
            AddBlockLevelLine(null);
        }
    }
}
