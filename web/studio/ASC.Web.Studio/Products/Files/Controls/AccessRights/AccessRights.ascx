<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccessRights.ascx.cs" Inherits="ASC.Web.Files.Controls.AccessRights" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Files.Core.Security" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:PlaceHolder ID="_sharingContainer" runat="server"></asp:PlaceHolder>

<div id="shareSelectorBody" class="page-menu <%= CoreContext.Configuration.Personal ? "only-outside" : string.Empty %>">
    <ul id="shareSidePanel" class="menu-list">
        <li id="shareSidePortal" class="menu-item none-sub-list">
            <a class="menu-item-label outer-text text-overflow">
                <span class="menu-item-label inner-text"><%= FilesUCResource.SharingListCaption %></span>
            </a>
        </li>
        <li id="shareSideOutside" class="menu-item none-sub-list">
            <a class="menu-item-label outer-text text-overflow">
                <span class="menu-item-label inner-text"><%= CoreContext.Configuration.Personal ? FilesUCResource.SharingListCaptionOutsidePersonal : FilesUCResource.SharingListCaptionOutside %></span>
            </a>
        </li>
        <% if (Global.EnableEmbedded)
           { %>
        <li id="shareSideEmbedded" class="menu-item none-sub-list">
            <a class="menu-item-label outer-text text-overflow">
                <span class="menu-item-label inner-text"><%= FilesUCResource.SharingListCaptionEmbedded %></span>
            </a>
        </li>
        <% } %>
    </ul>
    <div id="shareLinkBody">
        <span class="header-base">
            <%= FilesUCResource.SharingLinkCaption %>
        </span>
        <div class="sharing-link-items">
            <span id="chareLinkOpen" class="share-link-label">
                <%= FilesUCResource.AceStatus_Open %>
            </span>
            <label id="chareLinkClose" class="share-link-label">
                <input type="radio" class="link-share-opt" name="linkShareOpt" value="<%= (int) FileShare.Restrict %>"><%= FilesUCResource.AceStatusEnum_Restrict %>
            </label>
        </div>
        <div id="sharingLinkDeny" class="describe-text"><%= FilesUCResource.ShareLinkDeny %></div>
        <div id="shareLinkPanel">
            <span id="shareLinkCopy" class="baseLinkAction text-medium-describe"><%= FilesUCResource.CopyToClipboard %></span>
             <% if (!String.IsNullOrEmpty(ASC.Common.Utils.LinkShorterUtil.BitlyUrl)) { %>
            <span id="getShortenLink" class="baseLinkAction text-medium-describe"><%= FilesUCResource.GetShortenLink %></span>
            <% } %>

            <div id="sharingAcePanel">
                <label>
                    <input type="radio" class="link-share-opt" name="linkShareOpt" value="<%= (int) FileShare.Read %>" checked="checked"><%= FilesUCResource.AceStatusEnum_Read %>
                </label>
                <label>
                    <input type="radio" class="link-share-opt" name="linkShareOpt" value="<%= (int) FileShare.ReadWrite %>"><%= FilesUCResource.AceStatusEnum_ReadWrite %>
                </label>
                <label>
                    <input type="radio" class="link-share-opt" name="linkShareOpt" value="<%= (int) FileShare.Review %>"><%= FilesUCResource.AceStatusEnum_Review %>
                </label>
            </div>
            <textarea id="shareLink" class="textEdit" cols="10" rows="2" readonly="readonly"></textarea>

            <br />
            <ul id="shareViaSocPanel" class="clearFix">
                <li><a class="google" target="_blank" title="<%= FilesUCResource.ButtonViaGoogle %>"></a></li>
                <li><a class="facebook" target="_blank" title="<%= FilesUCResource.ButtonViaFacebook %>"></a></li>
                <li><a class="twitter" target="_blank" title="<%= FilesUCResource.ButtonViaTwitter %>"></a></li>
                <li><a class="mail" title="<%= FilesUCResource.LinkViaMail %>"></a></li>
            </ul>
            <div id="shareMailPanel">
                <div class="share-mail-list webkit-scrollbar">
                    <div class="recipient-mail">
                        <input type="email" class="recipient-email-input textEdit" placeholder="<%= FilesUCResource.ShareLinkMail %>" />
                        <span class="icon-link trash recipient-mail-remove recipient-mail-action" title="<%= FilesUCResource.ButtonDelete %>"></span>
                        <span class="icon-link plus recipient-mail-add recipient-mail-action" title="<%= FilesUCResource.LinkViaMailAdd %>"></span>
                    </div>
                </div>
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td class="recipient-mail-text">
                            <textarea id="shareMailText" class="textEdit" cols="10" rows="2" placeholder="<%= FilesUCResource.ShareLinkMailMessage %>"></textarea>
                        </td>
                        <td>
                            <a id="shareSendLinkToEmail" class="button middle gray"><%= FilesUCResource.LinkViaMailSend %></a>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
        <% if (Global.EnableEmbedded)
           { %>
        <div id="shareEmbeddedPanel">
            <%= FilesUCResource.EmbedSize %>
            <ul id="embeddedSizeTemplate" class="clearFix">
                <li class="embedded-size-item">
                    <span class="text-medium-describe"><%= FilesUCResource.EmbedSizeAuto %></span>
                    <div class="embedded-size-template"></div>
                </li>
                <li class="embedded-size-item embedded-size-8x6">
                    <span class="text-medium-describe">800x600</span>
                    <div class="embedded-size-template"></div>
                </li>
                <li class="embedded-size-item embedded-size-6x4">
                    <span class="text-medium-describe">600x400</span>
                    <div class="embedded-size-template"></div>
                </li>
                <li class="embedded-size-custom">
                    <span class="text-medium-describe"><%= FilesUCResource.EmbedSizeCustom %></span>
                    <div class="embedded-size-descr">
                        <span><%= FilesUCResource.EmbedSizeHeight %>:</span>
                        <input type="text" class="textEdit" name="height" />
                    </div>
                    <div class="embedded-size-descr">
                        <span><%= FilesUCResource.EmbedSizeWidth %>:</span>
                        <input type="text" class="textEdit" name="width" />
                    </div>
                </li>
            </ul>

            <%= FilesUCResource.EmbedCode %>
            <textarea id="shareEmbedded" class="textEdit" cols="10" rows="3" readonly="readonly"></textarea>
            <a id="embeddedCopy" class="button middle gray"><span><%= FilesUCResource.CopyToClipboard %></span></a>
        </div>
        <% } %>

        <div class="middle-button-container clearFix">
            <a class="sharing-notchanged-buttons sharing-cancel-button button middle gray"><%= Resource.CloseButton %></a>
            <div class="sharing-changed-buttons">
                <a class="sharing-save-button button blue middle"><%= Resource.SaveButton %></a>
                <span class="splitter-buttons"></span>
                <a class="sharing-cancel-button button middle gray"><%= Resource.CancelButton %></a>
            </div>
        </div>
    </div>
</div>

<% if (!CoreContext.Configuration.Personal && IsPopup)
   { %>
<div id="filesConfirmUnsubscribe" class="popup-modal">
    <sc:Container id="confirmUnsubscribeDialog" runat="server">
        <header><%=FilesUCResource.ConfirmRemove%></header>
        <body>
            <div id="confirmUnsubscribeText">
                <%=FilesUCResource.ConfirmUnsubscribe%>
            </div>
            <div id="confirmUnsubscribeList" class="files-remove-list webkit-scrollbar">
                <dl>
                    <dt class="confirm-remove-folders">
                        <%=FilesUCResource.Folders%>:</dt>
                    <dd class="confirm-remove-folders"></dd>
                    <dt class="confirm-remove-files">
                        <%=FilesUCResource.Documents%>:</dt>
                    <dd class="confirm-remove-files"></dd>
                </dl>
            </div>
            <div class="middle-button-container">
                <a id="unsubscribeConfirmBtn" class="button blue middle">
                    <%=FilesUCResource.ButtonOk%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%=FilesUCResource.ButtonCancel%>
                </a>
            </div>
        </body>
    </sc:Container>
</div>
<% } %>