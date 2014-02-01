using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using Exo.Misc;
using System.Text.RegularExpressions;

namespace Exo.Exoget.Model.Search
{
    public class SearchInfo
    {
        public static readonly Dictionary<IndexWordType, string> indexWordTypeToSearchModifier;
        public static readonly Dictionary<string, List<IndexWordType>> searchModifierToindexWordType;

        private string query;
        private SearchScope scope;
        private SearchOptions options;
        private byte optionDuration;
        private int startIndex, endIndex;

        static SearchInfo()
        {
            indexWordTypeToSearchModifier = new Dictionary<IndexWordType, string>();
            searchModifierToindexWordType = new Dictionary<string, List<IndexWordType>>();

            foreach (IndexWordType item in Enum.GetValues(typeof(IndexWordType)))
            {
                IndexWordAttribute[] attributes = (IndexWordAttribute[])item.GetType().GetField(item.ToString()).GetCustomAttributes(typeof(IndexWordAttribute), false);

                foreach (IndexWordAttribute attribute in attributes)
                {
                    indexWordTypeToSearchModifier[item] = attribute.SearchFieldName;

                    List<IndexWordType> list;

                    if (!searchModifierToindexWordType.TryGetValue(attribute.SearchFieldName, out list))
                    {
                        list = new List<IndexWordType>();
                        searchModifierToindexWordType.Add(attribute.SearchFieldName, list);
                    }

                    list.Add(item);
                }
            }
        }

        public SearchInfo()
        {
        }

        public string GetSearchUrl(string query)
        {
            return String.Format("/search?q={0}&amp;scope={1}",
                System.Web.HttpUtility.UrlEncode(Lucene.Net.QueryParsers.QueryParser.Escape(query.Trim())),
                this.scope == SearchScope.Audio ? "a" : "v");
        }

        public string Query
        {
            get { return query; }
            set { query = value; }
        }

        public SearchScope Scope
        {
            get { return scope; }
            set { scope = value; }
        }

        public SearchOptions Options
        {
            get { return options; }
            set { options = value; }
        }

        public byte OptionDuration
        {
            get
            {
                if ((options & SearchOptions.DurationTwo) == SearchOptions.DurationTwo)
                    return 1;

                else if ((options & SearchOptions.DurationThree) == SearchOptions.DurationThree)
                    return 2;

                else if ((options & SearchOptions.DurationFour) == SearchOptions.DurationFour)
                    return 3;

                else
                    return 0;
            }
        }

        public int StartIndex
        {
            get { return startIndex; }
            set { startIndex = value; }
        }

        public int EndIndex
        {
            get { return endIndex; }
            set { endIndex = value; }
        }

        public int ExpectedResultsCount
        {
            get { return endIndex - startIndex; }
        }
    }
}