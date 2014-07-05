<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Backup.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.Backup" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div id="backupSettings" class="clearFix <%=EnableBackup ? "" : "disable" %>">
    <div class="settings-block">
        <div class="header-base"><%= Resource.DataBackup %></div>
        <div><%= string.Format(Resource.BackupDesc, "<br/>") %></div>
        <div class="middle-button-container">
            <a id="startBackupBtn" class="button blue middle"><%= Resource.PerformBackupButton %></a>
        </div>
        <div style="padding:20px 0 0 0;">
            <div id="progressbar_container" style="display: none; margin-bottom: 10px;">
                <div class="asc-progress-wrapper">
                    <div class="asc-progress-value"></div>
                </div>
                <div style="padding-top: 2px;" class="text-medium-describe">
                    <%= Resource.CreatingBackup %>
                    <span id="backup_percent"></span>
                </div>
            </div>
            <div id="backup_error" class="errorText" style="display:none;"></div>
            <div id="backup_ready" class="display-none">
                <div id="backup_link" class="longWordsBreak"></div>
                <%= string.Format(Resource.BackupReadyText, "<p>", "</p><p>", "</p>") %>
            </div>
        </div>
    </div>
    <div class="settings-help-block">
        <% if (!EnableBackup) { %>
            <p><%= Resource.ErrorNotAllowedOption %></p>
            <a href="<%= TenantExtra.GetTariffPageLink() %>" target="_blank"><%= Resource.ViewTariffPlans %></a>
        <% } else { %>
            <p><%=String.Format(Resource.HelpAnswerDataBackup, "<br />", "<b>", "</b>")%></p>
            <a href="<%= CommonLinkUtility.GetHelpLink(true) + "gettingstarted/configuration.aspx#CreatingBackup_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>

<% if (SetupInfo.IsVisibleSettings("Restore")) { %>
<div id="restoreBlock" class="clearFix">
    <div class="settings-block restore-setting-block">
        <div class="header-base"><%= Resource.RestoreTitle %></div>
        <p class="restore-setting-desc"><%= Resource.RestoreDesc %></p>
        <div><%= Resource.Source %></div>
        <div class="restore-setting-form">
            <input type="file" class="restore-setting-file"/>
            <input readonly type="text" class="textEdit restore-setting-field"/>
            <button id="restoreChooseFile" class="button gray middle"><%= Resource.Choose %></button>
        </div>
        <div class="middle-button-container">
            <a id="startRestoreBtn" class="button blue middle"><%= Resource.RestoreBtn %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p></p>
    </div>
</div>
<% } %>

<div class="clearFix">
    <div id="accountDeactivationBlock" class="settings-block">
        <div class="header-base"><%=Resource.AccountDeactivation%></div>
        <div><%=Resource.DeactivationDesc%></div>
        <div class="middle-button-container">
                <a id="sendDeactivateInstructionsBtn" class="button blue middle"><%=Resource.DeactivateButton%></a>
        </div>
        <p id="deativate_sent" class="display-none"></p>
    </div>
    <div class="settings-help-block">
        <p><%=String.Format(Resource.HelpAnswerAccountDeactivation, "<br />", "<b>", "</b>")%></p>
    </div>
</div>

<div class="clearFix">
    <div id="accountDeletionBlock" class="settings-block">
        <div class="header-base"><%=Resource.AccountDeletion%></div>
        <div><%=Resource.DeletionDesc%></div>
        <div class="middle-button-container">
            <a id="sendDeleteInstructionsBtn" class="button blue middle"><%=Resource.DeleteButton%></a>
        </div>
        <p id="delete_sent" class="display-none"></p>
    </div>
</div>
