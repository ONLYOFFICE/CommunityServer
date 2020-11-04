<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserBookmarks.aspx.cs" Inherits="ASC.Web.Community.Bookmarking.UserBookmarks" MasterPageFile="~/Products/Community/Master/Community.Master" %>


<asp:Content ContentPlaceHolderID="CommunityTitleContent" runat="server">
    <div class="bookmarksHeaderBlock header-with-menu" style="margin-bottom: 16px;">
        <span class="main-title-icon bookmarks"></span>
        <span class="header"><%=HttpUtility.HtmlEncode(PageTitle)%></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="CommunityPageContent" runat="server">
    <asp:PlaceHolder ID="BookmarkingPageContent" runat="server"/>
</asp:Content>
