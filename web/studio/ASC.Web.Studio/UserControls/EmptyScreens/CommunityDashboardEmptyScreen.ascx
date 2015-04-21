<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommunityDashboardEmptyScreen.ascx.cs" Inherits="ASC.Web.Studio.UserControls.EmptyScreens.CommunityDashboardEmptyScreen" %>
<%@ Import Namespace="ASC.Web.Community.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<% if (!UseStaticPosition)
   { %>
    <div class="backdrop" blank-page=""></div>
<% } %>

<div id="content" blank-page="" class="dashboard-center-box community <%= UseStaticPosition ? "static" : "" %>">
    <div class="header">
        <% if (!UseStaticPosition)
           { %>
            <a href="<%= ProductStartUrl %>">
                <span class="close"></span>
            </a>
        <% } %>
        <%= CommunityResource.DashboardTitle %>
    </div>
    <div class="content clearFix">
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
        <div class="module-block">
            <div class="img helpcenter"></div>
            <div class="title"><%= CommunityResource.HelpModuleTitle %></div>
            <ul>
                <li><%= CommunityResource.HelpModuleFirstLine %></li>
                <li><%= CommunityResource.HelpModuleSecondLine %></li>
            </ul>
            <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
               { %>
                <a href="<%= CommonLinkUtility.GetHelpLink() %>" target="_blank" class="link underline">
                    <%= CommunityResource.HelpModuleLink %>
                </a>
            <% } %>
        </div>
    </div>
</div>