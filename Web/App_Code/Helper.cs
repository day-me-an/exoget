using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Exo.Exoget.Model.Search;
using Exo.Exoget.Model.User;
using Exo.Extensions;
using Exo.Web;
using System.Text;

namespace Exo.Exoget.Web
{
    public static class Helper
    {
        public static void SignIn(IPrincipal newUser)
        {
            HttpContext.Current.User = newUser;
            FormsAuthentication.SetAuthCookie(newUser.Identity.Name, true);

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket
                (
                1,
                newUser.Identity.Name,
                DateTime.Now,
                DateTime.Now.AddYears(1),
                true,
                ((UserIdentity)newUser.Identity).Id.ToString()
                );

            HttpCookie authenticationCookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            authenticationCookie.Expires = ticket.Expiration;

            HttpContext.Current.Response.Cookies.Add(authenticationCookie);
        }

        public static void SignIn(uint id, string username)
        {
            IPrincipal newUser = new GenericPrincipal(new UserIdentity(id, username), new string[0]);
            SignIn(newUser);
        }

        /// <summary>
        /// Returns a search or list URL
        /// </summary>
        public static string GetSearchUrl(MediaType mediaType, IndexWordType wordType, string query)
        {
            StringBuilder url = new StringBuilder();

            string searchModifier;
            bool hasSpace = query.Contains(' ');
            bool hasSearchModifier =
                wordType != IndexWordType.None
                && wordType != IndexWordType.MediaKeyword
                && SearchInfo.indexWordTypeToSearchModifier.TryGetValue(wordType, out searchModifier);

            url.Append('/');

            if (mediaType == MediaType.Video)
                url.Append("video/");

            else
                url.Append("audio/");

            if (hasSearchModifier)
            {
                url.Append("field/");

                url.Append(SearchInfo.indexWordTypeToSearchModifier[wordType]);

                url.Append('/');
                url.Append(String.Join("-", query.Tokenize()));
            }
            else
            {
                url.Append("search/");

                if (wordType == IndexWordType.MediaKeyword)
                    url.Append(String.Join("-", query.Tokenize()));

                else
                    url.Append(System.Web.HttpUtility.UrlEncode(Lucene.Net.QueryParsers.QueryParser.Escape(query)));
            }

            url.Append('/');

            return url.ToString();
        }

        /// <summary>
        /// Returns a search url with CurrentSearchOptions
        /// </summary>
        public static string GetSearchUrl(MediaType mediaType, string query)
        {
            return GetSearchUrl(mediaType, IndexWordType.None, query);
        }

        public static SearchOptions ParseSearchOptions(string optionsCookie)
        {
            SearchOptions options = SearchOptions.None;
            uint optionsValue;

            if (UInt32.TryParse(optionsCookie, out optionsValue))
            {
                options = (SearchOptions)optionsValue;
            }

            return options;
        }

        public static SearchOptions CurrentSearchOptions
        {
            get
            {
                SearchOptions currentSearchOptions = SearchOptions.All;
                SearchOptions urlOptions = ParseSearchOptions(HttpContext.Current.Request.QueryString["options"]);

                if (urlOptions != SearchOptions.None)
                    currentSearchOptions = urlOptions;

                else if (!HttpContext.Current.Request.Path.Contains("Search.aspx"))
                    currentSearchOptions = DefaultSearchOptions;

                return currentSearchOptions;
            }
        }

        public static SearchOptions DefaultSearchOptions
        {
            get
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies["searchOptions"];

                return cookie == null ? SearchOptions.All : ParseSearchOptions(cookie.Value);
            }
        }
    }
}