<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Backup.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.Backup" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div id="backupSettings" class="clearFix <%= EnableBackup ? "" : "disable" %>">
    <div class="settings-block backup-settings_block">
        <div class="header-base"><%= Resource.DataBackup %></div>
        <div class="backup-settings_desc"><%= Resource.BackupText %></div>
        <div class="backup-settings_title"><%= Resource.BackupStorage %>:</div>
        <ul class="backup-settings_places clearFix">
            <li>
                <input id="backupTempTeamlab" type="radio" name="backupStorageVariants" value="4"/>
                <label for="backupTempTeamlab"><%= Resource.BackupTempTeamlab %></label>
            </li>
            <li>
                <input id="backupDocsTeamlab" type="radio" name="backupStorageVariants" value="0"/>
                <label for="backupDocsTeamlab"><%= Resource.BackupDocsTeamlab %></label>
            </li>
            <li class="third-party-storage disabled" title="<%= String.Format(Resource.BackupNotAvailableThirdServices, "\n").HtmlEncode() %>" >
                <input disabled id="backupThirdStorage" type="radio" name="backupStorageVariants" value="1"/>
                <label for="backupThirdStorage">
                    <span id="helpBackupThirdStorageDisable" class="HelpCenterSwitcher expl"></span>
                    DropBox, Box.com, OneDrive, Google Drive...
                </label>
                <div class="popup_helper" id="backupThirdStorageDisable">
                    <p><%= Resource.BackupThirdStorageDisable %></p>
                    <div class="cornerHelpBlock pos_top"></div>
                </div>
            </li>
            <li>
                <input id="backupCloudAmazon" type="radio" name="backupStorageVariants" value="2"/>
                <label for="backupCloudAmazon"><%= Resource.BackupCloud %> Amazon</label>
            </li>
        </ul>
        <div class="backup-settings_path">
            <div class="backup-settings_folder">
                <input id="backupFolderPath" readonly="readonly" type="text" class="textEdit backup-settings_folder_field"/>
                <div id="backupFolderPathBtn" class="button gray middle"><%= Resource.Choose %></div>
            </div>
            <asp:PlaceHolder runat="server" ID="FolderSelectorHolder"></asp:PlaceHolder>
            <div id="backupAmazonSettings" class="backup-settings_amazon display-none">
                <input type="text" class="textEdit backup-settings_amazon_params access-key-id" placeholder="Access Key Id"/>
                <input type="text" class="textEdit backup-settings_amazon_params secret-access-key" placeholder="Secret Access Key" />
                <input type="text" class="textEdit backup-settings_amazon_params bucket" placeholder="Bucket Name" />
                <select class="textEdit backup-settings_amazon_params region">
                    <option value=""><%= Resource.ChooseRegion %></option>
                    <% foreach(var region in Regions ){ %>
                    <option value="<%= region.SystemName %>"><%= region.DisplayName %></option>
                    <%} %>
                </select>
            </div>
        </div>
        <div class="clearFix">
            <input id="backupWithMailCheck" type="checkbox" />
            <label for="backupWithMailCheck"><%= Resource.BackupMakeWithMail %></label>
        </div>
        <div class="clearFix">
            <input id="backupAutoSaving" type="checkbox" />
            <label for="backupAutoSaving"><%= Resource.BackupAutoSave %></label>
        </div>
        <div class="backup-settings_auto-params">
            <asp:PlaceHolder runat="server" ID="BackupTimePeriod"></asp:PlaceHolder>
            <div class="backup-settings_title"><%= Resource.BackupCopyCount %>:</div>
            <select id="backupCountCopy"></select>
        </div>
        <div class="middle-button-container">
            <a id="startBackupBtn" class="button blue middle"><%= Resource.BackupMakeCopyBtn %></a>
            <span class="splitter-buttons"></span>
            <a id="saveBackupBtn" class="button gray middle disabled"><%= Resource.SaveButton %></a>
        </div>

        <div id="progressbar_container" class="backup-settings_progress-cnt">
            <div class="asc-progress-wrapper">
                <div class="asc-progress-value"></div>
            </div>
            <div style="padding-top: 2px;" class="text-medium-describe">
                <%= Resource.CreatingBackup %>
                <span id="backup_percent"></span>
            </div>
        </div>

        <div id="backupLinkCnt">
            <p><%= Resource.BackupReadyText %></p>
            <a id="backupLinkTemp" target="_blank" class="link gray dotline"><%= Resource.BackupDownloadByLink %></a>
        </div>

    </div>

    <div class="settings-help-block">
        <% if (!EnableBackup) { %>
            <p><%= Resource.ErrorNotAllowedOption %></p>
            <a href="<%= TenantExtra.GetTariffPageLink() %>" target="_blank"><%= Resource.ViewTariffPlans %></a>
        <% } else { %>
            <p><%= String.Format(Resource.DataBackupHelp, "<b>", "</b>", "<br />")%></p>
        <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
           { %>
            <a href="<%= CommonLinkUtility.GetHelpLink(true) + "gettingstarted/configuration.aspx#CreatingBackup_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
        <% } %>
    </div>
</div>

<asp:PlaceHolder runat="server" ID="RestoreHolder"></asp:PlaceHolder>
