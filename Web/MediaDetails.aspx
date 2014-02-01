<%@ Import Namespace="Exo.Exoget.Model.Search" %>
<%@ Import Namespace="Exo.Exoget.Web" %>
<%@ Register TagPrefix="exo" Namespace="Exo.Exoget.Web.Controls" %>
<%@ Reference Control="~/UserControls/MediaComment.ascx" %>
<%@ Reference Control="~/UserControls/MediaCommentForm.ascx" %>
<%@ Reference Control="~/UserControls/MediaRow.ascx" %>
<%@ Page Language="C#" MasterPageFile="~/HeaderAndFooter.master" AutoEventWireup="true" CodeFile="MediaDetails.aspx.cs" Inherits="MediaDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="content" Runat="Server">

   <div class="box singleBox mediaDetails">
        <div class="top">
        <strong class="section"><%=Media.Type%></strong>
            <h1><%=HttpUtility.HtmlEncode(Media.Title)%></h1>
        </div>

<div>
<div class="media">
    <%if (Media.DocType == Exo.Web.DocumentType.ArtificalMedia)
      {
          string playerUrl = HttpUtility.HtmlEncode(Media.MediaUri.GetLeftPart(UriPartial.Path));
          string playFlashvars = Media.MediaUri.Query.TrimStart('?');
    %>
        <object type="application/x-shockwave-flash" data="<%=playerUrl%>" width="425" height="350">
            <param name="movie" value="<%=playerUrl%>" />
            <param name="flashvars" value="<%=HttpUtility.HtmlEncode(playFlashvars)%>&amp;autoplay=1&amp;a=1" />
        </object>
    <%
        }
      else if (Media.Type == Exo.Web.MediaType.Audio || Media.Type == Exo.Web.MediaType.Video)
      {
    %>
        <div class="cinema">
            <img src="<%=Media.ImagePath%>" alt="" />
            <a href="<%=Media.MediaUrl%>">Download from <strong><%=Media.MediaUri.Host%></strong></a>
        </div>
        <br class="cb" />
        <%
        if (Media.DocFormat == Exo.Web.DocumentFormat.MpegAudio3)
        {
        %>
        <object type="application/x-shockwave-flash" data="http://static.exoget.com/images/mediaplayer.swf" width="320" height="20">
            <param name="movie" value="http://static.exoget.com/images/mediaplayer.swf" />
            <param name="flashvars" value="file=<%=HttpUtility.UrlEncode(Media.MediaUrl)%>&amp;autostart=false&amp;repeat=true&amp;volume=100" />
        </object>
        <%
        }
      }
        %>
</div>

<div class="propList">
    <h2>Media Details</h2>

    <ul class="props">
        <%
            if (Media.Description != null)
            {
        %>
        <li class="prop"><strong>Description</strong><div><%=HttpUtility.HtmlEncode(Media.Description)%></div></li>
        <%
            } 

            if (Media.Keywords.Length > 0)
            {
        %>
        <li class="prop"><strong>Tags</strong><div><%=MediaRow.WriteKeywordsHtml(Media)%></div></li>
        <%
            } 
        %>
        
        <%
            if (Media.Author != null)
            {
        %>
        <li class="prop"><strong>Author</strong>
        <div>
        <%
        for(int i = 0; i < Media.Authors.Length; i++)
        {
        %>
            <a href="<%=Helper.GetSearchUrl(Media.Type, IndexWordType.MediaAuthor, Media.Authors[i])%>"><%=HttpUtility.HtmlEncode(Media.Authors[i])%></a><%if(i < Media.Authors.Length - 1){%>, <%}%><%
        }
        %>        
        </div>
        </li>
        <%
            } 

            if (Media.Album != null)
            {
        %>
        <li class="prop"><strong>Album</strong><div><a href="<%=Helper.GetSearchUrl(Media.Type, IndexWordType.MediaAlbum, Media.Album.Value)%>"><%=HttpUtility.HtmlEncode(Media.Album.Value)%></a></div></li>
        <%
            } 

            if (Media.Genre != null)
            {
        %>
        <li class="prop"><strong>Genre</strong><div><a href="<%=Helper.GetSearchUrl(Media.Type, IndexWordType.MediaGenre, Media.Genre.Value)%>"><%=HttpUtility.HtmlEncode(Media.Genre.Value)%></a></div></li>
        <%
            }
            
            if (Media.Year != null)
            {
        %>
        <li class="prop"><strong>Year</strong><div><a href="<%=Helper.GetSearchUrl(Media.Type, IndexWordType.MediaYear, Media.Year.Value)%>"><%=HttpUtility.HtmlEncode(Media.Year.Value)%></a></div></li>
        <%
            }

            if (Media.Quality > 0)
            {
        %>
        <li class="prop"><strong>Quality</strong><div><%=MediaRow.GetQualityFormatHtml(Media.Quality)%></div></li>
        <%
            }

            if (Media.Duration > 0)
            {
        %>
        <li class="prop"><strong>Duration</strong><div><%=Media.DurationTimeSpan%></div></li>
        <%
            }

            if (Media.DocFormat > 0)
            {
        %>
        <li class="prop"><strong>Format</strong><div><%=Media.DocFormatFriendly%></div></li>
        <%
            }
            
            if (Media.MediaFileSize > 0)
            {
        %>
        <li class="prop"><strong>File Size</strong><div><%=Exo.Misc.ExoUtil.StrFormatByteSize((long)Media.MediaFileSize)%></div></li>
        <%
            }

            if (Media.SourceUrl != null)
            {
                string encodedSourceUrl = HttpUtility.HtmlEncode(Media.SourceUrl);
        %>
        <li class="prop"><strong>Source</strong><div><a href="<%=encodedSourceUrl%>"><%=encodedSourceUrl%></a></div></li>
        <%
            }
        %>
        
        <%if (Media.HasPubdate)
          {%>
        <li class="prop published">
            <strong>Published</strong>
            <div>
                <span><%=Media.Pubdate.ToString("yyyy/MM/dd")%></span> (<em><%=Exo.Misc.ExoUtil.GetTimeDifferenceDescription(Media.Pubdate)%></em>)
            </div>
        </li>
        <%}%>
        
        <%if (Media.Views > 0)
          {%>        
        <li class="prop"><strong>Views</strong><div><%=Media.Views%></div></li>
        <%}%>
        
        <li class="prop"><strong>Rating</strong>
            <div>
                <div class="star-rating">
                    <ul>
                        <li class="current-rating" style="width:<%=Media.RatingPercentage%>%"></li>
                        <li><a href="#" class="one-star">1</a></li>
                        <li><a href="#" class="two-stars">2</a></li>
                        <li><a href="#" class="three-stars">3</a></li>
                        <li><a href="#" class="four-stars">4</a></li>
                        <li><a href="#" class="five-stars">5</a></li>
	                </ul>
                </div>
                
                <button class="addFavorite">Add To My Favorites</button>
            </div>
        </li>
            
    </ul>
</div>
</div>

<div class="commentSection">
    <h2>Comments</h2>
    
    <span class="addTip">Do you have an opinion on this? Why not <a href="#" id="toggleMediaCommentForm">Add a Comment</a></span>
    
    <form id="mediaCommentForm" class="filloutForm mediaCommentForm" action="#">
    
        <ul class="inputContainer">
            <li>
                <label for="addMediaCommentFormTitle">Title</label>
                <input id="addMediaCommentFormTitle" class="large" name="title" type="text" />        
            </li>
            
            <li>
                <label for="addMediaCommentFormMessage">Comment <span class="required">*</span></label>
                <textarea id="addMediaCommentFormMessage" class="large" name="body" rows="3" cols="100"></textarea>           
            </li>
            
            <li><button>Add Comment</button></li>
        </ul>
        
    </form>
    
    <asp:PlaceHolder runat="server" ID="CommentsPlaceholder" />
    
</div>
    </div>
</asp:Content>

