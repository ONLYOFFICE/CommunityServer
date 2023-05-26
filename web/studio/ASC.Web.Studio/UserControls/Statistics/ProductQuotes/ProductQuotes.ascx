<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductQuotes.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Statistics.ProductQuotes" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
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
<% if (LastUpdate != null)
    { %>
<div class="quotaSettingBlock clearFix">
    <div class="settings-block">
        <div class="header-base"><%=Resource.QuotaSettingsHeader %></div>
        <div class="clearFix">
            <div class="clearFix">
                <input id="quota-enabled" value="quota-enabled" type="radio" <% if (EnableUserQuota)
                    { %>checked="checked"
                    <% } %> name="quota">
                <label for="quota-enabled"><%=Resource.QuotaSettingsWithQuota %></label>
            </div>
            <div class="clearFix">
                <input id="quota-disabled" value="quota-disabled" type="radio" <% if (!EnableUserQuota)
                    { %>checked="checked"
                    <% } %> name="quota">
                <label for="quota-disabled"><%=Resource.QuotaSettingsNoQuota %></label>
            </div>
        </div>
        <div id="setQuotaForm" class="set-quota clearFix <% if (!EnableUserQuota)
            { %> display-none <% } %>">
            <input class="textEdit" value="<% if (EnableUserQuota) %> <%=DefaultUserQuota.Split(' ')[0]%> <% if (!EnableUserQuota) %> <%=0%>" />
            <div class="sizes">
                <div id="editQuotaVal" class="val" data="<%= DefaultUserQuota.Split(' ')[1] %>"></div>
            </div>
        </div>
        <div class="middle-button-container">
            <a id="saveQuota" class="button blue disable"><%=Resource.SaveButton %></a>
            <a id="recalculateQuota" class="button gray"><%=Resource.RecalculateButton %></a>
        </div>

        <p><%=Resource.UpdatingStatistics %></p>
        <% if (LastUpdate != null)
            { %>
        <p class="lastUpdate"><%=Resource.LastUpdate %> <%=LastUpdate%></p>
        <%} %>
    </div>
    <div class="settings-help-block">
        <p><%=Resource.QuotaSettingsDescription %></p>
    </div>

</div>
<div id="editQuotaMenu" class="studio-action-panel"></div>

<div class="memoryQuotaBlock clearFix">
    <div id="filterContainer">
        <div id="peopleFilter"></div>
    </div>
    <ul id="peopleHeaderMenu" class="clearFix contentMenu contentMenuDisplayAll <% if (!EnableUserQuota)
        { %> no-quota <% } %>">
        <li class="menuAction menuActionSelectAll menuActionSelectLonely">
            <div class="menuActionSelect">
                <input id="mainSelectAll" type="checkbox" title="<%=Resource.SelectAll %>" onclick="ASC.UserQuotaController.selectAll(this);" />
            </div>
        </li>
        <li class="editUsersQuotaForm display-none">
            <div class="set-user-quota-form set-quota">
                <input class="textEdit" />
                <div class="sizes">
                    <div class="edit-quota-val val"></div>
                </div>
                <div class="save-btn">
                    <span class="mark"></span>
                </div>
                <div class="close-btn">
                    <span class="mark"></span>
                </div>
            </div>
        </li>
        <li class="menuAction menuEditQuota">
            <span><%=Resource.QuotaSettingsEditQuota %></span>
        </li>
        <li class="menuAction menuNoQuota">
            <span><%=Resource.QuotaSettingsNoQuota %></span>
        </li>
        <li class="menu-action-checked-count">
            <span></span>
            <a id="mainDeselectAll" class="link dotline small">
                <%=Resource.QuotaSettingsDeselectAll %>
            </a>
        </li>
    </ul>
    <div id="peopleContent" class="people-content">
        <div class="content-list_scrollable webkit-scrollbar">
            <table id="peopleData" class="table-list height48" cellpadding="7" cellspacing="0">
                <tbody>
                </tbody>
            </table>
        </div>
    </div>
    <table id="tableForPeopleNavigation" cellpadding="0" cellspacing="0" border="0" class="people-content display-none">
        <tbody>
            <tr>
                <td>
                    <div id="peoplePageNavigator"></div>
                </td>
                <td style="text-align: right;">
                    <span class="gray-text"><%=Resource.QuotaSettingsTotalCount %>:&nbsp;</span>
                    <span class="gray-text" id="totalUsers"></span>
                    <span class="page-nav-info">
                        <span class="gray-text"><%=Resource.QuotaSettingsShowOnPage %>:&nbsp;</span>
                        <select id="countOfRows" class="top-align">
                            <option value="25">25</option>
                            <option value="50">50</option>
                            <option value="75">75</option>
                            <option value="100">100</option>
                        </select>
                    </span>
                </td>
            </tr>
        </tbody>
    </table>
</div>
<%}
else
{ %>
<div class="recalculate clearFix">
    <div class="recalculateQuotaBlock">
        <span class="inner">
            <span class="title header-base big"><%= Resource.RecalculateQuotaTitle %></span>
            <span class="description"><%= Resource.RecalculateQuotaDescription %></span>
            <br />
            <span class="description"><%=Resource.UpdatingStatistics %></span>

            <a id="initRecalculateQuota" class="button blue"><%=Resource.RecalculateButton %></a>

        </span>

    </div>
</div>

<%}%>
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