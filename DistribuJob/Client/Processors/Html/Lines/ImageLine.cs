using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace DistribuJob.Client.Processors.Html.Lines
{
    public class ImageLine : ObjectLine
    {
        public const int
            MaxLinkRepeatingImage = 2,
            MinLinkImageWidth = 50,
            MinLinkImageHeight = 50;

        private string altText;

        public ImageLine(int lineIndex, HtmlNode node, Uri imageUri)
            : base(LineType.Image, lineIndex, node, imageUri)
        {
        }

        public string AltText
        {
            get { return altText; }
            set { altText = value; }
        }

        public override string Text
        {
            get { return text ?? (text = altText + " " + TitleText); }
            set { base.Text = value; }
        }
    }
}