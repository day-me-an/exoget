using System;
using System.IO;
using System.Text;
using DistribuJob.Client.Extracts;
using DistribuJob.Client.Net.Policies;
using Exo.Misc;
using Exo.Web;

namespace DistribuJob.Client
{
    public partial class Job
    {
        [NonSerialized]
        internal Encoding encoding;
        [NonSerialized]
        private Uri uri;
        [NonSerialized]
        internal int[] fetchRanges;
        [NonSerialized]
        internal int readLimit;
        [NonSerialized]
        internal byte[] pendingBytesAtBeginning;
        [NonSerialized]
        internal bool readToEnd;

        public DistribuJob.Client.Net.Server Server
        {
            get { return Dj.Servers[this]; }
        }

        public Uri Uri
        {
            get { return uri ?? (uri = new Uri(Server.Uri + path, UriKind.Absolute)); }

            set
            {
                uri = value;

                if (path != null)
                    changedPath = true;

                if (value.IsAbsoluteUri)
                    path = value.PathAndQuery.TrimStart('/');

                else
                    path = value.ToString().TrimStart('/');
            }
        }

        public DateTime LastModifiedDate
        {
            get { return ExoUtil.UnixTimestampToDateTime(lastModified); }
            set { lastModified = ExoUtil.DateTimeToUnixTimestamp(value); }
        }

        public Encoding Encoding
        {
            get { return encoding; }
        }

        public bool HasBeenRedirected
        {
            get { return changedPath || changedServerId; }
        }

        public bool HasLastModified
        {
            get { return lastModified != 0; }
        }

        public bool IsFetched
        {
            get { return format != DocumentFormat.None; }
        }

        public int Timeout
        {
            get { return UriPolicyTimeout != null ? UriPolicyTimeout.IntValue : Server.Timeout; }
        }

        public bool AcceptRequest
        {
            get
            {
                if (Server.protocol == Exo.Net.NetworkProtocol.Http && Server.IsRobotsTxtExpired)
                    Server.LoadRobotsTxt(this);

                return Server.IsDelayComplete && !Server.IsNextRequestLastChance && AcceptedByPolicies;
            }
        }

        public bool AcceptedByPolicies
        {
            get
            {
                return Server.AcceptUri(Uri);
            }
        }

        internal bool HasFetchRanges
        {
            get { return fetchRanges != null; }
        }

        internal bool HasReadLimit
        {
            get { return readLimit != 0; }
        }

        internal bool HasPendingBytesAtBeginning
        {
            get { return pendingBytesAtBeginning != null && pendingBytesAtBeginning.Length > 0; }
        }

        public string FilePath
        {
            get
            {
                DocumentFormatInfo formatInfo;

                if (UriUtil.TryGetFormatInfoFromFormat(Format, out formatInfo))
                    return @"B:\docs\" + id + "." + formatInfo.Extensions[0];

                else
                    throw new NullReferenceException();
            }
        }

        public Stream FileReadStream
        {
            get { return new FileStream(FilePath, FileMode.Open, FileAccess.Read); }
        }

        #region Extract Properties

        public bool HasExtract
        {
            get { return extract != null; }
        }

        public PageExtract PageExtract
        {
            get { return (extract ?? (extract = new PageExtract())) as PageExtract; }
        }

        public FeedExtract FeedExtract
        {
            get { return (extract ?? (extract = new FeedExtract())) as FeedExtract; }
        }

        public MediaExtract MediaExtract
        {
            get { return (extract ?? (extract = new MediaExtract())) as MediaExtract; }
        }

        public PlaylistExtract MediaPlaylistExtract
        {
            get { return (extract ?? (extract = new PlaylistExtract())) as PlaylistExtract; }
        }

        public ImageExtract ImageExtract
        {
            get { return (extract ?? (extract = new ImageExtract())) as ImageExtract; }
        }

        #endregion

        #region UriPolicy Properties

        public UriPolicy UriPolicyTimeout
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.TIMEOUT, Uri); }
        }

        public UriPolicy[] UriPoliciesAllow
        {
            get { return Server.FilterUriPolicies(UriPolicy.UriPolicyType.ALLOW, Uri); }
        }

        public UriPolicy[] UriPoliciesDisallow
        {
            get { return Server.FilterUriPolicies(UriPolicy.UriPolicyType.DISALLOW, Uri); }
        }

        public UriPolicy UriPolicyLineHash
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.LINE_HASH, Uri); }
        }

        public MetaInfoUriPolicy UriPolicyMetaInfo
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.LINE_META_INFO, Uri) as MetaInfoUriPolicy; }
        }

        public MetaInfoUriPolicy UriPolicyMetaTitleInfo
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.LINE_META_TITLE_INFO, Uri) as MetaInfoUriPolicy; }
        }

        public MetaInfoUriPolicy UriPolicyMetaDescriptionInfo
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.LINE_META_DESCRIPTION_INFO, Uri) as MetaInfoUriPolicy; }
        }

        public MetaInfoUriPolicy UriPolicyMetaDurationInfo
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.LINE_META_DURATION_INFO, Uri) as MetaInfoUriPolicy; }
        }

        public UriPolicy UriPolicyTitleSegmentHash
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.TITLE_SEGMENT_HASH, Uri); }
        }

        public UriPolicy UriPolicyTitleSegmentExcludeIndices
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.TITLE_SEGMENT_EXLCUDE_INDICIES, Uri); }
        }

        public UriPolicy UriPolicyLinkMediaOnly
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.LINKS_MEDIA_ONLY, Uri); }
        }

        public UriPolicy UriPolicyLinkPicturesOnly
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.LINKS_PICTURES_ONLY, Uri); }
        }

        public RegexUriPolicy UriPolicyLinkDisallowUriMatches
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.LINKS_DISALLOW_URI_MATCHES, Uri) as RegexUriPolicy; }
        }

        public XPathExpressionUriPolicy UriPolicyDomMetaTargetXpath
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.DOM_META_TARGET_XPATH, Uri) as XPathExpressionUriPolicy; }
        }

        public XPathExpressionUriPolicy UriPolicyDomMetaTitleXpath
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.DOM_META_TITLE_XPATH, Uri) as XPathExpressionUriPolicy; }
        }

        public XPathExpressionUriPolicy UriPolicyDomMetaDescriptionXpath
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.DOM_META_DESCRIPTION_XPATH, Uri) as XPathExpressionUriPolicy; }
        }

        public XPathExpressionUriPolicy UriPolicyDomMetaTagsXpath
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.DOM_META_TAGS_XPATH, Uri) as XPathExpressionUriPolicy; }
        }

        public XPathExpressionUriPolicy UriPolicyDomMetaAuthorXpath
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.DOM_META_AUTHOR_XPATH, Uri) as XPathExpressionUriPolicy; }
        }

        public XPathExpressionUriPolicy UriPolicyDomMetaDurationXpath
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.DOM_META_DURATION_XPATH, Uri) as XPathExpressionUriPolicy; }
        }

        public ValueFromUriRegexUriPolicy UriPolicyUriValueFormatTarget
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.URI_VALUE_FORMAT_TARGET, Uri) as ValueFromUriRegexUriPolicy; }
        }

        public UriPolicy UriPolicyUriDocumentFormat
        {
            get { return Server.FilterUriPolicy(UriPolicy.UriPolicyType.URI_CONTENT_FORMAT, Uri); }
        }

        #endregion

        public override string ToString()
        {
            return serverId + ":" + id + " - " + Uri;
        }
    }
}
