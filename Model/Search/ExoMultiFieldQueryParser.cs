using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Index;
using Lucene.Net.Analysis;
using System.Diagnostics;
using Exo.Extensions;
using System.IO;
using Token = Lucene.Net.Analysis.Token;

namespace Exo.Exoget.Model.Search
{
    public class ExoMultiFieldQueryParser : MultiFieldQueryParser
    {
        internal List<string> queryTokens = new List<string>();

        public ExoMultiFieldQueryParser(string[] fields, Analyzer analyzer)
            : base(fields, analyzer)
        {
        }

        public override Query Parse(string query)
        {
            if (query.ContainsAny('-', '!'))
            {
                StringBuilder cleanQuery = new StringBuilder();

                for (int i = 0; i < query.Length; i++)
                {
                    bool hasWrongHyphen =
                        query[i] == '-'
                        && ((i < query.Length - 1 && Char.IsWhiteSpace(query[i + 1]))
                        || (i > 0 && !Char.IsWhiteSpace(query[i - 1])));

                    bool lastCharIsBackslash = i > 0 && query[i - 1] == '\\';
                    bool hasExclamationMark = query[i] == '!';

                    if ((!hasWrongHyphen && !hasExclamationMark) || lastCharIsBackslash
                        && !(cleanQuery.Length > 0 && cleanQuery[cleanQuery.Length - 1] == ' ' && query[i] == ' '))
                    {
                        cleanQuery.Append(query[i]);
                    }
                }

                return base.Parse(cleanQuery.ToString());
            }

            return base.Parse(query);
        }

        protected override Query GetFieldQuery(string field, string queryText, int slop)
        {
            queryTokens.Add(queryText);

            List<IndexWordType> list;

            if (field != null
                && queryText != null
                && SearchInfo.searchModifierToindexWordType.TryGetValue(field, out list))
            {
                Query q = base.GetFieldQuery(field, queryText, slop);
                BooleanQuery modifierQuery = new BooleanQuery();

                TokenStream tokens = GetAnalyzer().TokenStream(null, new StringReader(queryText));

                if (q is PhraseQuery)
                {
                    Term[] terms = ((PhraseQuery)q).GetTerms();

                    foreach (IndexWordType type in list)
                    {
                        PhraseQuery pq = new PhraseQuery();
                        string wordType = type.ToString().ToLower();
                        Token token = null;

                        while ((token = tokens.Next()) != null)
                            pq.Add(new Term(wordType, token.TermText()));

                        modifierQuery.Add(pq, BooleanClause.Occur.SHOULD);
                    }
                }
                else
                {
                    string termValue = tokens.Next().TermText();

                    foreach (IndexWordType type in list)
                        modifierQuery.Add(new TermQuery(new Term(type.ToString().ToLower(), termValue)), BooleanClause.Occur.SHOULD);
                }

                return modifierQuery;
            }
            else
                return base.GetFieldQuery(field, queryText, slop);
        }
    }
}
