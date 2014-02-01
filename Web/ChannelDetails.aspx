<%@ Import Namespace="Exo.Exoget.Web" %>
<%@ Reference Control="~/UserControls/MediaRow.ascx" %>
<%@ Page Language="C#" MasterPageFile="~/HeaderAndFooter.master" AutoEventWireup="false" CodeFile="ChannelDetails.aspx.cs" Inherits="ChannelDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="content" Runat="Server">

    <div class="box channelDetails">
        <div class="top">
            <strong class="section">Channel</strong>
            <h1><%=Feed.Title%></h1>
        </div>
        
        <style type="text/css">
        .channelDetails .details
        {
        	background:red;
        }
        
        .channelDetails .details .desc
        {
        	float:left;
        	width:60%;
        }
        
        .channelDetails .details .desc img
        {
            float:left;
        }
        
        .channelDetails .details .propList
        {
        	float:right;
        	width:40%;
        }
        
        .channelDetails .details .propList .props .prop div
        {
            margin-left:70px;
        }        
        
        .channelDetails .details .propList h2
        {
        	margin:0;
        	padding:0;
        }
        
        .channelDetails .actions
        {
        	clear:both;
        }
        
        .channelDetails .actions button
        {
        	display:block;
        	float:left;
        	margin-right:1em;
        }
        
        .channelDetails .actions .urls
        {
        	clear:both;
        }        
        
        .channelDetails .actions .urls a
        {
        	display:block;
        	padding-left:18px;
        	background-repeat:no-repeat;
        }
        
        .channelDetails .actions .urls a.rss
        {
        	background-image:url(http://static.exoget.com/images/icons/rss.png);
        }
        
        .channelDetails .actions .urls a.m3u,
        .channelDetails .actions .urls a.pls,
        .channelDetails .actions .urls a.xspf
        {
        	background-image:url(http://static.exoget.com/images/icons/sound.png);
        }
        
        .channelDetails .actions .urls a.asx
        {
        	background-image:url(http://static.exoget.com/images/icons/wm.png);
        }
        
        .channelDetails .mediaRowList
        {
        	clear:both;
        	padding-top:1em;
        }
        </style>
        
        <div class="details">
            <div class="desc">
                <img src="http://static.exoget.com<%=Feed.ImagePath%>" alt="" />
                <p><%=Feed.Description%></p>
            </div>
            
            <div class="propList">
                <h2>Channel Details</h2>

                <ul class="props">
                    <%
                        if (Feed.Type!= Exo.Web.Feed.FeedType.None)
                        {
                    %>
                        <li class="prop"><strong>Type</strong><div><%=Feed.Type%></div></li>
                    <%
                        } 
                    %>                  
                
                    <%
                        string host = new Uri(Feed.Url).Host;
                    %>
                
                    <li class="prop"><strong>Website</strong><div><a href="http://<%=host%>"><%=host%></a></div></li>
                
                    <%
                        if (Feed.Author!=null)
                        {
                    %>
                        <li class="prop"><strong>Author</strong><div><a href="<%=Helper.GetSearchUrl(Exo.Web.MediaType.Audio, Exo.Exoget.Model.Search.IndexWordType.MediaAuthor, Feed.Author.Value)%>"><%=Feed.Author.Value%></a></div></li>
                    <%
                        } 
                    %>    
                    
                    <%
                        if (Feed.HasPubdate)
                        {
                    %>
                        <li class="prop"><strong>Published</strong>
                            <div>
                                <span><%=Feed.Pubdate.ToString("yyyy/MM/dd")%></span> (<em><%=Exo.Misc.ExoUtil.GetTimeDifferenceDescription(Feed.Pubdate)%></em>)                        
                            </div>
                        </li>
                    <%
                        }
                    %>                                 
                
                    <%
                        if (Feed.Keywords.Length > 0)
                        {
                    %>
                        <li class="prop"><strong>Tags</strong><div><%=Exo.Exoget.Web.Controls.MediaRow.WriteKeywordsHtml(Feed, Exo.Web.MediaType.Audio, null)%></div></li>
                    <%
                        } 
                    %>
                </ul>
                
                <div class="actions">
                    <button>Add to my Subscriptions</button>
                    
                    <div class="urls">
                        <a href="<%=Feed.Url%>" class="rss">RSS</a>
                        <a href="<%=String.Format("/channel/{0}_{1}/episodes/latest.xspf",Id,SKey)%>" class="xspf">XSPF</a>
                        <a href="<%=String.Format("/channel/{0}_{1}/episodes/latest.m3u",Id,SKey)%>" class="m3u">M3U</a>
                        <a href="<%=String.Format("/channel/{0}_{1}/episodes/latest.pls",Id,SKey)%>" class="pls">PLS</a>
                        <a href="<%=String.Format("/channel/{0}_{1}/episodes/latest.asx",Id,SKey)%>" class="asx">ASX</a>
                    </div>
                </div>                
                
            </div>
            
        </div>
        
        <br class="cb" />
        
        <div>
            <h2 style="float:left">Latest Episodes</h2>
            <asp:PlaceHolder runat="server" ID="mediaResultHolder" />
        </div>
        
    </div>

</asp:Content>

