using System.Collections.Generic;
using System.Linq;
using DistribuJob.Client.Extracts.Links;
using Exo.Collections;
using Exo.Misc;
using Exo.Web;
using Exo.Web.Feed;

namespace DistribuJob.Client.Processors
{
    class FeedExtractor : Processor
    {
        public FeedExtractor(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            FeedParser parser = new FeedParser(job.FilePath);

            FeedDocument document;

            if (!parser.TryParse(out document))
            {
                job.Type = DocumentType.Unknown;
                return;
            }

            List<FeedChannel> channels = document.Channels;

            if (channels.Count == 0)
                return;

            else if (channels.Count > 1)
                channels.Sort(new ChannelComparer());

            FeedChannel channel = channels[0];

            if (channel.linkUri != null && UriUtil.TryValidateUri(ref channel.linkUri, job.Server.Uri, job.Uri))
            {
                DocumentType channelLinkDocumentType = UriUtil.GetDocumentTypeFromUri(channel.linkUri);

                if (channelLinkDocumentType != DocumentType.Media && channelLinkDocumentType != DocumentType.Feed)
                {
                    AbsoluteSourceLink channePageToFeed = new AbsoluteSourceLink(LinkType.FeedChannelPageToFeed, channel.linkUri, job.Uri);
                    job.FeedExtract.LinkList.Add(channePageToFeed);
                }
            }

            bool hasMediaEnclosures = false;

            foreach (FeedItem item in channel.Items)
            {
                bool addedMediaEnclosures = false;

                foreach (FeedEnclosure mediaEnclosure in item.MediaEnclosures)
                {
                    if (!UriUtil.TryValidateUri(ref mediaEnclosure.uri, currentJob.Server.Uri, currentJob.Uri))
                        continue;

                    FeedItemLink link = new FeedItemLink(mediaEnclosure.Uri);
                    AddFeedItemToLink(item, ref link);

                    job.FeedExtract.LinkList.Add(link);
                    addedMediaEnclosures = true;

                    if (link.PageUri != null
                        && UriUtil.TryValidateUri(ref link.pageUri, job.Server.Uri, job.Uri)
                        && UriUtil.GetDocumentTypeFromUri(link.PageUri) != Exo.Web.DocumentType.Media)
                    {
                        AbsoluteSourceLink pageToMediaLink = new AbsoluteSourceLink(LinkType.FeedItemPageToMedia, link.PageUri, mediaEnclosure.Uri);
                        job.FeedExtract.LinkList.Add(pageToMediaLink);
                    }
                }

                if (!addedMediaEnclosures)
                {
                    if (item.Link != null
                        && UriUtil.TryValidateUri(ref item.linkUri, currentJob.Server.Uri, job.Uri)
                        && UriUtil.GetDocumentTypeFromUri(item.linkUri) == DocumentType.Media)
                    {
                        FeedItemLink link = new FeedItemLink(item.Link);
                        AddFeedItemToLink(item, ref link);

                        job.FeedExtract.LinkList.Add(link);

                        addedMediaEnclosures = true;
                    }
                    else
                        addedMediaEnclosures = false;
                }

                if(!hasMediaEnclosures && addedMediaEnclosures)
                    hasMediaEnclosures = true;
            }

            if (hasMediaEnclosures)
            {
                job.FeedExtract.Type = document.Type;
                job.FeedExtract.Channel = channel;
            }
            else
                job.Type = DocumentType.Unknown;
        }

        private void AddFeedItemToLink(FeedItem item, ref FeedItemLink link)
        {
            link.PageUri = item.Link;
            link.Text = item.Title;
            link.Description = item.Description;
            link.Pubdate = item.Pubdate;

            if (item.Image != null && UriUtil.TryValidateUri(ref item.Image.uri, currentJob.Server.Uri, currentJob.Uri))
                link.ImageUri = item.Image.uri;

            if (item.Author != null)
                link.Author = item.Author;

            if (item.KeywordsSet != null)
                link.Keywords = item.KeywordsSet.ToArray();
        }

        public override void QueueControl(Job job)
        {
            Dj.Queues.exports.Enqueue(job);
        }

        class ChannelComparer : IComparer<FeedChannel>
        {
            #region IComparer<FeedChannel> Members

            public int Compare(FeedChannel x, FeedChannel y)
            {
                return Score(y) - Score(x);
            }

            #endregion

            private int Score(FeedChannel channel)
            {
                int score = 0;

                foreach (FeedItem item in channel.Items)
                    foreach (FeedEnclosure mediaEnclosure in item.MediaEnclosures)
                        score++;

                return score;
            }
        }
    }
}
