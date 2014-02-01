using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using Exo.Exoget.Model.Media;
using Exo.Misc;
using Exo.Exoget.Model.Search;
using Exo.Exoget.Model;
using Exo.Web;

namespace Exo.Exoget.Web.Controls
{
    public partial class MediaRow : System.Web.UI.UserControl
    {
        private MediaInfo media;
        private Regex termHighlightRegex;

        public MediaRow()
        {
        }

        public static void Display(IEnumerable<MediaInfo> medias, PlaceHolder placeHolder)
        {
            foreach (MediaInfo media in medias)
            {
                MediaRow mediaRow = new MediaRow();
                mediaRow.media = media;

                placeHolder.Controls.Add(mediaRow);
            }
        }

        public Regex TermHighlightRegex
        {
            get { return termHighlightRegex; }
            set { termHighlightRegex = value; }
        }

        public MediaInfo Media
        {
            get { return media; }
            set { media = value; }
        }

        protected string Format(string str)
        {
            return Format(str, termHighlightRegex);
        }

        private static string Format(string str, Regex termHighlightRegex)
        {
            if (str == null)
                return null;

            if (termHighlightRegex != null)
                return TextUtil.HighlightTerm(HttpUtility.HtmlEncode(str), termHighlightRegex);

            else
                return HttpUtility.HtmlEncode(str);
        }

        public static string WriteKeywordsHtml(PropertiesBase media, MediaType mediaType, Regex termHighlightRegex)
        {
            StringBuilder sb = new StringBuilder(); ;

            for (int i = 0; i < media.Keywords.Length; i++)
            {
                sb.AppendFormat
                    (
                    "<a href=\"{0}\">{1}</a>",
                    Helper.GetSearchUrl(mediaType, IndexWordType.MediaKeyword, media.Keywords[i].Value),
                    termHighlightRegex != null ? Format(media.Keywords[i].Value, termHighlightRegex) : media.Keywords[i].Value
                    );

                if (i != media.Keywords.Length - 1)
                    sb.Append(", ");
            }

            return sb.ToString();
        }

        private static string _WriteKeywordsHtml(MediaInfo media, Regex termHighlightRegex)
        {
            return WriteKeywordsHtml(media, media.Type, termHighlightRegex);
        }

        public static string WriteKeywordsHtml(MediaInfo media)
        {
            return _WriteKeywordsHtml(media, null);
        }

        protected string WriteKeywordsHtml()
        {
            return _WriteKeywordsHtml(media, termHighlightRegex);
        }

        public static string GetQualityFormatHtml(MediaInfo.MediaQuality quality)
        {
            switch (quality)
            {
                case MediaInfo.MediaQuality.Poor:
                    return "<span class=\"mediaQualityPoor\">Poor</span>";

                case MediaInfo.MediaQuality.Ok:
                    return "<span class=\"mediaQualityOk\">Ok</span>";

                case MediaInfo.MediaQuality.Good:
                    return "<span class=\"mediaQualityGood\">Good</span>";

                case MediaInfo.MediaQuality.Excellent:
                    return "<span class=\"mediaQualityExcellent\">Excellent</span>";

                default:
                    return String.Empty;
            }
        }
    }
}