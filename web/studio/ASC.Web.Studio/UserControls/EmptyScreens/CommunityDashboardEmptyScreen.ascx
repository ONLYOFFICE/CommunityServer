<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommunityDashboardEmptyScreen.ascx.cs" Inherits="ASC.Web.Studio.UserControls.EmptyScreens.CommunityDashboardEmptyScreen" %>
<%@ Import Namespace="ASC.Web.Community.Resources" %>

<% if (!UseStaticPosition)
   { %>
    <div class="backdrop" blank-page=""></div>
<% } %>

<div id="content" blank-page="" class="dashboard-center-box community <%= UseStaticPosition ? "static" : "" %>">
    <div class="header">
        <% if (!UseStaticPosition)
           { %>
            <a href="<%= ProductStartUrl %>">
                <span class="close">&times;</span>
            </a>
        <% } %>
        <%= CommunityResource.DashboardTitle %>
    </div>
    <div class="content clearFix">

        <% if (IsBlogsAvailable) %>
        <% { %>

        <div class="module-block">
            <div class="img blogs"></div>
            <div class="title"><%= CommunityResource.BlogsModuleTitle %></div>
            <ul>
                <li><%= CommunityResource.BlogsModuleFirstLine %></li>
                <li><%= CommunityResource.BlogsModuleSecondLine %></li>
            </ul>
            <a href="<%= VirtualPathUtility.ToAbsolute("~/products/community/modules/blogs/addblog.aspx") %>" class="link underline">
                <%= CommunityResource.BlogsModuleLink1 %>
            </a>
        </div>
        <% } %>
        <% if (IsEventsAvailable) %>
        <% { %>
        <div class="module-block">
            <div class="img events"></div>
            <div class="title"><%= CommunityResource.EventsModuleTitle %></div>
            <ul>
                <li><%= CommunityResource.EventsModuleFirstLine %></li>
                <li><%= CommunityResource.EventsModuleSecondLine %></li>
            </ul>
            <a href="<%= VirtualPathUtility.ToAbsolute("~/products/community/modules/news/editnews.aspx") %>" class="link underline">
                <%= CommunityResource.EventsModuleLink %>
            </a>
        </div>
        <% } %>
        <% if (IsBookmarksAvailable) %>
        <% { %>
        <div class="module-block wiki">
            <div class="img bookmarks"></div>
            <div class="title"><%= CommunityResource.WikiModuleTitle %></div>
            <ul>
                <li><%= CommunityResource.WikiModuleFirstLine %></li>
                <li><%= CommunityResource.WikiModuleSecondLine %></li>
            </ul>
            <a href="<%= VirtualPathUtility.ToAbsolute("~/products/community/modules/bookmarking/createbookmark.aspx") %>" class="link underline">
                <%= CommunityResource.WikiModuleLink2 %>
            </a>
        </div>
        <% } %>
        <% if (!string.IsNullOrEmpty(HelpLink)) %>
        <% { %>
        <div class="module-block">
            <div class="img helpcenter"></div>
            <div class="title"><%= CommunityResource.HelpModuleTitle %></div>
            <ul>
                <li><%= CommunityResource.HelpModuleFirstLine %></li>
                <li><%= CommunityResource.HelpModuleSecondLine %></li>
            </ul>
            <a href="<%= HelpLink %>" target="_blank" class="link underline">
                <%= CommunityResource.HelpModuleLink %>
            </a>
        </div>
        <% } %>
    </div>
</div>