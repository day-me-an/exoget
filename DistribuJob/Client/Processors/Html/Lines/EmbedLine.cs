using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using Exo.Web;

namespace DistribuJob.Client.Processors.Html.Lines
{
    public class EmbedLine : ObjectLine
    {
        private DocumentFormat specifiedFormat;

        public EmbedLine(int lineIndex, HtmlNode node, Uri embedUri)
            : base(LineType.Embed, lineIndex, node, embedUri)
        {
        }

        public override int Width
        {
            get
            {
                if (node.Name == "embed" || (node.Name == "object" && node.Attributes["width"] != null))
                    return base.Width;

                else
                {
                    int width = 0;

                    foreach (HtmlNode childNode in node.ChildNodes)
                        if (childNode.Name == "param" && childNode.Attributes["name"] != null && childNode.Attributes["name"].Value == "width" && childNode.Attributes["value"] != null && Int32.TryParse(childNode.Attributes["value"].Value, out width))
                            break;

                    return width;
                }
            }
        }

        public override int Height
        {
            get
            {
                if (node.Name == "embed" || (node.Name == "object" && node.Attributes["height"] != null))
                    return base.Height;

                else
                {
                    int height = 0;

                    foreach (HtmlNode childNode in node.ChildNodes)
                        if (childNode.Name == "param" && childNode.Attributes["name"] != null && childNode.Attributes["name"].Value == "height" && childNode.Attributes["value"] != null && Int32.TryParse(childNode.Attributes["value"].Value, out height))
                            break;

                    return height;
                }
            }
        }

        public DocumentFormat SpecifiedFormat
        {
            get { return specifiedFormat; }
            set { specifiedFormat = value; }
        }
    }
}
