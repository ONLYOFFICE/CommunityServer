<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BookmarkHeaderPageControl.ascx.cs" 
    Inherits="ASC.Web.Community.Bookmarking.UserControls.BookmarkHeaderPageControl" %>

<%@ Import Namespace="ASC.Web.Community.Product" %>
<%@ Import Namespace="ASC.Web.UserControls.Bookmarking.Resources" %>


<div class="bookmarksHeaderBlock header-with-menu" style="margin-bottom: 16px;">
    <span class="main-title-icon bookmarks"></span>
    <span class="header"><%=Title%></span>
    <% if(!CommunitySecurity.IsOutsider()) { %>
    <a id="unSubscribeOnBookmarkComments" style="display: none" class="follow-status subscribed display-none" href="javascript:unSubscribeOnBookmarkComments()" title="<%=UnsubscribeOnBookmarkComments%>"></a>
    <a id="subscribeOnBookmarkComments" style="display: none" class="follow-status unsubscribed" href="javascript:subscribeOnBookmarkComments()" title="<%=SubscribeOnBookmarkComments%>"></a>
    <% } %>
    <% if(IsAdmin || IsAuthor) {%>
        <a id="deleteBookmark" class="menu-small"></a>
    <%}%>
</div>

<% if(IsAdmin || IsAuthor) {%>
<div id="bookmarkActions" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a id="deleteBookmarkAction" class="dropdown-item" href="javascript:removeBookmark(<%=BookmarkID %>)"><%= BookmarkingUCResource.RemoveButton %></a></li>
    </ul>
</div>
<%}%>

