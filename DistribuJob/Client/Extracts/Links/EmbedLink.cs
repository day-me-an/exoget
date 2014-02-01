using System;
using System.Collections.Generic;
using System.Text;
using Exo.Web;
using Exo.Misc;

namespace DistribuJob.Client.Extracts.Links
{
    [Serializable]
    public class EmbedLink : Link
    {
        private DocumentFormat specifiedTargetFormat;

        public EmbedLink(LinkType type, Uri targetUri)
            : base(type, targetUri)
        {
        }

        public EmbedLink(Uri targetUri)
            : base(LinkType.Embed, targetUri)
        {
        }

        public override DocumentFormat TargetFormat
        {
            get
            {
                if (targetFormat == DocumentFormat.None &&
                    (targetFormat = UriUtil.GetFormatFromUri(TargetUri)) == DocumentFormat.Unknown &&
                    specifiedTargetFormat != DocumentFormat.None)
                {
                    targetFormat = specifiedTargetFormat;
                }

                return targetFormat;
            }

            set { base.TargetFormat = value; }
        }

        public DocumentFormat SpecifiedTargetFormat
        {
            get { return specifiedTargetFormat; }
            set { specifiedTargetFormat = value; }
        }

        public override void AddIndexProperties()
        {
            return;
        }
    }
}
