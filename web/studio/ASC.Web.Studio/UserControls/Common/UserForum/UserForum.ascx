<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserForum.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.UserForum.UserForum" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<% if(!String.IsNullOrEmpty(UserForumLink)) { %>
<li class="menu-item none-sub-list userforum add-block">
    <div class="category-wrapper">
        <a class="menu-item-label outer-text text-overflow" href="<%= UserForumLink %>" target="_blank" title="<%= Resource.UserForum%>">
            <span class="menu-item-icon userforum"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenuTalk"></use></svg></span>
            <span class="menu-item-label inner-text">
                <%= Resource.UserForum%>
            </span>
        </a>
    </div>
</li>
<% } %>