using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;
using System.IO;
using Exo.Extensions;

namespace Exo.Exoget.Model.Search
{
    public class ExoLuceneTokenizer : Tokenizer
    {
        private string[] words;
        private int totalLength = 0;
        private int index = -1;

        public ExoLuceneTokenizer(TextReader input)
            : base(input)
        {
        }

        public override Token Next()
        {
            if (index == -1)
            {
                words = input.ReadToEnd().Tokenize(TokenizeOptions.All | TokenizeOptions.FixTypos);
                words = words.RemoveAmbiguousWords(AmbiguousWordType.Format/* | AmbiguousWordType.Technical*/);

                if (words.Length == 0)
                    return null;

                index = 0;
            }
            else if (index == words.Length)
                return null;

            Token token = new Token(words[index], totalLength, totalLength + words[index].Length);

            totalLength += words[index].Length + 1;
            index++;

            return token;
        }
    }
}
