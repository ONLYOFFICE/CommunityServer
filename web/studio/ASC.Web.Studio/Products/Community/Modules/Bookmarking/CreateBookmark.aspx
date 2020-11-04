<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateBookmark.aspx.cs" Inherits="ASC.Web.Community.Bookmarking.CreateBookmark" MasterPageFile="~/Products/Community/Master/Community.Master" %>
<%@ Import Namespace="ASC.Web.UserControls.Bookmarking.Resources" %>


<asp:Content ContentPlaceHolderID="CommunityTitleContent" runat="server">
    <div class="bookmarksHeaderBlock header-with-menu" style="margin-bottom: 16px;"> 
        <span class="main-title-icon bookmarks"></span>
        <span class="header"><%=HttpUtility.HtmlEncode(BookmarkingUCResource.AddBookmarkLink)%></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="CommunityPageContent" runat="server">
    <asp:PlaceHolder ID="BookmarkingPageContent" runat="server"/>
</asp:Content>
