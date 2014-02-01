<%@ Import Namespace="Exo.Exoget.Model.Search" %>
<%@ Import Namespace="Exo.Exoget.Web" %>
<%@ Control Language="C#" CodeFile="SearchForm.ascx.cs" Inherits="Exo.Exoget.Web.Controls.SearchForm" AutoEventWireup="false" %>

<form id="searchForm" action="/search" method="get">
    <fieldset>
    
        <label for="s_query" class="queryLabel">Search</label>
        <input id="s_query" class="query" name="query" type="text"<% if(Query != String.Empty){ %> value="<%=Query%>"<% } %> />
        
        <input type="submit" value="Search" class="searchButton" />
        
        <div id="searchFormScopes">
        
            <input id="ss_audio" type="radio" name="scope" value="<%=(byte)Exo.Exoget.Model.Search.SearchScope.Audio%>"<% if(Scope == SearchScope.Audio){ %> checked="checked"<% } %>/>
            <label for="ss_audio"<% if(Scope == SearchScope.Audio) { %> class="current"<% } %>>Audio</label>

            <input id="ss_video" type="radio" name="scope" value="<%=(byte)Exo.Exoget.Model.Search.SearchScope.Video%>"<% if(Scope == SearchScope.Video){ %> checked="checked"<% } %>/>
            <label for="ss_video"<% if(Scope == SearchScope.Video) { %> class="current"<% } %>>Video</label>
          
        </div>
            
        <a href="#" class="searchFormOptionsOpener"<% if(Scope == SearchScope.Video) { %> style="display:none"<% } %>>Search Options</a>
        
    </fieldset>
</form>