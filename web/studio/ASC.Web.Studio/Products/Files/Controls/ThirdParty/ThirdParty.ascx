<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ThirdParty.ascx.cs" Inherits="ASC.Web.Files.Controls.ThirdParty" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Import" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="thirdPartyAccountContainer">
    <% if (ImportConfiguration.SupportInclusion)%>
    <% { %>
    <a class="account-connect button middle blue">
        <%: FilesUCResource.ThirdPartyConnectAccount %>
    </a>
    <% } %>

    <div id="thirdPartyAccountList"></div>
</div>

<asp:PlaceHolder runat="server" ID="EmptyScreenThirdParty" />


<div id="thirdPartyEditor" class="popup-modal">
    <sc:Container id="ThirdPartyEditorTemp" runat="server">
        <header>
            <span id="thirdPartyDialogCaption"></span>
        </header>
        <body>
            <div id="thirdPartyPanel">
                <div><%: FilesUCResource.ThirdPartyFolderTitle %></div>
                <input type="text" id="thirdPartyTitle" maxlength="<%=Global.MaxTitle%>" class="textEdit" />

                <%if (Global.IsAdministrator && !CoreContext.Configuration.Personal) %>
                <% { %>
                <label id="thirdPartyLabelCorporate">
                    <input type="checkbox" id="thirdPartyCorporate" class="checkbox" /><%: FilesUCResource.ThirdPartySetCorporate %></label>
                <% } %>
            </div>
            <div class="middle-button-container">
                <a id="submitThirdParty" class="button blue middle">
                    <%=FilesUCResource.ButtonOk%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();return false;">
                    <%=FilesUCResource.ButtonCancel%>
                </a>
            </div>
        </body>
    </sc:Container>
</div>

<div id="thirdPartyDelete" class="popup-modal">
    <sc:Container runat="server" id="ThirdPartyDeleteTmp">
        <header>
            <%: FilesUCResource.ThirdPartyDeleteCaption %>
        </header>
        <body>
            <div id="thirdPartyDeleteDescr"></div>
            <div class="middle-button-container">
                <a id="deleteThirdParty" class="button blue middle">
                    <%= FilesUCResource.ButtonOk %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();return false;">
                    <%= FilesUCResource.ButtonCancel %>
                </a>
            </div>
        </body>
    </sc:Container>
</div>

<div id="thirdPartyNewAccount" class="popup-modal">
    <sc:Container runat="server" id="ThirdPartyNewAccountTmp">
        <header>
            <%: FilesUCResource.ThirdPartyConnectingAccount %>
        </header>
        <body>
            <%: FilesUCResource.ThirdPartyConnectAccountsDescription %>
            <div class="clearFix">
                <% if (ImportConfiguration.SupportGoogleDriveInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button GoogleDrive" data-provider="GoogleDrive" title="<%= FilesUCResource.ThirdPartyGoogleDrive %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportBoxInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button Box" data-provider="Box" title="<%= FilesUCResource.ThirdPartyBoxNet %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportDropboxInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button DropBox" data-provider="DropBox" title="<%= FilesUCResource.ThirdPartyDropBox %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportSharePointInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button SharePoint" data-provider="SharePoint" title="<%= FilesUCResource.ThirdPartySharePoint %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportOneDriveInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button SkyDrive" data-provider="SkyDrive" title="<%= FilesUCResource.ThirdPartySkyDrive %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportSharePointInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button SharePoint SkyDrive" data-provider="SharePoint" title="<%= FilesUCResource.ThirdPartySharePointDescr %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportYandexInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button Yandex" data-provider="Yandex" title="<%= FilesUCResource.ThirdPartyYandex %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportWebDavInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button OwnCloud" data-provider="WebDav" title="<%= FilesUCResource.ThirdPartyOwnCloud %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportWebDavInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button WebDav" data-provider="WebDav" title="<%= FilesUCResource.ThirdPartyWebDav %>"><%= FilesUCResource.ButtonAddWebDav %></span>
                <% } %>
            </div>
        </body>
    </sc:Container>
</div>

<div id="thirdPartyActionPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <li id="accountEditLinkContainer">
            <a class="dropdown-item">
                <%= FilesUCResource.ButtonEdit %>
            </a>
        </li>
        <li id="accountDeleteLinkContainer">
            <a class="dropdown-item">
                <%= FilesUCResource.ButtonDelete %>
            </a>
        </li>
    </ul>
</div>
