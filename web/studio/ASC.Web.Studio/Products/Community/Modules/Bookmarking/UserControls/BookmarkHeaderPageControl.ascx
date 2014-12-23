<%@ Assembly Name="ASC.Web.Community.Bookmarking" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BookmarkHeaderPageControl.ascx.cs" 
    Inherits="ASC.Web.Community.Bookmarking.UserControls.BookmarkHeaderPageControl" %>
<%@ Import Namespace="ASC.Web.Community.Bookmarking.Resources" %>
<%@ Import Namespace="ASC.Web.Community.Product" %>


<div class="bookmarksHeaderBlock header-with-menu"> 
    <span class="main-title-icon bookmarks"></span>
    <span class="header"><%=Title%></span>
    <% if(!CommunitySecurity.IsOutsider()) { %>
    <a id="unSubscribeOnBookmarkComments" style="display: none" class="follow-status subscribed display-none" href="javascript:unSubscribeOnBookmarkComments()" title="<%=UnsubscribeOnBookmarkComments%>"></a>
    <a id="subscribeOnBookmarkComments" style="display: none" class="follow-status unsubscribed" href="javascript:subscribeOnBookmarkComments()" title="<%=SubscribeOnBookmarkComments%>"></a>
    <% } %>
</div>

