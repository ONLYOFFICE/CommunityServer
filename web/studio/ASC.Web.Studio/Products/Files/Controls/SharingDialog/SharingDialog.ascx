<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SharingDialog.ascx.cs" Inherits="ASC.Web.Files.Controls.SharingDialog" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.FederatedLogin.LoginProviders" %>
<%@ Import Namespace="ASC.Files.Core.Security" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<div id="sharingDialog" class="popup-modal">
    <sc:Container ID="SharingDialogContainer" runat="server">
        <header><%= UserControlsCommonResource.SharingSettingsTitle %></header>
        <body>
            <div class="dialog-content-container">
                <div class="top-button-container">
                    <a class="button user-button middle gray"><%= FilesUCResource.ButtonAddUsers %></a>
                    <span class="splitter-buttons"></span>
                    <a class="button link-button middle gray"><%= FilesUCResource.ButtonAddLink %></a>
                    <input type="hidden" class="link-settings" data-available="<%= FilesSettings.ExternalShare.ToString().ToLower() %>" data-social="<%= FilesSettings.ExternalShareSocialMedia.ToString().ToLower() %>">
                </div>
                <div class="us-search us-hot-search">
                    <input class="textEdit" placeholder="<%= ResourceJS.UserSelectorSearchPlaceholder %>">
                    <span class="clear-search"></span>
                    <div class="us-popup-dialog">
                        <div class="us-list-parent">
                            <div class="us-list"></div>
                        </div>
                    </div>
                </div>
                <div class="group-actions display-none">
                    <ul class="contentMenu" style="display: block;">
                        <li class="menuAction menuActionSelectAll">
                            <div class="menuActionSelect">
                                <input type="checkbox" title="<%= FilesUCResource.MainHeaderSelectAll %>">
                            </div>
                            <div class="down_arrow"></div>
                        </li>
                        <li class="menuAction menuActionChange unlockAction" title="<%= FilesUCResource.ButtonChangeAccessRights %>" style="display: list-item;">
                            <span><%= FilesUCResource.ButtonChangeAccessRights %></span>
                        </li>
                        <li class="menuAction menuActionRemove unlockAction" title="<%= FilesUCResource.ButtonRemove %>" style="display: list-item;">
                            <span><%= FilesUCResource.ButtonRemove %></span>
                        </li>
                        <li class="menuActionClose" title="<%= FilesUCResource.ButtonCancel %>">×</li>
                    </ul>
                    <div class="studio-action-panel menu-select-dialog">
                        <ul class="dropdown-content">
                            <li><a class="dropdown-item select-all"><%= FilesUCResource.ButtonSelectAll %></a></li>
                            <li><a class="dropdown-item select-links"><%= FilesUCResource.ButtonSelectLinks %></a></li>
                            <li><a class="dropdown-item select-groups"><%= FilesUCResource.ButtonSelectGroups %></a></li>
                            <li><a class="dropdown-item select-users"><%= FilesUCResource.ButtonSelectUsers %></a></li>
                            <li class="dropdown-item-seporator"></li>
                        </ul>
                        <ul class="dropdown-content"></ul>
                    </div>
                    <div class="studio-action-panel us-access-rights-dialog menu-access-rights-dialog"></div>
                </div>
                <div class="us-list-parent content-list">
                    <div class="us-list"></div>
                    <div class="studio-action-panel us-access-rights-dialog item-access-rights-dialog"></div>
                    <div class="studio-action-panel link-action-dialog">
                        <ul class="dropdown-content">
                            <li><a class="dropdown-item link-action-copy"><%= FilesUCResource.ButtonCopyExternalLink %></a></li>
                            <li><a class="dropdown-item link-action-share"><%= FilesCommonResource.Share %></a></li>
                            <li><a class="dropdown-item link-action-embed"><%= FilesUCResource.ButtonEmbeddingSettings %></a></li>
                            <% if (UrlShortener.Enabled)
                               { %>
                            <li><a class="dropdown-item link-action-short"><%= FilesUCResource.ButtonShortenLink %></a></li>
                            <% } %>
                            <li class="dropdown-item-seporator"></li>
                            <li><a class="dropdown-item link-action-delete"><%= FilesUCResource.ButtonDelete %></a></li>
                        </ul>
                    </div>
                    <div class="us-popup-dialog link-share-dialog">
                        <div class="link-share-around borderBase">
                            <input type="text" readonly="readonly" />
                        </div>
                        <div class="link-share-action-block">
                            <% if (!Request.DesktopApp() && !CoreContext.Configuration.CustomMode)
                               { %>
                            <div class="link-share-social">
                                <span><%= FilesUCResource.ShareTo %>:</span>
                                <% if (!Request.DesktopApp())
                                   { %>
                                <a class="mail" title="<%= FilesUCResource.LinkViaMail %>" target="_blank" href=""></a>
                                <% } %>
                                <% if (!CoreContext.Configuration.CustomMode)
                                   { %>
                                <a class="facebook" title="<%= FilesUCResource.ButtonViaFacebook %>" target="_blank" href=""></a>
                                <a class="twitter" title="<%= FilesUCResource.ButtonViaTwitter %>" target="_blank" href=""></a>
                                <% } %>
                            </div>
                            <% } %>
                            <div class="link-share-actions">
                                <% if (UrlShortener.Enabled)
                                   { %>
                                <span class="link dotline link-share-action-short"><%= FilesUCResource.GetShorten %></span>
                                <% } %>
                                <span class="link dotline link-share-action-copy"><%= FilesUCResource.ButtonCopyToClipboard %></span>
                            </div>
                        </div>
                    </div>
                    <div class="us-popup-dialog link-embed-dialog">
                        <table cellpadding="0" cellspacing="0">
                            <thead>
                                <tr>
                                    <th colspan="3"><%= FilesUCResource.EmbeddedSize %></th>
                                    <th class="embed-space"></th>
                                    <th><%= FilesUCResource.EmbedSizeWidth %></th>
                                    <th></th>
                                    <th><%= FilesUCResource.EmbedSizeHeight %></th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>
                                        <div class="embed-size-item embed-size-6x8">
                                            <span class="baseLinkAction">600 &#215; 800 px</span>
                                        </div>
                                    </td>
                                    <td>
                                        <div class="embed-size-item embed-size-4x6">
                                            <span class="baseLinkAction">400 &#215; 600 px</span>
                                        </div>
                                    </td>
                                    <td>
                                        <div class="embed-size-item embed-size-1x1">
                                            <span class="baseLinkAction"><%= FilesUCResource.EmbedSizeAuto %></span>
                                        </div>
                                    </td>
                                    <td>
                                    </td>
                                    <td>
                                        <input type="text" class="embed-size-custom embed-size-width textEdit" value="100%" />
                                    </td>
                                    <td>&nbsp;&#215;&nbsp;</td>
                                    <td>
                                        <input type="text" class="embed-size-custom embed-size-height textEdit" value="100%" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <div class="link-embed-around borderBase">
                            <input type="text" readonly="readonly" />
                            <span class="link dotline link-embed-action-copy"><%= FilesUCResource.ButtonCopyToClipboard %></span>
                        </div>
                    </div>
                    <div class="us-popup-dialog item-tooltip-dialog"></div>
                </div>
                <div class="empty-list describe-text"><%= string.Format(FilesJSResource.SharingSettingsEmpty, "<br/>") %></div>
            </div>

            <div class="small-button-container clearFix">
                <div class="bottom-button-container">
                    <a class="button middle blue main-part display-none"><%= Resource.SaveButton %></a><a class="button middle blue arrow-part display-none"></a><a class="button middle gray"><%= Resource.CancelButton %></a>
                    <a class="settings-link"><span class="link dotline"><%= FilesUCResource.AdvancedSettings %></span></a>
                </div>

                <div class="studio-action-panel save-action-dialog">
                    <ul class="dropdown-content">
                        <li><a class="dropdown-item save-action-message"><%= FilesUCResource.AddMessage %></a></li>
                        <li><a class="dropdown-item save-action-silent"><%= FilesUCResource.ButtonSaveWithoutNotification %></a></li>
                        <li class="dropdown-item-seporator"></li>
                        <li><a class="dropdown-item save-action-copy"><%= FilesUCResource.ButtonSaveAndCopyLink %></a></li>
                    </ul>
                </div>

                <div class="message-dialog us-popup-dialog">
                    <div class="message-dialog-hdr"><%= FilesUCResource.AddMessage %>:</div>
                    <div class="describe-text"><%= FilesUCResource.NotifyWithChangesInRights %></div>
                    <textarea></textarea>
                    <div class="small-button-container">
                        <a class="button blue"><%= FilesUCResource.ButtonSaveAndSend %></a>
                        <span class="splitter-buttons"></span>
                        <a class="button gray"><%= Resource.CancelButton %></a>
                    </div>
                </div>

                <div class="advanced-settings-dialog us-popup-dialog">
                    <div class="advanced-settings-dialog-hdr"><%= FilesUCResource.AdvancedSettings %></div>
                    <label>
                        <input type="checkbox" class="deny-download"/><%= FilesUCResource.DenyDownload %>
                    </label>
                    <label>
                        <input type="checkbox" class="deny-sharing"/><%= FilesUCResource.DenySharing %>
                    </label>
                    <div class="small-button-container">
                        <a class="button blue"><%= Resource.OKButton %></a>
                        <span class="splitter-buttons"></span>
                        <a class="button gray"><%= Resource.CancelButton %></a>
                    </div>
                </div>

            </div>
        </body>
    </sc:Container>
</div>