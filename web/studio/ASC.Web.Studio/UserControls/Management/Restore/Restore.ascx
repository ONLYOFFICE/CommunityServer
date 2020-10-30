<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Restore.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.Restore" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<% if (SetupInfo.IsVisibleSettings("Restore")) { %>
<div id="restoreBlock" class="clearFix <%= isFree ? "disable" : "" %>">
    <div class="settings-block restore-setting_block ">
        <div class="header-base"><%= Resource.RestoreTitle %></div>
        <p class="restore-setting_desc"><%: Resource.RestoreDesc %></p>
        <div class="restore-settings_title"><%= Resource.Source %>:</div>
        <ul class="restore-settings_places clearFix">
            <li>
                <input id="restoreDocsTeamlab" checked type="radio" name="restoreStorageVariants" value="0"/>
                <label for="restoreDocsTeamlab"><%: Resource.BackupDocsTeamlab %></label>
            </li>
            <li class="thirdPartyStorageSelectorBox third-party-storage disabled" title="<%: String.Format(Resource.BackupNotAvailableThirdServices, "\n") %>" >
                <input disabled id="restoreThirdStorage" type="radio" name="restoreStorageVariants" value="1"/>
                <label for="restoreThirdStorage">
                    <span id="helpRestoreThirdStorageDisable" class="HelpCenterSwitcher expl"></span>
                    <%: Resource.BackupDocsThirdparty %>
                </label>
                <div class="popup_helper" id="restoreThirdStorageDisable">
                    <p><%: Resource.BackupThirdStorageDisable %></p>
                    <div class="cornerHelpBlock pos_top"></div>
                </div>
            </li>
            <li>
                <input id="restoreConsumer" type="radio" name="restoreStorageVariants" value="5"/>
                <label for="restoreConsumer"><%= Resource.BackupConsumerStorage %></label>
            </li>
            <li>
                <input id="restoreComputerFile" type="radio" name="restoreStorageVariants" value="3"/>
                <label for="restoreComputerFile"><%= Resource.RestoreComputerFile %></label>
            </li>
        </ul>
        <div class="restore-settings_path">
            <asp:PlaceHolder runat="server" ID="FileSelectorHolder"></asp:PlaceHolder>
            <div id="restoreConsumerStorageSettingsBox" class="display-none consumerStorageSettingsBox"></div>
            <div class="restore-setting_teamlab-file display-none">
                <input id="restoreChosenTeamlabFile" readonly="readonly" type="text" class="textEdit restore-setting_filename"/>
                <div id="restoreChooseTeamlabFile" class="button gray middle"><%= Resource.Choose %></div>
            </div>
            <div class="restore-setting_computer-file display-none">
                <input id="restoreChosenFileField" type="text" readonly="readonly" class="textEdit restore-setting_filename"/>
                <button id="restoreChosenFileBtn" class="button gray middle"><%= Resource.Choose %></button>
            </div>
        </div>
        <span class="restore-settings_show-list link dotline gray"><%= Resource.RestoreShowListBackup %></span>
        <div class="clearFix">
            <input id="restoreSendNotification" type="checkbox" <% if (IsSendNotification){%>checked <%} %>/>
            <label for="restoreSendNotification"><%= Resource.RestoreSendNotification %></label>
        </div>
        <div class="restore-settings_warnings">
            <h3 class="header-base red-text"><%= Resource.Warning %></h3>
            <p><%: Resource.RestoreWarningText %></p>
        </div>
        <div class="middle-button-container">
            <a id="startRestoreBtn" class="button blue middle"><%= Resource.RestoreBtn %></a>
        </div>
    </div>

    <div class="settings-help-block">
        <p><% = String.Format(Resource.RestoreHelp.HtmlEncode(), "<b>", "</b>") %></p>
         <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#CreatingBackup_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
    </div>
    <div id="restoreChooseBackupDialog" style="display: none;">
        <sc:Container runat="server" ID="_restoreChooseBackupDialog">
            <Header>
                <%= Resource.RestoreBackupList %>
            </Header>
            <Body>
                <p class="restore-backup-text"><%= String.Format(Resource.RestoreBackupListClearText, "<a id=\"clearBackupList\" class=\"link dotline\">", "</a>") %></p>
                <div class="restore-backup-list_w">
                    <table class="table-list height32 restore-backup-list">
                   
                    </table>
                </div>
                <div class="loader-text-block"><%: FeedResource.LoadingMsg %></div>
                <div id="emptyListRestore" class="restore-backup-empty-list"><%: Resource.RestoreEmptyList %></div>
                <div class="middle-button-container">
                    <button type="button" class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();"><%= Resource.CloseButton %></button>
                </div>
            </Body>
        </sc:Container>
    </div>
</div>
<% } %>


<script id="backupList" type="text/x-jquery-tmpl">
    {{each(i, item) items}}
    <tr data-storage="${item.StorageType}" data-id="${item.Id}">
        <td class="restore-backup-list_name">${item.FileName}</td>
        <td><span class="link dotted restore-backup-list_action"><%= Resource.RestoreAction %></span></td>
        <td><span class="icon-link trash restore-backup-list_del"></span></td>
    </tr>
    {{/each}}
</script>
