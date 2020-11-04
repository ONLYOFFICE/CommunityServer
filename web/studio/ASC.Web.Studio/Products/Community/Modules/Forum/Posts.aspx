<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/Forum/Forum.Master" EnableViewState="false" AutoEventWireup="true" CodeBehind="Posts.aspx.cs" Inherits="ASC.Web.Community.Forum.Posts" %>

<%@ Import Namespace="ASC.Web.Community.Product" %>


<asp:Content ContentPlaceHolderID="ForumTitleContent" runat="server">
    <div class="forumsHeaderBlock header-with-menu" style="margin-bottom: 16px;">
        <span class="main-title-icon forums"></span>
        <span class="header"><%=HttpUtility.HtmlEncode(ForumPageTitle)%></span>
        <% if(!CommunitySecurity.IsOutsider()) { %>
        <asp:Literal ID="SubscribeLinkBlock" runat="server"></asp:Literal>
        <asp:Literal ID="menuToggle" runat="server"></asp:Literal>
        <% } %>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ForumPageContent" runat="server">
    <div class="forum-page-parent">
        <%=ForumPageParentIn%>
        <a id="postParent" class="link underline gray" href="<%=ForumPageParentURL%>">
            <%=HttpUtility.HtmlEncode(ForumPageParentTitle)%>
        </a>
    </div>
    <div id="post_errorMessage"></div>
    <asp:PlaceHolder ID="postListHolder" runat="server"/>
    <div id="forumsActionsMenuPanel" class="studio-action-panel">
        <ul id="forumsActions" class="dropdown-content"></ul>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ForumPagingContent" runat="server">
    <asp:PlaceHolder ID="PagingHolder" runat="server"/>
</asp:Content>