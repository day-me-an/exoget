using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;

namespace Exo.Exoget.Model.Search
{
    public static class ExoLuceneExtensions
    {
        public static string ToSentence(this TokenStream target)
        {
            StringBuilder sb = new StringBuilder();
            Token token = null;

            while ((token = target.Next()) != null)
            {
                sb.Append(token.TermText());
                sb.Append(' ');
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }
    }
}
