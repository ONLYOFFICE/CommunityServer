<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffStandalone.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffStandalone" %>

<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Billing" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="current-tariff-desc">
    <%= TariffDescription() %>

    <br />

    <% if (CurrentQuota.ActiveUsers != LicenseReader.MaxUserCount)
       { %>
    <br />
    <%= String.Format(Resource.TariffStatistics,
                      (PeopleModuleAvailable
                           ? "<a class=\"link-black-14 bold\" href=\"" + CommonLinkUtility.GetEmployees() + "\">" + UsersCount + "</a>"
                           : "<span class=\"bold\">" + UsersCount + "</span>")
                      + "/" + CurrentQuota.ActiveUsers) %>
    <% } %>

    <% if (CurrentQuota.CountPortals > 0 && CurrentQuota.CountPortals != LicenseReader.MaxUserCount)
       { %>
    <br />
    <%= String.Format(Resource.TariffPortalStatistics,
                      (TenantExtra.EnableControlPanel
                           ? "<a class=\"link-black-14 bold\" href=\"" + SetupInfo.ControlPanelUrl.TrimEnd('/') + "/multiportals" + "\" target=\"_blank\">" + TenantCount + "</a>"
                           : "<span class=\"bold\">" + TenantCount + "</span>")
                      + "/" + CurrentQuota.CountPortals) %>
    <% } %>
</div>

<div class="license-section">
    <span class="header-base"><%= UserControlsCommonResource.LicenseActivateAndGetV11.HtmlEncode() %></span>
</div>
<table class="license-list-v11" cellpadding="0" cellspacing="0">
    <tbody>
        <tr>
            <td>
                <div class="license-item license-item-pro"><%= UserControlsCommonResource.LicenseModulesProV11.HtmlEncode() %></div>
                <% if (false && !CoreContext.Configuration.CustomMode) { %>
                <div class="license-item license-item-private"><%= UserControlsCommonResource.LicenseModulesPrivateV11.HtmlEncode() %></div>
                <% } %>
                <div class="license-item license-item-mobile"><%= UserControlsCommonResource.LicenseModulesMobileV11.HtmlEncode() %></div>
                <div class="license-item license-item-update"><%= UserControlsCommonResource.LicenseModulesUpdatesV11.HtmlEncode() %></div>
                <div class="license-item license-item-support"><%= UserControlsCommonResource.LicenseModulesSupportV11.HtmlEncode() %></div>
            </td>
        </tr>
    </tbody>
</table>

<% if (!string.IsNullOrEmpty(Settings.BuyUrl))
   { %>
<div class="license-section">
    <span class="header-base"><%= UserControlsCommonResource.LicenseKeyBuyLabelV11 %></span>
</div>
<div><%= UserControlsCommonResource.LicenseKeyBuyDescrV11.HtmlEncode() %></div>

<div class="button-margin">
    <a href="<%= Settings.BuyUrl %>" class="button blue big" target="_blank"><%= Resource.TariffButtonBuy %></a>
</div>
<% } %>


<div class="license-section">
    <span class="header-base"><%= UserControlsCommonResource.LicenseKeyLabelV11 %></span>
</div>

<div id="activatePanel">
    <div><%= CoreContext.Configuration.CustomMode ? CustomModeResource.LicenseActivateDescrCustomMode.HtmlEncode() : UserControlsCommonResource.LicenseActivateDescrV11.HtmlEncode() %></div>
    <div class="button-margin clearFix">
        <input type="file" id="uploadButton" />
        <a id="licenseKey" class="button gray"><%= UserControlsCommonResource.UploadFile %></a>
        <span id="licenseKeyText"></span>
    </div>

    <% if (RequestLicenseAccept)
       { %>
    <div class="license-accept">
        <input type="checkbox" id="policyAccepted">
        <label for="policyAccepted">
            <%= string.Format(UserControlsCommonResource.LicenseAgreements,
                              "<a href=" + Settings.LicenseAgreementsUrl + " target=\"_blank\" class=\"link-black-12\">",
                              "</a>") %></label>
    </div>
    <% } %>

    <div class="button-margin">
        <a id="activateButton" class="button blue big disable">
            <%= UserControlsCommonResource.LicenseActivateButtonV11 %>
        </a>
    </div>
</div>

<% if (!string.IsNullOrEmpty(Settings.SalesEmail))
   { %>
<div class="license-questions">
    <%= string.Format(UserControlsCommonResource.SalesQuestionsV11,
                      string.Format("<a href=\"mailto:{0}\" class=\"link-black-12\">{0}</a>", Settings.SalesEmail)) %>
</div>
<% } %>

<% if (Settings.FeedbackAndSupportEnabled && !string.IsNullOrEmpty(Settings.FeedbackAndSupportUrl))
   { %>
<div class="support-questions">
    <%= string.Format(UserControlsCommonResource.SupportQuestionsV11,
                      string.Format("<a href=\"{0}\" target=\"_blank\" class=\"link-black-12\">{0}</a>", Settings.FeedbackAndSupportUrl)) %>
</div>
<% } %>

