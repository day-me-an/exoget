using System;
using System.Collections.Generic;
using System.Text;
using Exo.Exoget.Model.Search;
using Exo.Misc;
using Exo.Extensions;

namespace DistribuJob.Indexer
{
    public class SentenceInfo
    {
        private readonly IndexWordType type;
        private readonly string sentence;

        public SentenceInfo(string sentence, IndexWordType type)
        {
            this.sentence = sentence;
            this.type = type;
        }

        public string Sentence
        {
            get { return sentence; }
        }

        public IndexWordType Type
        {
            get { return type; }
        }

        public string[] Words
        {
            get { return sentence.Tokenize().RemoveAmbiguousWords(AmbiguousWordType.Technical | AmbiguousWordType.Format); }
        }

        public override int GetHashCode()
        {
            return sentence.GetHashCode() + type.GetHashCode(); ;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }
        
        public override string ToString()
        {
            return type + ":" + sentence;
        }
    }
}
