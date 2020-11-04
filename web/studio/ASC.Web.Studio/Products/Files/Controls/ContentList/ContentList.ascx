<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContentList.ascx.cs" Inherits="ASC.Web.Files.Controls.ContentList" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<%-- Link To Parent --%>
<div id="filesBreadCrumbs">
    <a class="to-parent-folder">
        <span class="link up link-blank">&nbsp;</span>
        <span class="link up link-dots">...</span>
    </a>
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
