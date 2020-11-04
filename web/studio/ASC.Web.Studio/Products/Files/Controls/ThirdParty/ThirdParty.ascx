<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ThirdParty.ascx.cs" Inherits="ASC.Web.Files.Controls.ThirdParty" %>

<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.FederatedLogin.LoginProviders" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Helpers" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="thirdPartyAccountContainer">
    <% if (ThirdpartyConfiguration.SupportInclusion) %>
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
        <header>
            <span id="thirdPartyDialogCaption"></span>
        </header>
        <body>
            <div id="thirdPartyPanel">
                <div class="thirdparty-editor-row">
                    <div><%: FilesUCResource.ThirdPartyReconnectTitle %></div>
                    <div id="thirdPartyAccount" class="edit-account-button button white"><span></span><%: FilesUCResource.ThirdPartyReconnect %></div>
                </div>

                <div class="thirdparty-editor-row">
                    <div><%: FilesUCResource.ConnectionUrl %></div>
                    <input type="url" id="thirdPartyConnectionUrl" class="textEdit" placeholder="<%: FilesUCResource.ThirdPartyCorrect %>" name="account-new-field" autocomplete="off" />
                </div>

                <div class="thirdparty-editor-row">
                    <div><%: FilesUCResource.Password %></div>
                    <input type="password" id="thirdPartyPassword" class="textEdit" placeholder="<%: FilesUCResource.ThirdPartyCorrect %>" name="account-new-field" autocomplete="new-password" />
                </div>

                <div class="thirdparty-editor-row">
                    <div><%: FilesUCResource.ThirdPartyFolderTitle %></div>
                    <input type="text" id="thirdPartyTitle" maxlength="<%= Global.MaxTitle %>" class="textEdit" />
                </div>

                <%if (Global.IsAdministrator && !CoreContext.Configuration.Personal) %>
                <% { %>
                <label id="thirdPartyLabelCorporate">
                    <input type="checkbox" id="thirdPartyCorporate" class="checkbox" /><%: FilesUCResource.ThirdPartySetCorporate %></label>
                <% } %>
            </div>
            <div class="middle-button-container">
                <a id="submitThirdParty" class="button blue middle">
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

<div id="thirdPartyDelete" class="popup-modal">
    <sc:Container runat="server" ID="ThirdPartyDeleteTmp">
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
    <sc:Container runat="server" ID="ThirdPartyNewAccountTmp">
        <header>
            <%: FilesUCResource.ThirdPartyConnectingAccount %>
        </header>
        <body>
            <%: FilesUCResource.ThirdPartyConnectAccountsDescription %>
            <% if (Global.IsAdministrator && !CoreContext.Configuration.Personal && !CoreContext.Configuration.CustomMode)
               { %>
                <%= " " + string.Format(FilesUCResource.ThirdPartyConnectAccountsKeys,
                                        "<a href=\"" + CommonLinkUtility.GetAdministration(ManagementType.ThirdPartyAuthorization) + "\">",
                                        "</a>") %>
            <% } %>
            <div class="clearFix">
                <% if (ThirdpartyConfiguration.SupportGoogleDriveInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button GoogleDrive" data-provider="GoogleDrive" title="<%= FilesUCResource.ThirdPartyGoogleDrive %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportBoxInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button Box" data-provider="Box" title="<%= FilesUCResource.ThirdPartyBoxNet %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportDropboxInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button DropboxV2" data-provider="DropboxV2" title="<%= FilesUCResource.ThirdPartyDropBox %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportSharePointInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button SharePoint" data-provider="SharePoint" title="<%= FilesUCResource.ThirdPartySharePoint %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportOneDriveInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button OneDrive" data-provider="OneDrive" title="<%= FilesUCResource.ThirdPartySkyDrive %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportSharePointInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button SharePoint SkyDrive" data-provider="SharePoint" title="<%= FilesUCResource.ThirdPartySharePointDescr %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportNextcloudInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button Nextcloud" data-provider="WebDav" title="<%= FilesUCResource.ThirdPartyNextcloud %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportOwncloudInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button OwnCloud" data-provider="WebDav" title="<%= FilesUCResource.ThirdPartyOwnCloud %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportkDriveInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button kDrive <%= Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName %>" data-provider="kDrive" title="<%= FilesUCResource.ThirdPartykDrive %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportYandexInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button Yandex <%= Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName %>" data-provider="Yandex" title="<%= FilesUCResource.ThirdPartyYandex %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportDocuSignInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button DocuSign" data-provider="DocuSign" title="<%= FilesUCResource.ThirdPartyDocuSign %>" data-signed="<%= (DocuSignToken.GetToken() != null).ToString().ToLower() %>"></span>
                <% } %>
                <% if (ThirdpartyConfiguration.SupportWebDavInclusion) %>
                <% { %>
                <span class="add-account-big add-account-button WebDav" data-provider="WebDav" title="<%= FilesUCResource.ThirdPartyWebDav %>"><%= FilesUCResource.ButtonAddWebDav %></span>
                <% } %>
            </div>
        </body>
    </sc:Container>
</div>

<div id="thirPartyConfirmMove" class="popup-modal">
    <sc:Container ID="ThirPartyConfirmMoveTmp" runat="server">
        <header><%= FilesUCResource.ConfirmThirdPartyMove %></header>
        <body>
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
        </body>
    </sc:Container>
</div>

<% if (ThirdpartyConfiguration.SupportDocuSignInclusion)
   { %>
<div id="thirdpartyToDocuSign" class="popup-modal">
    <sc:Container ID="thirdpartyToDocuSignDialog" runat="server">
        <header>
            <div class="thirdparty-todocusign-header"></div>
        </header>
        <body>
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
        </body>
    </sc:Container>
</div>
<% } %>
<% if (ThirdpartyConfiguration.ThirdPartyProviders.Contains("docusign")
        && !DocuSignLoginProvider.Instance.IsEnabled)
   { %>
<div id="thirdpartyToDocuSignHelper" class="popup-modal">
    <sc:Container ID="thirdpartyToDocuSignHelperDialog" runat="server">
        <header><%= FilesUCResource.DocuSignCaption %></header>
        <body>
            <div class="headerPanelSmall-splitter"><%= FilesUCResource.DocuSignConnect %></div>
            <div class="headerPanelSmall-splitter">
                <%= Global.IsAdministrator
                        ? string.Format(FilesUCResource.DocuSignAdmin,
                            "<a class=\"link underline\" href=\"" + CommonLinkUtility.GetAdministration(ManagementType.ThirdPartyAuthorization) + "#DocuSign\">",
                            "</a>")
                        : FilesUCResource.DocuSignUser %>
            </div>
            <% if (!string.IsNullOrEmpty(HelpLink))
               { %>
            <div class="headerPanelSmall-splitter">
                <%= string.Format(FilesUCResource.DocuSignHelp, "<a class=\"link underline\" href=\"" + HelpLink + "/guides/send-documents-for-e-signature.aspx\" target=\"_blank\">", "</a>") %>
            </div>
            <% } %>
            <div class="middle-button-container">
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= FilesUCResource.ButtonOk %>
                </a>
            </div>
        </body>
    </sc:Container>
</div>
<% } %>

<div id="thirdPartyActionPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <li id="accountEditLinkContainer">
            <a class="dropdown-item">
                <%= FilesUCResource.ButtonChangeThirdParty %>
            </a>
        </li>
        <li id="accountDeleteLinkContainer">
            <a class="dropdown-item">
                <%= FilesUCResource.ButtonDeleteThirdParty %>
            </a>
        </li>
    </ul>
</div>
