<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SmtpSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SmtpSettings" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="Resources" %>

<div id="smtpSettingsView" class="display-none">
    <div class="settings-block">
        <div class="header-base clearFix"><%= Resource.SmtpSettings %></div>
        <p class="smtp-settings-text"><%: Resource.SmtpSettingsText %> </p>
        
        <div id="currentSettingsBox">
            <input id="currentHost" type="hidden" value="<%= CurrentSmtpSettings.Host %>" />
            <input id="currentPort" type="hidden" value="<%= CurrentSmtpSettings.Port %>" />
            <input id="currentCredentialsUserName" type="hidden" value="<%= CurrentSmtpSettings.CredentialsUserName %>" />
            <input id="currentCredentialsUserPassword" type="hidden" value="" />
            <input id="currentSenderDisplayName" type="hidden" value="<%= CurrentSmtpSettings.SenderDisplayName %>" />
            <input id="currentSenderAddress" type="hidden" value="<%= CurrentSmtpSettings.SenderAddress %>" />
            <input id="currentEnableSsl" type="hidden" value="<%= CurrentSmtpSettings.EnableSSL %>" />
            <input id="currentEnableAuth" type="hidden" value="<%= CurrentSmtpSettings.EnableAuth %>" />
            <input id="currentIsDefault" type="hidden" value="<%= CoreContext.Configuration.SmtpSettings.IsDefaultSettings %>" />
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
            <div class="smtp-settings-item clearFix">
                <div class="host requiredField">
                    <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                    <div class="smtp-settings-title headerPanelSmall"><%= Resource.HostName %>:</div>
                    <input type="text" class="smtp-settings-field textEdit" value="${ host }" />
                </div>
                <div class="port requiredField">
                    <div class="smtp-settings-title headerPanelSmall"><%= Resource.Port %>:</div>
                    <input type="text" class="smtp-settings-field textEdit" value="${ port }" />
                </div>
            </div>
            <div class="smtp-settings-item">
                <input id="customSettingsAuthenticationRequired" type="checkbox" {{if enableAuth }} checked="checked" {{/if}} />
                <label for="customSettingsAuthenticationRequired"><%= Resource.Authentication %></label>
            </div>
            <div class="smtp-settings-item host-login requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="smtp-settings-title headerPanelSmall"><%= Resource.HostLogin %>:</div>
                <input type="text" class="smtp-settings-field textEdit" value="${ credentialsUserName }" 
                    {{if !enableAuth }} disabled="disabled" {{/if}}/>
            </div>
            <div class="smtp-settings-item host-password requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="smtp-settings-title headerPanelSmall"><%= Resource.HostPassword %>:</div>
                <input style="display:none" type="password" name="fakepasswordremembered"/>
                <input autocomplete="off" type="password" class="smtp-settings-field textEdit" value="${ credentialsUserPassword }" 
                    {{if !enableAuth }} disabled="disabled"{{else}} placeholder="**********"{{/if}} />
            </div>
            <div class="smtp-settings-item display-name">
                <div class="smtp-settings-title"><%= Resource.SenderName %>:</div>
                <input type="text" class="smtp-settings-field textEdit" value="${ senderDisplayName }" />
            </div>
            <div class="smtp-settings-item email-address requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="smtp-settings-title headerPanelSmall"><%= Resource.SenderEmailAddress %>:</div>
                <input type="text" class="smtp-settings-field textEdit" value="${ senderAddress }" />
            </div>
            <div class="smtp-settings-item clearFix enable-ssl">
                <input id="customSettingsEnableSsl" type="checkbox" {{if enableSSL }} checked="checked" {{/if}} />
                <label for="customSettingsEnableSsl"><%= Resource.EnableSSL %></label>
            </div>
        </script>

        <div id="mailserverSettingsBox" class="smtp-settings-block clearFix"></div>

        <script id="mailserverSettingsBoxTmpl" type="text/x-jquery-tmpl">
            {{if domains.length}}
            <div id="notificationBox">
                <div class="smtp-settings-item display-name">
                    <div class="smtp-settings-title"><%= Resource.SenderName %>:</div>
                    <input type="text" id="notificationSenderDisplayName" class="smtp-settings-field textEdit" value="${senderDisplayName}">
                </div>
                <div class="smtp-settings-item email-address requiredField">
                    <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                    <div class="smtp-settings-title headerPanelSmall"><%= Resource.SenderEmailAddress %>:</div>
                    <input id="notificationLogin" type="text" class="textEdit" value="${login.replace(/@.*/, '')}">
                    <span id="notificationHostDomainSplitter">@</span>
                    <select id="notificationDomain" class="comboBox">
                        {{each domains}}
                        <option value="${id}" {{if name == $data.login.replace(/.*@/, '')}} selected="selected"{{/if}}>${name}</option>
                        {{/each}}
                    </select>
                </div>
            </div>
            {{else}}
            <p id="noDomainsMsg">
                <%= string.Format(Resource.NoMailServerDomainsMsg.HtmlEncode(), "<a href=\"/addons/mail/#administration\" class=\"link\" target=\"_blank\">", "</a>") %>
            </p>
            {{/if}}
        </script>
        
        <div class="middle-button-container">
            <button id="saveSettingsBtn" class="button blue"><%= Resource.SaveButton %></button>
            <span class="splitter-buttons"></span>
            <button id="saveDefaultCustomSettingsBtn" class="button gray<% if (CurrentSmtpSettings.IsDefaultSettings)
                                                                           { %> disable" disabled="disabled<% } %>"><%= Resource.DefaultSettings %></button>
            <span class="splitter-buttons"></span>
            <button id="sendTestMailBtn" class="button gray <% if (CurrentSmtpSettings.IsDefaultSettings)
                                                                           { %> disable" disabled="disabled<% } %>"><%= Resource.SendTestMail %></button>
        </div>
    </div>
    <div class="settings-help-block">
        <%= String.Format(Resource.SMTPSettingsHelp.HtmlEncode(), "<br />") %>
    </div>
</div>