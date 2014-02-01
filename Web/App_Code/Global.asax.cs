using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Diagnostics;
using System.Security.Principal;
using Exo.Exoget.Model.User;
using System.Threading;
using System.Globalization;
using System.Web.Profile;

namespace Exo.Exoget.Web
{
    public class Global : HttpApplication
    {
        public Global()
        {
            base.PostAuthenticateRequest += Application_PostAuthenticateRequest;
        }

        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown
        }

        void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            if (Request.Cookies[FormsAuthentication.FormsCookieName] == null)
                return;

            FormsAuthenticationTicket ticket;

            try
            {
                ticket = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value);
            }
            catch
            {
                FormsAuthentication.SignOut();
                return;
            }

            uint userId;

            if (String.IsNullOrEmpty(ticket.Name)
                || String.IsNullOrEmpty(ticket.UserData)
                || !UInt32.TryParse(ticket.UserData, out userId))
            {
                FormsAuthentication.SignOut();
                return;
            }

            IPrincipal principal = new GenericPrincipal(new UserIdentity(userId, ticket.Name), new string[0]);

            Context.User = principal;
            Thread.CurrentPrincipal = principal;

            FormsAuthentication.RenewTicketIfOld(ticket);
        }

        void Application_Error(object sender, EventArgs e)
        {
            const string sourceName = "Exoget Web";

            if (!EventLog.SourceExists(sourceName))
                EventLog.CreateEventSource(sourceName, "Application");

            using (EventLog log = new EventLog("Application", ".", sourceName))
            {
                log.WriteEntry(Server.GetLastError().Message, EventLogEntryType.Error);
            }
        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

        public override string GetVaryByCustomString(HttpContext context, string arg)
        {
            switch (arg)
            {
                case "UICulture":
                    return CultureInfo.CurrentUICulture.ToString();

                case "UserSearchOptions":
                    return ((uint)Helper.CurrentSearchOptions).ToString();

                default:
                    return String.Empty;
            }
        }
    }
}
