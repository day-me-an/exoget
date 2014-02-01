using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace DistribuJob.Client.Processors.Html.Lines
{
    public abstract class ObjectLine : LinkLine
    {
        private int width, height;

        public ObjectLine(LineType type, int lineIndex, HtmlNode node, Uri objectUri)
            : base(type, lineIndex, node, null, objectUri) 
        {
        }

        public override int GetHashCode()
        {
            return TargetUri.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("Pos: {0} Type: {1} ObjectUri: {2} Width: {3} Height: {4}", LineIndex, Type, TargetUri, Width, Height);
        }

        public virtual int Width
        {
            get { return width; }
            set { width = value; }
        }

        public virtual int Height
        {
            get { return height; }
            set { height = value; }
        }
    }
}
