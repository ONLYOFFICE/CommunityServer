<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImpersonateSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ImpersonateUser.ImpersonateSettings" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>

<% if (Available)
    { %>
<div id="studio_impersonateSettings" class="clearFix">
    <div  class="settings-block">

        <div class="header-section">
            <span class="header-base impersonateTitle clearFix">
                <%= Resource.ImpersonationSettings %>
                <span class="beta"></span>
                <span id="impersonateSettingsSpoiler" class="toggle-button" data-switcher="0" data-showtext="<%= Resource.Show%>" data-hidetext="<%= Resource.Hide%>"><%= Resource.Hide%></span>
            </span>
        </div>

        <div id="impersonateSettingsSpoilerBlock">

            <label class="impersonate-settings">
                <a id="impersonateSettingsCheckbox" class="on_off_button on"></a>
                <span class="settings-checkbox-text"><%= Resource.EnableImpersonation %></span>
            </label>

            <p><%= Resource.ImpersonateSettingsEnableHelp %></p>

            <div id="admins-access" class="clearFix">
                <div class="header-base-small">
                    <%= Resource.ImpersonationAccessForAdmins %>
                </div>

                <div>
                    <input type="radio" class="switcher" id="impersonateSettingsOff" name="settingsSwitch"/>
                    <label for="impersonateSettingsOff"> <%= Resource.DisableImpersonateSettings %></label>
                </div>

                <div>
                    <input type="radio" class="switcher" id="impersonateSettingsOn" name="settingsSwitch"/>
                    <label for="impersonateSettingsOn"><%= Resource.EnableImpersonateSettingsForAllAdmins %></label>
                </div>

                <div>
                    <input type="radio" class="switcher" id="impersonateSettingsOnWithLimits" name="settingsSwitch"/>
                    <label for="impersonateSettingsOnWithLimits"><%= Resource.EnableImpersonateSettingsWithLimits %></label>
                </div>

                <div id="impersonate-adminSelctorContent" style="display:none;">
                    <div id="adminSelector" class="link dotline plus"><%: CustomNamingPeople.Substitute<Resource>("AccessRightsAddUser") %></div>
                    <div id="selectedAdmins" class="clearFix"></div>
                </div>

            </div>

            <div id="restrictions-access" class="clearFix" style="display:none;">

                <div class="header-base-small">
                    <%= Resource.ImpersonationRestrictAccess %>
                </div>

                <div>
                    <input type="checkbox" id="onlyForOwnGroups" />
                    <label for="onlyForOwnGroups"><%= Resource.ImpersonationOnlyForOwnGroupsChb %></label>
                </div>

                <div>
                    <input type="checkbox" id="userRestrictions" />
                    <label for="userRestrictions"><%= Resource.ImpersonationRestrictUsersChb %></label>
                </div>

                <div id="impersonate-userSelectorContent" style="display:none;">
                    <div id="userSelector" class="link dotline plus"><%:CustomNamingPeople.Substitute<Resource>("AccessRightsAddUser")%></div>
                    <div id="selectedUsers" class="clearFix"></div>
                </div>

                <div id="impersonate-groupSelectorContent" style="display:none;">
                    <div id="groupSelector" class="link dotline plus"><%: CustomNamingPeople.Substitute<Resource>("AccessRightsAddGroup") %></div>
                    <div id="selectedGroups" class="clearFix"></div>
                </div>
            </div>

            <div class="middle-button-container">
                <a id="impersonateSettingSaveBtn" class="button blue"><%= Resource.SaveButton %></a>
            </div>
        </div>
    </div>

    <div class="settings-help-block">
        <p><%= string.Format(Resource.ImpersonationDescription.HtmlEncode(), "<b>", "</b>") %></p>
    </div>
</div>
<% } %>