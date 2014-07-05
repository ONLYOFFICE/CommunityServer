<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FacebookUserInfoView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.SocialMedia.FacebookUserInfoView" %>
<div id="sm_RelationWindow_InternalContainer">
    <div class="sm_RelationWindow_BackButton">
        <a href="javascript:ASC.CRM.SocialMedia.ShowContactSearchPanel();">
            <%= CRMSocialMediaResource.Back %></a>
    </div>
    <div style="margin-bottom: 10px;">
        <span class="header-base-small sn_userName" style="color: Black !important;" runat="server"
            id="_ctrlUserName"></span>
    </div>
    <div class="sm_RelationWindow_Avatar">
        <asp:Image runat="server" ID="_ctrlImageUserAvatar" Width="100%" />
    </div>
    <div class="sm_RelationWindow_Items">
        <div style="padding-left: 10px;">
            <asp:CheckBox runat="server" ID="_ctrlChbAddImage" Checked="true" />
            <br />
        </div>
    </div>
    <div class="clearFix">
    </div>
    <div style="text-align: right; margin-top: -25px;">
        <input runat="server" id="_ctrlBtRelate" type="button" onclick="javascript:ASC.CRM.SocialMedia.RelateFacebookContactToSocialMedia();"
            style="width: 100px;" /></div>
    <asp:HiddenField runat="server" ID="_ctrlHiddenContactID" />
    <asp:HiddenField runat="server" ID="_ctrlHiddenFacebookUserID" />
    <asp:HiddenField runat="server" ID="_ctrlHiddenUserAvatarUrl" />
</div>
