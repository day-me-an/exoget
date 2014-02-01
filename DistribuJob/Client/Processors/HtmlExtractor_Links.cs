using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DistribuJob.Client.Extracts;
using DistribuJob.Client.Extracts.Links;
using DistribuJob.Client.Net.Policies;
using DistribuJob.Client.Processors.Html.Lines;
using Exo.Extensions;
using Exo.Misc;
using Exo.Web;
using HtmlAgilityPack;

namespace DistribuJob.Client.Processors
{
    partial class HtmlExtractor
    {
        private const int MaxDescriptionTableDensity = 6;
        private const int MaxMediaTableRowUnambiguousMediaLinks = 2;
        private const int MaxMediaTableRowAmbiguousLinks = 5;
        private const int MaxMediaTableRowWords = 180;
        private const double MinMediaLinkToOtherLinkTableRowProportion = 0.6;

        private static readonly Regex MediaRegex = new Regex("audio|video|play|watch|stream|media|movie|film|divx|trailer|footage|rss|feed|podcast", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private Link[] ExtractLinks(LinkLine[] linkLines)
        {
            Array.Sort(linkLines, new LinkHasImageComparer());
            C5.HashBag<string> linkTextHashBag = new C5.HashBag<string>();
            C5.HashSet<Link> links = new C5.HashSet<Link>();
            Dictionary<int, List<Link>> duplicateLinks = new Dictionary<int, List<Link>>();
            Dictionary<Link, LinkLine> linkToLinkline = new Dictionary<Link, LinkLine>();

            var validLinkLines =
                from linkLine in linkLines
                where
                linkLine.TargetUri != currentJob.Uri
                && linkLine.TargetUri.ToString().Length <= 255
                && UriUtil.TryValidateUri(ref linkLine.targetUri, currentJob.Server.Uri, currentJob.Uri)
                select linkLine;

            foreach (LinkLine linkLine in validLinkLines)
                linkTextHashBag.Add(linkLine.Text);

            foreach (LinkLine linkLine in validLinkLines)
            {
                Uri linkUri = linkLine.TargetUri;

                if (UriUtil.IsHttpDirectoryIndexSortQuery(linkUri))
                {
                    if (linkUri.IsAbsoluteUri)
                        linkUri = UriUtil.RemoveQuery(linkUri);

                    else
                        continue;

                    linkLine.TargetUri = linkUri;
                }

                Link link;

                if (linkLine.Type == LineType.Embed)
                {
                    link = new EmbedLink(linkUri);
                    ((EmbedLink)link).SpecifiedTargetFormat = ((EmbedLine)linkLine).SpecifiedFormat;
                }
                else
                    link = new PageLink(linkUri);

                Link oldLink;
                if (links.Remove(link, out oldLink))
                {
                    links.Add(oldLink);

                    if (!duplicateLinks.ContainsKey(link.GetHashCode()))
                    {
                        duplicateLinks.Add(link.GetHashCode(), new List<Link>(3));
                        duplicateLinks[link.GetHashCode()].Add(oldLink);
                    }

                    duplicateLinks[link.GetHashCode()].Add(link);
                }

                link.TargetFormat = UriUtil.GetFormatFromUri(linkUri);

                if (link.TargetType == DocumentType.Image)
                    continue;

                bool isMedia = link.TargetType == DocumentType.Media
                    || link.TargetType == DocumentType.MediaPlaylist
                    || link.TargetType == DocumentType.Feed
                    || (link.TargetType != DocumentType.Image && MediaRegex.IsMatch(linkUri.IsAbsoluteUri ? linkUri.GetLeftPart(UriPartial.Path | UriPartial.Query) : linkUri.ToString()));

                if ((currentJob.UriPolicyLinkMediaOnly != null && !isMedia) || !currentJob.Server.AcceptUri(linkUri))
                    continue;

                link.Text = linkLine.Text != null ? linkLine.Text.Clean(StringCleanOptions.RemoveWhitespaceAndLines) : null;

                if (link.Text == String.Empty)
                    link.Text = null;

                if (linkLine.Text != null && linkLine.Text.StartsWith("http://"))
                    link.Text = null;

                link.IsAmbiguous = link.Text != null && linkTextHashBag.ContainsCount(link.Text) > PageLink.MaxRepeatingLinkText;

                if (link.TargetType == DocumentType.Media || link.TargetType == DocumentType.MediaPlaylist)
                {
                    string description = null;
                    bool hasTable = false;

                    HtmlNodeParentEnumerator tdPne = new HtmlNodeParentEnumerator(linkLine.Node);

                    // find a parent table element to indicate whether the link is within a table
                    for (int i = 0; !hasTable && tdPne.MoveNext() && i < MaxDescriptionTableDensity; i++)
                        if (tdPne.Current.Name == "td")
                            hasTable = true;

                    #region Table Row Text Finding Algorithm
                    if (hasTable)
                    {
                        HtmlNode
                            td = tdPne.Current,
                            tableNode = null;

                        HtmlNodeParentEnumerator tablePne = new HtmlNodeParentEnumerator(td);

                        while (tableNode == null && tablePne.MoveNext())
                            if (tablePne.Current.Name == "table")
                                tableNode = tdPne.Current;

                        if (tableNode != null)
                        {
                            /*var tableRows =
                                from node in tablePne.Current
                                where
                                (node.ParentNode == tablePne.Current && node.Name == "tr")
                                || (node.ParentNode.ParentNode == tablePne.Current && node.ParentNode.Name == "tbody")
                                select node;*/

                            List<HtmlNode> tableRows = new List<HtmlNode>();

                            tablePne.Current.Recurse(
                                delegate(HtmlNode node, ref bool stop)
                                {
                                    if ((node.ParentNode == tablePne.Current && node.Name == "tr")
                                        || (node.ParentNode.ParentNode == tablePne.Current && node.ParentNode.Name == "tbody"))
                                    {
                                        tableRows.Add(node);
                                    }
                                }
                                );

                            int mediaTableRows = 0;

                            foreach (HtmlNode tableRow in tableRows)
                            {
                                /*HtmlNode[] tableRowTds =
                                    (from node in tableRow
                                     where node.ParentNode == tableRow && node.Name == "td"
                                     select node).ToArray();*/


                                List<HtmlNode> tableRowTds = new List<HtmlNode>();

                                tableRow.Recurse(
                                    delegate(HtmlNode node, ref bool stop)
                                    {
                                        if (node.ParentNode == tableRow && node.Name == "td")
                                            tableRowTds.Add(node);
                                    }
                                    );


                                if (tableRowTds.Count == 1)
                                {
                                    if (tableRowTds[0].InnerTextEscaped == null)
                                        continue;

                                    string[] singleTdWords = tableRowTds[0].InnerTextEscaped.Tokenize();
                                    int descriptiveSingleTdWordsCount = singleTdWords.Count(word => { return word.Length >= 3; });

                                    if (descriptiveSingleTdWordsCount < 3)
                                        continue;
                                }

                                List<HtmlNode> tableRowLinks = new List<HtmlNode>();

                                tableRow.Recurse(
                                    delegate(HtmlNode node, ref bool stop)
                                    {
                                        if (node.Name == "a")
                                            tableRowLinks.Add(node);
                                    }
                                    );

                                if (tableRowLinks.Count < 10)
                                {
                                    int
                                        mediaLinks = 0,
                                        ambiguousMediaLinks = 0;

                                    foreach (HtmlNode tableRowLink in tableRowLinks)
                                    {
                                        LinkLine tableRowLinkLine;

                                        if (linkNodeToLinkline.TryGetValue(tableRowLink, out tableRowLinkLine)
                                            && UriUtil.GetDocumentTypeFromUri(tableRowLinkLine.TargetUri) == DocumentType.Media)
                                        {
                                            mediaLinks++;

                                            string[] tableRowLinkLineUnambiguous;

                                            if ((tableRowLinkLine.Words.Length == 0 && tableRowLinkLine.HasImageLineChildren)
                                                || (tableRowLinkLineUnambiguous = tableRowLinkLine.Words.RemoveAmbiguousWords()).Length > 0 && 1d / (tableRowLinkLine.Words.Length / tableRowLinkLineUnambiguous.Length) >= 0.6)
                                            {
                                                ambiguousMediaLinks++;
                                            }
                                        }
                                    }

                                    /* 
                                     * Done: allow max 2 unambiguous media links
                                     * Done: allow max 5 ambiguous "format choice" media links
                                     * Done: limit the amount of text a row can contain, if theres too much its likely to be a mistake
                                     */
                                    if (mediaLinks > 0
                                        && ambiguousMediaLinks <= MaxMediaTableRowAmbiguousLinks
                                        && mediaLinks - ambiguousMediaLinks <= MaxMediaTableRowUnambiguousMediaLinks
                                        && 1d / (tableRowLinks.Count / mediaLinks) >= MinMediaLinkToOtherLinkTableRowProportion
                                        && tableRow.InnerTextEscaped.Tokenize().Length <= MaxMediaTableRowWords)
                                    {
                                        mediaTableRows++;
                                    }
                                }
                            }

                            if (mediaTableRows > 0 && tableRows.Count >= 2 && 1d / (tableRows.Count / mediaTableRows) >= 0.6)
                            {
                                /*var td2s =
                                    from node in tdPne.Current.ParentNode
                                    where
                                    node.ParentNode == tdPne.Current.ParentNode
                                    && node.Name == "td"
                                    && node.NodeType != HtmlNodeType.Comment
                                    select node;*/

                                List<HtmlNode> td2s = new List<HtmlNode>();

                                tdPne.Current.ParentNode.Recurse(
                                    delegate(HtmlNode node, ref bool stop)
                                    {
                                        if (node.ParentNode == tdPne.Current.ParentNode && node.Name == "td")
                                            td2s.Add(node);
                                    }
                                    );

                                StringBuilder tableDesc = new StringBuilder();

                                foreach (HtmlNode td2 in td2s)
                                    tableDesc.AppendSpace(td2.GetInnerText(linkLine.Node));

                                description = tableDesc.ToString().Clean(StringCleanOptions.RemoveWhitespaceAndLines | StringCleanOptions.DecodeHtmlEntities);
                            }
                        }
                    }
#endregion

                    if (String.IsNullOrEmpty(description))
                    {
                        List<string> descriptionWords = new List<string>(30);

                        if (linkLine.LineIndex - 1 >= 0)
                        {
                            for (int i = linkLine.LineIndex; i > 0 && descriptionWords.Count <= 15; i--)
                            {
                                if (!((allLines[i].Type == LineType.Link || allLines[i].Type == LineType.Text)
                                    && !String.IsNullOrEmpty(allLines[i].Text)))
                                {
                                    continue;
                                }

                                string[] lineWords = allLines[i].Words;

                                for (int j = lineWords.Length - 1; j >= 0 && descriptionWords.Count <= 14; j--)
                                    descriptionWords.Add(lineWords[j]);
                            }

                            descriptionWords.Reverse();
                        }

                        if (linkLine.LineIndex + 1 <= allLines.Count)
                        {
                            for (int i = linkLine.LineIndex + 1; i < allLines.Count && descriptionWords.Count <= 30; i++)
                            {
                                if (!((allLines[i].Type == LineType.Link || allLines[i].Type == LineType.Text)
                                    && !String.IsNullOrEmpty(allLines[i].Text)))
                                {
                                    continue;
                                }

                                string[] lineWords = allLines[i].Words;

                                foreach (string lineWord in lineWords)
                                {
                                    if (descriptionWords.Count <= 29)
                                        descriptionWords.Add(lineWord);

                                    else
                                        break;
                                }
                            }
                        }

                        description = String.Join(" ", descriptionWords.ToArray());
                    }

                    link.Description = description.Clean(StringCleanOptions.RemoveWhitespaceAndLines);

                    if (link.Description == String.Empty)
                        link.Description = null;
                }

                Link existingLink;
                LinkLine existingLinkLine;

                if (links.Remove(link, out existingLink) && (existingLinkLine = linkToLinkline[existingLink]).HasImageLineChildren)
                {
                    int imageCount = imageLinesUniqueness.ContainsCount(existingLinkLine.ImageLineChildren[0]);

                    if (imageCount > 0 && imageCount <= 50)
                        link.ImageUri = existingLink.ImageUri;
                }

                if (linkLine.HasImageLineChildren)
                {
                    Uri imageTargetUri = linkLine.ImageLineChildren[0].TargetUri;

                    if (UriUtil.TryValidateUri(ref imageTargetUri, currentJob.Server.Uri, currentJob.Uri))
                        link.ImageUri = imageTargetUri;
                }

                links.Add(link);
                linkToLinkline[link] = linkLine;
            }

            foreach (System.Collections.Generic.KeyValuePair<int, List<Link>> pair in duplicateLinks)
            {
                pair.Value.Sort(new LinkComparer());
                links.Update(pair.Value[0]);
            }

            if (currentJob.UriPolicyLinkPicturesOnly != null)
            {
                return
                    (from link in links
                     where
                     link.HasImage || (currentJob.Server.FilterUriPolicy(UriPolicy.UriPolicyType.ALLOW, link.TargetUri) != null && (currentJob.UriPolicyLinkDisallowUriMatches != null && !currentJob.UriPolicyLinkDisallowUriMatches.Matches(link.TargetUri)))
                     select link).ToArray();
            }
            else
                return links.ToArray();
        }
    }
}
