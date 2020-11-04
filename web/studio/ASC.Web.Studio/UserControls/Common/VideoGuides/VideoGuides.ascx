<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VideoGuides.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.VideoGuides.VideoGuidesControl" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<% if (!DisableVideo)
    { %>

<% if (IsSubItem)
    { %>

<li class="menu-sub-item video-guides">
    <a class="menu-item-label outer-text text-overflow" href="<%= AllVideoLink %>" target="_blank" title="<%= Resource.VideoGuides %>">
        <%= Resource.VideoGuides %>
    </a>
    <span class="new-label-menu" title="<%= Resource.VideoShowUnwatchedVideo %>"></span>
</li>

<% }
   else
   { %>

<li class="menu-item none-sub-list video-guides add-block">
    <div class="category-wrapper">
        <a class="menu-item-label outer-text text-overflow video-link" href="<%= AllVideoLink %>" target="_blank" title="<%= Resource.VideoGuides %>">
            <span class="menu-item-icon video-guides"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenuvideo-guides"></use></svg></span>
            <span class="menu-item-label inner-text">
                <%= Resource.VideoGuides %>
            </span>
        </a>
        <span class="new-label-menu" title="<%= Resource.VideoShowUnwatchedVideo %>"></span>
    </div>
</li>

<% } %>

<div id="studio_videoPopupPanel" class="video-popup-panel studio-action-panel">
    <div id="dropVideoList" class="drop-list">
        <ul class="video-list">
            <% foreach (var data in VideoGuideItems)
                { %>
            <li>
                <a class="link" id="<%= data.Id %>" href="<%= data.Link %>" target="_blank"><%= data.Title %></a>
            </li>
            <% } %>
        </ul>
    </div>
    <a class="video-link" href="<%= AllVideoLink %>">
        <%= Resource.SeeAllVideos %>
    </a>
    <a id="markVideoRead" class="video-link" href="javascript:void(0);">
        <%= Resource.MarkAllAsRead %>
    </a>
</div>
<% } %>
