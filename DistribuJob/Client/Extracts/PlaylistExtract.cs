using System;
using System.Collections.Generic;
using System.Text;
using Exo.Exoget.Model.Search;

namespace DistribuJob.Client.Extracts
{
    [Serializable]
    public class PlaylistExtract : Extract
    {
        private string title, description, author, copyright;

        public PlaylistExtract()
            : base()
        {
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string Author
        {
            get { return author; }
            set { author = value; }
        }

        public string Copyright
        {
            get { return copyright; }
            set { copyright = value; }
        }

        public override void AddIndexProperties()
        {
            TryAddIndexProperty(IndexPropertyType.Author, Author);
            TryAddIndexProperty(IndexPropertyType.Copyright, Copyright);
        }
    }
}