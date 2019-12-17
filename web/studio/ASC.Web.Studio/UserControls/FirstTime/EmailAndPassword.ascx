<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EmailAndPassword.ascx.cs" Inherits="ASC.Web.Studio.UserControls.FirstTime.EmailAndPassword" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div id="requiredStep" class="clearFix">
    <div class="passwordBlock">
        <div class="header-base"><%= Resource.Password %></div>
        <div class="clearFix">
            <div class="pwd clearFix">
                <div class="label">
                    <%= Resource.EmailAndPasswordTypePassword %> <span class="info"><%= Resource.EmailAndPasswordTypePasswordRecommendations %></span><span>*</span>
                </div>
                <div class="float-left">
                    <input type="password" id="newPwd" class="textEdit" maxlength="<%= PasswordSettings.MaxLength %>" />
                </div>
            </div>
            <div class="pwd">
                <div class="label">
                    <%= Resource.EmailAndPasswordConfirmPassword %><span>*</span>
                </div>
                <div>
                    <input type="password" id="confPwd" class="textEdit" maxlength="<%= PasswordSettings.MaxLength %>" />
                </div>
            </div>
            <% if (IsVisiblePromocode)
                { %>
            <div class="promocode">
                <div class="label"><%= Resource.EmailAndPasswordPromocode %></div>
                <div>
                    <input id="promocode_input" class="textEdit" maxlength="30" />
                </div>
            </div>
            <% } %>
            <% if (IsAmi)
                { %>
            <div>
                <div class="label"><%= Resource.EmailAndPasswordAmiId %></div>
                <div>
                    <input id="amiid" class="textEdit" maxlength="50" />
                </div>
            </div>
            <% } %>
        </div>
        <% if (RequestLicense)
            { %>
        <br />
        <br />
        <div class="header-base"><%= UserControlsCommonResource.LicenseKeyHeader %></div>
        <div class="pwd">
            <div class="label">
                <%= UserControlsCommonResource.LicenseKeyLabel %><span>*</span>
            </div>
            <div class="clearFix">
                <input type="file" id="uploadButton" />
                <a id="licenseKey" class="button gray"><%= UserControlsCommonResource.UploadFile %></a>
                <span id="licenseKeyText"></span>
            </div>
        </div>
        <% } %>
    </div>

    <div class="portal">
        <div class="header-base"><%= Resource.WizardRegistrationSettings %></div>
        <div class="emailBlock">
            <span class="info">
                <%= Resource.EmailAndPasswordRegEmail %>
            </span>
            <span class="email">
                <span class="emailAddress">
                    <%= CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).Email %>
                </span>
                <span class="changeEmail">
                    <span id="dvChangeMail"><a class="info link dotline blue" onclick="ASC.Controls.EmailAndPasswordManager.ShowChangeEmailAddress();"><%= Resource.EmailAndPasswordTypeChangeIt %></a></span>
                </span>
            </span>
        </div>
        <div class="domainBlock">
            <span class="info">
                <%= Resource.EmailAndPasswordDomain %>
            </span>
            <span class="domainname">
                <%= CoreContext.TenantManager.GetCurrentTenant().TenantDomain %>
                <% if (ShowPortalRename)
                   { %>
                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ChangeAliasHelper'});"></span>
                <% } %>
            </span>
        </div>
        <% if (ShowPortalRename)
           { %>
        <div class="popup_helper" id="ChangeAliasHelper">
            <p>
                <%= UserControlsCommonResource.ChangeAliasHelper %>
            </p>
        </div>
        <% } %>

        <div class="header-base"><%= Resource.WizardGenTimeLang %></div>
        <asp:PlaceHolder ID="_dateandtimeHolder" runat="server"></asp:PlaceHolder>
    </div>
</div>

<% if (RequestLicense && RequestLicenseAccept)
   { %>
<div class="license-accept">
    <input type="checkbox" id="policyAccepted">
    <label for="policyAccepted">
        <%= string.Format(UserControlsCommonResource.LicenseAgreements,
                          "<a href=\"" + Settings.LicenseAgreementsUrl + "\" target=\"_blank\">",
                          "</a>") %></label>
</div>
<% }
   else if (TenantExtra.Opensource && !CoreContext.Configuration.CustomMode)
   { %>
<div class="analytics-accept">
    <input type="checkbox" id="analyticsAcceptedOpenSource" checked="checked">
    <label for="analyticsAcceptedOpenSource">
        <%= string.Format(UserControlsCommonResource.AnalyticsOpenSource) %></label>
</div>
<div class="license-accept">
    <input type="checkbox" id="policyAcceptedOpenSource">
    <label for="policyAcceptedOpenSource">
        <%= string.Format(UserControlsCommonResource.LicenseAgreements,
                          "<a href=\"https://www.gnu.org/licenses/gpl-3.0.html\" target=\"_blank\">",
                          "</a>") %></label>
</div>
<% } %>
