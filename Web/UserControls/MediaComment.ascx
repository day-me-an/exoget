<%@ Control Language="C#" CodeFile="MediaComment.ascx.cs" Inherits="Exo.Exoget.Web.Controls.MediaComment" %>

<li id="userMediaComment_<%=Comment.Id%>" class="comment">

    <div class="header">
    
        <div class="title">
            <strong><%=HttpUtility.HtmlEncode(Comment.Title)%></strong>
            <%if (Comment.Rating > 0)
              { %>
            <div class="star-rating">
                <ul>
                    <li class="current-rating" style="width:<%=Comment.RatingPercentage%>%"></li>
                </ul>
            </div>
            <% }%>
        </div>
        
        <div class="info">
            <span>By</span>#s#
            <a href="/user/<%=Comment.User.Username%>/history" class="username">
                <strong><%=Comment.User.Username%></strong>
            </a>#s#
            
            <em><%=Comment.ModifiedTimeDifferenceDescription%>, <a name="userMediaComment_<%=Comment.Id%>" href="#userMediaComment_<%=Comment.Id%>"><%=String.Format("{0:R}", Comment.Modified)%></a></em>
        </div>            
        
    </div>
    
    <p><%=HttpUtility.HtmlEncode(Comment.Body)%></p>
    
    <div class="actions">
        <a href="#">Reply</a>
        <ul class="rating">
            <li><strong class="approve"><%=Comment.Approve%></strong><span>up</span>,</li>#s#
            <li><strong class="disapprove"><%=Comment.Disapprove%></strong><span>down</span></li>
        </ul>
    </div>
    
    <asp:PlaceHolder runat="server" ID="RepliesHolder" />
    
</li>

