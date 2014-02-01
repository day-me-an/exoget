<%@ Page Language="C#" MasterPageFile="~/HeaderAndFooter.master" AutoEventWireup="true" CodeFile="UserProfile.aspx.cs" Inherits="UserProfile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="content" runat="Server">

    <div class="box singleBox userProfile">
    <%if (User != null)
      {%>
        <div class="top">
            <strong class="section"><asp:Literal runat="server" ID="sectionLiteral" /></strong>
            <h1><%=User.Username%></h1>
        </div>
        <%} %>
        
        <asp:PlaceHolder ID="tipsPlaceHolder" runat="server" />

        <asp:Panel runat="server" ID="content">
                <ul class="menu">
                    <li<%if(CurrentView == "History"){%> class="current"<%}%>><a href="/user/<%=User.Username%>/history">History</a></li>
                    <li<%if(CurrentView == "Favorite"){%> class="current"<%}%>><a href="/user/<%=User.Username%>/favs">Favorites</a></li>
                    <li<%if(CurrentView == "Rated"){%> class="current"<%}%>><a href="/user/<%=User.Username%>/rated">Rated</a></li>
                </ul>

                <div class="content">
                    <asp:PlaceHolder runat="server" ID="mediaResultHolder" />
                </div>
        </asp:Panel>
    </div>
    
</asp:Content>

