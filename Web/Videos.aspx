<%@ Page Language="C#" MasterPageFile="~/SingleBox.master" AutoEventWireup="false" CodeFile="Videos.aspx.cs" Inherits="Videos" Title="exoGet Video" %>
<%--@ OutputCache Location="ServerAndClient" Duration="15" VaryByParam="page" VaryByCustom="UserSearchOptions" --%>

<asp:Content ContentPlaceHolderID="heading" runat="server">Featured Video</asp:Content>

<asp:Content ContentPlaceHolderID="box" runat="Server">
    <exo:SqlMediaResult ID="SqlMediaResult1"
        runat="server"
        Sql="SELECT SQL_CALC_FOUND_ROWS mediafeatured.mediaId FROM mediafeatured, media WHERE media.id = mediafeatured.mediaId AND media.mediaType = 2 ORDER BY mediafeatured.id DESC"
        PagingUriFormat="/video/featured/page{0}"
        FirstPagingUriFormat="/video"
    />
</asp:Content>