using System;
using System.Collections.Generic;
using System.Text;
using Exo.Web.Feed;
using Exo.Exoget.Model.Search;

namespace DistribuJob.Client.Extracts
{
    [Serializable]
    public class FeedExtract : Extract
    {
        private FeedType type;
        private FeedChannel channel;

        public FeedExtract()
            : base()
        {
        }

        public FeedType Type
        {
            get { return type; }
            set { type = value; }
        }

        public FeedChannel Channel
        {
            get { return channel; }
            set { channel = value; }
        }

        public override void AddIndexProperties()
        {
            TryAddIndexProperty(IndexPropertyType.Author, Channel.Author);
            TryAddIndexProperty(IndexPropertyType.Copyright, Channel.Copyright);
            TryAddIndexProperty(IndexPropertyType.Pubdate, Channel.Pubdate);
            TryAddIndexProperty(IndexPropertyType.Keyword, Channel.Keywords);
        }
    }
}