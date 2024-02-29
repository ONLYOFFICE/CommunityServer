<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContentList.ascx.cs" Inherits="ASC.Web.Files.Controls.ContentList" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<%-- Link To Parent --%>
<div id="filesBreadCrumbs">
    <a class="to-parent-folder">
        <span class="link up">&nbsp;</span>
    </a>
    <a id="linkCurrentFolder"></a>
    <span class="to-parent-folder-dropdown">
        <span class="to-parent-folder-link"><span class="inner-ellipsis"></span></span>
        <input id="promptRenameParentFolder" type="text" maxlength="170" class="textEdit input-rename">
        <input type="hidden" name="entry_data" data-entryType="folder" data-parent_folder_id="" data-deny_download="" data-deny_sharing="" data-access="" data-shared="" data-id="" data-create_by_id="" data-title="" data-folder_id="" data-provider_key="" data-provider_id=""/>
    </span>
    <span class="search-bread-crumbs gray-text"><%= FilesUCResource.TitleSearchIn %></span>
    <a id="searchBreadCrumbs" class="search-bread-crumbs files-clear-filter link"></a>
    <span id="searchBreadCrumbsSub" class="search-bread-crumbs gray-text">
        <%= string.Format(FilesUCResource.TitleSearchSubfolder,
        ("<a id=\"filesWithoutSubfolders\" class=\"link baseLinkAction\" title=\"" + FilesUCResource.TitleSearchWithoutSubfolder + "\" >"),
        "</a>") %>
    </span>
</div>

<%-- Main Content --%>
<div id="mainContent">
    <% if (AddFilterContainer) { %>
    <div class="files-filter"><div class="files-folder-filter"></div></div>
    <% } %>
    <ul id="filesMainContent" class="user-select-none"></ul>
    <div id="pageNavigatorHolder">
        <a class="button blue gray"><%= FilesUCResource.ShowMore %></a>
    </div>
    <div id="emptyContainer">
        <asp:PlaceHolder runat="server" ID="EmptyScreenFolder"></asp:PlaceHolder>
    </div>
</div>

<%--tooltip--%>
<div id="entryTooltip" class="studio-action-panel"></div>
