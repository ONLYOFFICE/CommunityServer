<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Authorize.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Authorize" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Settings" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Users.UserProfile" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div id="authMessage" class="auth-form_message"><%= ErrorMessage + LoginMessage %></div>

<div id="authForm" class="auth-form">
    <%--login by email email--%>
    <div id="_AuthByEmail" class="login" runat="server">
        <input maxlength="255" class="pwdLoginTextbox" 
            <% if (EnableLdap)
               { %>
                  type="text"
            <% }
               else
               { %>
                  type="email"
            <% } %>
            placeholder="<%= Resource.RegistrationEmailWatermark %>" id="login" name="login"
            <%= String.IsNullOrEmpty(Login)
                ? ""
                : ("value=\"" + Login.HtmlEncode() + "\"") %> />
    </div>

    <%--password--%>
    <div class="auth-form_password">
        <input type="password" id="pwd" class="pwdLoginTextbox" name="pwd" maxlength="64" placeholder="<%= Resource.Password %>" />
    </div>
    <%--buttons--%>
    <div class="auth-form_submenu clearFix">
        <div class="auth-form_subtext clearFix">
            <span class="link gray underline float-right" onclick="PasswordTool.ShowPwdReminderDialog()">
                <%= Resource.ForgotPassword %>
            </span>
            <div>
                <input type="checkbox" id="remember" name="remember" checked />
                <label for="remember">
                    <%= Resource.Remember%>
                </label>
            </div>
        </div>
        <div class="auth-form_submenu_login clearFix">
            <asp:PlaceHolder ID="pwdReminderHolder" runat="server" />
            <a id="loginButton" class="button blue big signIn" onclick="Authorize.Login(); return false;">
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
         <% if (EnableSso)
        { %>
            <a id="ssoButton" class="link gray underline singleSignOn" 
                <% if (IsSaml) { %> href="samllogin.ashx?auth=true"<% } else { %> href="jwtlogin.ashx?auth=true" <%} %>>Single sign-on</a>
        <% } %>
    </div>
</div>
