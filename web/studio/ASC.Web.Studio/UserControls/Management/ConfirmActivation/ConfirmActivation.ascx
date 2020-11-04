<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmActivation.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ConfirmActivation" %>

<%@ Import Namespace="ASC.Core.Common.Settings" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

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
        <input type="password" id="studio_confirm_pwd" placeholder="<%= isPersonal ? "" : Resource.InvitePassword %>" value="" class="pwdLoginTextbox" autofocus
            data-maxlength="<%= PasswordSettings.MaxLength %>"
            data-regex="<%: PasswordSettings.GetPasswordRegex(PasswordSettings.Load()) %>"
            data-help="<%= ASC.Web.Studio.Core.Users.UserManagerWrapper.GetPasswordHelpMessage() %>" />
        <input type="hidden" id="passwordHash" name="passwordHash" />
    </div>
    <asp:Button runat="server" ID="ButtonEmailAndPasswordOK"
        CssClass="button blue big" OnClientClick="window.ConfirmActivacion(); return false;" />

</asp:Panel>
