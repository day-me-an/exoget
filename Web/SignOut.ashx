<%@ WebHandler Language="C#" Class="SignOut" %>

using System;
using System.Web;

public class SignOut : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
    public void ProcessRequest (HttpContext context)
    {
        if (context.Session != null)
            context.Session.Abandon();
        
        System.Web.Security.FormsAuthentication.SignOut();

        if (!String.IsNullOrEmpty(context.Request.Headers["Referer"]))
            context.Response.Redirect(context.Request.Headers["Referer"]);

        else
            context.Response.Redirect("/");
    }
 
    public bool IsReusable
    {
        get
        {
            return true;
        }
    }
}