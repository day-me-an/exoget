using Exo.Extensions;

namespace DistribuJob.Client.Processors.Html
{
    public class Sentence
    {
        private readonly string str;
        private string[] words;

        public Sentence(string str)
        {
            this.str = str;
        }

        public string[] Words
        {
            get { return words ?? (words = str.Tokenize()); }
        }

        public override int GetHashCode()
        {
            return str.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == GetHashCode();
        }

        public override string ToString()
        {
            return str;
        } 
    }
}