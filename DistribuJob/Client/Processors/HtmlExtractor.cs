using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DistribuJob.Client.Extracts.Links;
using DistribuJob.Client.Net.Policies;
using DistribuJob.Client.Processors.Html.Lines;
using Exo.Extensions;
using Exo.Misc;
using Exo.Web;
using HtmlAgilityPack;

namespace DistribuJob.Client.Processors
{
    partial class HtmlExtractor : Processor
    {
        private const int MaxFrames = 2;
        private const int DefaultScreenWidth = 1024;
        private const int DefaultScreenHeight = 768;

        private static readonly Regex mediaTargetWithinScriptRegex = new Regex("(?<=\")(.*)\\.swf(.*?)?(?=\")", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex durationRegex = new Regex(@"(?<Hours>([0-9]+)(:|\.| ?h(ours?)? ))(?<Minutes>([0-9]+)(:|\.| ?m(in(s|utes?)?)? ))?(?<Seconds>([0-9]+)( ?s(ecs?(onds?)?)?)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// The parsed HTML Document for this Job
        /// </summary>
        internal HtmlDocument document;

        /// <summary>
        /// Maps each html node to its corresponding Line object
        /// </summary>
        private Dictionary<HtmlNode, LinkLine> linkNodeToLinkline;

        public HtmlExtractor(Exo.Collections.IQueue<Job> queue, int startDelay)
            : base(queue, startDelay)
        {
        }

        public override void Process(Job job)
        {
            document = new HtmlDocument();

            try
            {
                if (job.Encoding == null)
                    document.DetectEncodingAndLoad(job.FilePath);

                else
                    document.Load(job.FilePath, job.Encoding);
            }
            catch (InvalidDataException e)
            {
                Trace.TraceWarning("HtmlExtractor invalid html elements ({0}, {1}, {2}): {3}", job.Id, job.Uri, job.Format, e);

                job.FetchStatus = FetchStatus.ErrorFormatWrong;
                job.Extract = null;

                return;
            }

            // parsing successful, init collections for analysis
            linkNodeToLinkline = new Dictionary<HtmlNode, LinkLine>();

            document.DocumentNode.Recurse(
                delegate(HtmlNode node, ref bool stop)
                {
                    if (node.Name == "title")
                    {
                        job.PageExtract.Title = node.InnerTextEscaped;
                        stop = true;
                    }
                }
                );

            document.DocumentNode.Recurse(
                delegate(HtmlNode node, ref bool stop)
                {
                    if (node.Name == "frameset")
                    {
                        job.PageExtract.HasFrameset = true;
                        stop = true;
                    }
                }
                );

            
            CreateLineRep();

            textWeightSortedLines = headingLines.ToArray();
            Array.Sort(textWeightSortedLines, new TextWeightComparer(this));

            descriptivenessSortedLines = textLines.ToArray();
            Array.Sort(descriptivenessSortedLines, new DescriptionComparer());

            if (job.Server.FilterUriPolicy(UriPolicy.UriPolicyType.LINKS_NONE, job.Uri) == null)
            {
                job.PageExtract.LinkList.AddRange(ExtractLinks(Exo.Array.Join<LinkLine>(linkLines.ToArray(), embedLines.ToArray())));
            }

            string heading = ExtractHeading();
            job.PageExtract.Heading = heading != null ? heading.Clean(StringCleanOptions.RemoveWhitespaceAndLines) : null;

#if DEBUG && !DEBUG_WITH_IMPORT_EXPORT
            foreach (Link link in job.Extract.LinkList)
                Console.WriteLine(link + "\n");
#endif

            if (job.ServerId == 653474 && job.Path.StartsWith("index.cfm?fuseaction=vids.individual"))
            {
                string videoId = UriUtil.ParseQueryString(job.Uri)["videoid"];
                Debug.Assert(videoId != null);
                Uri targetUri = new Uri("http://lads.myspace.com/videos/vplayer.swf?m=" + videoId + "&type=video", UriKind.Absolute);

                ArtificialMediaLink artificialMediaLink = new ArtificialMediaLink(targetUri);
                artificialMediaLink.MediaExtract.MediaType = MediaType.Video;

                document.DocumentNode.Recurse(new HtmlNode.RecurseCallback(
                    delegate(HtmlNode node, ref bool stop)
                    {
                        if (node.Name == "title")
                        {
                            int byPos = node.InnerTextEscaped.LastIndexOf("by ");

                            artificialMediaLink.MediaExtract.Author = node.InnerTextEscaped.Substring(byPos + 3, node.InnerTextEscaped.Length - (byPos + 3));
                        }
                        else if (node.Name == "h1")
                        {
                            artificialMediaLink.MediaExtract.Title = node.InnerTextEscaped;
                        }
                        else if (node.Name == "div"
                            && node.HasAttribute("class")
                            && node.Attributes["class"].Value == "description")
                        {
                            artificialMediaLink.MediaExtract.Description = node.InnerTextEscaped;
                        }
                        else if (node.Name == "div"
                            && node.HasAttribute("class")
                            && node.Attributes["class"].Value == "userinfo"
                            && node.InnerText.Contains("Runtime:"))
                        {
                            Match match = durationRegex.Match(node.InnerTextEscaped);

                            if (!match.Success)
                                return;

                            string[] segs = match.Value.Split(':');
                            int minutes, seconds;

                            minutes = Int32.Parse(segs[0]);
                            seconds = Int32.Parse(segs[1]);

                            artificialMediaLink.MediaExtract.Duration = (uint)new TimeSpan(0, minutes, seconds).TotalSeconds;

                            node.Recurse(new HtmlNode.RecurseCallback(
                                delegate(HtmlNode childNode, ref bool childStop)
                                {
                                    if (childNode.Name == "a" && childNode.HasAttribute("href"))
                                    {
                                        string value = childNode.InnerTextEscaped;

                                        if (!String.IsNullOrEmpty(value))
                                        {
                                            value = value.Trim(',');

                                            if (!String.IsNullOrEmpty(value))
                                                artificialMediaLink.MediaExtract.Keywords.Add(value);
                                        }
                                    }
                                }
                                ));
                        }
                    }
                    ));

                job.PageExtract.LinkList.Add(artificialMediaLink);
            }

            if (job.UriPolicyDomMetaTitleXpath != null)
            {
                Uri mediaTargetUri = null;

                if (job.UriPolicyDomMetaTargetXpath != null)
                    mediaTargetUri = ExtractTargetWithXpath();

                else if (job.UriPolicyUriValueFormatTarget != null)
                    mediaTargetUri = new Uri(job.UriPolicyUriValueFormatTarget.GetValueFromUri(job.Uri.ToString()), UriKind.RelativeOrAbsolute);

                if (mediaTargetUri != null)
                {
                    ArtificialMediaLink artificialMediaLink = new ArtificialMediaLink(mediaTargetUri);

                    UriPolicy uriContentPolicy;

                    if ((uriContentPolicy = job.Server.FilterUriPolicy(UriPolicy.UriPolicyType.URI_CONTENT_FORMAT, mediaTargetUri)) != null)
                        artificialMediaLink.TargetFormat = (DocumentFormat)uriContentPolicy.IntValue;

                    else
                        artificialMediaLink.TargetFormat = UriUtil.GetFormatFromUri(mediaTargetUri);

                    artificialMediaLink.Text = ExtractTitleWithXpath();
                    artificialMediaLink.Description = ExtractDescriptionWithXpath();

                    artificialMediaLink.MediaExtract.MediaType = MediaType.Video;
                    //artificialMediaLink.MediaExtract.Keywords = ExtractTagsWithXpath();
                    artificialMediaLink.MediaExtract.Author = ExtractAuthorWithXpath();

                    job.Extract.LinkList.Add(artificialMediaLink);
                }
            }
        }

        public static int ParseDimensionToPixels(string dimensionStr, DimensionType type)
        {
            dimensionStr = dimensionStr.Trim();

            if (dimensionStr == String.Empty)
                return 0;

            int dimensionPx;

            if (dimensionStr[dimensionStr.Length - 1] == '%')
            {
                if (Int32.TryParse(dimensionStr.Substring(0, dimensionStr.Length - 2), out dimensionPx))
                    dimensionPx = type == DimensionType.Width ? (DefaultScreenWidth / 100) * dimensionPx : (DefaultScreenHeight / 100) * dimensionPx;
            }
            else if (!Int32.TryParse(dimensionStr, out dimensionPx))
                return 0;

            return dimensionPx;
        }

        #region Xpath Meta Data Extraction

        public Uri ExtractTargetWithXpath()
        {
            HtmlNode targetNode = document.DocumentNode.SelectSingleNode(currentJob.UriPolicyDomMetaTargetXpath.XPathExpression);

            if (targetNode != null)
            {
                string target;

                if (targetNode.Name == "script")
                {
                    MatchCollection matches = mediaTargetWithinScriptRegex.Matches(targetNode.InnerText);
                    target = matches[0].Value;

                }
                else
                    target = targetNode.InnerText;

                Uri targetUri;

                try
                {
                    targetUri = new Uri(target, UriKind.RelativeOrAbsolute);
                }
                catch
                {
                    targetUri = null;
                }

                return targetUri;

            }
            else
                return null;
        }

        public string ExtractTitleWithXpath()
        {
            HtmlNode titleNode = document.DocumentNode.SelectSingleNode(currentJob.UriPolicyDomMetaTitleXpath.XPathExpression);

            if (titleNode != null)
                return TextUtil.DecodeHtmlEntities(titleNode.InnerText.Trim());

            else
                return null;
        }

        public string ExtractDescriptionWithXpath()
        {
            HtmlNodeCollection descriptionNodes = document.DocumentNode.SelectNodes(currentJob.UriPolicyDomMetaDescriptionXpath.XPathExpression);

            if (descriptionNodes != null && descriptionNodes.Count > 0)
            {
                HtmlNode descriptionNode;

                if (descriptionNodes.Count > 1)
                    descriptionNode = descriptionNodes[1];

                else
                    descriptionNode = descriptionNodes[0];

                return TextUtil.DecodeHtmlEntities(descriptionNode.InnerText.Trim());

            }
            else
                return null;
        }

        public string[] ExtractTagsWithXpath()
        {
            HashSet<string> tags = new HashSet<string>();
            HtmlNodeCollection tagNodes = document.DocumentNode.SelectNodes(currentJob.UriPolicyDomMetaTagsXpath.XPathExpression);

            if (tagNodes != null && tagNodes.Count > 0)
                foreach (HtmlNode tagNode in tagNodes)
                    if (tagNode.Attributes["href"] != null && tagNode.Attributes["href"].Value[0] != '#')
                        tags.Add(TextUtil.DecodeHtmlEntities(tagNode.InnerText.Trim()));

            return tags.ToArray();
        }

        public string ExtractAuthorWithXpath()
        {
            HtmlNode authorNode = document.DocumentNode.SelectSingleNode(currentJob.UriPolicyDomMetaAuthorXpath.XPathExpression);

            if (authorNode != null)
                return TextUtil.DecodeHtmlEntities(authorNode.InnerText.Trim());

            else
                return null;
        }

        public int? ExtractDurationWithXpath()
        {
            HtmlNode durationNode = document.DocumentNode.SelectSingleNode(currentJob.UriPolicyDomMetaDurationXpath.XPathExpression);

            if (durationNode != null)
            {
                MatchCollection durationMatches = durationRegex.Matches(durationNode.InnerText);

                if (durationMatches.Count > 0)
                    return -1;//parse the time to get seconds and return it
            }

            return null;
        }

        #endregion

        public override void QueueControl(Job job)
        {
            Console.WriteLine(job.Extract);

            Dj.Queues.exports.Enqueue(job);
        }
    }

    public enum DimensionType
    {
        Width,
        Height
    }
}