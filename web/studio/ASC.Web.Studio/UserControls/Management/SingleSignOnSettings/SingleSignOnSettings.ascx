<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SingleSignOnSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.SingleSignOnSettings" %>
<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="Resources" %>

<link rel="stylesheet" type="text/css" href="<%= WebPath.GetPath("UserControls/Management/SingleSignOnSettings/css/singlesignonsettings.css") %>"/>

<div class="clearFix">
    <div class="sso-settings-main-container settings-block">
        <div class="header-base sso-settings-title"><%= Resource.SingleSignOnSettings %></div>
        <div class="clearFix">
            <input type="radio" id="ssoSettingsDisable" name="ssoSettings" <% if (!Settings.EnableSso) { %> checked <% } %>>
            <label for="ssoSettingsDisable"><%= Resource.DisableUserButton %></label>
        </div>
        <div class="clearFix">
            <input type="radio" id="ssoSettingsEnable" name="ssoSettings" <% if (Settings.EnableSso) { %> checked <% } %>>
            <label id="ssoSettingsEnableLabel" for="ssoSettingsEnable"><%= Resource.EnableUserButton %></label>
        </div>
        <div class="sso-settings-container clearFix <% if (!Settings.EnableSso)
                                                       { %>sso-settings-disabled<% } %>">
            <div class="sso-settings-block">
                <div class="sso-settings-text">
                    <%= Resource.SsoSettingsSsoType %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SsoSettingsSsoTypeHelper'});" />
                </div>
                <div class="popup_helper" id="SsoSettingsSsoTypeHelper">
                    <p><%: Resource.SsoSettingsSsoTypeHelper %></p>
                </div>
            </div>
            <div class="sso-settings-block requiredField">
                <span id="ssoSettingsIssuerUrlError" class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="sso-settings-text"><%= Resource.IssuerURL %>
                    <span class="sso-required-field">*</span>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SsoSettingsIssuerUrlHelper'});" />
                </div>
                <input type="text" class="sso-settings-issuer-url textEdit" value="<%= Settings.IdpSettings.EntityId %>" <% if (!Settings.EnableSso)
                                                                                                              { %> disabled <% } %> />
                <div class="popup_helper" id="SsoSettingsIssuerUrlHelper">
                    <p><%: Resource.SsoSettingsIssuerUrlHelper %></p>
                </div>
            </div>
            <div class="sso-settings-block requiredField">
                <span id="ssoSettingsEndpointUrlError" class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="sso-settings-text"><%= Resource.SsoEndpointURL %>
                    <span class="sso-required-field">*</span>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SsoSettingsSsoEndpointUrlHelper'});" />
                </div>
                <input type="text" class="sso-settings-endpoint-url textEdit" value="<%= Settings.IdpSettings.SsoUrl %>" <% if (!Settings.EnableSso)
                                                                                                                     { %> disabled <% } %> />
                <div class="popup_helper" id="SsoSettingsSsoEndpointUrlHelper">
                    <p><%: Resource.SsoSettingsSsoEndpointUrlHelper %></p>
                </div>
            </div>
            <div class="sso-settings-block">
                <div class="sso-settings-text"><%= Resource.SloEndpointURL %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SsoSettingsSloEndpointUrlHelper'});" />
                </div>
                <input type="text" class="slo-settings-endpoint-url textEdit" value="<%= Settings.IdpSettings.SloUrl %>" <% if (!Settings.EnableSso)
                                                                                                                     { %> disabled <% } %> />
                <div class="popup_helper" id="SsoSettingsSloEndpointUrlHelper">
                    <p><%: Resource.SsoSettingsSloEndpointUrlHelper %></p>
                </div>
            </div>
        </div>
        <div class="sso-settings-loader display-none"></div>
        <div class="sso-settings-status display-none"></div>
        <div class="middle-button-container">
            <a class="button blue sso-settings-save disable" title="<%= Resource.LdapSettingsSaveSettings %>"><%= Resource.SaveButton %></a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.SsoSettingsHelp.HtmlEncode(), "<b>", "</b>", String.Empty) %></p>
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <p>
            <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#ChangingSecuritySettings_block" %>" 
                target="_blank"><%= Resource.LearnMore %></a>
        </p>
        <% } %>
    </div>
</div>