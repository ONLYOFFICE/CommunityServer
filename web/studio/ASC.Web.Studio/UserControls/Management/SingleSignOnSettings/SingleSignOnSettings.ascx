<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SingleSignOnSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.SingleSignOnSettings" %>
<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="ASC.SingleSignOn.Common" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<link rel="stylesheet" type="text/css" href="<%= WebPath.GetPath("usercontrols/management/singlesignonsettings/css/singlesignonsettings.css") %>"/>

<div class="clearFix">
<div class="sso-settings-main-container">
        <div class="header-base sso-settings-title"><%= Resource.SingleSignOnSettings %></div>
        <label class="sso-settings-label-checkbox">
            <input id="ssoSettingsCheckbox" type="checkbox" <% if (Settings.EnableSso) { %> checked <% } %>><%= Resource.SsoSettingsEnableSso %>
        </label>
        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'EnableSsoHelper'});"></span>
        <div class="popup_helper" id="EnableSsoHelper">
            <p><%= Resource.SsoSettingsEnableSsoHelper %></p>
        </div>
        <div class="sso-settings-container clearFix <% if (!Settings.EnableSso) { %>sso-settings-disabled<% } %>">
            <div class="sso-settings-block">
                <div class="sso-settings-text"><%= Resource.SsoSettingsSsoType %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SsoSettingsSsoTypeHelper'});"/>
                </div>
                <select class="sso-settings-token-type" <% if (!Settings.EnableSso) { %> disabled <% } %>>
                    <option <% if (Settings.TokenType == TokenTypes.SAML) { %> selected <% } %> value="SAML">SAML</option>
                    <option <% if (Settings.TokenType == TokenTypes.JWT) { %> selected <% } %> value="JWT">JWT</option>
                </select>
                <div class="popup_helper" id="SsoSettingsSsoTypeHelper">
                    <p><%= Resource.SsoSettingsSsoTypeHelper %></p>
                </div>
            </div>
            <div class="sso-settings-block requiredField">
                <span id="ssoSettingsIssuerUrlError" class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>

                <div class="sso-settings-text">
                    <%= Resource.IssuerURL %>
                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SsoSettingsIssuerUrlHelper'});" />
                </div>
                <input type="text" class="sso-settings-issuer-url textEdit" value="<%= Settings.Issuer %>" <% if (!Settings.EnableSso) { %> disabled <% } %>/>
                <div class="popup_helper" id="SsoSettingsIssuerUrlHelper">
                    <p><%= Resource.SsoSettingsIssuerUrlHelper %></p>
                </div>
            </div>
            <div class="sso-settings-block requiredField">
                <span id="ssoSettingsEndpointUrlError" class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>

                <div class="sso-settings-text">
                    <%= Resource.SsoEndpointURL %>
                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SsoSettingsSsoEndpointUrlHelper'});" />
                </div>
                <input type="text" class="sso-settings-endpoint-url textEdit" value="<%= Settings.SsoEndPoint %>" <% if (!Settings.EnableSso) { %> disabled <% } %>/>
                <div class="popup_helper" id="SsoSettingsSsoEndpointUrlHelper">
                    <p><%= Resource.SsoSettingsSsoEndpointUrlHelper %></p>
                </div>
            </div>
            <div class="sso-settings-block">
                <div class="sso-settings-text">
                    <%= Resource.SloEndpointURL %>
                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SsoSettingsSloEndpointUrlHelper'});" />
                </div>
                <input type="text" class="slo-settings-endpoint-url textEdit" value="<%= Settings.SloEndPoint %>" <% if (!Settings.EnableSso) { %> disabled <% } %>/>
                <div class="popup_helper" id="SsoSettingsSloEndpointUrlHelper">
                    <p><%= Resource.SsoSettingsSloEndpointUrlHelper %></p>
                </div>
            </div>
            <div class="sso-settings-block">
                <div class="sso-settings-text sso-settings-validation-text <% if (Settings.TokenType == TokenTypes.SAML) { %>sso-settings-disabled<% } %>"><%= Resource.SsoSettingsValidationType %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SsoSettingsValidationTypeHelper'});"/>
                </div>
                <select class="sso-settings-validation-type" <% if (Settings.TokenType == TokenTypes.SAML || !Settings.EnableSso) { %> disabled <% } %>>
                    <option <% if (Settings.ValidationType == ValidationTypes.X509) { %> selected <% } %> value="X.509">X.509</option>
                    <option <% if (Settings.ValidationType == ValidationTypes.HMAC_SHA256) { %> selected <% } %> value="HMAC SHA-256">HMAC SHA-256</option>
                    <option <% if (Settings.ValidationType == ValidationTypes.RSA_SHA256)
                               { %> selected <% } %> value="RSA SHA-256">RSA SHA-256</option>
                </select>
                <div class="popup_helper" id="SsoSettingsValidationTypeHelper">
                    <p><%= Resource.SsoSettingsValidationTypeHelper %></p>
                </div>
            </div>
            <div class="sso-settings-block requiredField">
                <span id="ssoSettingsPublicKeyError" class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="sso-settings-text"><%= Resource.SsoSettingsPublicKey %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SsoSettingsPublicKeyHelper'});"/>
                </div>
                <textarea class="sso-settings-public-key-area webkit-scrollbar" placeholder="<%= Resource.SsoSettingsPublicKey %>"
                    cols="40" rows="12" <% if (!Settings.EnableSso) { %> disabled <% } %>><%= Settings.PublicKey %></textarea>
                <div class="popup_helper" id="SsoSettingsPublicKeyHelper">
                    <p><%= Resource.SsoSettingsPublicKeyHelper %></p>
                </div>
            </div>
        </div>
        <div class="middle-button-container">
            <a class="button blue sso-settings-save disable" title="<%= Resource.LdapSettingsSaveSettings %>"><%= Resource.SaveButton %></a>
        </div>
        <div class="sso-settings-loader display-none"></div>
        <div class="sso-settings-status display-none"></div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.SsoSettingsHelp, "<b>", "</b>") %></p>
        <p><a href="<%= CommonLinkUtility.GetHelpLink(true) + "gettingstarted/configuration.aspx" %>" target="_blank"><%= Resource.LearnMore %></a></p>
    </div>
</div>