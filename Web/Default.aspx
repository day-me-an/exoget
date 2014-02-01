<%@ Page Language="C#" MasterPageFile="~/HeaderAndFooter.master" AutoEventWireup="false" CodeFile="Default.aspx.cs" Inherits="_Default" %>
<%--@ OutputCache Location="ServerAndClient" Duration="15" VaryByParam="page" VaryByCustom="UserSearchOptions" --%>

<asp:Content ContentPlaceHolderID="content" Runat="Server">

<p>
exoGet is a <strong>MP3 Search Engine</strong> that allows you to search music <em>MP3 files</em> on the web. 
</p>
    <div class="box">
        <div class="top">
            <h2>Featured</h2>
        </div>
        
        <exo:SqlMediaResult
            runat="server"
            Sql="SELECT SQL_CALC_FOUND_ROWS mediaId FROM mediafeatured ORDER BY id DESC"
            PagingUriFormat="/featured/page{0}"
            FirstPagingUriFormat="/"
        />
        
    </div>

</asp:Content>