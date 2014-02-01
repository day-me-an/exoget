using System;
using System.Collections.Generic;
using System.Text;
using Exo.Web;

namespace DistribuJob.Client.Extracts.Links
{
    [Serializable]
    public class ArtificialMediaLink : EmbedLink
    {
        private MediaExtract mediaExtract;

        public ArtificialMediaLink(LinkType type, Uri targetUri)
            : base(type, targetUri)
        {
        }

        public ArtificialMediaLink(Uri targetUri)
            : base(LinkType.ArtificialMedia, targetUri)
        {
        }

        public override DocumentType TargetType
        {
            get { return Type == LinkType.ArtificialMedia ? DocumentType.ArtificalMedia : base.TargetType; }
        }

        public override string Text
        {
            get { return MediaExtract.Title; }
            set { MediaExtract.Title = value; }
        }

        public override string Description
        {
            get { return MediaExtract.Description; }
            set { MediaExtract.Description = value; }
        }

        public MediaExtract MediaExtract
        {
            get { return mediaExtract ?? (mediaExtract = new MediaExtract()); }
        }
    }
}