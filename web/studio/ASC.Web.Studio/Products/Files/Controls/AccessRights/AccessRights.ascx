<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccessRights.ascx.cs" Inherits="ASC.Web.Files.Controls.AccessRights" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.FederatedLogin.LoginProviders" %>
<%@ Import Namespace="ASC.Files.Core.Security" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:PlaceHolder ID="_sharingContainer" runat="server"></asp:PlaceHolder>

<div id="shareLinkBody" class="clearFix">
    <div class="clearFix">
        <div class="sharing-link-items">
            <input type="checkbox" id="shareLinkOpen" class="on-off-checkbox" />
            <label for="shareLinkOpen"><%= FilesUCResource.SharingLinkCaption %></label>
        </div>
        <div id="sharingLinkAce">
            <select>
                <option value="<%= (int) FileShare.ReadWrite %>"><%= FilesUCResource.AceStatusEnum_ReadWrite %></option>
                <option value="<%= (int) FileShare.CustomFilter %>"><%= FilesUCResource.AceStatusEnum_CustomFilter %></option>
                <option value="<%= (int) FileShare.Review %>"><%= FilesUCResource.AceStatusEnum_Review %></option>
                <option value="<%= (int) FileShare.FillForms %>"><%= FilesUCResource.AceStatusEnum_FillForms %></option>
                <option value="<%= (int) FileShare.Comment %>"><%= FilesUCResource.AceStatusEnum_Comment %></option>
                <option value="<%= (int) FileShare.Read %>" selected="selected"><%= FilesUCResource.AceStatusEnum_Read %></option>
                <option value="<%= (int) FileShare.Restrict %>"><%= FilesUCResource.AceStatusEnum_Restrict %></option>
            </select>
        </div>
        <span id="shareLinkDescr" class="text-medium-describe"><%= FilesUCResource.SharingLinkDescr %></span>
    </div>
    <div id="shareLinkPanel">
        <div class="sharelink-around borderBase">
            <input type="text" id="shareLink" readonly="readonly" />
            <div id="shareLinkAction">
                <% if (UrlShortener.Enabled)
                   { %>
                <span id="getShortenLink" class="baseLinkAction"><%= FilesUCResource.GetShorten %></span>
                <% } %>
                <span id="shareLinkCopy" class="baseLinkAction"><%= FilesUCResource.ButtonCopyToClipboard %></span>
            </div>
            <ul id="shareViaSocPanel">
                <% if (!Request.DesktopApp())
                   { %>
                <li><a class="mail" title="<%= FilesUCResource.LinkViaMail %>" target="_blank"><span></span></a></li>
                <% } %>
                <% if (!CoreContext.Configuration.CustomMode)
                   { %>
                <li><a class="facebook" target="_blank" title="<%= FilesUCResource.ButtonViaFacebook %>"><span></span></a></li>
                <li><a class="twitter" target="_blank" title="<%= FilesUCResource.ButtonViaTwitter %>"><span></span></a></li>
                <% } %>
            </ul>
        </div>
        <div id="toggleEmbeddPanel">
            <span class="baseLinkAction"><%= FilesUCResource.ButtonEmbedd %></span>
        </div>
        <div id="shareEmbeddedPanel" class="studio-action-panel">
            <table cellpadding="0" cellspacing="0">
                <thead>
                    <tr>
                        <th colspan="3"><%= FilesUCResource.EmbeddedSize %></th>
                        <th class="embedd-space"></th>
                        <th><%= FilesUCResource.EmbedSizeWidth %></th>
                        <th></th>
                        <th><%= FilesUCResource.EmbedSizeHeight %></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>
                            <div class="embedded-size-item embedded-size-6x8">
                                <span class="baseLinkAction">600 &#215; 800 px</span>
                            </div>
                        </td>
                        <td>
                            <div class="embedded-size-item embedded-size-4x6">
                                <span class="baseLinkAction">400 &#215; 600 px</span>
                            </div>
                        </td>
                        <td>
                            <div class="embedded-size-item embedded-size-1x1">
                                <span class="baseLinkAction"><%= FilesUCResource.EmbedSizeAuto %></span>
                            </div>
                        </td>
                        <td>
                        </td>
                        <td>
                            <input type="text" id="embeddSizeWidth" class="embedded-size-custom textEdit" value="100%" />
                        </td>
                        <td>&nbsp;&#215;&nbsp;</td>
                        <td>
                            <input type="text" id="embeddSizeHeight" class="embedded-size-custom textEdit" value="100%" />
                        </td>
                    </tr>
                </tbody>
            </table>
            <div class="share-embed-around borderBase">
                <input type="text" id="shareEmbedded" readonly="readonly" />
                <div id="shareEmbedCopy" class="text-medium">
                    <span class="baseLinkAction"><%= FilesUCResource.ButtonCopyToClipboard %></span>
                </div>
            </div>
        </div>
    </div>
</div>

<% if (!CoreContext.Configuration.Personal && IsPopup)
   { %>
<div id="filesConfirmUnsubscribe" class="popup-modal">
    <sc:Container ID="confirmUnsubscribeDialog" runat="server">
        <header><%= FilesUCResource.ConfirmRemove %></header>
        <body>
            <div id="confirmUnsubscribeText">
                <%= FilesUCResource.ConfirmUnsubscribe %>
            </div>
            <div id="confirmUnsubscribeList" class="files-remove-list webkit-scrollbar">
                <dl>
                    <dt class="confirm-remove-folders">
                        <%= FilesUCResource.Folders %>:</dt>
                    <dd class="confirm-remove-folders"></dd>
                    <dt class="confirm-remove-files">
                        <%= FilesUCResource.Documents %>:</dt>
                    <dd class="confirm-remove-files"></dd>
                </dl>
            </div>
            <div class="middle-button-container">
                <a id="unsubscribeConfirmBtn" class="button blue middle">
                    <%= FilesUCResource.ButtonOk %>
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