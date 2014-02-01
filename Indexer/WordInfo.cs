using System;
using System.Collections.Generic;
using System.Text;
using Exo.Exoget.Model.Search;
using Exo.Misc;

namespace DistribuJob.Indexer
{
    internal class WordInfo
    {
        private readonly string word;
        private readonly IndexWordType type;

        internal WordInfo(string word, IndexWordType type)
        {
            this.word = word;
            this.type = type;
        }

        public string Word
        {
            get { return word; }
        }

        public IndexWordType Type
        {
            get { return type; }
        }

        public override int GetHashCode()
        {
            return word.GetHashCode() + type.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        public override string ToString()
        {
            return type + ":" + word;
        }
    }
}
