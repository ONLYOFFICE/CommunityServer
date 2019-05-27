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
    <span class="header-base"><%= UserControlsCommonResource.LicenseActivateAndGet.HtmlEncode() %></span>
</div>
<table class="license-list" cellpadding="0" cellspacing="0">
    <tbody>
        <tr>
            <td rowspan="5">
                <div class="license-item">
                    <div class="license-item-modules"><%= UserControlsCommonResource.LicenseModules.HtmlEncode() %></div>
                    <%= string.Format(CoreContext.Configuration.CustomMode ? CustomModeResource.LicenseModulesListCustomMode : UserControlsCommonResource.LicenseModulesList, "- ", "<br />") %>
                </div>
            </td>
            <td>
                <div class="license-item license-item-controlpanel"><%= UserControlsCommonResource.LicenseModulesControlPanel %></div>
                <div class="license-item license-item-multitenancy"><%= UserControlsCommonResource.LicenseModulesMultitenancy %></div>
                <div class="license-item license-item-updates"><%= UserControlsCommonResource.LicenseModulesUpdates.HtmlEncode() %></div>
                <div class="license-item license-item-support"><%= UserControlsCommonResource.LicenseModulesSupport %></div>
            </td>
        </tr>
    </tbody>
</table>

<% if (!string.IsNullOrEmpty(Settings.BuyUrl))
   { %>
<div class="license-section">
    <span class="header-base"><%= UserControlsCommonResource.LicenseKeyBuyLabel %></span>
</div>
<div><%= UserControlsCommonResource.LicenseKeyBuyDescr.HtmlEncode() %></div>

<div class="button-margin">
    <a href="<%= Settings.BuyUrl %>" class="button blue big" target="_blank"><%= Resource.TariffButtonBuy %></a>
</div>
<% } %>


<div class="license-section">
    <span class="header-base"><%= UserControlsCommonResource.LicenseKeyLabel %></span>
</div>

<div id="activatePanel">
    <div><%= CoreContext.Configuration.CustomMode ? CustomModeResource.LicenseActivateDescrCustomMode.HtmlEncode() : UserControlsCommonResource.LicenseActivateDescr.HtmlEncode() %></div>
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
                              "<a href=" + Settings.LicenseAgreementsUrl + " target=\"_blank\">",
                              "</a>") %></label>
    </div>
    <% } %>

    <div class="button-margin">
        <a id="activateButton" class="button blue big disable">
            <%= UserControlsCommonResource.LicenseActivateButton %>
        </a>
    </div>
</div>

<% if (!string.IsNullOrEmpty(Settings.SalesEmail))
   { %>
<div class="license-questions">
    <%= string.Format(UserControlsCommonResource.SalesQuestions,
                      string.Format("<a href=\"mailto:{0}\" >{0}</a>", Settings.SalesEmail)) %>
</div>
<% } %>

