<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LdapSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.LdapSettings" %>
<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="ASC.Security.Cryptography" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<link rel="stylesheet" type="text/css" href="<%= WebPath.GetPath("usercontrols/management/ldapsettings/css/default/ldapsettings.css") %>"/>

<div class="ldap-settings-main-container">
    <div class="header-base ldap-settings-title"><%= Resource.LdapSettingsTitle %></div>
    <label class="ldap-settings-label-checkbox ldap-settings-never-disable">
        <input id="ldapSettingsCheckbox" type="checkbox" <% if (settings.EnableLdapAuthentication) { %> checked <% } %>><%= Resource.EnableLDAPAuthentication %>
    </label>
    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'EnableLDAPAuthenticationHelper'});"></span>
    <div class="popup_helper" id="EnableLDAPAuthenticationHelper">
        <p><%= Resource.LdapSettingsEnableLdapAuthenticationHelper %></p>
    </div>
    <div class="ldap-settings-user-container clearFix <% if (!settings.EnableLdapAuthentication) { %>ldap-settings-disabled<% } %>">
        <div class="ldap-settings-column">
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsServer %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsServerHelper'});"></span>
                </div>
                <input id="ldapSettingsServer" class="textEdit" type="text" value="<%= settings.Server %>" <% if (!settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsServerHelper">
                <p><%= Resource.LdapSettingsServerHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsUserDN %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserDNHelper'});"></span>
                </div>
                <input id="ldapSettingsUserDN" class="textEdit" type="text" value="<%= settings.UserDN %>" <% if (!settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsUserDNHelper">
                <p><%= Resource.LdapSettingsUserDNHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsBindAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsBindAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsBindAttribute" class="textEdit" type="text" value="<%= settings.BindAttribute %>" <% if (!settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsBindAttributeHelper">
                <p><%= Resource.LdapSettingsBindAttributeHelper %></p>
            </div>
        </div>
        <div class="ldap-settings-second-column">
            <div class="ldap-settings-block requiredField">
                <span id="ldapSettingsPortNumberError" class="requiredErrorText"></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsPortNumber %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsPortNumberHelper'});"></span>
                </div>
                <input id="ldapSettingsPortNumber" class="textEdit" type="text" value="<%= settings.PortNumber %>" <% if (!settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsPortNumberHelper">
                <p><%= Resource.LdapSettingsPortNumberHelper %></p>
            </div>
            <div class="ldap-settings-block">
                <div class="ldap-settings-text"><%= Resource.LdapSettingsUserFilter %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserFilterHelper'});"></span>
                </div>                
                <input id="ldapSettingsUserFilter" class="textEdit" type="text" value="<%= settings.UserFilter %>" <% if (!settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsUserFilterHelper">
                <p><%= Resource.LdapSettingsUserFilterHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsLoginAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsLoginAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsLoginAttribute" class="textEdit" type="text" value="<%= settings.LoginAttribute %>" <% if (!settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsLoginAttributeHelper">
                <p><%= Resource.LdapSettingsLoginAttributeHelper %></p>
            </div>
        </div>
    </div>
    <label class="ldap-settings-label-checkbox <% if (!settings.EnableLdapAuthentication) { %>ldap-settings-disabled<% } %>">
        <input id="ldapSettingsGroupCheckbox" type="checkbox" <% if (!settings.EnableLdapAuthentication) { %> disabled <% } %><% if (settings.GroupMembership) { %> checked <% } %>><%= Resource.LdapSettingsGroupMembership %>
    </label>
    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupMembershipHelper'});"></span>
    <div class="popup_helper" id="LdapSettingsGroupMembershipHelper">
        <p><%= Resource.LdapSettingsGroupMembershipHelper %></p>
    </div>
    <div class="ldap-settings-group-container clearFix <% if (!settings.EnableLdapAuthentication || !settings.GroupMembership) { %>ldap-settings-disabled<% } %>">
        <div class="ldap-settings-column">
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsGroupDN %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupDNHelper'});"></span>
                </div>               
                <input id="ldapSettingsGroupDN" class="textEdit" type="text" value="<%= settings.GroupDN %>"
                    <% if (!settings.EnableLdapAuthentication || !settings.GroupMembership) { %> disabled <% } %>>
            </div>
             <div class="popup_helper" id="LdapSettingsGroupDNHelper">
                <p><%= Resource.LdapSettingsGroupDNHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsUserAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsUserAttribute" class="textEdit" type="text" value="<%= settings.UserAttribute %>"
                    <% if (!settings.EnableLdapAuthentication || !settings.GroupMembership) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsUserAttributeHelper">
                <p><%= Resource.LdapSettingsUserAttributeHelper %></p>
            </div>
        </div>
        <div class="ldap-settings-second-column">
            <div class="ldap-settings-block">
                <div class="ldap-settings-text"><%= Resource.LdapSettingsGroupNames %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupNameHelper'});"></span>
                </div>                
                <input id="ldapSettingsGroupName" class="textEdit" type="text" value="<%= settings.GroupName %>"
                    <% if (!settings.EnableLdapAuthentication || !settings.GroupMembership) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsGroupNameHelper">
                <p><%= Resource.LdapSettingsGroupNameHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsGroupAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsGroupAttribute" class="textEdit" type="text" value="<%= settings.GroupAttribute %>"
                    <% if (!settings.EnableLdapAuthentication || !settings.GroupMembership) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsGroupAttributeHelper">
                <p><%= Resource.LdapSettingsGroupAttributeHelper %></p>
            </div>
        </div>
    </div>
    <label class="ldap-settings-label-checkbox  <% if (!settings.EnableLdapAuthentication) { %>ldap-settings-disabled<% } %>">
        <input id="ldapSettingsAuthenticationCheckbox" type="checkbox" <% if (!settings.EnableLdapAuthentication) { %> disabled <% } %> 
            <% if (settings.Authentication) { %> checked <% } %>><%= Resource.LdapSettingsAuthentication %>
    </label>
    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsAuthenticationHelper'});"></span>
    <div class="popup_helper" id="LdapSettingsAuthenticationHelper">
        <p><%= Resource.LdapSettingsAuthenticationHelper %></p>
    </div>
    <div class="ldap-settings-auth-container clearFix
        <% if (!settings.EnableLdapAuthentication || !settings.Authentication) { %>ldap-settings-disabled<% } %>">
        <div class="ldap-settings-column">
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsUserName %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserNameHelper'});"></span>
                </div>
                <input id="ldapSettingsLogin" class="textEdit" type="text" value="<%= settings.Login %>"
                    <% if (!settings.EnableLdapAuthentication || !settings.Authentication) { %> disabled <% } %>>
            </div>
        </div>
        <div class="popup_helper" id="LdapSettingsUserNameHelper">
            <p><%= Resource.LdapSettingsUserNameHelper %></p>
        </div>
        <div class="ldap-settings-second-column">
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsPassword %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsPasswordHelper'});"></span>
                </div>
                <input id="ldapSettingsPassword" class="textEdit" type="password"
                    value="<%= settings.PasswordBytes != null && settings.PasswordBytes.Length != 0 ?
                    new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(settings.PasswordBytes)) :
                    string.Empty %>" <% if (!settings.EnableLdapAuthentication || !settings.Authentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsPasswordHelper">
                <p><%= Resource.LdapSettingsPasswordHelper %></p>
            </div>
        </div>
    </div>
    <div class="middle-button-container">
        <a class="button blue ldap-settings-save" title="<%= Resource.LdapSettingsSaveSettings %>"><%= Resource.SaveButton %></a>
        <span class="splitter-buttons"></span>
        <a class="button gray ldap-settings-restore-default-settings" title="<%= Resource.LdapSettingsRestoreDefaultSettings %>"><%= Resource.LdapSettingsRestoreDefault %></a>
    </div>
    <div class="ldap-settings-progressbar-container display-none">
        <div class="asc-progress-wrapper">
            <div class="asc-progress-value" style="width: 0%;"></div>
        </div>
        <div style="padding-top: 2px;" class="text-medium-describe">
            <span id="ldapSettingsStatus"></span>
            <span id="ldapSettingsPercent">0% </span>
        </div>
    </div>
    <div id="ldapSettingsError" class="errorText display-none"></div>
    <div id="ldapSettingsReady" class="display-none"><%= Resource.LdapSettingsSuccess %></div>
    <div id="ldapSettingsInviteDialog" class="display-none">
        <sc:Container runat="server" ID="ldapSettingsConfirmContainerId">
            <Header>
                <div><%= Resource.ConfirmationTitle %></div>
            </Header>
            <Body>
                <%= Resource.CancelConfirmMessage %>
                <div class="big-button-container">
                    <a class="button blue middle ldap-settings-ok"><%= Resource.OKButton %></a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle ldap-settings-cancel"><%= Resource.CancelButton %></a>
                </div>
            </Body>
        </sc:Container>
    </div>
    <div id="ldapSettingsImportUserLimitPanel" class="display-none">
        <sc:Container runat="server" ID="ldapSettingsLimitPanelId">
            <Header>
                <div><%= Resource.ImportUserLimitTitle %></div>
            </Header>
            <Body>
                <%= Resource.LdapSettingsImportUserLimitDecision %>
                <div class="big-button-container">
                    <a class="blue button middle ldap-settings-ok"><%= Resource.ImportUserLimitOkButtons %></a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle ldap-settings-cancel"><%= Resource.ImportContactsCancelButton %></a>
                </div>
            </Body>
        </sc:Container>
    </div>
</div>