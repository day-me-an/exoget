using System;
using System.Collections.Generic;
using System.Text;
using Exo.Web;
using Exo.Misc;

namespace DistribuJob.Client.Extracts.Links
{
    [Serializable]
    public class AbsoluteSourceLink : Link
    {
        Uri sourceUri;
        private DocumentFormat sourceFormat;
        private DocumentType sourceType;

        public AbsoluteSourceLink(LinkType type, Uri sourceUri, Uri targetUri)
            : base(type, targetUri)
        {
            this.sourceUri = sourceUri;
        }

        public override void AddIndexProperties()
        {
            return;
        }

        public override string ToString()
        {
            return String.Format("({0}) \"{1}\" <{2}> [{3}] -> [{4}]",
                Type,
                Text,
                ImageUri,
                SourceUri,
                TargetUri
                );
        }

        public Uri SourceUri
        {
            get { return sourceUri; }
        }

        public DocumentFormat SourceFormat
        {
            get { return sourceFormat != DocumentFormat.None ? sourceFormat : (sourceFormat = UriUtil.GetFormatFromUri(sourceUri)); }
            set { sourceFormat = value; }
        }

        public DocumentType SourceType
        {
            get { return sourceType != DocumentType.None ? sourceType : (sourceType = UriUtil.GetDocumentTypeFromFormat(SourceFormat)); }
        }
    }
}