<%@ Page Language="C#" MasterPageFile="~/HeaderAndFooter.master" AutoEventWireup="true" CodeFile="UserSettings.aspx.cs" Inherits="UserSettings" Title="Untitled Page" %>
<%@ OutputCache Location="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="content" Runat="Server">

<div class="box singleBox">
    <div class="top">
        <strong class="section">Settings</strong>
        <h1><%=userForm.User.Username%></h1>
    </div>
    
    <exo:UserForm runat="server" ID="userForm" Mode="Edit" />
    
</div>

</asp:Content>

