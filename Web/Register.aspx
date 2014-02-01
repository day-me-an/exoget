<%@ Register TagPrefix="cc1" Namespace="WebControlCaptcha" Assembly="WebControlCaptcha" %>
<%@ Reference Control="~/UserControls/UserForm.ascx" %>
<%@ Page Language="C#" MasterPageFile="~/SingleBox.master" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="Register" Async="true" %>
<%@ OutputCache Location="None" %>

<asp:Content ContentPlaceHolderID="heading" runat="server">
    <asp:localize runat="server" Text="<%$ Resources:Resource,Register %>" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="box" Runat="Server">
    
    <exo:UserForm Runat="server" ID="userForm" Mode="Create" />
    
    <asp:Panel ID="accountCreated" runat="server" Visible="false">
        <h2>Success</h2>
        <p>Your exoGet account has been created and you are now signed in. You may be interested in the following new features:</p>
        
        <exo:RegisterBenefits runat="server" />
        
        <%if (userForm.ReturnUrl != null)
          { %>
            <p style="margin-top:3em; font-size:large">OK, I'd like to <a href="<%=HttpUtility.HtmlEncode(userForm.ReturnUrl)%>">return</a> to where I was</p>
        <% }%>
    </asp:Panel>    
    
</asp:Content>