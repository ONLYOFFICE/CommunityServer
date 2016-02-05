<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffStandalone.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffStandalone" %>

<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="current-tariff-desc">
    <%= TariffDescription() %>
    <br />
    <br />
    <%= String.Format(Resource.TariffStatistics,
                      "<a class=\"link-black-14 bold\" href=\"" + CommonLinkUtility.GetEmployees() + "\">" + UsersCount + "</a>"
                      + (CurrentQuota.ControlPanel ? "/" + CurrentQuota.ActiveUsers : string.Empty)) %>
</div>

<div class="activate-block">
    <div class="tabs-section">
        <span class="header-base"><%= UserControlsCommonResource.LicenseActivateHeader %></span>
        <span id="switcherActivate" class="toggle-button" data-switcher="0"
            data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Hide %>
        </span>
    </div>

    <div id="activatePanel">
        <div>
            <%= UserControlsCommonResource.LicenseActivateDescr %>
            <a href="<%= SetupInfo.TeamlabSiteRedirect + "/enterprise-edition.aspx" %>" target="_blank"><% = Resource.LearnMore %></a>
        </div>
        <br />

        <div class="header-base-small"><%= UserControlsCommonResource.LicenseKeyLabel %></div>
        <input type="text" id="licenseKeyText" class="textEdit" tabindex="1" maxlength="100" autocomplete="off" />
        <a id="licenseKey" class="button gray"><%= UserControlsCommonResource.UploadFile %></a>

        <% if (RequestLicenseAccept)
           { %>
        <div class="license-accept">
            <input type="checkbox" id="policyAccepted">
            <% var licenseUrls = (new Dictionary<string, string>
                   {
                       {"en", "http://onlyo.co/1HRBEvK"},
                       {"ru", "http://onlyo.co/1l7Hkx9"},
                   });
               var licenseUrl = licenseUrls.ContainsKey(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                                    ? licenseUrls[CultureInfo.CurrentUICulture.TwoLetterISOLanguageName]
                                    : licenseUrls["en"];
            %>
            <label for="policyAccepted">
                By checking this box you accept the terms of the <a href="<%= licenseUrl %>" target="_blank">License Agreements</a></label>
        </div>
        <% } %>

        <div class="middle-button-container">
            <a id="activateButton" class="button blue big disable">
                <%= UserControlsCommonResource.LicenseActivateButton %>
            </a>
            <span class="splitter-buttons"></span>
            <a id="activateCancel" class="button gray big">
                <%= Resource.CancelButton %>
            </a>
        </div>
    </div>
</div>

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
        <input type="tel" class="text-edit-phone textEdit" maxlength="64" tabindex="6" required="required" placeholder="<%= Resource.TitlePhone %>" value="<%= userInfo.MobilePhone %>">

        <div class="header-base-small">
            <%= UserControlsCommonResource.CompanyTitle %><span class="required-mark">*</span>
        </div>
        <input type="text" class="text-edit-ctitle textEdit" maxlength="64" tabindex="7" required="required" placeholder="<%= UserControlsCommonResource.CompanyTitle %>">

        <div class="header-base-small">
            <%= UserControlsCommonResource.CompanySizeTitle %><span class="required-mark">*</span>
        </div>
        <select class="text-edit-csize textEdit" required="required" tabindex="8">
            <% var usersCount = new[] { 5, 15, 65, 350, 1500 };
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
