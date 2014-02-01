<%@ Page Language="C#" MasterPageFile="~/SingleBox.master" AutoEventWireup="true" CodeFile="SignIn.aspx.cs" Inherits="SignIn" %>
<%@ OutputCache Location="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="heading" runat="server">
    <asp:localize ID="Localize1" runat="server" Text="<%$ Resources:Resource,SignIn %>" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="box" runat="Server">

    <form runat="server" ID="signInForm" class="filloutForm" method="post" style="float:left; width:40%">
        <ul class="inputContainer" style="width:400px">
            <li<%if(!UserNamePropertyProxyValidator.IsValid){ %> class="highlight"<%} %>>
                <asp:label ID="Label1" runat="server" associatedcontrolid="userName" Text="<%$ Resources:Resource,UserName %>" />
                <input runat="server" id="userName" type="text" class="medium" />
                
                <el:PropertyProxyValidator
                    runat="server"
                    ID="UserNamePropertyProxyValidator"
                    ControlToValidate="userName"
                    PropertyName="Username"
                    SourceTypeName="Exo.Exoget.Model.User.UserInfo"
                    Display="Dynamic"
                    EnableClientScript="false"
                    CssClass="error"
                />
            </li>
            
            <li<%if(!PasswordPropertyProxyValidator.IsValid || !PasswordCustomValidator.IsValid){ %> class="highlight"<%} %>>
                <asp:label ID="Label2" runat="server" associatedcontrolid="password" Text="<%$ Resources:Resource,Password %>" />
                <input runat="server" id="password" type="password" class="medium" />
                
                <el:PropertyProxyValidator
                    runat="server"
                    ID="PasswordPropertyProxyValidator"
                    ControlToValidate="password"
                    PropertyName="Password"
                    SourceTypeName="Exo.Exoget.Model.User.UserInfo"
                    Display="Dynamic"
                    EnableClientScript="false"
                    CssClass="error"
                />
                
                <asp:CustomValidator runat="server" ID="PasswordCustomValidator" ControlToValidate="password" OnServerValidate="ValidateLogin" Display="Dynamic" EnableClientScript="false" CssClass="error">
                The password was incorrect.
                </asp:CustomValidator>
            </li>
            
            <li style="margin-left:150px">
                <span><a href="/recover_user">Forgot</a> sign in details?</span>
            </li>
        </ul>
        
        <asp:HiddenField runat="server" ID="returnUrl" Value='<%#Request.Headers["Referer"]%>' />
        <input id="Submit1" name="signIn" type="submit" class="button" style="margin-left:255px" value="<%$ Resources:Resource,SignIn %>" runat="server" />
    </form>
    
    <div style="float:left; width:60%">
        <h2>Why Register?</h2>
        <exo:RegisterBenefits ID="RegisterBenefits1" runat="server" />
        
        <div style="margin-top:3em; font-size:large">OK, I'd like to <a href="/register">Register</a></div>
        
    </div>
    
</asp:Content>