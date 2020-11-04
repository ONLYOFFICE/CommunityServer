<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Authorize.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Authorize" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Settings" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Users.UserProfile" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div id="authForm" class="auth-form">
    <% if (EnableSso)
    { %>
    <div class="auth-form_sso clearFix">
        <a id="ssoButton" class="button blue big singleSignOn" href="<%= SsoUrl %>"><%: SsoLabel %></a>
    </div>
    <div class="auth-form_splitter clearFix">
        <span><%= Resource.Or.ToLowerInvariant() %></span>
    </div>
    <% } %>

    <%--login by email email--%>
    <div id="_AuthByEmail" class="login" runat="server">
        <input maxlength="255" class="pwdLoginTextbox <%= IsLoginInvalid ? "error" : ""%>" type="<%= (EnableLdap ? "text" : "email") %>"
            placeholder="<%= EnableLdap ? Resource.RegistrationLoginWatermark : Resource.RegistrationEmailWatermark %>" id="login" name="login"
            <%= String.IsNullOrEmpty(Login)
                ? ""
                : ("value=\"" + Login.HtmlEncode() + "\"") %> />
    </div>

    <%--password--%>
    <div class="auth-form_password">
        <input type="password" id="pwd" class="pwdLoginTextbox <%= IsPasswordInvalid ? "error" : ""%>" maxlength="<%= PasswordSettings.MaxLength %>" placeholder="<%= Resource.Password %>" />
        <input type="hidden" id="passwordHash" name="passwordHash" />
    </div>
    <%--buttons--%>
    <div class="auth-form_submenu clearFix">
        <div class="auth-form_subtext clearFix">
            <span class="link gray underline float-right" onclick="PasswordTool.ShowPwdReminderDialog()">
                <%= Resource.ForgotPassword %>
            </span>
            <% if (EnableSession && !Request.DesktopApp()) { %>
            <div>
                <input type="checkbox" id="remember" name="remember"/>
                <label for="remember">
                    <%= Resource.Remember%>
                </label>
                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'rememberHelper'});"></span>
                <div class="popup_helper" id="rememberHelper">
                    <p><%= Resource.RememberHelper %></p>
                </div>
            </div>
            <% } %>
            <% if (EnableLdap)
               { %>
            <div class="auth-ldap-checkbox">
                <input type="checkbox" id="ldapPassword" checked="checked" />
                <label for="ldapPassword">
                    <%= string.Format(Resource.SignInLDAP, ASC.ActiveDirectory.Base.Settings.LdapCurrentDomain.Load().CurrentDomain) %>
                </label>
                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ldapPasswordHelper'});"></span>
                <div class="popup_helper" id="ldapPasswordHelper">
                    <p><%= Resource.SignInLDAPHelper %></p>
                </div>
            </div>
            <% } %>
        </div>
        <div class="auth-form_submenu_login clearFix">
            <asp:PlaceHolder ID="pwdReminderHolder" runat="server" />
            <a id="loginButton" class="button blue big signIn" onclick="Authorize.Submit(); return false;">
                <%= Resource.LoginButton %>
            </a>
        </div>
        <% if (RecaptchaEnable && ShowRecaptcha)
           { %>
        <div id="recaptchaHiddenContainer" class="captchaContainer g-recaptcha"
            data-sitekey="<%= SetupInfo.RecaptchaPublicKey %>"
            data-hl="<%= Culture %>">&nbsp;
        </div>
        <% } %>
        <div id="authMessage" class="auth-form_message"><%= ErrorMessage + LoginMessage %></div>
        <% if (ThirdpartyEnable)
           { %>
        <div id="social">
            <div><%= Resource.LoginWithAccount %></div>
            <asp:PlaceHolder ID="signInPlaceholder" runat="server" />
        </div>
        <% } %>
    </div>
</div>
