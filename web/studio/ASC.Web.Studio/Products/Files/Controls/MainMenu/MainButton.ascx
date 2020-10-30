<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainButton.ascx.cs" Inherits="ASC.Web.Files.Controls.MainButton" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<% if (!Global.IsOutsider)
   { %>
<ul id="mainMenuHolder" class="menu-actions">
    <li id="menuCreateNewButton" class="menu-main-button without-separator disable middle" title="<%= FilesUCResource.ButtonCreate %>">
        <span class="main-button-text"><%= FilesUCResource.ButtonCreate %></span>
        <span class="white-combobox">&nbsp;</span>
    </li>
    <li id="menuUploadActionsButton" class="menu-upload-button menu-gray-button disable" title="<%= FilesUCResource.ButtonUpload %>">
        <span class="menu-upload-icon btn_other-actions">
            <svg class="upload-svg">
                <use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/documents-icons.svg?ver=<%= HttpUtility.UrlEncode(ASC.Web.Core.Client.ClientSettings.ResetCacheKey) %>#documentsIconsupload"></use>
            </svg></span>
    </li>
</ul>

<div id="uploadActions" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a id="buttonUpload" class="dropdown-item disable not-ready"><%= FilesUCResource.ButtonUploadFiles %></a></li>
        <% if (!MobileDetector.IsMobile)
           { %>
            <li><a id="buttonFolderUpload" class="dropdown-item disable not-ready"><%= FilesUCResource.ButtonUploadFolder %></a></li>
        <% } %>
    </ul>
</div>
<% } %>

<div id="newDocumentPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <asp:PlaceHolder runat="server" ID="CreateMenuHolder"></asp:PlaceHolder>
    </ul>
</div>