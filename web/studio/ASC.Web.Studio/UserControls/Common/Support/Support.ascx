<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Support.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Support.Support" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="System.Globalization" %>


<% if (BaseCondition)
   { %>

<li class="menu-item sub-list add-block">
    <div class="category-wrapper">
        <span class="expander"></span>
        <a class="menu-item-label outer-text text-overflow support-link" href="<%= SupportFeedbackLink %>" target="_blank" title="<%= Resource.FeedbackSupport %>">
            <span class="menu-item-icon support">
                <svg class="menu-item-svg" xmlns:xlink="http://www.w3.org/1999/xlink"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusupport" xlink:href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusupport"></use></svg>
            </span>
            <span class="menu-item-label inner-text">
                <%= Resource.FeedbackSupport %>
            </span>
        </a>
    </div>
    <ul class="menu-sub-list">
        <% if (LiveChat) { %>
        <li id="liveChatSwitch" class="menu-sub-item ">
            <a class="menu-item-label outer-text text-overflow" title="<%= Resource.LiveChat %>">
                <span class="menu-item-label inner-text"><%= Resource.LiveChat %></span>
            </a>
            <span class="switch-btn"></span>
        </li>
        <% } %>
        <% if (EmailSupport) { %>
        <li class="menu-sub-item ">
            <a class="menu-item-label outer-text text-overflow" target="_blank" href="<%= SupportFeedbackLink %>" title="<%= Resource.EmailSupport %>">
                <span class="menu-item-label inner-text"><%= Resource.EmailSupport %></span>
            </a>
        </li>
        <% } %>
        <% if (RequestTraining) { %>
        <li class="menu-sub-item">
            <a class="menu-item-label outer-text text-overflow" target="_blank" href="<%= SetupInfo.RequestTraining %>" title="<%= Resource.RequestTraining %>">
                <span class="menu-item-label inner-text"><%= Resource.RequestTraining %></span>
            </a>
        </li>
        <% } %>
        <% if (ProductDemo) { %>
        <li class="menu-sub-item">
            <a class="menu-item-label outer-text text-overflow" target="_blank" href="<%= CommonLinkUtility.GetRegionalUrl(SetupInfo.DemoOrder, CultureInfo.CurrentCulture.TwoLetterISOLanguageName) %>" title="<%= Resource.ProductDemo %>">
                <span class="menu-item-label inner-text"><%= Resource.ProductDemo %></span>
            </a>
        </li>
        <% } %>
    </ul>

</li>
<% } %>
