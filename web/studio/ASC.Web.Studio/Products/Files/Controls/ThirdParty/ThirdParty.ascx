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
    <a id="thirdPartyConnectAccount" class="button middle blue">
        <%= FilesUCResource.ThirdPartyConnectAccount %>
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
                <div><%=FilesUCResource.ThirdPartyFolderTitle%></div>
                <input type="text" id="thirdPartyTitle" maxlength="<%=Global.MaxTitle%>" class="textEdit" />

                <%if (Global.IsAdministrator && !CoreContext.Configuration.Personal) %>
                <% { %>
                <label id="thirdPartyLabelCorporate">
                    <input type="checkbox" id="thirdPartyCorporate" class="checkbox" /><%= FilesUCResource.ThirdPartySetCorporate %></label>
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
            <%=FilesUCResource.ThirdPartyDeleteCaption%>
        </header>
        <body>
            <div id="thirdPartyDeleteDescr"></div>
            <div class="middle-button-container">
                <a id="deleteThirdParty" class="button blue middle">
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

<div id="thirdPartyNewAccount" class="popup-modal">
    <sc:Container runat="server" id="ThirdPartyNewAccountTmp">
        <header>
            <%=FilesUCResource.ThirdPartyConnectingAccount%>
        </header>
        <body>
            <div>
                <%=FilesUCResource.ThirdPartyConnectAccountsDescription%>
            </div>
            <table cellpadding="0" cellspacing="0" width="100%">
                <tbody>
                    <tr>
                        <% if (ImportConfiguration.SupportBoxNetInclusion) %>
                        <% { %>
                        <td align="center">
                            <div class="account-popup-icon BoxNetBig"></div>
                            <a class="button middle blue add-account-button BoxNet" data-provider="BoxNet">
                                <%= FilesUCResource.ThirdPartyBoxNet %>
                            </a>
                        </td>
                        <% } %>
                        <% if (ImportConfiguration.SupportDropboxInclusion) %>
                        <% { %>
                        <td align="center">
                            <div class="account-popup-icon DropBoxBig"></div>
                            <a class="button middle blue add-account-button DropBox" data-provider="DropBox">
                                <%= FilesUCResource.ThirdPartyDropBox %>
                            </a>
                        </td>
                        <% } %>
                        <% if (ImportConfiguration.SupportGoogleInclusion) %>
                        <% { %>
                        <td align="center">
                            <div class="account-popup-icon GoogleBig"></div>
                            <a class="button middle blue add-account-button Google" data-provider="Google">
                                <%= FilesUCResource.ThirdPartyGoogleDrive %>
                            </a>
                        </td>
                        <% } %>
                        <% if (ImportConfiguration.SupportGoogleDriveInclusion) %>
                        <% { %>
                        <td align="center">
                            <div class="account-popup-icon GoogleBig"></div>
                            <a class="button middle blue add-account-button GoogleDrive" data-provider="GoogleDrive">
                                <%= FilesUCResource.ThirdPartyGoogleDrive %>
                            </a>
                        </td>
                        <% } %>
                        <% if (ImportConfiguration.SupportSkyDriveInclusion) %>
                        <% { %>
                        <td align="center">
                            <div class="account-popup-icon SkyDriveBig"></div>
                            <a class="button middle blue add-account-button SkyDrive" data-provider="SkyDrive">
                                <%= FilesUCResource.ThirdPartySkyDrive %>
                            </a>
                        </td>
                        <% } %>
                        <% if (ImportConfiguration.SupportSharePointInclusion) %>
                        <% { %>
                        <td align="center">
                            <div class="account-popup-icon SharePointBig"></div>
                            <a class="button middle blue add-account-button SharePoint" data-provider="SharePoint">
                                <%= FilesUCResource.ThirdPartySharePoint %>
                            </a>
                        </td>
                        <% } %>
                    </tr>
                </tbody>
            </table>
        </body>
    </sc:Container>
</div>

<div id="thirdPartyActionPanel" class="studio-action-panel">
    <div class="corner-top right">
    </div>
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
