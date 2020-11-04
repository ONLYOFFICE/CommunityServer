<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/Forum/Forum.Master" EnableViewState="false" AutoEventWireup="true" CodeBehind="Topics.aspx.cs" Inherits="ASC.Web.Community.Forum.Topics" Title="Untitled Page" %>

<%@ Import Namespace="ASC.Web.Community.Product" %>


<asp:Content ContentPlaceHolderID="ForumTitleContent" runat="server">
    <div class="forumsHeaderBlock header-with-menu" style="margin-bottom: 16px;">
        <span class="main-title-icon forums"></span>
        <span class="header"><%=HttpUtility.HtmlEncode(ForumTitle)%></span>
        <% if(!CommunitySecurity.IsOutsider()) { %>
        <asp:Literal ID="SubscribeLinkBlock" runat="server"></asp:Literal>
        <span class="menu-small" style="display: <%=(EnableDelete? "inline-block":"none") %>"></span>
        <% } %>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ForumPageContent" runat="server">
    <asp:PlaceHolder ID="topicsHolder" runat="server"/>
    <div id="errorMessageTopic"></div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ForumPagingContent" runat="server">
    <asp:PlaceHolder ID="PagingHolder" runat="server"/>
</asp:Content>
