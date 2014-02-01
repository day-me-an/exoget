<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaCommentForm.ascx.cs" Inherits="MediaCommentForm" %>

<form runat="server" class="filloutForm">
    <fieldset class="inputContainer" style="width:70%">
        <legend>Add a Comment</legend>
        <ul>
            <li>
                <asp:Label ID="Label1" runat="server" AssociatedControlID="Subject">Subject</asp:Label>
                <asp:TextBox runat="server" ID="Subject" class="large"></asp:TextBox>
            </li>
            <li>
                <asp:Label ID="Label2" runat="server" AssociatedControlID="Body">Body</asp:Label>
                <asp:TextBox TextMode=MultiLine runat="server" ID="Body" class="large"></asp:TextBox>
            </li>
        </ul>
    </fieldset>
    
    <asp:Button ID="Button1" runat="server" Text="Post" />
    
</form>