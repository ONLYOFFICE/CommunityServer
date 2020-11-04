<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FeedList.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Feed.FeedList" %>
<%@ Import Namespace="Resources" %>

<div id="feed-list-box">
    <table id="feed-table">
        <tr>
            <td><div id="feed-list"></div></td>
        </tr>
    </table>
    
    <div id="show-next-feeds-btn"><%= UserControlsCommonResource.ShowNextNews %></div>
    <div id="show-next-feeds-loader"></div>

    <div id="empty-feed-list-ctrl" class="noContent"></div>
    <div id="empty-feed-filter-ctrl" class="noContent"></div>
</div>

<div id="hintPanel" class="studio-action-panel">
    <div class="feed-params-hint">
        <span class="feed-hint-responsible"><%= FeedResource.Responsible %>:</span>
        <span class="feed-hint-contact"><%= FeedResource.Contact %>:</span>
        <span class="feed-hint-members"><%= FeedResource.Members %>:</span>
        <span class="feed-hint-manager"><%= FeedResource.ProjectManager %>:</span>
        <span class="feed-hint-deadline"><%= FeedResource.Deadline %>:</span>
        <span class="feed-hint-responsibles"><%= FeedResource.Responsibles %>:</span>     
        <span class="feed-hint-size"><%= FeedResource.Size %>:</span>
    </div>
    <div class="feed-values-hint">
        <span class="feed-hint-responsible"></span>
        <span class="feed-hint-contact"></span>
        <span class="feed-hint-members"></span>
        <span class="feed-hint-manager"></span>
        <span class="feed-hint-deadline"></span>
        <span class="feed-hint-responsibles"></span>
        <span class="feed-hint-size"></span>
    </div>
</div>