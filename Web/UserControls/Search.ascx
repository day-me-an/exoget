<%@ Reference Control="~\UserControls\MediaRow.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="SearchControl" %>

<div class="top">
    <strong class="section"><%=SectionTitle%></strong>
    <h1><%=HtmlEncodedQuery%></h1>
    
    <%if (Result != null)
      {%>
    <div class="resultCount">Results<strong>#s#<%=StartIndex%>-<%=(StartIndex + Result.ResultsCount)%></strong> of <strong><%=String.Format("{0:#,##0}", Result.ResultsFoundCount)%></strong></div>
    <%}%>
</div>

<asp:PlaceHolder ID="tipsPlaceHolder" runat="server" />
<asp:PlaceHolder ID="results" runat="server" />