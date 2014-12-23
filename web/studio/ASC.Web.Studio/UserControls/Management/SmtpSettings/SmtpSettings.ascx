<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SmtpSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SmtpSettings" %>
<%@ Import Namespace="Resources" %>

<div id="smtpSettingsContainer" class="clearFix">
    <div class="settings-block">
        <div class="header-base"><%= Resource.SmtpSettings %></div>
        <p class="smtp-settings-text"><%= Resource.SmtpSettingsText %> </p>
        <div class="smtp-settings-block clearFix">
            <div class="smtp-settings-item">
                <div class="host">
                    <div class="smtp-settings-title"><%= Resource.HostName %>:</div>
                    <input type="text" class="smtp-settings-field textEdit" value="<%= CurrentSmtpSettings.Host %>" />
                </div>
                <div class="port">
                    <div class="smtp-settings-title"><%= Resource.Port %>:</div>
                    <input type="text" class="smtp-settings-field textEdit" value="<%= CurrentSmtpSettings.Port %>" />
                </div>
            </div>
            <div class="smtp-settings-item">
                <input id="smtpSettingsAuthentication" type="checkbox" <% if (CurrentSmtpSettings.IsRequireAuthentication) { %> checked="checked" <% } %> />
                <label for="smtpSettingsAuthentication"><%= Resource.Authentication %></label>
            </div>
            <div class="smtp-settings-item host-login requiredField">
                <div class="smtp-settings-title"><%= Resource.HostLogin %>:</div>
                <input type="text" class="smtp-settings-field textEdit" value="<%= CurrentSmtpSettings.CredentialsUserName %>" />
            </div>
            <div class="smtp-settings-item host-password requiredField">
                <div class="smtp-settings-title"><%= Resource.HostPassword %>:</div>
                <input type="password" class="smtp-settings-field textEdit" value="<%= CurrentSmtpSettings.CredentialsUserPassword %>" />
            </div>
            <div class="smtp-settings-item display-name">
                <div class="smtp-settings-title"><%= Resource.SenderName %>:</div>
                <input type="text" class="smtp-settings-field textEdit" value="<%= CurrentSmtpSettings.SenderDisplayName %>" />
            </div>
            <div class="smtp-settings-item email-address">
                <div class="smtp-settings-title"><%= Resource.SenderEmailAddress %>:</div>
                <input type="text" class="smtp-settings-field textEdit" value="<%= CurrentSmtpSettings.SenderAddress %>" />
            </div>
            <div class="smtp-settings-item">
                <input id="smtpSettingsEnableSsl" type="checkbox" <% if (CurrentSmtpSettings.EnableSSL) { %> checked="checked" <% } %> />
                <label for="smtpSettingsEnableSsl"><%= Resource.EnableSSL %></label>
            </div>
        </div>

        <div class="middle-button-container">
            <a id="smtpSettingsButtonSave" class="button blue" href="javascript:void(0);"><%=  Resource.SaveButton %></a>
            <span class="splitter-buttons"></span>
            <a id="smtpSettingsButtonDefault" class="button blue" href="javascript:void(0);"><%=  Resource.DefaultSettings %></a>
            <span class="splitter-buttons"></span>
            <a id="smtpSettingsButtonTest" class="button gray" href="javascript:void(0);"><%=  Resource.SendTestMail %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <%= String.Format(Resource.SMTPSettingsHelp, "<br />") %>
    </div>
</div>