<%@ Import Namespace="Exo.Exoget.Model.Search" %>
<%@ Import Namespace="Exo.Exoget.Web" %>
<%@ Control Language="C#" AutoEventWireup="false" inherits="Exo.Exoget.Web.Controls.MediaRow" %>

<div class="mediaRow">
    
    <a href="/<%=(Media.Type == Exo.Web.MediaType.Audio ? "audio" : "video")%>/<%=Media.Id %>_<%=Media.SKey %>" class="title">
        <img alt="" src="<%=Media.ImagePath%>" />
        <%=Format(Media.Title)%>
    </a>
    
    <div class="info">
        <%
            if (Media.Feed != null && (Context.User.Identity.Name == "Damian" || Context.User.Identity.Name == "Dan"))
            {
        %>
        <strong>Channel: <a href="/channel/<%=Media.Feed.Id%>_<%=Media.Feed.SKey%>"><%=Media.Feed.Title%></a></strong>
        <%
            }
            else
            {
        %>
        <strong>From: <a href="http://<%=Media.SourceHost%>/" class="source"><%=Media.SourceHost%></a></strong>
        <%
            }
        %>
        
        <p><%=Format(Media.Description)%></p>
        
        <ul class="props">
            <%if (Media.Keywords.Length > 0)
              {%>
            <li class="keywords"><strong>Tags:</strong>#s#<%=WriteKeywordsHtml(Media)%></li>
            <%}
                
              if (Media.Quality != Exo.Exoget.Model.Media.MediaInfo.MediaQuality.None)
              { %>
            <li><strong>Quality:</strong>#s#<%=GetQualityFormatHtml(Media.Quality)%></li>
            <%}
                
              if (Media.Duration > 0)
              { %>
            <li><strong>Duration:</strong>#s#<%=Media.DurationTimeSpan%></li>
            <%}
                
              if (Media.DocFormat > 0)
              { %>
            <li><strong>Format:</strong>#s#<%=Media.DocFormatFriendly%></li>
            <%}                
                
              if (Media.Author != null)
              { %>
            <li><strong>Author:</strong>#s#<%
                for (int i = 0; i < Media.Authors.Length; i++)
                {
            %><a href="<%=Helper.GetSearchUrl(Media.Type, IndexWordType.MediaAuthor, Media.Authors[i])%>"><%=Format(Media.Authors[i])%></a><%if (i < Media.Authors.Length - 1){%>, <%}%><%
                }
            %>
            </li>
            <%}
              
              if (Media.Album != null)
              { %>
            <li><strong>Album:</strong>#s#<a href="<%=Helper.GetSearchUrl(Media.Type, IndexWordType.MediaAlbum, Media.Album.Value)%>"><%=Format(Media.Album.Value)%></a></li>
            <%}
              
              if (Media.Genre != null)
              {%>
            <li><strong>Genre:</strong>#s#<a href="<%=Helper.GetSearchUrl(Media.Type, IndexWordType.MediaGenre, Media.Genre.Value)%>"><%=Format(Media.Genre.Value)%></a></li>
            <%}
                
              if (Media.Year != null)
              {%>
            <li><strong>Year:</strong>#s#<a href="<%=Helper.GetSearchUrl(Media.Type, IndexWordType.MediaYear, Media.Year.Value)%>"><%=Format(Media.Year.Value)%></a></li>
            <%
              }
                
              if (Media.HasPubdate)
              {%>
            <li class="published"><strong>Published:</strong>#s#<%=Media.Pubdate.ToString("yyyy/MM/dd")%></li>
            <%
              } 
            %>

        </ul>
    </div>
</div>