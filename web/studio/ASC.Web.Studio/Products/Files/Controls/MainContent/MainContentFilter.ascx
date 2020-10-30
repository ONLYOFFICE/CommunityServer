<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainContentFilter.ascx.cs" Inherits="ASC.Web.Files.Controls.MainContentFilter" %>

<%@ Import Namespace="ASC.ElasticSearch" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Core.Search" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>


<%-- Advansed Filter --%>
<div class="files-filter"
    data-sort="<%= FilesSettings.DefaultOrder.SortedBy %>"
    data-asc="<%= FilesSettings.DefaultOrder.IsAsc.ToString().ToLower() %>"
    data-content="<%= FactoryIndexer<FilesWrapper>.CanSearchByContent().ToString().ToLower() %>">
    <div></div>
</div>

<%-- Main Content Header --%>
<ul id="mainContentHeader" class="contentMenu">
    <li class="menuAction menuActionSelectAll">
        <div class="menuActionSelect">
            <input type="checkbox" id="filesSelectAllCheck" title="<%= FilesUCResource.MainHeaderSelectAll %>" />
        </div>
        <div class="down_arrow" title="<%= FilesUCResource.TitleSelectFile %>">
        </div>
    </li>
    <% if (!Global.IsOutsider)
        { %>
    <li id="mainShare" class="menuAction" title="<%= FilesUCResource.ButtonShareAccess %>">
        <span><%= FilesUCResource.ButtonShareAccessShort %></span>
    </li>
    <% } %>
    <li id="mainDownload" class="menuAction" title="<%= FilesUCResource.ButtonDownload %>">
        <span><%= FilesUCResource.ButtonDownload %></span>
    </li>
    <% if (0 < FileUtility.ExtsConvertible.Count)
        { %>
    <li id="mainConvert" class="menuAction" title="<%= FilesUCResource.DownloadAs %>">
        <span><%= FilesUCResource.DownloadAs %></span>
    </li>
    <% } %>
    <% if (!Global.IsOutsider)
        { %>
    <li id="mainMove" class="menuAction" title="<%= FilesUCResource.ButtonMoveTo %>">
        <span><%= FilesUCResource.ButtonMoveTo %></span>
    </li>
    <li id="mainCopy" class="menuAction" title="<%= FilesUCResource.ButtonCopyTo %>">
        <span><%= FilesUCResource.ButtonCopyTo %></span>
    </li>
    <li id="mainMarkRead" class="menuAction" title="<%= FilesUCResource.RemoveIsNew %>">
        <span><%= FilesUCResource.RemoveIsNewShort %></span>
    </li>
    <li id="mainRemoveFavorite" class="menuAction" title="<%= FilesUCResource.ButtonRemoveFavorite %>">
        <span><%= FilesUCResource.ButtonRemoveFavoriteShort %></span>
    </li>
    <li id="mainRemoveTemplate" class="menuAction" title="<%= FilesUCResource.ButtonRemoveTemplate %>">
        <span><%= FilesUCResource.ButtonRemoveTemplateShort %></span>
    </li>
    <li id="mainUnsubscribe" class="menuAction" title="<%= FilesUCResource.Unsubscribe %>">
        <span><%= FilesUCResource.Unsubscribe %></span>
    </li>
    <li id="mainRestore" class="menuAction" title="<%= FilesUCResource.ButtonRestore %>">
        <span><%= FilesUCResource.ButtonRestore %></span>
    </li>
    <li id="mainDelete" class="menuAction" title="<%= FilesUCResource.ButtonDelete %>">
        <span><%= FilesUCResource.ButtonDelete %></span>
    </li>
    <li id="mainEmptyTrash" class="menuAction" title="<%= FilesUCResource.ButtonEmptyTrash %>">
        <span><%= FilesUCResource.ButtonEmptyTrash %></span>
    </li>
    <% } %>
    <li id="switchViewFolder" class="menuSwitchViewFolder">
        <div id="switchToNormal" class="switchToNormal" title="<%= FilesUCResource.SwitchViewToNormal %>">
            &nbsp;
        </div>
        <div id="switchToCompact" class="switchToCompact" title="<%= FilesUCResource.SwitchViewToCompact %>">
            &nbsp;
        </div>
    </li>
    <li class="menu-action-on-top" title="<%= FilesUCResource.ButtonUp %>">
        <span class="on-top-link"><%= FilesUCResource.ButtonUp %></span>
    </li>
</ul>
