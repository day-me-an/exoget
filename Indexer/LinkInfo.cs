using System;
using System.Collections.Generic;
using System.Text;
using DistribuJob.Client.Extracts.Links;
using Exo.Web;

namespace DistribuJob.Indexer
{
    public class LinkInfo
    {
        private uint id;
        private LinkType type;
        private uint sourceDocId, imageDocId;
        private DocumentType sourceDocType;
        private string text, description;
        private bool isAmbiguous;

        public LinkInfo()
        {
        }

        public uint Id
        {
            get { return id; }
            set { id = value; }
        }

        public LinkType Type
        {
            get { return type; }
            set { type = value; }
        }

        public uint SourceDocId
        {
            get { return sourceDocId; }
            set { sourceDocId = value; }
        }

        public DocumentType SourceDocType
        {
            get { return sourceDocType; }
            set { sourceDocType = value; }
        }

        public uint ImageDocId
        {
            get { return imageDocId; }
            set { imageDocId = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public bool IsAmbiguous
        {
            get { return isAmbiguous; }
            set { isAmbiguous = value; }
        }
    }
}
