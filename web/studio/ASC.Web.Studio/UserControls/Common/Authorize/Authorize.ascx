<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Authorize.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Authorize" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Settings" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Users.UserProfile" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div id="authForm" class="auth-form">
    <% if (EnableSso)
    { %>
    <div class="auth-form_sso clearFix">
        <a id="ssoButton" class="button blue big singleSignOn"
            <% if (IsSaml) { %> href="<%= SetupInfo.SsoSamlLoginUrl %>" <% } else { %> href="jwtlogin.ashx?auth=true" <%} %>><%: SsoLabel %></a>
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
        <input type="password" id="pwd" class="pwdLoginTextbox <%= IsPasswordInvalid ? "error" : ""%>" name="pwd" maxlength="64" placeholder="<%= Resource.Password %>" />
    </div>
    <%--buttons--%>
    <div class="auth-form_submenu clearFix">
        <div class="auth-form_subtext clearFix">
            <span class="link gray underline float-right" onclick="PasswordTool.ShowPwdReminderDialog()">
                <%= Resource.ForgotPassword %>
            </span>
            <% if (EnableSession && !Request.DesktopApp()) { %>
            <div>
                <input type="checkbox" id="remember" name="remember" checked />
                <label for="remember">
                    <%= Resource.Remember%>
                </label>
                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'rememberHelper'});"></span>
                <div class="popup_helper" id="rememberHelper">
                    <p><%= Resource.RememberHelper %></p>
                </div>
            </div>
            <% } %>
        </div>
        <div class="auth-form_submenu_login clearFix">
            <asp:PlaceHolder ID="pwdReminderHolder" runat="server" />
            <a id="loginButton" class="button blue big signIn" onclick="jQuery('#authMessage').hide(); jQuery('.pwdLoginTextbox').removeClass('error'); Authorize.Login(); return false;">
                <%= Resource.LoginButton %>
            </a>
            <% if (AccountLinkControl.IsNotEmpty)
               { %>
            <div id="social" class="social_nets clearFix" style="display: <%= SetupInfo.ThirdPartyAuthEnabled ? "block" : "none" %>">
                <span><%= Resource.LoginWithAccount %></span>
                <div class="float-right">
                    <asp:PlaceHolder ID="signInPlaceholder" runat="server" />
                </div>
            </div>
            <% } %>
        </div>
        <div id="authMessage" class="auth-form_message"><%= ErrorMessage + LoginMessage %></div>
    </div>
</div>
