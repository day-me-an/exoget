<%@ Page Language="C#" MasterPageFile="~/SingleBox.master" CodeFile="AddContent.aspx.cs" Inherits="AddContent" Async="true" Title="Add Content - exoGet" %>
<asp:Content ID="Content1" ContentPlaceHolderID="heading" Runat="Server">Add Content</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="box" Runat="Server">

<asp:Panel runat="server" ID="instructions" Visible="true">
    <p>If you have special circumstances for submitting content to exoGet, such as large amounts, we will create an efficient custom submission procedure. For further details contact <a href="mailto:spider@exøget.com">spider@exøget.com</a>.</p>

    <p>
    Otherwise you can submit content by entering <acronym title="Universal Resource Locator">URL</acronym>(s) below, please separate multiple URL's with a new line.
    For your conveinance, best search result ranking and automatic discovery of new content you should enter a page or podcast URL that <em>links to the media</em> and not each individual media URL.
    </p>
</asp:Panel>

<form runat="server" id="urlForm" class="filloutForm" style="margin-top:2em">

    <fieldset>
        <legend>URL Addition</legend>
        <ul class="inputContainer" style="width:100%">
            <li<%if(!UrlValidator.IsValid){%> class="highlight"<%}%>>
                <asp:label ID="Label1" runat="server" AssociatedControlID="urls">URL(s)</asp:label>
                <asp:textbox runat="server" ID="urls" TextMode="MultiLine" Rows="10" Columns="80" />
                
                <asp:CustomValidator runat="server" ID="UrlValidator" ControlToValidate="urls" OnServerValidate="OnUrlValidator" ValidateEmptyText="true" Display="Dynamic" EnableClientScript="false" CssClass="error">Please enter a valid URL or multiple separated by a new line.</asp:CustomValidator>
            </li>
        </ul>
    </fieldset>

    <div><input type="submit" value="Add URL(s)" /></div>
    
</form>

<asp:Panel runat="server" ID="success" Visible="false">
Thank you, the URL(s) have been submitted.
</asp:Panel>

</asp:Content>

