<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffStandalone.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffStandalone" %>

<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Billing" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
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
                           ? "<a class=\"link-black-14 bold\" href=\"" + SetupInfo.ControlPanelUrl + "multiportals" + "\" target=\"_blank\">" + TenantCount + "</a>"
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
                    <%= string.Format(UserControlsCommonResource.LicenseModulesList, "- ", "<br />") %>
                </div>
            </td>
            <td>
                <div class="license-item license-item-controlpanel"><%= UserControlsCommonResource.LicenseModulesControlPanel %></div>
                <div class="license-item license-item-whitelabel"><%= UserControlsCommonResource.LicenseModulesWhiteLabel %></div>
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
    <div><%= UserControlsCommonResource.LicenseActivateDescr.HtmlEncode() %></div>
    <div class="button-margin clearFix">
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

<% if (false)
   { %>
<div class="request-block">
    <div class="tabs-section">
        <span class="header-base"><%= UserControlsCommonResource.LicenseRequestHeader %></span>
        <span id="switcherRequest" class="toggle-button" data-switcher="1"
            data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Show %>
        </span>
    </div>

    <div id="requestPanel">
        <div><%= UserControlsCommonResource.LicenseRequestDescr %></div>
        <br />
        <% var userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID); %>

        <div class="header-base-small">
            <%= Resource.FirstName %><span class="required-mark">*</span>
        </div>
        <input type="text" class="text-edit-fname textEdit" maxlength="64" tabindex="2" required="required" placeholder="<%= Resource.FirstName %>" value="<%= userInfo.FirstName %>">

        <div class="header-base-small">
            <%= Resource.LastName %><span class="required-mark">*</span>
        </div>
        <input type="text" class="text-edit-lname textEdit" maxlength="64" tabindex="3" required="required" placeholder="<%= Resource.LastName %>" value="<%= userInfo.LastName %>">

        <div class="header-base-small">
            <%= CustomNamingPeople.Substitute<Resource>("UserPost") %>
        </div>
        <input type="text" class="text-edit-title textEdit" maxlength="64" tabindex="4" required="required" placeholder="<%= CustomNamingPeople.Substitute<Resource>("UserPost").HtmlEncode() %>" value="<%= userInfo.Title %>">

        <div class="header-base-small">
            <%= Resource.Email %><span class="required-mark">*</span>
        </div>
        <input type="email" class="text-edit-email textEdit" maxlength="64" tabindex="5" required="required" placeholder="<%= Resource.Email %>" value="<%= userInfo.Email %>">

        <div class="header-base-small">
            <%= Resource.TitlePhone %><span class="required-mark">*</span>
        </div>
        <input type="tel" class="text-edit-phone textEdit" maxlength="64" tabindex="6" required="required" title="<%= Resource.TitlePhone %>" pattern="\+?\d{4,63}" placeholder="<%= Resource.TitlePhone %>" value="<%= userInfo.MobilePhone %>">

        <div class="header-base-small">
            <%= UserControlsCommonResource.CompanyTitle %><span class="required-mark">*</span>
        </div>
        <input type="text" class="text-edit-ctitle textEdit" maxlength="64" tabindex="7" required="required" placeholder="<%= UserControlsCommonResource.CompanyTitle %>">

        <div class="header-base-small">
            <%= UserControlsCommonResource.CompanySizeTitle %><span class="required-mark">*</span>
        </div>
        <select class="text-edit-csize textEdit" required="required" tabindex="8">
            <% var usersCount = new[] { 5, 10, 20, 30, 50, 100, 200, 300, 500, 700, 1000 };

               var selected = usersCount.FirstOrDefault(c => c >= UsersCount);
               for (var i = 0; i <= usersCount.Length; i++)
               {
                   var opt =
                       i == usersCount.Length
                           ? string.Format(UserControlsCommonResource.LicenseRequestQuotaMore, usersCount[i - 1])
                           : string.Format(UserControlsCommonResource.LicenseRequestQuota,
                                           i == 0 ? 1 : usersCount[i - 1] + 1,
                                           usersCount[i]); %>
            <option value="<%= opt %>"
                <%= i < usersCount.Length && usersCount[i] == selected
                    || i == usersCount.Length && selected == 0
                        ? "selected=\"selected\"" : "" %>>
                <%= opt %>
            </option>
            <% } %>
        </select>

        <div class="header-base-small">
            <%= UserControlsCommonResource.SiteTitle %><span class="required-mark">*</span>
        </div>
        <input type="text" class="text-edit-site textEdit" maxlength="64" tabindex="9" required="required" placeholder="<%= UserControlsCommonResource.SiteTitle %>">

        <div class="header-base-small">
            <%= UserControlsCommonResource.TariffRequestContent %>
        </div>
        <textarea rows="4" class="text-edit-message textEdit" tabindex="10" required="required" placeholder="<%= UserControlsCommonResource.TariffRequestContentHolder %>"></textarea>

        <div class="middle-button-container">
            <a id="licenseRequest" class="button blue big" tabindex="11">
                <%= UserControlsCommonResource.TariffRequestBtn %></a>
        </div>
    </div>
</div>
<% } %>
