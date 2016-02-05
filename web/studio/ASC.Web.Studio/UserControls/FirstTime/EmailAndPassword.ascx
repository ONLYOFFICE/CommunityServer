<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EmailAndPassword.ascx.cs" Inherits="ASC.Web.Studio.UserControls.FirstTime.EmailAndPassword" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Core" %>
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
                    <input type="password" id="newPwd" class="textEdit" maxlength="30" />
                </div>
            </div>
            <div class="pwd">
                <div class="label">
                    <%= Resource.EmailAndPasswordConfirmPassword %><span>*</span>
                </div>
                <div>
                    <input type="password" id="confPwd" class="textEdit" maxlength="30" />
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
        </div>
        <% if (Enterprise)
            { %>
        <br />
        <br />
        <div class="header-base"><%= UserControlsCommonResource.LicenseKeyHeader %></div>
        <div class="pwd">
            <div class="label">
                <%= UserControlsCommonResource.LicenseKeyLabel %><span>*</span>
            </div>
            <div>
                <input type="text" id="licenseKeyText" class="textEdit" maxlength="100" />
                <a id="licenseKey" class="button gray"><%= UserControlsCommonResource.UploadFile %></a>
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
            </span>
        </div>
        <div class="header-base"><%= Resource.WizardGenTimeLang %></div>
        <asp:PlaceHolder ID="_dateandtimeHolder" runat="server"></asp:PlaceHolder>
    </div>
</div>

<% if (Enterprise)
    { %>
<div class="license-accept">
    <input type="checkbox" id="policyAccepted">
    <%
        var licenseUrl = "http://onlyo.co/1HRBEvK";
        if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru")
        {
            licenseUrl = "http://onlyo.co/1l7Hkx9";
        }
    %>
    <label for="policyAccepted">
        By checking this box you accept the terms of the <a href="<%= licenseUrl %>" target="_blank">License Agreements</a></label>
</div>
<% } %>
