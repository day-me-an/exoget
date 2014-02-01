<%@ Page Language="C#" MasterPageFile="~/SingleBox.master" AutoEventWireup="false" CodeFile="Audios.aspx.cs" Inherits="Audios" Title="exoGet Audio" %>
<%--@ OutputCache Location="ServerAndClient" Duration="15" VaryByParam="page" VaryByCustom="UserSearchOptions" --%>

<asp:Content ContentPlaceHolderID="heading" runat="server">Featured Audio</asp:Content>

<asp:Content ContentPlaceHolderID="box" runat="Server">
    <exo:SqlMediaResult ID="SqlMediaResult1"
        runat="server"
        Sql="SELECT SQL_CALC_FOUND_ROWS mediafeatured.mediaId FROM mediafeatured,media WHERE media.id = mediafeatured.mediaId AND media.mediaType = 1 ORDER BY mediafeatured.id DESC"
        PagingUriFormat="/audio/featured/page{0}"
        FirstPagingUriFormat="/audio"
    />
</asp:Content>
