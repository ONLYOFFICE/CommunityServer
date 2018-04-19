<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SmsValidationSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SmsValidationSettings" %>

<%@ Import Namespace="ASC.Thrdparty.Configuration" %>
<%@ Import Namespace="ASC.Web.Studio.Core.SMS" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<% if (IsEnableSmsValidation)
   { %>
<style>
    .line-height-min {
        line-height: 10px;
    }
</style>

<div class="clearFix <%= SmsEnable ? "" : "disable" %>">
    <div id="studio_smsValidationSettings" class="settings-block">
        <div class="header-base">
            <%= Resource.SmsAuthTitle %>
        </div>

        <div class="sms-validation-settings">
            <% if (SmsEnable)
               {
                   var empty = true; %>

            <% if (SmsProviderManager.ClickatellProvider.Enable() && KeyStorage.CanSet("clickatellapiKey"))
               {
                   empty = false; %>
            <p class="line-height-min"><%= string.Format(Resource.SmsBalance, "Clickatell") %>: <span class="gray-text"><%= string.Format(Resource.SmsBalanceAccount, "clickatell") %></span></p>
            <% } %>

            <% if (SmsProviderManager.TwilioProvider.Enable() && KeyStorage.CanSet("twilioAuthToken"))
               {
                   empty = false; %>
            <p class="line-height-min"><%= string.Format(Resource.SmsBalance, "Twilio") %>: <span class="gray-text"><%= string.Format(Resource.SmsBalanceAccount, "twilio") %></span></p>
            <% } %>

            <% if (SmsProviderManager.SmscProvider.Enable() && KeyStorage.CanSet("smscpsw"))
               {
                   empty = false; %>
            <p class="line-height-min"><%= string.Format(Resource.SmsBalance, "SMSC") %>: <b><%= SmsProviderManager.SmscProvider.GetBalance() %></b></p>
            <% } %>

            <% if (!empty)
               { %>
            <br />
            <% } %>
            <% } %>

            <div class="clearFix">
                <input type="radio" id="chk2FactorAuthEnable" name="chk2FactorAuth" <%= StudioSmsNotificationSettings.Enable ? "checked=\"checked\"" : "" %>
                    <%= SmsEnable ? "" : "disabled='disabled'" %> />
                <label for="chk2FactorAuthEnable">
                    <%= Resource.ButtonSmsEnable %></label>
            </div>
            <div class="clearFix">
                <input type="radio" id="chk2FactorAuthDisable" name="chk2FactorAuth" <%= !StudioSmsNotificationSettings.Enable ? "checked=\"checked\"" : "" %>
                    <%= SmsEnable ? "" : "disabled='disabled'" %> />
                <label for="chk2FactorAuthDisable">
                    <%= Resource.ButtonSmsDisable %></label>
            </div>
            <div class="middle-button-container">
                <a id="chk2FactorAuthSave" class="button blue <%= SmsEnable ? "" : "disable" %> />">
                    <%= Resource.SaveButton %></a>
            </div>
        </div>
    </div>

    <div class="settings-help-block">
        <p>
            <%= String.Format(Resource.SmsAuthDescription.HtmlEncode(), "<b>", "</b>", "<br/>") %>
        </p>
        <p>
            <% if (SmsEnable)
               { %>
            <%= String.Format(Resource.SmsAuthNoteDescription.HtmlEncode(), "<b>", "</b>", "<br/>") %>
            <% }
               else if (!StudioSmsNotificationSettings.IsVisibleSettings)
               { %>
            <%= String.Format(Resource.SmsAuthNoteQuotaDescription.HtmlEncode(), "<b>", "</b>", "<br/>") %>
            <% }
               else
               { %>
            <%= String.Format(Resource.SmsAuthNoteKeysDescription.HtmlEncode(), "<b>", "</b>", "<br/>", "<a href=\"" + CommonLinkUtility.GetAdministration(ManagementType.ThirdPartyAuthorization) + "\" target=\"_blank\">", "</a>") %>
            <% } %>
        </p>

        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <p>
            <a href="<%= HelpLink + "/guides/two-factor-authentication.aspx" %>" target="_blank"><%= Resource.LearnMore %></a>
        </p>
        <% } %>
    </div>
</div>
<% } %>