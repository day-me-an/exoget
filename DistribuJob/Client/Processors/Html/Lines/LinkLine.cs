using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace DistribuJob.Client.Processors.Html.Lines
{
    public class LinkLine : Line
    {
        internal Uri targetUri;
        private List<ImageLine> imageLineChildren;
        private string titleText;

        public LinkLine(LineType type, int lineIndex, HtmlNode node, string text, Uri targetUri)
            : base(type, lineIndex, node, text)
        {
            this.targetUri = targetUri;
        }

        public LinkLine(int lineIndex, HtmlNode node, string text, Uri targetUri)
            : this(LineType.Link, lineIndex, node, text, targetUri)
        {
        }

        public override int GetHashCode()
        {
            if (hash == 0)
                hash = (text != null ? text.GetHashCode() : 0) + TargetUri.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            return String.Format("{{\"{0}\" -> {1}}}", Text, TargetUri);
        }

        public Uri TargetUri
        {
            get { return targetUri ?? (targetUri = new Uri(node.Attributes["href"].Value, UriKind.RelativeOrAbsolute)); }
            set { targetUri = value; }
        }

        public string TitleText
        {
            get
            {
                if (titleText == null)
                {
                    titleText = "";

                    if (node.Attributes["title"] != null)
                        titleText = node.Attributes["title"].Value;

                    if (ImageLineChildren.Count > 0 && ImageLineChildren[0].TitleText != null)
                        titleText += " " + ImageLineChildren[0].TitleText;
                }

                return titleText;
            }

            set { titleText = value; }
        }

        public override string Text
        {
            get
            {
                if (base.Text != null || base.Text != String.Empty)
                    return base.Text;

                else if (ImageLineChildren.Count > 0 && ImageLineChildren[0].AltText != null)
                    return ImageLineChildren[0].AltText;

                else
                    return TitleText;
            }

            set { base.Text = value; }
        }

        public List<ImageLine> ImageLineChildren
        {
            get { return imageLineChildren ?? (imageLineChildren = new List<ImageLine>()); }
        }

        public bool HasImageLineChildren
        {
            get { return imageLineChildren != null && imageLineChildren.Count > 0; }
        }
    }
}
