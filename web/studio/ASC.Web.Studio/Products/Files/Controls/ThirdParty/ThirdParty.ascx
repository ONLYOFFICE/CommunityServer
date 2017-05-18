<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ThirdParty.ascx.cs" Inherits="ASC.Web.Files.Controls.ThirdParty" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Import" %>
<%@ Import Namespace="ASC.Web.Files.Import.DocuSign" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="thirdPartyAccountContainer">
    <% if (ImportConfiguration.SupportInclusion) %>
    <%
       { %>
    <a class="account-connect button middle blue">
        <%: FilesUCResource.ThirdPartyConnectAccount %>
    </a>
    <% } %>

    <div id="thirdPartyAccountList"></div>
</div>

<asp:PlaceHolder runat="server" ID="EmptyScreenThirdParty" />


<div id="thirdPartyEditor" class="popup-modal">
    <sc:Container ID="ThirdPartyEditorTemp" runat="server">
        <Header>
            <span id="thirdPartyDialogCaption"></span>
        </Header>
        <Body>
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
        </Body>
    </sc:Container>
</div>

<div id="thirdPartyDelete" class="popup-modal">
    <sc:Container runat="server" ID="ThirdPartyDeleteTmp">
        <Header>
            <%: FilesUCResource.ThirdPartyDeleteCaption %>
        </Header>
        <Body>
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
        </Body>
    </sc:Container>
</div>

<div id="thirdPartyNewAccount" class="popup-modal">
    <sc:Container runat="server" ID="ThirdPartyNewAccountTmp">
        <Header>
            <%: FilesUCResource.ThirdPartyConnectingAccount %>
        </Header>
        <Body>
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
                <span class="add-account-big add-account-button DropboxV2" data-provider="DropboxV2" title="<%= FilesUCResource.ThirdPartyDropBox %>"></span>
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
                <% if (ImportConfiguration.SupportWebDavInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button Nextcloud" data-provider="WebDav" title="<%= FilesUCResource.ThirdPartyOwnCloud %>"></span>
                <span class="add-account-big add-account-button OwnCloud" data-provider="WebDav" title="<%= FilesUCResource.ThirdPartyOwnCloud %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportYandexInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button Yandex" data-provider="Yandex" title="<%= FilesUCResource.ThirdPartyYandex %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportDocuSignInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button DocuSign" data-provider="DocuSign" title="<%= FilesUCResource.ThirdPartyDocuSign %>" data-signed="<%= (DocuSignToken.GetToken() != null).ToString().ToLower() %>"></span>
                <% } %>
                <% if (ImportConfiguration.SupportWebDavInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button WebDav" data-provider="WebDav" title="<%= FilesUCResource.ThirdPartyWebDav %>"><%= FilesUCResource.ButtonAddWebDav %></span>
                <% } %>
            </div>
        </Body>
    </sc:Container>
</div>

<div id="thirPartyConfirmMove" class="popup-modal">
    <sc:Container ID="ThirPartyConfirmMoveTmp" runat="server">
        <Header><%= FilesUCResource.ConfirmThirdPartyMove %></Header>
        <Body>
            <div id="moveThirdPartyMessage"></div>
            <div class="middle-button-container">
                <a id="buttonMoveThirdParty" class="button blue middle">
                    <%= FilesUCResource.ButtonMoveTo %>
                </a>
                <span class="splitter-buttons"></span>
                <a id="buttonCopyThirdParty" class="button gray middle">
                    <%= FilesUCResource.ButtonCopyTo %>
                </a>
                <span class="splitter-buttons"></span>
                <a id="buttonCancelMoveThirdParty" class="button gray middle">
                    <%= FilesUCResource.ButtonCancel %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>

<% if (ImportConfiguration.SupportDocuSignInclusion)
   { %>
<div id="thirdpartyToDocuSign" class="popup-modal">
    <sc:Container ID="thirdpartyToDocuSignDialog" runat="server">
        <Header>
            <div class="thirdparty-todocusign-header"></div>
        </Header>
        <Body>
            <div><%= FilesUCResource.DocuSignName %></div>
            <input type="text" class="thirdparty-todocusign-title textEdit" maxlength="<%= Global.MaxTitle %>" />

            <div class="thirdparty-todocusign-folder">
                <div><%= FilesUCResource.DocuSignFolder %></div>
                <div id="thirdpartyToDocuSignFolder">
                    <a class="link dotline"></a>
                </div>
                <div id="thirdpartyToDocuSignFolderSelector" class="studio-action-panel webkit-scrollbar">
                    <asp:PlaceHolder runat="server" ID="DocuSignFolderSelectorHolder"></asp:PlaceHolder>
                </div>
            </div>

            <div class="thirdparty-todocusign-recipients">
                <div><%= FilesUCResource.DocuSignRecipients %></div>
                <span id="thirdpartyToDocuSignUserSelector" class="addUserLink">
                    <a class="link dotline"><%= CustomNamingPeople.Substitute<FilesUCResource>("DocuSignRecipientAdd").HtmlEncode() %></a>
                    <span class="sort-down-black"></span>
                </span>
                <div id="thirdpartyToDocuSignRecipientsList"></div>
            </div>

            <div id="thirdpartyToDocuSignMessagePanel">
                <a id="thirdpartyToDocuSignAddMessage" class="baseLinkAction linkMedium"><%= FilesUCResource.DocuSignMessageAdd %></a>
                <a id="thirdpartyToDocuSignRemoveMessage" class="baseLinkAction linkMedium"><%= FilesUCResource.DocuSignMessageRemove %></a>
                <textarea id="thirdpartyToDocuSignMessage" maxlength="<%= DocuSignHelper.MaxEmailLength %>" placeholder="<%= FilesUCResource.DocuSignMessage %>"></textarea>
            </div>

            <div class="middle-button-container">
                <a id="thirdpartyToDocuSignSend" class="button blue middle">
                    <%= FilesUCResource.ButtonSendDocuSignDialog %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= FilesUCResource.ButtonCancel %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>
<% } %>

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
