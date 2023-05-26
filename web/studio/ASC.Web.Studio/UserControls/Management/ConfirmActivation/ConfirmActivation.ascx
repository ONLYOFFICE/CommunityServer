<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmActivation.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ConfirmActivation" %>

<%@ Import Namespace="ASC.Core.Common.Settings" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<asp:Panel runat="server" ID="passwordSetter" Visible="false">
    <p class="confirm-block-text">
        <%= Type == ConfirmType.PasswordChange
                ? Resource.PassworResetTitle
                : Resource.AccountActivationTitle %>
    </p>
    <%--Pwd--%>
    <div class="confirm-block-field">
        <% if (isPersonal)
           { %>
        <label class="default-personal-popup_label"><%= Resource.Password %></label>
        <% } %>
        <input type="password" id="studio_confirm_pwd" placeholder="<%= isPersonal ? "" : Resource.InvitePassword %>" value="" class="pwdLoginTextbox" autocomplete="new-password" autofocus
            data-maxlength="<%= TenantPasswordSettings.MaxLength %>"
            data-regex="<%: PasswordSettings.GetPasswordRegex(TenantPasswordSettings) %>"
            data-help="<%= UserManagerWrapper.GetPasswordHelpMessage() %>" />
        <input type="hidden" id="passwordHash" name="passwordHash" />
        <label class="eye-label hide-label" id="passwordShowLabel"></label>
    </div>
    <div class ="confirm-block-field">
        <% if (isPersonal)
           { %>
        <label class="default-personal-popup_label"><%= Resource.ConfirmPasswordMatch %></label>
        <% } %>
        <input type="password" id="studio_confirm_pwd_match" placeholder="<%= Resource.ConfirmPasswordMatch %>"  class="pwdLoginTextbox" autocomplete="new-password"
            data-maxlength="<%= TenantPasswordSettings.MaxLength %>"
            data-regex="<%: PasswordSettings.GetPasswordRegex(TenantPasswordSettings) %>" />
        <label class="eye-label hide-label" id="passwordShowLabelMatch"></label>
    </div>

    <p class="confirm-block-password-text" id="password-match-text">
        <%= Resource.PasswordMatch %>
    </p>
    <p class="confirm-block-password-text" id="password-do-not-match-text">
        <%= Resource.PasswordDoNotMatch %>
    </p>
    <div class="small-button-container">
        <asp:Button runat="server" ID="ButtonEmailAndPasswordOK"
            CssClass="button blue big" OnClientClick="window.ConfirmManager.confirmActivacion(); return false;" />
    </div>
</asp:Panel>
