<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmActivation.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ConfirmActivation" %>

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
        <% if (isPersonal){ %>
        <label class="default-personal-popup_label"><%= Resource.Password %></label>
        <% } %>
        <input type="password" id="studio_confirm_pwd" placeholder="<%= isPersonal ? "" :Resource.InvitePassword %>" value="" name="pwdInput" class="pwdLoginTextbox" data-maxlength="<%= PasswordSettings.MaxLength %>" autofocus/>
    </div>
    <asp:Button runat="server" ID="ButtonEmailAndPasswordOK"
        CssClass="button blue big" OnClick="LoginToPortal" />

</asp:Panel>
<asp:Panel runat="server" ID="emailChange" Visible="false">
    <p class="confirm-block-text">
        <%= Resource.MessageEmailAddressChanging %>
    </p>
    <p id="currentEmailText">
        <%= Resource.MessageCurrentEmailAddressIs %>:
        <a href="mailto:<%=User.Email %>">
            <%= User.Email %></a>
    </p>
    <div id="studio_confirmMessage" class="message-box" style="display: none;">
        <div id="studio_confirmMessage_successtext" style="display: none;">
        </div>
        <div id="studio_confirmMessage_errortext" class="errorText" style="display: none;">
        </div>
    </div>
    <div id="emailInputContainer">
        <%--Email--%>
        <div class="confirm-block-field">
            <% if (isPersonal){ %>
        <label class="default-personal-popup_label"><%= Resource.NewEmail %></label>
        <% } %>
            <input type="email" id="email1" value="" name="emailInput" class="pwdLoginTextbox" placeholder="<%= Resource.TypeNewEmail %>" />
        </div>
        <asp:Button ID="btChangeEmail" runat="server"
            CssClass="button blue big" OnClientClick="window.btChangeEmailOnClick(); return false;" />
    </div>
</asp:Panel>
