<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SmtpSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SmtpSettings" %>
<%@ Import Namespace="Resources" %>

<div id="smtpSettingsView" class="display-none">
    <div class="settings-block">
        <div class="header-base"><%= Resource.SmtpSettings %></div>
        <p class="smtp-settings-text"><%: Resource.SmtpSettingsText %> </p>
        
        <div id="currentSettingsBox">
            <input id="currentHost" type="hidden" value="<%= CurrentSmtpSettings.Host %>" />
            <input id="currentPort" type="hidden" value="<%= CurrentSmtpSettings.Port %>" />
            <input id="currentCredentialsUserName" type="hidden" value="<%= CurrentSmtpSettings.CredentialsUserName %>" />
            <input id="currentSenderDisplayName" type="hidden" value="<%= CurrentSmtpSettings.SenderDisplayName %>" />
            <input id="currentSenderAddress" type="hidden" value="<%= CurrentSmtpSettings.SenderAddress %>" />
            <input id="currentEnableSsl" type="hidden" value="<%= CurrentSmtpSettings.EnableSSL %>" />
        </div>
        
        <div id="settingsSwitch" class="settings-block display-none">
            <div id="settingsSwitchHeader"><%= Resource.Source %>:</div>
            <div class="clearFix">
                <input id="customSettingsRadio" type="radio" name="settingsRadio" value="0" checked="checked">
                <label for="customSettingsRadio"><%= Resource.SmtpCustomSettings %></label>
            </div>
            <div class="clearFix">
                <input id="mailserverSettingsRadio" type="radio" name="settingsRadio" value="1">
                <label for="mailserverSettingsRadio"><%= Resource.SmtpMailServerSettings %></label>
            </div>
        </div>
        
        <div id="customSettingsBox" class="smtp-settings-block clearFix"></div>
        <script id="customSettingsBoxTmpl" type="text/x-jquery-tmpl">
            <div class="smtp-settings-item">
                <div class="host">
                    <div class="smtp-settings-title"><%= Resource.HostName %>:</div>
                    <input type="text" class="smtp-settings-field textEdit" value="${ Host }" />
                </div>
                <div class="port">
                    <div class="smtp-settings-title"><%= Resource.Port %>:</div>
                    <input type="text" class="smtp-settings-field textEdit" value="${ Port }" />
                </div>
            </div>
            <div class="smtp-settings-item">
                <input id="customSettingsAuthenticationRequired" type="checkbox" {{if CredentialsUserName || CredentialsUserPassword}} checked="checked" {{/if}} />
                <label for="customSettingsAuthenticationRequired"><%= Resource.Authentication %></label>
            </div>
            <div class="smtp-settings-item host-login requiredField">
                <div class="smtp-settings-title"><%= Resource.HostLogin %>:</div>
                <input type="text" class="smtp-settings-field textEdit" value="${ CredentialsUserName }" 
                {{if !CredentialsUserName }} disabled="disabled" {{/if}}/>
            </div>
            <div class="smtp-settings-item host-password requiredField">
                <div class="smtp-settings-title"><%= Resource.HostPassword %>:</div>
                <input type="password" class="smtp-settings-field textEdit" value="${ CredentialsUserPassword }"
                {{if !CredentialsUserName && !CredentialsUserPassword }} disabled="disabled" {{/if}}/>
            </div>
            <div class="smtp-settings-item display-name">
                <div class="smtp-settings-title"><%= Resource.SenderName %>:</div>
                <input type="text" class="smtp-settings-field textEdit" value="${ SenderDisplayName }" />
            </div>
            <div class="smtp-settings-item email-address">
                <div class="smtp-settings-title"><%= Resource.SenderEmailAddress %>:</div>
                <input type="text" class="smtp-settings-field textEdit" value="${ SenderAddress }" />
            </div>
            <div class="smtp-settings-item clearFix enable-ssl">
                <input id="customSettingsEnableSsl" type="checkbox" {{if EnableSSL}} checked="checked" {{/if}} />
                <label for="customSettingsEnableSsl"><%= Resource.EnableSSL %></label>
            </div>
            
            <div class="middle-button-container">
                <button id="saveCustomSettingsBtn" class="button blue"><%= Resource.SaveButton %></button>
                <span class="splitter-buttons"></span>
                <button id="saveDefaultCustomSettingsBtn" class="button gray"><%= Resource.DefaultSettings %></button>
                <span class="splitter-buttons"></span>
                <button id="sendTestMailBtn" class="button gray"><%= Resource.SendTestMail %></button>
            </div>
        </script>
        
        <div id="mailserverSettingsBox"></div>
        <script id="mailserverSettingsBoxTmpl" type="text/x-jquery-tmpl">
            {{if domains.length}}
            <div id="notificationBox">
                <div class="smtp-settings-item">
                    <div class="smtp-settings-title"><%= Resource.HostLogin %>:</div>
                    <input id="notificationLogin" type="text" class="textEdit" value="${login.replace(/@.*/, '')}">
                    <span id="notificationHostDomainSplitter">@</span>
                    <select id="notificationDomain" class="comboBox">
                        {{each domains}}
                        <option value="${id}" {{if name == $data.login.replace(/.*@/, '')}} selected="selected"{{/if}}>${name}</option>
                        {{/each}}
                    </select>
                </div>
                <div class="smtp-settings-item">
                    <div class="smtp-settings-title"><%= Resource.SenderDisplayName %>:</div>
                    <input type="text" id="notificationSenderDisplayName" class="smtp-settings-field textEdit" value="${senderDisplayName}">
                </div>
            </div>
            {{else}}
            <p id="noDomainsMsg">
                <%= string.Format(Resource.NoMailServerDomainsMsg.HtmlEncode(), "<a href=\"/addons/mail/#administration\" class=\"link\" target=\"_blank\">", "</a>") %>
            </p>
            {{/if}}
            <div class="middle-button-container">
                <button id="saveMailserverSettingsBtn" class="button blue"><%= Resource.SaveButton %></button>
            </div>
        </script>
    </div>
    <div class="settings-help-block">
        <%= String.Format(Resource.SMTPSettingsHelp.HtmlEncode(), "<br />") %>
    </div>
</div>