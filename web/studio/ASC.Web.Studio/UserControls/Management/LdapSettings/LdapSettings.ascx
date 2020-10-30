<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LdapSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.LdapSettings" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<link rel="stylesheet" type="text/css" href="<%= WebPath.GetPath("UserControls/Management/LdapSettings/css/Default/ldapsettings.css") %>"/>

<div class="ldap-settings-main-container <% if (WorkContext.IsMono) { %>ldap-settings-is-mono<% } %> <% if (Settings.IsDefault) { %> ldap-settings-is-default<% } %>">
    <div class="header-base ldap-settings-title"><%= Resource.LdapSettingsTitle %></div>
    <p class="ldap-settings-descr"><%: Resource.LdapSettingsDscr %></p>
    <div class="ldap-settings_warnings">
            <h3 class="header-base red-text"><%: Resource.NoteShouldKnowHeader %></h3>
            <p><%: Resource.NoteLDAPShouldKnow %></p>
        </div>
    <label class="ldap-settings-label-checkbox ldap-settings-never-disable">
        <input id="ldapSettingsCheckbox" type="checkbox" <% if (Settings.EnableLdapAuthentication) { %> checked <% } %>><%= Resource.EnableLDAPAuthentication %>
    </label>
    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'EnableLDAPAuthenticationHelper'});"></span>
    <div class="popup_helper" id="EnableLDAPAuthenticationHelper">
        <p><%: Resource.LdapSettingsEnableLdapAuthenticationHelper %></p>
    </div>
    <div class="ldap-settings-user-container clearFix <% if (!Settings.EnableLdapAuthentication) { %>ldap-settings-disabled<% } %>">
        <div class="ldap-settings-column">
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsServer %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsServerHelper'});"></span>
                </div>
                <input id="ldapSettingsServer" class="textEdit" type="text" value="<%= Settings.Server %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsServerHelper">
                <p><%: Resource.LdapSettingsServerHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsUserDN %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserDNHelper'});"></span>
                </div>
                <input id="ldapSettingsUserDN" class="textEdit" type="text" value="<%= Settings.UserDN %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsUserDNHelper">
                <p><%: Resource.LdapSettingsUserDNHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsLoginAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsLoginAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsLoginAttribute" class="textEdit" type="text" value="<%= Settings.LoginAttribute %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsLoginAttributeHelper">
                <p><%: Resource.LdapSettingsLoginAttributeHelper %></p>
            </div>
            <div class="ldap-settings-block">
                <div class="ldap-settings-text"><%= Resource.LdapSettingsSecondNameAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsSecondNameAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsSecondNameAttribute" class="textEdit" type="text" value="<%= Settings.SecondNameAttribute %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsSecondNameAttributeHelper">
                <p><%: Resource.LdapSettingsSecondNameAttributeHelper %></p>
            </div>
            <div class="ldap-settings-block">
                <div class="ldap-settings-text"><%= Resource.LdapSettingsMailAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsMailAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsMailAttribute" class="textEdit" type="text" value="<%= Settings.MailAttribute %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsMailAttributeHelper">
                <p><%: Resource.LdapSettingsMailAttributeHelper %></p>
            </div>
            <div class="ldap-settings-block">
                <div class="ldap-settings-text"><%= Resource.LdapSettingsMobilePhoneAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsMobilePhoneAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsMobilePhoneAttribute" class="textEdit" type="text" value="<%= Settings.MobilePhoneAttribute %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsMobilePhoneAttributeHelper">
                <p><%: Resource.LdapSettingsMobilePhoneAttributeHelper %></p>
            </div>
        </div>
        <div class="ldap-settings-second-column">
            <div class="ldap-settings-block requiredField">
                <span id="ldapSettingsPortNumberError" class="requiredErrorText"></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsPortNumber %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsPortNumberHelper'});"></span>
                </div>
                <input id="ldapSettingsPortNumber" class="textEdit" type="text" value="<%= Settings.PortNumber %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsPortNumberHelper">
                <p><%: Resource.LdapSettingsPortNumberHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsUserFilter %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserFilterHelper'});"></span>
                </div>                
                <input id="ldapSettingsUserFilter" class="textEdit" type="text" value="<%= Settings.UserFilter %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsUserFilterHelper">
                <p><%: Resource.LdapSettingsUserFilterHelper %></p>
            </div>
            <div class="ldap-settings-block">
                <div class="ldap-settings-text"><%= Resource.LdapSettingsFirstNameAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsFirstNameAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsFirstNameAttribute" class="textEdit" type="text" value="<%= Settings.FirstNameAttribute %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsFirstNameAttributeHelper">
                <p><%: Resource.LdapSettingsFirstNameAttributeHelper %></p>
            </div>
            <div class="ldap-settings-block">
                <div class="ldap-settings-text"><%= Resource.LdapSettingsTitleAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsTitleAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsTitleAttribute" class="textEdit" type="text" value="<%= Settings.TitleAttribute %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsTitleAttributeHelper">
                <p><%: Resource.LdapSettingsTitleAttributeHelper %></p>
            </div>
            <div class="ldap-settings-block">
                <div class="ldap-settings-text"><%= Resource.LdapSettingsLocationAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsLocationAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsLocationAttribute" class="textEdit" type="text" value="<%= Settings.LocationAttribute %>" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsLocationAttributeHelper">
                <p><%: Resource.LdapSettingsLocationAttributeHelper %></p>
            </div>
        </div>
    </div>
    <label class="ldap-settings-label-checkbox <% if (!Settings.EnableLdapAuthentication) { %>ldap-settings-disabled<% } %>">
        <input id="ldapSettingsGroupCheckbox" type="checkbox" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %><% if (Settings.GroupMembership) { %> checked <% } %>><%= Resource.LdapSettingsGroupMembership %>
    </label>
    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupMembershipHelper'});"></span>
    <div class="popup_helper" id="LdapSettingsGroupMembershipHelper">
        <p><%: Resource.LdapSettingsGroupMembershipHelper %></p>
    </div>
    <div class="ldap-settings-group-container clearFix <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership) { %>ldap-settings-disabled<% } %>">
        <div class="ldap-settings-column">
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsGroupDN %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupDNHelper'});"></span>
                </div>
                <input id="ldapSettingsGroupDN" class="textEdit" type="text" value="<%= Settings.GroupDN %>"
                    <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership) { %> disabled <% } %>>
            </div>
             <div class="popup_helper" id="LdapSettingsGroupDNHelper">
                <p><%: Resource.LdapSettingsGroupDNHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsUserAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsUserAttribute" class="textEdit" type="text" value="<%= Settings.UserAttribute %>"
                    <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsUserAttributeHelper">
                <p><%: Resource.LdapSettingsUserAttributeHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsGroupNameAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupNameAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsGroupNameAttribute" class="textEdit" type="text" value="<%= Settings.GroupNameAttribute %>"
                    <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsGroupNameAttributeHelper">
                <p><%: Resource.LdapSettingsGroupNameAttributeHelper %></p>
            </div>
        </div>
        <div class="ldap-settings-second-column">
            <div class="ldap-settings-block">
                <div class="ldap-settings-text"><%= Resource.LdapSettingsGroupFilter %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupFilterHelper'});"></span>
                </div>
                <input id="ldapSettingsGroupFilter" class="textEdit" type="text" value="<%= Settings.GroupFilter %>"
                    <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsGroupFilterHelper">
                <p><%: Resource.LdapSettingsGroupFilterHelper %></p>
            </div>
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsGroupAttribute %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupAttributeHelper'});"></span>
                </div>
                <input id="ldapSettingsGroupAttribute" class="textEdit" type="text" value="<%= Settings.GroupAttribute %>"
                    <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsGroupAttributeHelper">
                <p><%: Resource.LdapSettingsGroupAttributeHelper %></p>
            </div>
        </div>
    </div>
    <% if (!WorkContext.IsMono) { %>
    <label class="ldap-settings-label-checkbox  <% if (!Settings.EnableLdapAuthentication) { %>ldap-settings-disabled<% } %>">
        <input id="ldapSettingsAuthenticationCheckbox" type="checkbox" <% if (!Settings.EnableLdapAuthentication) { %> disabled <% } %> 
            <% if (Settings.Authentication) { %> checked <% } %>><%= Resource.LdapSettingsAuthentication %>
    </label>
    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsAuthenticationHelper'});"></span>
    <div class="popup_helper" id="LdapSettingsAuthenticationHelper">
        <p><%: Resource.LdapSettingsAuthenticationHelper %></p>
    </div>
    <% } %>
    <div class="ldap-settings-auth-container clearFix
        <% if (!Settings.EnableLdapAuthentication || !Settings.Authentication) { %>ldap-settings-disabled<% } %>">
        <div class="ldap-settings-column">
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsLogin %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserNameHelper'});"></span>
                </div>
                <input id="ldapSettingsLogin" class="textEdit" type="text" value="<%= Settings.Login %>"
                    <% if (!Settings.EnableLdapAuthentication || !Settings.Authentication) { %> disabled <% } %>>
            </div>
        </div>
        <div class="popup_helper" id="LdapSettingsUserNameHelper">
            <p><%= String.Format(Resource.LdapSettingsUserNameHelper.HtmlEncode(),
               WorkContext.IsMono ? Resource.LdapSettingsMonoLogin : Resource.LdapSettingsLogin) %></p>
        </div>
        <div class="ldap-settings-second-column">
            <div class="ldap-settings-block requiredField">
                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                <div class="ldap-settings-text"><%= Resource.LdapSettingsPassword %>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsPasswordHelper'});"></span>
                </div>
                <input id="ldapSettingsPassword" class="textEdit" type="password"
                    value="" <% if (!Settings.EnableLdapAuthentication || !Settings.Authentication) { %> disabled <% } %>>
            </div>
            <div class="popup_helper" id="LdapSettingsPasswordHelper">
                <p><%: Resource.LdapSettingsPasswordHelper %></p>
            </div>
        </div>
    </div>
    <div class="middle-button-container">
        <a class="button blue disable ldap-settings-save" title="<%= Resource.LdapSettingsSaveSettings %>"><%= Resource.SaveButton %></a>
        <span class="splitter-buttons"></span>
        <a class="button gray ldap-settings-restore-default-settings <% if (Settings.IsDefault) { %>disable<% } %>" title="<%= Resource.LdapSettingsRestoreDefaultSettings %>">
            <%: Resource.LdapSettingsRestoreDefault %></a>
        <span class="splitter-buttons"></span>
        <a class="button gray ldap-settings-sync-users <% if (Settings.IsDefault) { %>disable<% } %>" title="<%= Resource.LdapSettingsSyncUsers %>">
            <%: Resource.LdapSettingsSyncUsers %></a>
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
                <%: Resource.LdapSettingsImportUserLimitDecision %>
                <div class="big-button-container">
                    <a class="blue button middle ldap-settings-ok"><%= Resource.ImportUserLimitOkButtons %></a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle ldap-settings-cancel"><%= Resource.ImportContactsCancelButton %></a>
                </div>
            </Body>
        </sc:Container>
    </div>
</div>