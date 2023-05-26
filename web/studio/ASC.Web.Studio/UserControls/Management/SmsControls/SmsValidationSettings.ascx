<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SmsValidationSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SmsValidationSettings" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Sms" %>
<%@ Import Namespace="ASC.Web.Studio.Core.SMS" %>
<%@ Import Namespace="ASC.Web.Studio.Core.TFA" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>


<div class="clearFix <%= SmsEnable || TfaAppEnable ? "" : "disable" %>">
    <div id="studio_smsValidationSettings" class="settings-block">
        <div class="header-base">
            <%= Resource.SmsAuthTitle %>
        </div>

        <script id="restriction-tmpl" type="text/x-jquery-tmpl">
            <div class="restriction clearFix">
                <div>
                    <input value="${ip}" class="ip textEdit" />
                    <span class="menu-item-icon trash delete-btn icon-link"></span>
                </div>
            </div>
        </script>

        <div class="sms-validation-settings">
            <div class="clearFix">
                <input type="radio" id="chk2FactorAuthDisable" name="chk2FactorAuth" <%= !StudioSmsNotificationSettings.Enable 
                                                                                        && !TfaAppAuthSettings.Enable ? "checked=\"checked\"" : "" %> />
                <label for="chk2FactorAuthDisable"><%= Resource.ButtonSmsDisable %></label>
            </div>

            <% if (SmsVisible)
               { %>

            <% if (SmsEnable)
               { %>

            <% if (SmsProviderManager.ClickatellProvider.Enable() && SmsProviderManager.ClickatellProvider.CanSet)
               { %>
            <p class="line-height-min"><%= string.Format(Resource.SmsBalance, "Clickatell") %>: <span class="gray-text"><%= string.Format(Resource.SmsBalanceAccount, "clickatell") %></span></p>
            <% } %>

            <% if (SmsProviderManager.TwilioProvider.Enable() && SmsProviderManager.TwilioProvider.CanSet)
               { %>
            <p class="line-height-min"><%= string.Format(Resource.SmsBalance, "Twilio") %>: <span class="gray-text"><%= string.Format(Resource.SmsBalanceAccount, "twilio") %></span></p>
            <% } %>

            <% if (SmsProviderManager.SmscProvider.Enable() && SmsProviderManager.SmscProvider.CanSet)
               { %>
            <p class="line-height-min"><%= string.Format(Resource.SmsBalance, "SMSC") %>: <b><%= SmsProviderManager.SmscProvider.GetBalance() %></b></p>
            <% } %>

            <% } %>

            <div class="clearFix">
                <input type="radio" id="chk2FactorAuthEnable" name="chk2FactorAuth" <%= StudioSmsNotificationSettings.Enable ? "checked=\"checked\"" : "" %>
                    <%= SmsEnable ? "" : "disabled='disabled'" %> />
                <label for="chk2FactorAuthEnable">
                    <%= Resource.ButtonSmsEnable %>
                    <%--<% if (TenantExtra.Saas && SmsEnable) { %>
                    <span class="new-label-banner"><%= Resource.FreeSms %></span>
                    <% } %>--%>
                </label>
            </div>
            <% } %>

            <div class="clearFix">
                <input type="radio" id="chk2FactorAppAuthEnable" name="chk2FactorAuth" <%= TfaAppAuthSettings.Enable ? "checked=\"checked\"" : "" %>  <%= TfaAppEnable ? "" : "disabled='disabled'" %>  />
                <label id="chk2FactorAppAuthLabel" for="chk2FactorAppAuthEnable"><%= Resource.ButtonTfaAppEnable %></label>
            </div>

            <div class="sms-advanced-settings">

                <div id="hide-view" style="display:none;">
                    <span id="ShowAdvancedSettings" class="toggle-button" onclick="window.ASC.Controls.SmsValidationSettings.ShowAdvancedSettings();"><%= Resource.SmsValidationShowAdvancedSettings %></span>
                </div>

                <div id="show-view" style="display:none;">

                    <div class="header-base-small" id="mandatorys"><%= Resource.SmsValidationMandatoryBlockHeader %></div>

                    <div id="emptyUserListLabel" class="describe-text smsValidationEmptyUserList"><%= Resource.SmsValidationEmptyUserList %></div>
                    <div id="selectedUsers" class="clearFix"></div>
                    <div id="selectedGroups" class="clearFix"></div>

                    <div class="smsValidation-SelectorContent">
                        <div id="userSelector" class="link dotline plus"><%: CustomNamingPeople.Substitute<Resource>("AccessRightsAddUser") %></div>
                        <div id="groupSelector" class="link dotline plus"><%: CustomNamingPeople.Substitute<Resource>("AccessRightsAddGroup") %></div>
                    </div>

                    <div id="restrictions-list">
                        <div class="header-base-small"><%= Resource.SmsValidationTrustedBlockHeader%></div>
                        <div id="IpListInfo" class="describe-text"><%= Resource.SmsValidationTrustedBlockDescription %></div>
                        <span id="add-restriction-btn" class="link dotline plus"><%= Resource.SmsValidationAddTrustedIP %></span>
                    </div>

                    <span id="HideAdvancedSettings" class="toggle-button" onclick="window.ASC.Controls.SmsValidationSettings.ShowAdvancedSettingsButton();"><%= Resource.SmsValidationHideAdvancedSettings %></span>
                </div>
                
            </div>
            <div class="middle-button-container">
                <a id="chk2FactorAuthSave" class="button blue"><%= Resource.SaveButton %></a>
            </div>
        </div>
    </div>

    <div class="settings-help-block">
        <p>
            <%= String.Format((!SmsVisible ? CustomModeResource.SmsAuthDescriptionCustomMode : Resource.SmsAuthDescription).HtmlEncode(), "<b>", "</b>", "<br/>") %>
        </p>

        <% if (SmsVisible)
           { %>
        <p>
            <% if (SmsEnable)
               { %>
            <%= String.Format(Resource.SmsAuthNoteDescription.HtmlEncode(), "<b>", "</b>", "<br/>") %>
            <% }
               else if (!SmsAvailable)
               { %>
            <%= String.Format(Resource.SmsAuthNoteQuotaDescription.HtmlEncode(), "<b>", "</b>", "<br/>") %>
            <% }
               else if (ThirdPartyVisible)
               { %>
            <%= String.Format(Resource.SmsAuthNoteKeysDescription.HtmlEncode(), "<b>", "</b>", "<br/>", "<a href=\"" + CommonLinkUtility.GetAdministration(ManagementType.ThirdPartyAuthorization) + "\" target=\"_blank\">", "</a>") %>
            <% } %>
        </p>
        <% } %>

        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <p>
            <a href="<%= HelpLink + "/guides/two-factor-authentication.aspx" %>" target="_blank"><%= Resource.LearnMore %></a>
        </p>
        <% } %>
    </div>
</div>