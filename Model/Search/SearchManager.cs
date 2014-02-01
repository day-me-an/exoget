using System;
using System.Linq;
using System.Text.RegularExpressions;
using Exo.Exoget.Model.Media;
using Exo.Exoget.Model.Properties;
using Exo.Web;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using MySql.Data.MySqlClient;

namespace Exo.Exoget.Model.Search
{
    public class SearchManager
    {
        private static readonly IndexSearcher searcher = new IndexSearcher(Settings.Default.SearchIndexPath);

        //private static readonly Directory spellDirectory = FSDirectory.GetDirectory(Settings.Default.SpellIndexPath, false);
        //private static readonly SpellChecker.Net.Search.Spell.SpellChecker spellChecker = new SpellChecker.Net.Search.Spell.SpellChecker(spellDirectory);

        private static readonly string[] searchFields;

        private readonly SearchInfo search;
        private readonly MySqlConnection conn;

        static SearchManager()
        {
            IndexWordType[] searchWordTypes =
            {
                IndexWordType.MediaTitle,
                IndexWordType.MediaDescription,
                IndexWordType.MediaTranscript,
                IndexWordType.MediaKeyword,
                IndexWordType.MediaAuthor,
                IndexWordType.MediaAlbum,
                IndexWordType.MediaGenre,
                IndexWordType.MediaYear,
                IndexWordType.MediaFilename,
                IndexWordType.MediaFilenameTitle,
                IndexWordType.MediaFilenameAuthor,

                IndexWordType.PageLinkText,
                IndexWordType.PageLinkDescription,
                IndexWordType.PageTitle,
                IndexWordType.PageHeading,

                IndexWordType.FeedLinkTitle,
                IndexWordType.FeedLinkKeyword,
                IndexWordType.FeedLinkDescription,
                IndexWordType.FeedLinkAuthor,
                IndexWordType.FeedLinkCopyright,

                IndexWordType.FeedTitle,
                IndexWordType.FeedKeyword,
                IndexWordType.FeedDescription,
                IndexWordType.FeedAuthor,
                IndexWordType.FeedCopyright,
            };

            searchFields = new string[searchWordTypes.Length];

            for (int i = 0; i < searchFields.Length; i++)
            {
                string field = searchWordTypes[i].ToString().ToLower();
                searchFields[i] = field;
            }
        }

        public SearchManager(SearchInfo search, MySqlConnection conn)
        {
            this.search = search;
            this.conn = conn;
        }

        public ResultInfo GetSearchResults()
        {
            ResultInfo result = new ResultInfo();
            Analyzer analyzer = new ExoLuceneAnalyzer();

            ExoMultiFieldQueryParser mfqp = new ExoMultiFieldQueryParser(searchFields, analyzer);

            if ((search.Options & SearchOptions.OperatorOR) == SearchOptions.OperatorOR)
                mfqp.SetDefaultOperator(QueryParser.Operator.OR);

            else
                mfqp.SetDefaultOperator(QueryParser.Operator.AND);

            Query query = mfqp.Parse(search.Query);
            BooleanQuery mainQuery = new BooleanQuery();
            mainQuery.Add(query, BooleanClause.Occur.MUST);
            mainQuery.Add(new TermQuery(new Term("type", ((byte)search.Scope).ToString())), BooleanClause.Occur.MUST);

            #region Search Option Criteria

            if (search.OptionDuration > 0)
            {
                for (sbyte i = (sbyte)(search.OptionDuration - 1); i >= 0; i--)
                {
                    mainQuery.Add(new TermQuery(new Term("duration", i.ToString())), BooleanClause.Occur.MUST_NOT);
                }
            }

            if ((search.Options & SearchOptions.FormatAllExceptMp3) != SearchOptions.FormatAllExceptMp3 && (search.Options & SearchOptions.FormatMp3) == SearchOptions.FormatMp3)
            {
                mainQuery.Add(new TermQuery(new Term("format", ((byte)DocumentFormat.MpegAudio3).ToString())), BooleanClause.Occur.MUST);
            }
            else
            {
                if ((search.Options & SearchOptions.FormatMp3) != SearchOptions.FormatMp3)
                    mainQuery.Add(new TermQuery(new Term("format", ((byte)DocumentFormat.MpegAudio3).ToString())), BooleanClause.Occur.MUST_NOT);

                if ((search.Options & SearchOptions.FormatMsMedia) != SearchOptions.FormatMsMedia)
                    mainQuery.Add(new TermQuery(new Term("format", ((byte)DocumentFormat.MsMedia).ToString())), BooleanClause.Occur.MUST_NOT);

                if ((search.Options & SearchOptions.FormatRealmedia) != SearchOptions.FormatRealmedia)
                    mainQuery.Add(new TermQuery(new Term("format", ((byte)DocumentFormat.Realmedia).ToString())), BooleanClause.Occur.MUST_NOT);

                if ((search.Options & SearchOptions.FormatQuicktime) != SearchOptions.FormatQuicktime)
                    mainQuery.Add(new TermQuery(new Term("format", ((byte)DocumentFormat.Quicktime).ToString())), BooleanClause.Occur.MUST_NOT);

                if ((search.Options & SearchOptions.FormatMp4) != SearchOptions.FormatMp4)
                    mainQuery.Add(new TermQuery(new Term("format", ((byte)DocumentFormat.Mpeg4).ToString())), BooleanClause.Occur.MUST_NOT);
            }

            if ((search.Options & SearchOptions.QualityPoor) != SearchOptions.QualityPoor)
                mainQuery.Add(new TermQuery(new Term("quality", ((byte)MediaInfo.MediaQuality.Poor).ToString())), BooleanClause.Occur.MUST_NOT);

            if ((search.Options & SearchOptions.QualityOk) != SearchOptions.QualityOk)
                mainQuery.Add(new TermQuery(new Term("quality", ((byte)MediaInfo.MediaQuality.Ok).ToString())), BooleanClause.Occur.MUST_NOT);

            if ((search.Options & SearchOptions.QualityGood) != SearchOptions.QualityGood)
                mainQuery.Add(new TermQuery(new Term("quality", ((byte)MediaInfo.MediaQuality.Good).ToString())), BooleanClause.Occur.MUST_NOT);

            if ((search.Options & SearchOptions.QualityExcellent) != SearchOptions.QualityExcellent)
                mainQuery.Add(new TermQuery(new Term("quality", ((byte)MediaInfo.MediaQuality.Excellent).ToString())), BooleanClause.Occur.MUST_NOT);

            #endregion

            Hits hits = searcher.Search(mainQuery);
            int hitCount = hits.Length();

            if (hitCount > 0 || ((search.Options & SearchOptions.OperatorOR) == SearchOptions.OperatorOR))
            {
                result.ResultsFoundCount = (uint)hitCount;

                MediaManager mediaManager = new MediaManager(conn);

                uint[] mediaIds = new uint[Math.Min(search.EndIndex - search.StartIndex, hitCount - search.StartIndex)];
                int mediaIdsIndex = 0;

                for (int i = search.StartIndex; i < hitCount && i < search.EndIndex; i++)
                {
                    Document doc = hits.Doc(i);

                    uint id;

                    if (UInt32.TryParse(doc.GetField("id").StringValue(), out id))
                        mediaIds[mediaIdsIndex++] = id;
                }

                result.Medias = mediaManager.GetMedia(mediaIds, MediaInfoTypes.Properties);
                result.TokenRegex = new Regex(@"(\b" + String.Join(@"\b)|(\b", mfqp.queryTokens.ToArray()) + @"\b)", RegexOptions.IgnoreCase);
            }
            // re-search using the OR operator
            else
            {
                search.Options |= SearchOptions.OperatorOR;
                return GetSearchResults();
            }

            /*if (hitCount < 5)
            {
                result.QuerySuggestions = spellChecker.SuggestSimilar(query.ToString(), 1);
            }*/

            return result;
        }

        public static string[] SearchFields
        {
            get { return searchFields; }
        }
    }
}