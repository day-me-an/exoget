using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;
using System.IO;

namespace Exo.Exoget.Model.Search
{
    public class ExoLuceneAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            return new PorterStemFilter(new ExoLuceneTokenizer(reader));
        }
    }
}
