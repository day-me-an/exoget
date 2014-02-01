using System;
using System.Collections.Generic;
using System.Text;
using Exo.Misc;
using HtmlAgilityPack;
using Exo.Extensions;

namespace DistribuJob.Client.Processors.Html.Lines
{
    public class Line
    {
        protected readonly LineType type;
        protected readonly int lineIndex;
        protected readonly HtmlNode node;
        protected string text;

        [NonSerialized]
        private StringBuilder textBuilder;
        [NonSerialized]
        private string[] words;
        [NonSerialized]
        protected int hash;

        public Line(LineType type, int lineIndex, HtmlNode node, string text)
        {
            this.type = type;
            this.lineIndex = lineIndex;
            this.node = node;

            if (text == null)
            {
                textBuilder = new StringBuilder();
                textBuilder.Append(text);

            } else
                this.text = text;
        }

        public Line(LineType type, int lineIndex)
            : this(type, lineIndex, null, null)
        {
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == GetHashCode();//obj is Line ? ((Line)obj). && obj.GetHashCode() == GetHashCode() : base.Equals(obj);
        }
        
        public override int GetHashCode()
        {
            if (hash == 0)
                foreach (string word in Words)
                        hash += word.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} - {2}:\n{3}", type, lineIndex, (node != null ? node.Name : ""), Text);
        }

        public void CommitText()
        {
            if (textBuilder != null)
            {
                textBuilder.Remove(textBuilder.Length - 1, 0);
                text = textBuilder.ToString();
                textBuilder = null;
            }
        }

        public int LineIndex
        {
            get { return lineIndex; }
        }

        public LineType Type
        {
            get { return type; }
        }

        public HtmlNode Node
        {
            get { return node; }
        }

        public virtual string Text
        {
            get { return text; }

            set
            {
                if (textBuilder != null)
                    textBuilder.Append(value);

                else
                    text = value;
            }
        }

        public string[] Words
        {
            get { return words ?? (words = (text == null ? new string[0] : Text.Tokenize())); }
        }
    }

    public enum LineType
    {
        Text,
        Link,
        Image,
        Embed
    }
}
