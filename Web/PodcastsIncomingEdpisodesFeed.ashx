<%@ WebHandler Language="C#" Class="PodcastsIncomingEdpisodesFeed" %>

using System;
using System.Web;

public class PodcastsIncomingEdpisodesFeed : IHttpHandler
{
    public void ProcessRequest (HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        context.Response.Write("Hello World");
    }
 
    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}