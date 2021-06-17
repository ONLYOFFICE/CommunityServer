<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LdapSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.LdapSettings" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<div id="ldapBlock" class="<%= !isAvailable ? "disable" : "" %> <% if (WorkContext.IsMono)
    { %>ldap-settings-is-mono<% } %> <% if (Settings.IsDefault)
    { %> ldap-settings-is-default<% } %>">
    <div class="clearFix">
        <div class="settings-block">
            <div class="header-base ldap-settings-title uppercase"><%= Resource.LdapSettingsHeader %></div>
            <div class="clearFix">
                <label class="ldap-settings-label-checkbox ldap-settings-never-disable">
                    <a id="ldapSettingsCheckbox" class="on_off_button 
                        <% if (!Settings.EnableLdapAuthentication) 
                            { %> off <% }
                        else
                            { %> on <% } 
                        %>"
                    ></a>
                    <span class="settings-checkbox-text"><%= Resource.EnableLDAPAuthentication %></span>

                </label>
                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'EnableLDAPAuthenticationHelper'});"></span>
            </div>

            <div class="ldap-settings-main-container clearFix">
                <div class="header-base ldap-settings-title"><%= Resource.LdapSettingsTitleNew %></div>
                <a class="link dotted ldap-settings-spoiler-link" title="<%= Resource.Show%>"><%= Resource.Show%></a>

                <div id="ldapSettingsSpoiler" class="display-none">

                    <div class="ldap-settings-security-container clearFix <% if (!Settings.EnableLdapAuthentication)
                        { %>
                                ldap-settings-disabled <% } %>">
                        <div class="clearFix">
                            <label class="ldap-settings-label-checkbox">
                                <input id="ldapSettingsSslCheckbox" type="checkbox" <% if (!Settings.EnableLdapAuthentication)
                                    { %>
                                    disabled <% } %> <% if (Settings.Ssl) {  %> checked <% } %> ><%= Resource.EnableLdapSSL %>
                            </label>
                            <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SettingsSslCheckboxHelper'});"></span>
                            <div class="popup_helper" id="SettingsSslCheckboxHelper">
                                <p><%: Resource.LdapSettingsSslCheckboxHelper %></p>
                            </div>
                        </div>

                        <div class="clearFix">
                            <label class="ldap-settings-label-checkbox ">
                                <input id="ldapSettingsStartTLSCheckbox" type="checkbox" <% if (!Settings.EnableLdapAuthentication)
                                    { %>
                                    disabled <% } %> <% if (Settings.StartTls) {  %> checked <% } %> ><%= Resource.EnableLdapStartTLS %>
                            </label>
                            <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SettingsStartTLSCheckboxHelper'});"></span>
                            <div class="popup_helper" id="SettingsStartTLSCheckboxHelper">
                                <p><%: Resource.LdapSettingsStartTLSCheckboxHelper %></p>
                            </div>
                        </div>
                    </div>



                    <div class="ldap-settings-user-container <% if (!Settings.EnableLdapAuthentication)
                        { %>
                                ldap-settings-disabled <% } %>">
                        <div class="ldap-settings-block requiredField">
                            <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                            <div class="ldap-settings-text">
                                <span class="ldap-settings-text-title"><%= Resource.LdapSettingsServer %>:</span>
                                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsServerHelper'});"></span>
                            </div>
                            <input id="ldapSettingsServer" class="textEdit" type="text" value="<%= Settings.Server %>" <% if (!Settings.EnableLdapAuthentication)
                                { %>
                                disabled <% } %>>
                        </div>
                        <div class="popup_helper" id="LdapSettingsServerHelper">
                            <p><%: Resource.LdapSettingsServerHelper %></p>
                        </div>

                        <div class="ldap-settings-block requiredField">
                            <span id="ldapSettingsPortNumberError" class="requiredErrorText"></span>
                            <div class="ldap-settings-text">
                                <span class="ldap-settings-text-title"><%= Resource.LdapSettingsPortNumber %>:</span>
                                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsPortNumberHelper'});"></span>
                            </div>
                            <input id="ldapSettingsPortNumber" class="textEdit" type="text" value="<%= Settings.PortNumber %>" <% if (!Settings.EnableLdapAuthentication)
                                { %>
                                disabled <% } %>>
                        </div>
                        <div class="popup_helper" id="LdapSettingsPortNumberHelper">
                            <p><%: Resource.LdapSettingsPortNumberHelper %></p>
                        </div>


                        <div class="ldap-settings-block requiredField">
                            <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                            <div class="ldap-settings-text">
                                <span class="ldap-settings-text-title"><%= Resource.LdapSettingsUserDN %>:</span>
                                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserDNHelper'});"></span>
                            </div>
                            <input id="ldapSettingsUserDN" class="textEdit" type="text" value="<%= Settings.UserDN %>" <% if (!Settings.EnableLdapAuthentication)
                                { %>
                                disabled <% } %>>
                        </div>
                        <div class="popup_helper" id="LdapSettingsUserDNHelper">
                            <p><%: Resource.LdapSettingsUserDNHelper %></p>
                        </div>

                        <div class="ldap-settings-block requiredField">
                            <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                            <div class="ldap-settings-text">
                                <span class="ldap-settings-text-title"><%= Resource.LdapSettingsLoginAttribute %>:</span>
                                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsLoginAttributeHelper'});"></span>
                            </div>
                            <input id="ldapSettingsLoginAttribute" class="textEdit" type="text" value="<%= Settings.LoginAttribute %>" <% if (!Settings.EnableLdapAuthentication)
                                { %>
                                disabled <% } %>>
                        </div>
                        <div class="popup_helper" id="LdapSettingsLoginAttributeHelper">
                            <p><%: Resource.LdapSettingsLoginAttributeHelper %></p>
                        </div>

                        <div class="ldap-settings-block requiredField">
                            <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                            <div class="ldap-settings-text">
                                <span class="ldap-settings-text-title"><%= Resource.LdapSettingsUserFilter %>:</span>
                                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserFilterHelper'});"></span>
                            </div>
                            <textarea id="ldapSettingsUserFilter" class="textBox" <% if (!Settings.EnableLdapAuthentication)
                                { %>
                                disabled <% } %>> <%= Settings.UserFilter %> </textarea>
                        </div>
                        <div class="popup_helper" id="LdapSettingsUserFilterHelper">
                            <p><%: Resource.LdapSettingsUserFilterHelper %></p>
                        </div>
                    </div>





                    <div class="ldap-settings-user-container ldap-attributes-container <% if (!Settings.EnableLdapAuthentication)
                        { %>
                                ldap-settings-disabled <% } %>">

                        <div class="ldap-settings-text">
                            <%= Resource.LdapAttributeHeader %>
                            <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapAttributeHeaderHelper'});"></span>
                        </div>
                        <div class="popup_helper" id="LdapAttributeHeaderHelper">
                            <p><%: Resource.LdapAttributeHeaderHelper %></p>
                        </div>
                        <div id="ldapMappingSettings" data-value='<%= LdapMapping %>'></div>
                        <a id="ldapMappingAddBtn" class="button gray <% if (Settings.IsDefault)
                            { %>disable<% } %>"
                            title="<%= Resource.LdapSettingsAddAttr %>"><%= Resource.LdapSettingsAddAttr %></a>

                    </div>



                    <div class="ldap-settings-group-membership clearFix">
                        <label class="ldap-settings-label-checkbox">
                            <a id="ldapSettingsGroupCheckbox" class="on_off_button 
                                    <% if (Settings.GroupMembership)
                                { %> on <% }
                                else
                                {%> off <%}%>"></a>
                            <span class="settings-checkbox-text"><%= Resource.LdapSettingsGroupMembership %></span>

                        </label>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupMembershipHelper'});"></span>
                        <div class="popup_helper" id="LdapSettingsGroupMembershipHelper">
                            <p><%: Resource.LdapSettingsGroupMembershipHelper %></p>
                        </div>
                        <div class="ldap-settings-group-container clearFix <% if (!Settings.EnableLdapAuthentication)
                            { %>ldap-settings-disabled<% } %>">
                            <div class="ldap-settings-block requiredField clearFix">
                                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                                <div class="ldap-settings-text">
                                    <span class="ldap-settings-text-title"><%= Resource.LdapSettingsGroupDN %>:</span>
                                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupDNHelper'});"></span>
                                </div>
                                <input id="ldapSettingsGroupDN" class="textEdit" type="text" value="<%= Settings.GroupDN %>"
                                    <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership)
                                    { %>
                                    disabled <% } %>>
                            </div>


                            <div class="ldap-settings-block requiredField clearFix">
                                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                                <div class="ldap-settings-text">
                                    <span class="ldap-settings-text-title"><%= Resource.LdapSettingsUserAttribute %>:</span>
                                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserAttributeHelper'});"></span>
                                </div>
                                <input id="ldapSettingsUserAttribute" class="textEdit" type="text" value="<%= Settings.UserAttribute %>"
                                    <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership)
                                    { %>
                                    disabled <% } %>>
                            </div>
                            <div class="popup_helper" id="LdapSettingsUserAttributeHelper">
                                <p><%: Resource.LdapSettingsUserAttributeHelper %></p>
                            </div>

                            <div class="ldap-settings-block requiredField clearFix">
                                <div class="ldap-settings-text">
                                    <span class="ldap-settings-text-title"><%= Resource.LdapSettingsGroupFilter %>:</span>
                                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupFilterHelper'});"></span>
                                </div>
                                <input id="ldapSettingsGroupFilter" class="textEdit" type="text" value="<%= Settings.GroupFilter %>"
                                    <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership)
                                    { %>
                                    disabled <% } %>>
                            </div>
                            <div class="popup_helper" id="LdapSettingsGroupFilterHelper">
                                <p><%: Resource.LdapSettingsGroupFilterHelper %></p>
                            </div>

                            <div class="ldap-settings-block requiredField clearFix">
                                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                                <div class="ldap-settings-text">
                                    <span class="ldap-settings-text-title"><%= Resource.LdapSettingsGroupNameAttribute %>:</span>
                                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupNameAttributeHelper'});"></span>
                                </div>
                                <input id="ldapSettingsGroupNameAttribute" class="textEdit" type="text" value="<%= Settings.GroupNameAttribute %>"
                                    <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership)
                                    { %>
                                    disabled <% } %>>
                            </div>
                            <div class="popup_helper" id="LdapSettingsGroupNameAttributeHelper">
                                <p><%: Resource.LdapSettingsGroupNameAttributeHelper %></p>
                            </div>

                            <div class="ldap-settings-block requiredField clearFix">
                                <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                                <div class="ldap-settings-text">
                                    <span class="ldap-settings-text-title"><%= Resource.LdapSettingsGroupAttribute %>:</span>
                                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsGroupAttributeHelper'});"></span>
                                </div>
                                <input id="ldapSettingsGroupAttribute" class="textEdit" type="text" value="<%= Settings.GroupAttribute %>"
                                    <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership)
                                    { %>
                                    disabled <% } %>>
                            </div>
                            <div class="popup_helper" id="LdapSettingsGroupAttributeHelper">
                                <p><%: Resource.LdapSettingsGroupAttributeHelper %></p>
                            </div>
                        </div>
                    </div>





                    <% if (!WorkContext.IsMono)
                        { %>
                    <div class="ldap-settings-user-container ldap-settings-authentication clearFix <% if (!Settings.EnableLdapAuthentication)
                        { %>ldap-settings-disabled<% } %>">
                        <label class="ldap-settings-label-checkbox">
                            <a id="ldapSettingsAuthenticationCheckbox" class="on_off_button <% if (Settings.Authentication)
                                { %> on <% }
                                else
                                { %> off <% }
                                if (!Settings.EnableLdapAuthentication)
                                { %> disable <% } %>"></a>
                            <span class="settings-checkbox-text"><%= Resource.LdapSettingsAuthentication %></span>

                        </label>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsAuthenticationHelper'});"></span>
                        <div class="popup_helper" id="LdapSettingsAuthenticationHelper">
                            <p><%: Resource.LdapSettingsAuthenticationHelper %></p>
                        </div>
                        <div class="ldap-settings-auth-container clearFix <% if (!Settings.EnableLdapAuthentication || !Settings.Authentication)
                            { %>ldap-settings-disabled<% } %>">

                            <div class="ldap-settings-block clearFix">
                                <div class="ldap-settings-block requiredField">
                                    <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                                    <div class="ldap-settings-text">
                                        <span class="ldap-settings-text-title"><%= Resource.LdapSettingsLogin %>:</span>
                                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsUserNameHelper'});"></span>
                                    </div>
                                    <input id="ldapSettingsLogin" class="textEdit" type="text" value="<%= Settings.Login %>"
                                        <% if (!Settings.EnableLdapAuthentication || !Settings.Authentication)
                                        { %>
                                        disabled <% } %>>
                                </div>
                            </div>
                            <div class="popup_helper" id="LdapSettingsUserNameHelper">
                                <p>
                                    <%= String.Format(Resource.LdapSettingsUserNameHelper.HtmlEncode(), WorkContext.IsMono ? Resource.LdapSettingsMonoLogin : Resource.LdapSettingsLogin) %>
                                </p>
                            </div>

                            <div class="ldap-settings-second-block clearFix">
                                <div class="ldap-settings-block requiredField">
                                    <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
                                    <div class="ldap-settings-text">
                                        <span class="ldap-settings-text-title"><%= Resource.LdapSettingsPassword %>:</span>
                                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsPasswordHelper'});"></span>
                                    </div>
                                    <input id="ldapSettingsPassword" class="textEdit" type="password"
                                        value="" <% if (!Settings.EnableLdapAuthentication || !Settings.Authentication)
                                        { %>
                                        disabled <% } %>>
                                </div>
                                <div class="popup_helper" id="LdapSettingsPasswordHelper">
                                    <p><%: Resource.LdapSettingsPasswordHelper %></p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <% } %>



                    <div class="ldap-settings-advanced-container clearFix">
                        <div class="ldap-settings-block">
                            <div class="ldap-settings-text">
                                <%= Resource.LdapAdvancedSettings %>
                            </div>
                            <div class="clearFix">
                                <label class="ldap-settings-label-checkbox ldap-settings-never-disable">
                                    <input id="ldapSettingsSendWelcomeEmail" type="checkbox"  <% if (Settings.SendWelcomeEmail) {  %> checked <% } %> ><%= Resource.LdapSettingsSendWelcomeEmail %>
                                </label>
                                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ldapSettingsSendWelcomeEmailHelper'});"></span>
                            </div>
                            <div class="popup_helper" id="ldapSettingsSendWelcomeEmailHelper">
                                <p><%: Resource.ldapSettingsSendWelcomeEmailHelper %></p>
                            </div>
                        </div>
                    </div>

                    <div class="middle-button-container">
                        <a class="button blue disable ldap-settings-save" title="<%= Resource.LdapSettingsSaveSettings %>"><%= Resource.SaveButton %></a>
                        <span class="splitter-buttons"></span>
                        <a id="ldapSettingsRestoreBtn" class="button gray ldap-settings-restore-default-settings <% if (!Settings.EnableLdapAuthentication)
                            { %>disable<% } %>"
                            title="<%= Resource.LdapSettingsRestoreDefaultSettings %>">
                            <%: Resource.LdapSettingsRestoreDefault %></a>

                    </div>
                    <div class="ldap-settings-progressbar-container" style="visibility: hidden;">
                        <div class="asc-progress-wrapper">
                            <div class="asc-progress-value" style="width: 0%;"></div>
                        </div>
                        <div style="padding-top: 2px;" class="text-medium-describe">
                            <span id="ldapSettingsStatus"></span>
                            <span id="ldapSettingsPercent">0% </span>
                            <span id="ldapSettingsSource"></span>
                        </div>
                    </div>
                    <div id="ldapSettingsError" class="errorText display-none"></div>
                </div>
            </div>
        </div>
        <div class="settings-help-block">
            <% if (!isAvailable)
                { %>
                    <p>
                        <%= Resource.ErrorNotAllowedOption %>
                    </p>
                    <% if (TenantExtra.EnableTariffSettings)
                        { %>
                    <a href="<%= TenantExtra.GetTariffPageLink() %>" target="_blank">
                        <%= Resource.ViewTariffPlans %></a>
                    <% }
                }
                else
                { %>
                    <p><%: Resource.LdapSettingsDescription %></p>
                <%} %>

        </div>
    </div>
    <div class="clearFix">
        <div class="settings-block">
            <div id="ldapSettingsCronContainer" class="settings-block">
                <div class="header-base ldap-settings-title"><%= Resource.LdapSettingsSyncDataHeader %></div>
                <div class="clearFix">
                    <label class="ldap-settings-label-checkbox ldap-settings-never-disable">
                        <a id="ldapSettingsAutoSyncBtn" class="on_off_button off <% if (!Settings.EnableLdapAuthentication)
                            { %>disable<% } %>"></a>
                        <span class="settings-checkbox-text"><%= Resource.LdapSettingsAutoSync %></span>
                        
                    </label>
                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LdapSettingsAutoSyncHelper'});"></span>
                    <a class="link dotted display-none" id="ldapCronEditLink" style="display: none;"><%= Resource.EditButton %></a>
                </div>
                <div class="popup_helper" id="LdapSettingsAutoSyncHelper">
                    <p><%: Resource.LdapSettingsAutoSyncHelper %></p>
                </div>

                <span id="ldapCronHumanText" style="display: none;">
                    <%= Resource.LdapNextSync %>
                    <span class="cronHumanReadable"></span>
                </span>
                <a id="ldapSettingsSyncBtn" class="button blue ldap-settings-sync-users <% if (!Settings.EnableLdapAuthentication)
                    { %>disable<% } %>"
                    title="<%= Resource.LdapSettingsSyncUsers %>">
                    <%: Resource.LdapSettingsSyncUsers %></a>
                <div class="ldap-settings-sync-progressbar-container" style="visibility: hidden;">
                    <div class="asc-progress-wrapper">
                        <div class="asc-progress-value" style="width: 0%;"></div>
                    </div>
                    <div class="text-medium-describe" style="padding-top: 2px;">
                        <span id="ldapSettingsSyncStatus"></span>
                        <span id="ldapSettingsSyncPercent">0% </span>
                        <span id="ldapSettingsSyncSource"></span>
                    </div>
                </div>
                <div id="ldapSettingsSyncError" class="errorText display-none"></div>
            </div>
        </div>
        <% if (isAvailable)
            { %>
                <div class="settings-help-block">
                    <p><%: Resource.LdapSettingsSyncDescription %></p>
                </div>
        <%  } %>
    </div>
    <div class="popup_helper" id="EnableLDAPAuthenticationHelper">
        <p><%: Resource.LdapSettingsEnableLdapAuthenticationHelper %></p>
    </div>


    <div class="ldap-settings-group-container clearFix <% if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership)
        { %>ldap-settings-disabled<% } %>">
        <div class="ldap-settings-column">

            <div class="popup_helper" id="LdapSettingsGroupDNHelper">
                <p><%: Resource.LdapSettingsGroupDNHelper %></p>
            </div>


        </div>
        <div class="ldap-settings-second-column">
        </div>
    </div>

   
    <div id="ldapSettingsTurnOffDialog" class="display-none">
        <sc:Container runat="server" ID="ldapSettingsTurnOffContainer">
            <Header>
                <div><%= Resource.ConfirmationTitle %></div>
            </Header>
            <Body>
                <%: Resource.ShortCancelConfirmMessage %>
                <div class="big-button-container">
                    <a class="button blue middle ldap-settings-ok"><%= Resource.OKButton %></a>
                    <a class="button gray middle ldap-settings-cancel"><%= Resource.CancelButton %></a>
                </div>
            </Body>
        </sc:Container>
    </div>
    <div id="ldapSettingsCronDialog" class="display-none">
        <sc:Container runat="server" ID="ldapSettingsCronPanel">
            <Header>
                <div><%= Resource.SettingsCronTitle %></div>
            </Header>
            <Body>
                <%: Resource.LdapSettingsCronDecision %>
                <div id="ldapSettingsAutoSyncCron"></div>
                <input id="ldapSettingsAutoSyncCronInput" type="hidden" />

                <div class="cronHumanReadableContainer">
                    <%= Resource.LdapNextSync %>
                    <span class="cronHumanReadable"></span>
                </div>


                <a class="button blue middle ldap-sync-save"><%= Resource.OKButton %></a>
                <a class="button gray middle ldap-settings-cancel"><%= Resource.CancelButton %></a>
            </Body>
        </sc:Container>
    </div>
    <div id="ldapSettingsCronTurnOffDialog" class="display-none">
        <sc:Container runat="server" ID="ldapSettingsCronTurnOffContainer">
            <Header>
                <div><%= Resource.ConfirmationTitle %></div>
            </Header>
            <Body>
                <%: Resource.ShortCancelConfirmMessage %>
                <div class="big-button-container">
                    <a class="button blue middle ldap-settings-ok"><%= Resource.OKButton %></a>
                    <a class="button gray middle ldap-settings-cancel"><%= Resource.CancelButton %></a>
                </div>

            </Body>
        </sc:Container>
    </div>
    <div id="ldapSettingsCertificateValidationDialog" class="display-none">
        <sc:Container runat="server" ID="ldapSettingsCertificateValidationContainer">
            <Header>
                <div><%= Resource.CertificateConfirm %></div>
            </Header>
            <Body>
                <div class="ldap-settings-crt-details ldap-settings-crt-serial-number"><%= Resource.LdapSettingsSerialNumber %>: <span class="ldap-settings-crt-details-val ldap-settings-serial-number"></span></div>
                <div class="ldap-settings-crt-details"><%= Resource.LdapSettingsIssuerName %>: <span class="ldap-settings-crt-details-val ldap-settings-issuer-name"></span></div>
                <div class="ldap-settings-crt-details"><%= Resource.LdapSettingsSubjectName %>: <span class="ldap-settings-crt-details-val ldap-settings-subject-name"></span></div>
                <div class="ldap-settings-crt-details"><%= Resource.LdapSettingsValidFrom %>: <span class="ldap-settings-crt-details-val ldap-settings-valid-from"></span></div>
                <div class="ldap-settings-crt-details"><%= Resource.LdapSettingsValidUntil %>: <span class="ldap-settings-crt-details-val ldap-settings-valid-until"></span></div>
                <div class="ldap-settings-crt-details ldap-settings-crt-details-last"><%= Resource.LdapSettingsUniqueHash %>: <span class="ldap-settings-crt-details-val ldap-settings-unique-hash"></span></div>
                <%= Resource.AddCertificateToStoreConfirmation %>
                <div class="big-button-container">
                    <a class="button blue middle ldap-settings-ok"><%= Resource.OKButton %></a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle ldap-settings-cancel"><%= Resource.CancelButton %></a>
                </div>
            </Body>
        </sc:Container>
    </div>
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
                <div><%= Resource.LdapSettingsImportConfirmationTitle %></div>
            </Header>
            <Body>
                <%: Resource.LdapSettingsImportConfirmation %>
                <div class="big-button-container">
                    <a class="blue button middle ldap-settings-ok"><%= Resource.OKButton %></a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle ldap-settings-cancel"><%= Resource.ImportContactsCancelButton %></a>
                </div>
            </Body>
        </sc:Container>
    </div>
</div>

<script id="ldapCrtErrorTmpl" type="text/x-jquery-tmpl">
  <div class="ldap-settings-crt-errors">{{each errors}}
    <div class="toast-container">
      <div class="toast toast-error" style="display: block;">
        <div class="toast-message">${message}</div>
      </div>
    </div>{{/each}}
  </div>
</script>

<script id="ldapMappingFieldTmpl" type="text/x-jquery-tmpl">
  <div class="ldap-mapping-row clear-fix requiredField">
    <div class="selectBox unselectable" data-value="${key}">
      <div class="selectBoxValue unselectable">${humanKey}</div>
      <div class="selectBoxSwitch"></div>
      <div class="selectOptionsBox">
        <div class="selectOptionsInnerBox fullwidth">{{html options}}</div>
      </div>
    </div>
    <input class="textEdit" value="${value}" type="text" placeholder="${placeholder}"/>{{if !required}}
    <span class="requiredErrorText"><%= Resource.LdapSettingsEmptyField %></span>
    <div class="ldap-mapping-remove-row remove-btn-icon">
        <span class="menu-item-icon">
            <svg width="16" height="16" viewBox="0 0 16 16" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M8 15C11.866 15 15 11.866 15 8C15 4.13401 11.866 1 8 1C4.13401 1 1 4.13401 1 8C1 11.866 4.13401 15 8 15ZM7.96899 12.9723C10.7133 12.9913 12.9533 10.782 12.9723 8.03775C12.9913 5.29348 10.7821 3.0534 8.03779 3.0344C5.29352 3.0154 3.05345 5.22467 3.03445 7.96894C3.01545 10.7132 5.22472 12.9533 7.96899 12.9723Z" />
                <path d="M5.5 5.5L10.5 10.5" stroke-width="2" />
                <path d="M5.5 10.5L10.5 5.5" stroke-width="2" />
            </svg>
        </span>
    </div>{{/if}}
  </div>
</script>