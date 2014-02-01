using System;
using System.Text.RegularExpressions;
using Exo.Exoget.Model.Media;
using Exo.Exoget.Model.Search;
using System.Web.UI;

namespace Exo.Exoget.Web.Controls
{
    [Flags]
    public enum MediaResultOptions
    {
        Paging = 1
    }

    public class MediaResult : System.Web.UI.Control
    {
        private MediaResultOptions options = MediaResultOptions.Paging;
        private Regex termHighlightRegex;
        private int resultsPerPage = 10;
        private int maxPageNumber = 9;
        protected ResultInfo result;
        private string pagingUriFormat, firstPagingUriFormat;

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<div class=\"mediaResult\">");

            if (Result != null && Result.ResultsCount != 0)
            {
                writer.Write("<div class=\"mediaRowList\">");

                foreach (MediaInfo media in Result.Medias)
                {
                    MediaRow mediaRow = (MediaRow)Page.LoadControl("~/UserControls/MediaRow.ascx");
                    mediaRow.Media = media;
                    mediaRow.TermHighlightRegex = termHighlightRegex;

                    mediaRow.RenderControl(writer);
                }

                writer.Write("</div>");
                writer.Write("<br class=\"cb\" />");

                if ((Options & MediaResultOptions.Paging) == MediaResultOptions.Paging && Result.ResultsFoundCount > 10)
                {
                    writer.Write("<div class=\"paging\">");

                    if (CanNavigatePrevious)
                        writer.Write("<a href=\"" + String.Format(PageNumber - 1 == 1 ? FirstPagingUriFormat : PagingUriFormat, PageNumber - 1) + "\" class=\"action\">&laquo; Previous</a>");

                    else
                        writer.Write("<span class=\"numbersLabel\">Results Page</span>");

                    #region Page Numbers
                    writer.Write("<div class=\"numbers\">");

                    const int perSide = 4;

                    int currentPageNumber = PageNumber;
                    int start = Math.Max(1, currentPageNumber - perSide);

                    int maxEnd = (int)Math.Ceiling(Result.ResultsFoundCount / (double)ResultsPerPage);
                    int end = currentPageNumber > perSide ? (currentPageNumber + perSide) : (perSide * 2 - start);

                    if (end > maxEnd)
                        end = maxEnd;

                    for (int i = start; i <= end && i <= MaxPageNumber; i++)
                    {
                        int startIndex = (i - 1) * resultsPerPage;

                        if (startIndex > MaxPageStartIndex)
                            break;

                        if (startIndex != StartIndex)
                            writer.Write("<a href=\"" + String.Format(i == 1 ? FirstPagingUriFormat : PagingUriFormat, i) + "\">" + i + "</a> ");

                        else
                            writer.Write("<span>" + i + "</span> ");
                    }

                    writer.Write("</div>");
                    #endregion

                    if (CanNavigateNext)
                        writer.Write("<a href=\"" + String.Format(PagingUriFormat, PageNumber + 1) + "\" class=\"action\">Next &raquo;</a>");

                    writer.Write("</div>");
                }
            }

            writer.Write("</div>");
        }

        public int ResultsPerPage
        {
            get { return resultsPerPage; }
            set { resultsPerPage = value; }
        }

        public int MaxPageNumber
        {
            get { return maxPageNumber; }
            set { maxPageNumber = value; }
        }

        public virtual ResultInfo Result
        {
            get { return result; }
            set { result = value; }
        }

        public string PagingUriFormat
        {
            get { return pagingUriFormat; }
            set { pagingUriFormat = value; }
        }

        public string FirstPagingUriFormat
        {
            get { return firstPagingUriFormat; }
            set { firstPagingUriFormat = value; }
        }

        protected int PageNumber
        {
            get
            {
                if (Context.Request.QueryString["page"] != null)
                {
                    int pageNumber;

                    if (!Int32.TryParse(Context.Request.QueryString["page"], out pageNumber))
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
            get { return (PageNumber - 1) * resultsPerPage; }
        }

        protected int MaxPageStartIndex
        {
            get { return Math.Min((int)result.ResultsFoundCount, maxPageNumber * resultsPerPage); }
        }

        protected bool CanNavigatePrevious
        {
            get { return StartIndex - resultsPerPage >= 0; }
        }

        protected bool CanNavigateNext
        {
            get { return StartIndex + resultsPerPage < MaxPageStartIndex; }
        }

        public MediaResultOptions Options
        {
            get { return options; }
            set { options = value; }
        }

        public Regex TermHighlightRegex
        {
            get { return termHighlightRegex; }
            set { termHighlightRegex = value; }
        }
    }
}