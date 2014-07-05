<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainMenu.ascx.cs" Inherits="ASC.Web.Files.Controls.MainMenu" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Files.Import" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<ul id="mainMenuHolder" class="menu-actions">
    <li id="menuCreateNewButton" class="menu-main-button <%= CreateButtonClass %>" title="<%= FilesUCResource.ButtonCreate %>">
        <span class="main-button-text"><%= FilesUCResource.ButtonCreate %></span>
        <span class="white-combobox">&nbsp;</span>
    </li>
    <% if (!MobileDetector.IsMobile)
       {%>
    <li id="buttonUpload" class="menu-upload-button" title="<%= FilesUCResource.ButtonUpload %>">
        <span class="menu-upload-icon">&nbsp;</span>
    </li>
    <% } %>
    <% if (EnableImport)
       {%>
    <li id="buttonThirdparty" class="menu-gray-button" title="<%= FilesUCResource.ButtonAddThirdParty %>">
        <span class="btn_other-actions">...</span>
    </li>
    <% } %>
</ul>

<asp:PlaceHolder runat="server" ID="ControlHolder"></asp:PlaceHolder>

<ul id="treeSecondary" class="menu-list">

    <li id="treeSetting" class="menu-item sub-list add-block">
        <div class="category-wrapper">
            <span class="expander"></span>
            <a href="#setting" class="menu-item-label outer-text text-overflow" title="<%= FilesUCResource.SideCaptionSettings %>">
                <span class="menu-item-icon settings"></span>
                <span class="menu-item-label inner-text"><%= FilesUCResource.SideCaptionSettings %></span>
            </a>
        </div>
        <ul class="menu-sub-list">
            <li class="menu-sub-item settings-link-common">
                <a class="menu-item-label outer-text text-overflow" href="#setting" title="<%= FilesUCResource.ThirdPartyConnectAccounts %>">
                    <span class="menu-item-label inner-text"><%= FilesUCResource.CommonSettings %></span>
                </a>
            </li>
            <% if (EnableThirdParty)
               { %>
            <li class="menu-sub-item settings-link-thirdparty">
                <a class="menu-item-label outer-text text-overflow" href="#setting=thirdparty" title="<%= FilesUCResource.ThirdPartyConnectAccounts %>">
                    <span class="menu-item-label inner-text"><%= FilesUCResource.ThirdPartyConnectAccounts %></span>
                </a>
            </li>
            <% } %>
        </ul>
    </li>

    <asp:PlaceHolder runat="server" ID="sideHelpCenter"></asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="sideSupport"></asp:PlaceHolder>
</ul>

<%-- popup window --%>
<div id="newDocumentPanel" class="studio-action-panel">
    <div class="corner-top left"></div>
    <ul class="dropdown-content">
        <asp:PlaceHolder runat="server" ID="CreateMenuHolder"></asp:PlaceHolder>
    </ul>
</div>
<div id="thirdPartyListPanel" class="studio-action-panel">
    <div class="corner-top left">
    </div>
    <ul class="dropdown-content">

        <% if (EnableImport)
           {%>
        <% if (ImportConfiguration.SupportBoxNetImport)
           { %>
        <li id="importFromBoxNet"><a class="dropdown-item">
            <%= FilesUCResource.ImportFromBoxNet %></a></li>
        <% } %>
        <% if (ImportConfiguration.SupportGoogleImport)
           { %>
        <li id="importFromGoogle"><a class="dropdown-item">
            <%= FilesUCResource.ImportFromGoogle %></a></li>
        <% } %>
        <% if (ImportConfiguration.SupportZohoImport)
           { %>
        <li id="importFromZoho"><a class="dropdown-item">
            <%= FilesUCResource.ImportFromZoho %></a></li>
        <% } %>
        <% } %>
    </ul>
</div>

<% if (EnableThirdParty)
   { %>
<div class="tree-thirdparty">
    <span class="header-base medium gray-text"><%= FilesUCResource.AddAccount %></span>

    <ul class="tree-thirdparty-list">
        <% if (ImportConfiguration.SupportBoxNetInclusion)
           { %>
        <li class="add-account-button BoxNet" data-provider="BoxNet" title="<%= FilesUCResource.ButtonAddBoxNet %>" ></li>
        <% } %>
        <% if (ImportConfiguration.SupportDropboxInclusion)
           { %>
        <li class="add-account-button DropBox" data-provider="DropBox" title="<%= FilesUCResource.ButtonAddDropBox %>" ></li>
        <% } %>
        <% if (ImportConfiguration.SupportGoogleInclusion)
           { %>
        <li class="add-account-button Google" data-provider="Google" title="<%= FilesUCResource.ButtonAddGoogle %>" ></li>
        <% } %>
        <% if (ImportConfiguration.SupportGoogleDriveInclusion)
           { %>
        <li class="add-account-button GoogleDrive" data-provider="GoogleDrive" title="<%= FilesUCResource.ButtonAddGoogle %>" ></li>
        <% } %>
        <% if (ImportConfiguration.SupportSkyDriveInclusion)
           { %>
        <li class="add-account-button SkyDrive" data-provider="SkyDrive" title="<%= FilesUCResource.ButtonAddSkyDrive %>" ></li>
        <% } %>
        <% if (ImportConfiguration.SupportSharePointInclusion)
           { %>
        <li class="add-account-button SharePoint" data-provider="SharePoint" title="<%= FilesUCResource.ButtonAddSharePoint %>" ></li>
        <% } %>
    </ul>
</div>
<% } %>
