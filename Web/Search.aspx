<%@ Page Language="C#" MasterPageFile="~/HeaderAndFooter.master" CodeFile="Search.aspx.cs" Inherits="Search" %>
<%@ OutputCache Location="Client" Duration="43829" VaryByParam="none" %>

<asp:Content runat="server" ContentPlaceHolderID="content">
    
    <div class="box singleBox searchResults">
    
        <asp:PlaceHolder runat="server" ID="exoCacheControlHolder" />
        
        <div class="sideAdvert">
            <script type="text/javascript">google_ad_client="pub-7528862467109682";google_ad_channel="SearchResultsRightOne";google_alternate_color="ffffff";google_ad_width=160;google_ad_height=600;google_ad_format="160x600_as";google_ad_type="image";google_color_border="ffffff";google_color_bg="ffffff";google_color_link="0066CC";google_color_text="000000";google_color_url="008000";</script>  
            <script type="text/javascript" src="http://pagead2.googlesyndication.com/pagead/show_ads.js"></script>
            
            <%--
            <script type="text/javascript">google_ad_client="pub-7528862467109682";google_ad_channel="SearchResultsRightTwo";google_alternate_color="ffffff";google_ad_width=160;google_ad_height=600;google_ad_format="160x600_as";google_ad_type="text_image";google_color_border="ffffff";google_color_bg="ffffff";google_color_link="0066CC";google_color_text="000000";google_color_url="008000";</script>  
            <script type="text/javascript" src="http://pagead2.googlesyndication.com/pagead/show_ads.js"></script>
            --%>
            
            <a href="http://www.thefreesite.com/">
                <img src="http://www.thefreesite.com/free88.gif" alt="The Free Site!" />
            </a>            
        </div>        
        
    </div>
    
</asp:Content>
