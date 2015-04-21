<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainMenu.ascx.cs" Inherits="ASC.Web.Files.Controls.MainMenu" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Import" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<% if(!Global.IsOutsider) { %>
<ul id="mainMenuHolder" class="menu-actions">
    <li id="menuCreateNewButton" class="menu-main-button disable <%= CreateButtonClass %>" title="<%= FilesUCResource.ButtonCreate %>">
        <span class="main-button-text"><%= FilesUCResource.ButtonCreate %></span>
        <span class="white-combobox">&nbsp;</span>
    </li>
    <% if (!MobileDetector.IsMobile)
       {%>
    <li id="buttonUpload" class="menu-upload-button disable" title="<%= FilesUCResource.ButtonUpload %>">
        <span class="menu-upload-icon">&nbsp;</span>
    </li>
    <% } %>
</ul>
<% } %>

<asp:PlaceHolder runat="server" ID="ControlHolder"></asp:PlaceHolder>

<ul id="treeSecondary" class="menu-list">
    <asp:PlaceHolder ID="InviteUserHolder" runat="server"></asp:PlaceHolder>

    <% if (!CoreContext.Configuration.Personal)
       { %>
    <% if(!Global.IsOutsider) { %>
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
    <% } %>

    <asp:PlaceHolder runat="server" ID="sideHelpCenter"></asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="sideSupport"></asp:PlaceHolder>
    <asp:PlaceHolder ID="UserForumHolder" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="VideoGuides" runat="server"></asp:PlaceHolder>
    <% } %>
</ul>

<%-- popup window --%>
<div id="newDocumentPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <asp:PlaceHolder runat="server" ID="CreateMenuHolder"></asp:PlaceHolder>
    </ul>
</div>

<% if (EnableThirdParty)
   { %>
<div class="tree-thirdparty">
    <span class="account-connect header-base medium gray-text link dotted"><%= FilesUCResource.AddAccount %></span>

    <ul class="tree-thirdparty-list clearFix">
        <% if (ImportConfiguration.SupportGoogleDriveInclusion)
           { %>
        <li class="add-account-button GoogleDrive" data-provider="GoogleDrive" title="<%= FilesUCResource.ButtonAddGoogle %>" ></li>
        <% } %>
        <% if (ImportConfiguration.SupportBoxNetInclusion)
           { %>
        <li class="add-account-button BoxNet" data-provider="BoxNet" title="<%= FilesUCResource.ButtonAddBoxNet %>" ></li>
        <% } %>
        <% if (ImportConfiguration.SupportDropboxInclusion)
           { %>
        <li class="add-account-button DropBox" data-provider="DropBox" title="<%= FilesUCResource.ButtonAddDropBox %>" ></li>
        <% } %>
        <% if (ImportConfiguration.SupportOneDriveInclusion)
           { %>
        <li class="add-account-button SkyDrive" data-provider="SkyDrive" title="<%= FilesUCResource.ButtonAddSkyDrive %>" ></li>
        <% } %>
        <% if (ImportConfiguration.SupportWebDavInclusion)
           { %>
        <li class="add-account-button OwnCloud" data-provider="WebDav" title="<%= FilesUCResource.ButtonAddOwnCloud %>" ></li>
        <% } %>
        <% if (ImportConfiguration.SupportWebDavInclusion)
           { %>
        <li class="account-connect add-account-button WebDav" title="<%= FilesUCResource.AddAccount %>" ></li>
        <% } %>
    </ul>
</div>
<% } %>
