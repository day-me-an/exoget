using System;
using System.Collections.Generic;
using System.Text;
using Exo.Web.Feed;

namespace DistribuJob.Client.Extracts.Links
{
    [Serializable]
    public class FeedItemLink : Link
    {
        internal Uri pageUri;
        private DateTime pubdate;
        private string author;
        private string[] keywords;

        public FeedItemLink(Uri mediaTargetUri)
            : base(LinkType.Feed, mediaTargetUri)
        {
        }

        public Uri PageUri
        {
            get { return pageUri; }
            set { pageUri = value; }
        }

        public DateTime Pubdate
        {
            get { return pubdate; }
            set { pubdate = value; }
        }

        public string Author
        {
            get { return author; }
            set { author = value; }
        }

        public string[] Keywords
        {
            get { return keywords; }
            set { keywords = value; }
        }

        public override void AddIndexProperties()
        {
            TryAddIndexProperty(Exo.Exoget.Model.Search.IndexPropertyType.Author, author);
            TryAddIndexProperty(Exo.Exoget.Model.Search.IndexPropertyType.Pubdate, pubdate);
            TryAddIndexProperty(Exo.Exoget.Model.Search.IndexPropertyType.Keyword, keywords);
        }
    }
}