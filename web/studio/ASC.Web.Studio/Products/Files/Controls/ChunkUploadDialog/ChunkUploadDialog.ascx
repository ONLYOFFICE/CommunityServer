<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChunkUploadDialog.ascx.cs" Inherits="ASC.Web.Files.Controls.ChunkUploadDialog" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Utils" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<div id="chunkUploadDialog" class="progress-dialog">

    <div class="progress-dialog-header menu-upload-icon">
        <a class="actions-container close" title="<%= FilesUCResource.ButtonUploadCancelAndClose %>" >&times;</a>
        <a class="actions-container minimize" title="<%= FilesUCResource.ButtonUploadMinimize %>" ></a>
        <a class="actions-container maximize" title="<%= FilesUCResource.ButtonUploadMaximize %>" ></a>
        <span id="chunkUploadDialogHeader"></span>
    </div>

    <div class="progress-dialog-body">
        <div class="settings-container">
            <span id="uploadSettingsSwitcher">
                <a class="baseLinkAction gray-text"><%= FilesUCResource.SideCaptionSettings %></a>
                <span class="sort-down-gray"></span>
            </span>
            <a id="abortUploadigBtn" class="linkMedium gray-text">
                <%= FilesUCResource.ButtonCancelAll %>
            </a>
        </div>
        <div class="files-container">
            <table id="uploadFilesTable" class="tableBase" cellspacing="0" cellpadding="5">
                <colgroup>
                    <col class="fu-width-icon" />
                    <col />
                    <col class="fu-width-progress" />
                    <col class="fu-width-action" />
                </colgroup>
                <tbody></tbody>
            </table>
        </div>
        <div class="upload-info-container">
            <% if (!CoreContext.Configuration.Standalone)
               { %>
            <% if (CoreContext.Configuration.Personal)
               { %>
            <span class="gray-text"><%= string.Format(FilesUCResource.MaxFileSize, FileSizeComment.FilesSizeToString(TenantExtra.GetTenantQuota().MaxFileSize)) %></span>
            <% }
               else
               { %>
            <span class="free-space gray-text"></span>
            <% if (TenantExtra.EnableTarrifSettings && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
               { %>
            <span class="splitter"></span>
            <a class="link underline gray" target="_blank" href="<%= TenantExtra.GetTariffPageLink() %>">
                <%= FilesUCResource.UpgradeYourPlan %>
            </a>
            <% } %>
            <% } %>
            <% } %>
        </div>
        <div id="uploadSettingsPanel" class="studio-action-panel">
            <ul class="dropdown-content">
                <li>
                    <label class="gray-text">
                        <input type="checkbox" class="update-if-exist checkbox" <%= FilesSettings.UpdateIfExist ? "checked=\"checked\"" : string.Empty %>>
                        <%= FilesUCResource.UpdateIfExist %>
                    </label>
                </li>
                <% if (FileConverter.EnableAsUploaded) %>
                <% { %>
                <li>
                    <label class="gray-text">
                        <input type="checkbox" class="store-original checkbox" <%= FilesSettings.StoreOriginalFiles ? "checked=\"checked\"" : string.Empty %> />
                        <%= FilesUCResource.ConfirmStoreOriginalUploadCbxLabelText %>
                    </label>
                </li>
                <% } %>
                <li>
                    <label class="gray-text">
                        <input type="checkbox" id="uploadCompactViewCbx" class="checkbox">
                        <%= FilesUCResource.ShowThisWindowMinimized %>
                    </label>
                </li>
            </ul>
        </div>
    </div>

</div>

<script id="fileUploaderRowTmpl" type="text/x-jquery-tmpl">
    <tr id="${id}" class="fu-row">
        <td class="borderBase">
            <div class="${fileTypeCssClass}"></div>
        </td>
        <td class="borderBase">
            <div class="fu-title-cell" >
                <span title="${name}">${name}</span>
            </div>
        </td>
        <td class="borderBase">
            <div class="fu-progress-cell">
                {{if showAnim == true}}
                <div class="progress-color-anim">&nbsp;</div>
                {{/if}}
                {{if showAnim == false}}
                <progress max="100" value="0" >
                    <div class="asc-progress-wrapper">
                        <div class="asc-progress-value"></div>
                    </div>
                </progress>
                {{/if}}
                <div class="progress-label">${actionText}</div>
                <span class="upload-complete gray-text">${completeActionText}</span>
                <span class="upload-canceled gray-text"><%= FilesUCResource.Canceled %></span>
                <span class="upload-error red-text expl"><%= FilesUCResource.Error %></span>
                <div class="popup_helper"></div>
            </div>
        </td>
        <td class="borderBase">
            <div class="fu-action-cell">
                <a class="linkMedium gray-text abort-file-uploadig"><%= FilesUCResource.ButtonCancel %></a>
                {{if canShare === true}}
                    <a class="linkMedium gray-text share"><%= FilesUCResource.Share %></a>
                {{/if}}
            </div>
        </td>
    </tr>
</script>
