<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SmsValidationSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SmsValidationSettings" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Sms" %>
<%@ Import Namespace="ASC.Web.Studio.Core.SMS" %>
<%@ Import Namespace="ASC.Web.Studio.Core.TFA" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>


<style>
    .line-height-min {
        line-height: 10px;
    }
</style>

<div class="clearFix <%= SmsEnable || TfaAppEnable ? "" : "disable" %>">
    <div id="studio_smsValidationSettings" class="settings-block">
        <div class="header-base">
            <%= Resource.SmsAuthTitle %>
        </div>

        <div class="sms-validation-settings">
            <div class="clearFix">
                <input type="radio" id="chk2FactorAuthDisable" name="chk2FactorAuth" <%= !StudioSmsNotificationSettings.Enable 
                                                                                        && !TfaAppAuthSettings.Enable ? "checked=\"checked\"" : "" %> />
                <label for="chk2FactorAuthDisable">
                    <%= Resource.ButtonSmsDisable %></label>
            </div>

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

            <% if (!CoreContext.Configuration.CustomMode)
               { %>
            <div class="clearFix">
                <input type="radio" id="chk2FactorAuthEnable" name="chk2FactorAuth" <%= StudioSmsNotificationSettings.Enable ? "checked=\"checked\"" : "" %>
                    <%= SmsEnable ? "" : "disabled='disabled'" %> />
                <label for="chk2FactorAuthEnable">
                    <%= Resource.ButtonSmsEnable %>
                    <% if (TenantExtra.Saas && SmsEnable) { %>
                    <span class="new-label-banner"><%= Resource.FreeSms %></span>
                    <% } %>
                </label>
            </div>
            <% } %>

            <div class="clearFix">
                <input type="radio" id="chk2FactorAppAuthEnable" name="chk2FactorAuth" <%= TfaAppAuthSettings.Enable ? "checked=\"checked\"" : "" %>  <%= TfaAppEnable ? "" : "disabled='disabled'" %>  />
                <label for="chk2FactorAppAuthEnable">
                    <%= Resource.ButtonTfaAppEnable %></label>
            </div>
            <div class="middle-button-container">
                <a id="chk2FactorAuthSave" class="button blue disable">
                    <%= Resource.SaveButton %></a>
            </div>
        </div>
    </div>

    <div class="settings-help-block">
        <p>
            <%= String.Format((CoreContext.Configuration.CustomMode ? CustomModeResource.SmsAuthDescriptionCustomMode : Resource.SmsAuthDescription).HtmlEncode(), "<b>", "</b>", "<br/>") %>
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
               else if (!CoreContext.Configuration.CustomMode)
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