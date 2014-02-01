<%@ Page Language="C#" MasterPageFile="~/SingleBox.master" AutoEventWireup="true" CodeFile="UserRecover.aspx.cs" Inherits="UserRecover" Title="Recover User ID - exoGet" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="heading" Runat="Server">Recover User ID</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="box" Runat="Server">

<asp:Panel runat="server" ID="beforeSend" Visible="true">
    <p>Please enter the email address you used to create the exoGet account. The username and a new password for the account will be emailed to you.</p>

    <form runat="server" class="filloutForm">

        <ul class="inputContainer">
            <li<%if(!EmailPropertyProxyValidator.IsValid || !EmailCustomValidator.IsValid){%> class="highlight"<%}%>>
                <asp:Label runat="server" AssociatedControlID="email">Email</asp:Label>
                <asp:TextBox runat="server" ID="email" CssClass="large" />
                
                <el:PropertyProxyValidator
                    runat="server"
                    ID="EmailPropertyProxyValidator"
                    ControlToValidate="email"
                    PropertyName="Email"
                    SourceTypeName="Exo.Exoget.Model.User.UserInfo"
                    Display="Dynamic"
                    EnableClientScript="false"
                    CssClass="error"
                />
                
                <asp:CustomValidator
                    runat="server"
                    ID="EmailCustomValidator"
                    ControlToValidate="email"
                    OnServerValidate="ValidateEmail"
                    Display="Dynamic"
                    EnableClientScript="false"
                    CssClass="error"
                >
                Sorry, There is no account with that email address.
                </asp:CustomValidator>            
                
            </li>
        </ul>
        
        <div style="clear:both"><input type="submit" value="Send" /></div>
        
    </form>
</asp:Panel>

<asp:Panel runat="server" ID="afterSend" Visible="false">
    <p>Success, your username and a new password have been emailed to <%=user.Email%>.</p>
</asp:Panel>

</asp:Content>

