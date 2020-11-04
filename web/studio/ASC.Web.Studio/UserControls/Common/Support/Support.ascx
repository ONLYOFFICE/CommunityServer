<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Support.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Support.Support" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<% if (!String.IsNullOrEmpty(SupportFeedbackLink))
   { %>
<li class="menu-item none-sub-list support add-block">
    <div class="category-wrapper">
        <a class="menu-item-label outer-text text-overflow support-link" href="<%= SupportFeedbackLink %>" target="_blank" title="<%= Resource.FeedbackSupport %>">
            <span class="menu-item-icon support">
                <svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusupport"></use></svg>
            </span>
            <span class="menu-item-label inner-text">
                <%= Resource.FeedbackSupport %>
            </span>
        </a>
    </div>
</li>
<% } %>