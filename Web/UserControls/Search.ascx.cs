using System;
using System.Linq;
using System.Web.UI;
using Exo.Exoget.Model.Search;
using Exo.Exoget.Web.Controls;
using Exo.Exoget.Model.Media;
using Exo.Exoget.Web;

public partial class SearchControl : System.Web.UI.UserControl
{
    private const int ResultsPerPage = 10;
    private const int MaxPageNumber = 10;

    private string query, urlEncodedQuery, htmlEncodedQuery;
    private SearchInfo search;
    private ResultInfo result;
    private SearchScope scope = SearchScope.Audio;

    public SearchControl()
    {
        Load += PreResultFlashes;
        Load += GetResults;
        Load += PostResultFlashes;
    }

    private void PreResultFlashes(object sender, EventArgs e)
    {
        if (PageNumber >= MaxPageNumber)
        {
            DisplayFlash(String.Format("Sorry, exoGet does not show more than the first {0} results for a search.", ResultsPerPage * MaxPageNumber));

            Load -= GetResults;
            Load -= PostResultFlashes;
        }
    }

    private void GetResults(object sender, EventArgs e)
    {
        if (Request.QueryString["scope"] == "2")
            scope = SearchScope.Video;

        search = new SearchInfo
        {
            Query = Query,
            StartIndex = StartIndex,
            EndIndex = StartIndex + ResultsPerPage,
            Scope = scope
        };

        if (scope == SearchScope.Audio)
            search.Options = Helper.CurrentSearchOptions;

        SearchManager searcher = new SearchManager(search, ((CommonPage)Page).DatabaseConnection);

        try
        {
            result = searcher.GetSearchResults();
        }
        catch (Exception ex)
        {
            DisplayFlash("Sorry, an error occured during the processing of your search. Please examine your search query, it may be malformed.");
            Load -= PostResultFlashes;
            Response.StatusCode = 500;

            System.Diagnostics.Trace.TraceError(ex.ToString());

            return;
        }

        MediaResult mediaResult = new MediaResult()
        {
            ResultsPerPage = ResultsPerPage,
            MaxPageNumber = MaxPageNumber,
            Options = MediaResultOptions.Paging,
            Result = result,
            TermHighlightRegex = result.TokenRegex
        };

        if (search.Scope == SearchScope.Audio && search.Options != SearchOptions.All)
		{
            mediaResult.PagingUriFormat = String.Format("?options={0}&page={{0}}", (uint)search.Options);
			mediaResult.FirstPagingUriFormat = String.Format("?options={0}", (uint)search.Options);
		}
        else
		{
            mediaResult.PagingUriFormat = "?page={0}";
			mediaResult.FirstPagingUriFormat = "./";
		}

        results.Controls.Add(mediaResult);
    }

    private void PostResultFlashes(object sender, EventArgs e)
    {
        if (result == null)
            return;

        bool hasSearchOptions =
            scope == SearchScope.Audio && ((CommonPage)Page).CurrentSearchOptions != SearchOptions.All;

        if (result.ResultsFoundCount == 0)
        {
            if (!hasSearchOptions)
                DisplayFlash("Sorry, no results were found");

            else
                DisplayFlash("Sorry, no results were found. Try changing your <a href=\"#\" class=\"searchFormOptionsOpener\">Search Options</a>"); ;
        }
        else if ((search.Options & SearchOptions.OperatorOR) == SearchOptions.OperatorOR)
            DisplayFlash("This is a partial result due to some of your query terms not being matched.");

        else if (hasSearchOptions)
            DisplayFlash("These results have been limited by your <a href=\"#\" class=\"searchFormOptionsOpener\">Search Options</a>");

        else if (scope == SearchScope.Audio)
        {
            bool clipsTooShort = result.Medias.Count(media => { return media.Duration < 60; }) > ResultsPerPage * 0.5;
            bool clipsBadQuality = result.Medias.Count(media => { return media.Quality != MediaInfo.MediaQuality.None && media.Quality <= MediaInfo.MediaQuality.Ok; }) > ResultsPerPage * 0.5;

            if (clipsTooShort && clipsBadQuality)
                DisplayFlash("Looking for better clips? try adjusting the duration and quality in the <a href=\"#\" class=\"searchFormOptionsOpener\">Search Options</a>");

            else if (clipsTooShort)
                DisplayFlash("Looking for longer clips? try adjusting the duration in the <a href=\"#\" class=\"searchFormOptionsOpener\">Search Options</a>");

            else if (clipsBadQuality)
                DisplayFlash("Looking for higher quality clips? try adjusting the quality in the <a href=\"#\" class=\"searchFormOptionsOpener\">Search Options</a>");
        }
    }

    private void DisplayFlash(string contents)
    {
        tipsPlaceHolder.Controls.Add(new LiteralControl("<em class=\"flash\">" + contents + "</em>"));
    }

    protected string Query
    {
        get
        {
            if (query == null)
            {
                query = Request.QueryString["query"].Trim();

                if (Request.QueryString["rewritten"] != null)
                {
                    if (query[0] == '-')
                    {
                        query = query.Substring(1);
                        query = query.Insert(0, "_____");
                    }

                    query = query.Replace("---", "_____");
                    query = query.Replace('-', ' ');
                    query = query.Replace("_____", " -");
                }
            }

            return query;
        }
    }

    protected string HtmlEncodedQuery
    {
        get { return htmlEncodedQuery ?? (htmlEncodedQuery = Server.HtmlEncode(Query)); }
    }

    protected string UrlEncodedQuery
    {
        get { return urlEncodedQuery ?? (urlEncodedQuery = Server.UrlEncode(Request.QueryString["query"])); }
    }

    protected string SectionTitle
    {
        get
        {
            string field = Request.QueryString["field"];

            if (field != null)
                return field;

            else
                return Scope.ToString() + " Search";
        }
    }

    protected SearchScope Scope
    {
        get
        {
            SearchScope scope = SearchScope.Audio;

            if (Request.QueryString["scope"] == "2")
                scope = SearchScope.Video;

            return scope;
        }
    }

    protected ResultInfo Result
    {
        get { return result; }
    }

    protected int PageNumber
    {
        get
        {
            if (Request.QueryString["page"] != null)
            {
                int pageNumber;

                if (!Int32.TryParse(Request.QueryString["page"], out pageNumber))
                    return 1;

                else if (pageNumber < 1 || pageNumber > MaxPageNumber)
                    return -1;

                else
                    return pageNumber;
            }
            else
                return 1;
        }
    }

    protected int StartIndex
    {
        get { return (PageNumber - 1) * ResultsPerPage; }
    }
}
