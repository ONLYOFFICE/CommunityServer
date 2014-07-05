<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuthCommunications.ascx.cs" Inherits="ASC.Web.Studio.UserControls.AuthCommunications" %>
<asp:Panel ID="_sendAdmin" runat="server" CssClass="signUpBlock message">

<div class="overview">
<%=Resources.Resource.AdminMessageDescription%>
</div>

    <a class="signUp mess" href="javascript:void(0);" onclick="AuthCommunications.ShowAdminMessageDialog(); return false;">
    <%= Resources.Resource.AdminMessageLink %>
        </a>
    <div id="studio_admMessDialog" class="login-hint-block display-none">
        <div id="studio_admMessContent">
            <div id="studio_admMessInfo">
            </div>
            <div class="desc">
                <%= Resources.Resource.AdminMessageTitle %>
            </div>
            <div>
                <div class="label">
                    <%= Resources.Resource.AdminMessageSituation %>:
                </div>
                <textarea id="studio_yourSituation" style="width: 100%; height: 54px; padding: 0;
                    resize: none;"></textarea>
            </div>
            <div>
                <div style="margin-top: 20px;" class="label">
                    <%= Resources.Resource.AdminMessageEmail %>:
                </div>
                <input class="textEdit" type="text" id="studio_yourEmail" style="width: 100%; margin-right: 20px;" />
            </div>
            <div class="middle-button-container">
                <a class="button gray" href="javascript:void(0);" onclick="AuthCommunications.SendAdminMessage()">
                    <%= Resources.Resource.AdminMessageButton %></a>
            </div>
        </div>
        <div id="studio_admMessage" style="padding: 20px 0px; text-align: center; display: none;">
        </div>
    </div>
</asp:Panel>

<asp:Panel ID="_joinBlock" runat="server" CssClass="signUpBlock join">
<div class="overview">
<%= Resources.Resource.SendInviteToJoinDescription %>
</div>
    <a class="signUp join" href="javascript:void(0);" onclick="AuthCommunications.ShowInviteJoinDialog(); return false;">
        <%= Resources.Resource.SendInviteToJoinButtonBlock %></a>
    <div id="studio_invJoinDialog" class="display-none">
        <div id="studio_invJoinContent" class="login-hint-block">
            <div id="studio_invJoinInfo">
            </div>
            <div class="desc">
                <%=RenderTrustedDominTitle()%>
            </div>
            <div>
            <div class="label">
            <%=Resources.Resource.SendInviteToJoinEmailDesc%>
            </div>
                <input class="textEdit" type="text" id="studio_joinEmail" style="width: 100%" />
            </div>
            <div class="middle-button-container">
                <a class="button gray" href="javascript:void(0);" onclick="AuthCommunications.SendInviteJoinMail()">
                    <%=Resources.Resource.SendInviteToJoinButton%></a>
            </div>
        </div>
        <div id="studio_invJoinMessage" style="padding: 20px 0px; text-align: center; display: none;">
        </div>
    </div>
</asp:Panel>
