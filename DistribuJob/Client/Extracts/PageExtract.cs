using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using DistribuJob.Client.Extracts;
using DistribuJob.Client.Extracts.Links;

namespace DistribuJob.Client.Extracts
{
    [Serializable]
    public class PageExtract : Extract
    {
        private string title, heading, description;
        private bool hasFrameset;

        public PageExtract()
            : base()
        {
        }

        public override void AddIndexProperties()
        {
            return;
        }

        public override string ToString()
        {
            return String.Format("Title: {0} Heading: {1} Description: {2} HasFrameset: {3}",
                Title,
                Heading,
                Description,
                HasFrameset);
        }

        public bool HasFrameset
        {
            get { return hasFrameset; }
            set { hasFrameset = value; }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Heading
        {
            get { return heading; }
            set { heading = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public PageLink[] PageLinks
        {
            get { return FilterLinksByType<PageLink>(LinkType.Page); }
        }

        public EmbedLink[] EmbedLinks
        {
            get { return FilterLinksByType<EmbedLink>(LinkType.Embed); }
        }
    }
}
