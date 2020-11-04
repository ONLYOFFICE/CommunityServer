<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainMenu.ascx.cs" Inherits="ASC.Web.Files.Controls.MainMenu" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Helpers" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<asp:PlaceHolder runat="server" ID="ControlHolder"></asp:PlaceHolder>

<ul id="treeSecondary" class="menu-list">
    <asp:PlaceHolder ID="InviteUserHolder" runat="server"></asp:PlaceHolder>

    <% if (!CoreContext.Configuration.Personal)
       { %>
    <% if (!Global.IsOutsider)
       { %>
    <li id="treeSetting" class="menu-item sub-list add-block">
        <div class="category-wrapper">
            <span class="expander"></span>
            <a href="#setting" class="menu-item-label outer-text text-overflow" title="<%= FilesUCResource.SideCaptionSettings %>">
                <span class="menu-item-icon settings">
                    <svg class="menu-item-svg">
                        <use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/documents-icons.svg?ver=<%= HttpUtility.UrlEncode(ASC.Web.Core.Client.ClientSettings.ResetCacheKey) %>#documentsIconssettings"></use>
                    </svg>
                </span>
                <span class="menu-item-label inner-text"><%= FilesUCResource.SideCaptionSettings %></span>
            </a>
        </div>
        <ul class="menu-sub-list">
            <li class="menu-sub-item settings-link-common">
                <a class="menu-item-label outer-text text-overflow" href="#setting" title="<%= FilesUCResource.CommonSettings %>">
                    <span class="menu-item-label inner-text"><%= FilesUCResource.CommonSettings %></span>
                </a>
            </li>
            <% if (Global.IsAdministrator)
               { %>
            <li class="menu-sub-item settings-link-admin">
                <a class="menu-item-label outer-text text-overflow" href="#setting=admin" title="<%= FilesUCResource.AdminSettings %>">
                    <span class="menu-item-label inner-text"><%= FilesUCResource.AdminSettings %></span>
                </a>
            </li>
            <% } %>
            <% if (EnableThirdParty)
               { %>
            <li class="menu-sub-item settings-link-thirdparty <%= FilesSettings.EnableThirdParty ? "" : "display-none" %>">
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

<% if (EnableThirdParty)
   { %>
<div class="tree-thirdparty <%= FilesSettings.EnableThirdParty ? "" : "display-none" %>">
    <span class="account-connect header-base medium gray-text link dotted"><%= FilesUCResource.AddAccount %></span>

<% if (ThirdpartyConfiguration.SupportGoogleDriveInclusion
       || ThirdpartyConfiguration.SupportBoxInclusion
       || ThirdpartyConfiguration.SupportDropboxInclusion
       || ThirdpartyConfiguration.SupportOneDriveInclusion
       || ThirdpartyConfiguration.SupportNextcloudInclusion
       )
   { %>
    <ul class="tree-thirdparty-list clearFix">
        <% if (ThirdpartyConfiguration.SupportGoogleDriveInclusion)
           { %>
        <li class="add-account-button GoogleDrive" data-provider="GoogleDrive" title="<%= FilesUCResource.ButtonAddGoogle %>"></li>
        <% } %>
        <% if (ThirdpartyConfiguration.SupportBoxInclusion)
           { %>
        <li class="add-account-button Box" data-provider="Box" title="<%= FilesUCResource.ButtonAddBoxNet %>"></li>
        <% } %>
        <% if (ThirdpartyConfiguration.SupportDropboxInclusion)
           { %>
        <li class="add-account-button DropboxV2" data-provider="DropboxV2" title="<%= FilesUCResource.ButtonAddDropBox %>"></li>
        <% } %>
        <% if (ThirdpartyConfiguration.SupportOneDriveInclusion)
           { %>
        <li class="add-account-button OneDrive" data-provider="OneDrive" title="<%= FilesUCResource.ButtonAddSkyDrive %>"></li>
        <% } %>
        <% if (ThirdpartyConfiguration.SupportNextcloudInclusion)
           { %>
        <li class="add-account-button Nextcloud" data-provider="WebDav" title="<%= FilesUCResource.ButtonAddNextcloud %>"></li>
        <% } %>
        <% if (ThirdpartyConfiguration.ThirdPartyProviders.Count() > 1)
           { %>
        <li class="account-connect add-account-button WebDav" title="<%= FilesUCResource.AddAccount %>">...</li>
        <% } %>
    </ul>
    <% } %>
</div>
<% } %>
