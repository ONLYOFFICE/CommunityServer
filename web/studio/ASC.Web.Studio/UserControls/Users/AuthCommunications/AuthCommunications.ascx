<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuthCommunications.ascx.cs" Inherits="ASC.Web.Studio.UserControls.AuthCommunications" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="Resources" %>
<asp:Panel ID="_sendAdmin" runat="server" CssClass="signUpBlock message">

<div class="overview">
<%= Resource.AdminMessageDescription %>
</div>

    <a class="signUp mess" onclick="AuthCommunications.ShowAdminMessageDialog()">
    <%= Resource.AdminMessageLink %>
        </a>
    <div id="studio_admMessDialog" class="login-hint-block display-none">
        <div id="studio_admMessContent">
            <div class="desc">
                <%= CustomNamingPeople.Substitute<Resource>("AdminMessageTitle").HtmlEncode() %>
            </div>
            <div>
                <div class="label">
                    <%= Resource.AdminMessageSituation %>:
                </div>
                <textarea id="studio_yourSituation" style="width: 100%; height: 54px; padding: 0;
                    resize: none;"></textarea>
            </div>
            <div>
                <div style="margin-top: 20px;" class="label">
                    <%= Resource.AdminMessageEmail %>:
                </div>
                <input class="textEdit" type="text" id="studio_yourEmail" style="width: 100%; margin-right: 20px;" />
            </div>
            <div class="middle-button-container">
                <a class="button gray disable" onclick="AuthCommunications.SendAdminMessage()">
                    <%= Resource.AdminMessageButton %></a>
            </div>
        </div>
    </div>
</asp:Panel>

<asp:Panel ID="_joinBlock" runat="server" CssClass="signUpBlock join">
<div class="overview">
<%= CustomNamingPeople.Substitute<Resource>("SendInviteToJoinDescription").HtmlEncode() %>
</div>
    <a class="signUp join" onclick="AuthCommunications.ShowInviteJoinDialog()">
        <%= Resource.SendInviteToJoinButtonBlock %></a>
    <div id="studio_invJoinDialog" class="display-none">
        <div id="studio_invJoinContent" class="login-hint-block">
            <div class="desc">
                <%= RenderTrustedDominTitle() %>
            </div>
            <div>
            <div class="label">
            <%: Resource.SendInviteToJoinEmailDesc %>
            </div>
                <input class="textEdit" type="text" id="studio_joinEmail" style="width: 100%" />
            </div>
            <div class="middle-button-container">
                <a class="button gray disable" onclick="AuthCommunications.SendInviteJoinMail()">
                    <%= Resource.SendInviteToJoinButton %></a>
            </div>
        </div>
    </div>
</asp:Panel>
