<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductQuotes.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Statistics.ProductQuotes" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<div class="informationBlock clearFix">
    <div class="settings-block">
        <div class="header-base"><%= Resource.StatisticsTitle %></div>
        <div class="clearFix">
            <table>
                <tr>
                    <td class="header-base-small"><%= Resource.TenantCreated %>:</td>
                    <td><% =RenderCreatedDate() %></td>
                </tr>
                <tr>
                    <td class="header-base-small"><%= CustomNamingPeople.Substitute<Resource>("TenantUsersTotal").HtmlEncode() %>:</td>
                    <td><%= RenderUsersTotal() %></td>
                </tr>
                <% if (!CoreContext.Configuration.Standalone) { %>
                <tr>
                    <td class="header-base-small"><%= Resource.TenantStorageSpace %>:</td>
                    <td><%= RenderMaxTotalSpace() %></td>
                </tr>
                <% } %>
            </table> 
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format((CoreContext.Configuration.Standalone ? Resource.StatisticsDescriptionStandalone : Resource.StatisticsDescription).HtmlEncode(), "<br />") %></p>
    </div>
</div>

<div class="quotesBlock">
    <div class="header-base">
        <%= Resource.TenantUsedSpace %>:&nbsp;<span class="<%= RenderUsedSpaceClass() %>"><%= RenderUsedSpace() %></span>
        <span class="toggle-button" data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>"><%= Resource.Show %></span>
    </div>
    <div class="toggle-content display-none">
        <div class="tabs-header clearFix">
            <% foreach (var webItem in GetWebItems()) { %>
            <span data-id="<%= webItem.ID %>" class="tabs-header-item">
                <svg class="statistics-icons-svg"> 
                    <use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenu<%= webItem.ProductClassName %>"></use>
                </svg>
                <%= webItem.Name.HtmlEncode() %>
               <!--<img src="<%= webItem.GetIconAbsoluteURL() %>" alt="<%= webItem.Name.HtmlEncode() %>"><%= webItem.Name.HtmlEncode() %> -->
            </span>
             <% } %>
            <div class="tabs-header-corner"></div>
        </div>
        <div class="tabs-content"></div>
    </div>
</div>

<script id="statisticsItemListTmpl" type="text/x-jquery-tmpl">
    <div id="tabsContent_${id}" class="tabs-content-item active">
        {{if items.length}}
            <table class="table-list height40">
                {{each(i, item) items}}
                    <tr class="borderBase{{if i > 9}} display-none{{/if}}" >
                        <td class="icon">
                            {{if item.icon}}<img src="${item.icon}" alt="${item.name}"/>{{/if}}
                            {{if item.disabled}}<span class="disabled"></span>{{/if}}
                        </td>
                        <td class="name">
                            {{if item.url}}
                            <a href="${item.url}" class="link bold" title="${item.name}">${item.name}</a>
                            {{else}}
                            <span class="header-base-small" title="${item.name}">${item.name}</span>
                            {{/if}}
                        </td>
                        <td class="value">${item.size}</td>
                    </tr>
                {{/each}}
            </table>
            {{if items.length > 10}}
                <div class="moreBox"><%= Resource.Top10Title %><a class="link dotted gray"><%= Resource.ShowAllButton %></a></div>
            {{/if}}
        {{else}}
            <div class="empty describe-text borderBase"><%= Resource.EmptyUsageSpace %></div>
        {{/if}}
    </div>
</script>