using System;
using System.Collections.Generic;
using System.Text;
using Exo.Web;
using Exo.Extensions;
using Exo.Misc;

namespace DistribuJob.Client.Extracts.Links
{
    [Serializable]
    public abstract class Link : IndexPropertiesBase
    {
        private readonly LinkType type;
        private readonly Uri targetUri;
        private Uri imageUri;
        protected DocumentFormat targetFormat, imageFormat;
        protected DocumentType targetType;
        private string text, description;
        private bool ambiguous;

        [NonSerialized]
        private string[] textWords, descriptionWords;

        public Link(LinkType type, Uri targetUri)
        {
            this.type = type;
            this.targetUri = targetUri;
        }

        public override int GetHashCode()
        {
            return targetUri.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("({0}) \"{1}\", \"{7}\" ({2}) -> {3} ambiguous: {4} [{5}, {6}]",
                Type,
                Text,
                ImageUri,
                TargetUri,
                IsAmbiguous,
                TargetFormat,
                TargetType,
                Description);
        }

        public LinkType Type
        {
            get { return type; }
        }

        public Uri TargetUri
        {
            get { return targetUri; }
        }

        public Uri ImageUri
        {
            get { return imageUri; }
            set { imageUri = value; }
        }

        public bool HasImage
        {
            get { return imageUri != null; }
        }

        public virtual DocumentFormat TargetFormat
        {
            get { return targetFormat != DocumentFormat.None ? targetFormat : (targetFormat = UriUtil.GetFormatFromUri(targetUri)); }
            set { targetFormat = value; }
        }

        public virtual DocumentType TargetType
        {
            get { return targetType != DocumentType.None ? targetType : (targetType = UriUtil.GetDocumentTypeFromFormat(TargetFormat)); }
        }

        public DocumentFormat ImageFormat
        {
            get { return imageFormat != DocumentFormat.None ? imageFormat : (imageFormat = UriUtil.GetFormatFromUri(imageUri)); }
        }

        public virtual string Text
        {
            get { return text; }
            set { text = value; }
        }

        public virtual string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string[] TextWords
        {
            get { return textWords ?? (textWords = Text.Tokenize(TokenizeOptions.All)); }
        }

        public string[] DescriptionWords
        {
            get { return descriptionWords ?? (descriptionWords = Description.Tokenize(TokenizeOptions.All)); }
        }

        public bool IsAmbiguous
        {
            get { return ambiguous; }
            set { ambiguous = value; }
        }
    }
}
