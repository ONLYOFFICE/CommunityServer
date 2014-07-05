<%@ Assembly Name="ASC.Web.UserControls.SocialMedia" %>
<%@ Import Namespace="ASC.Web.UserControls.SocialMedia.Resources" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserActivityView.ascx.cs"
    Inherits="ASC.Web.UserControls.SocialMedia.UserControls.UserActivityView" %>
<div runat="server" id="_ctrlErrorDescriptionContainer" class="infoPanel sm_UserActivityView_ErrorDescription" style="display:none;">
    <div runat="server" id="_ctrlErrorDescription">
    </div>
</div>
<div class="clearFix" style="float: right; margin: 0px 0 20px 0; display:none;">
    <%= SocialMediaResource.MessageCount %>
    <asp:DropDownList runat="server" ID="_ctrlMessageCount" onchange="ddlMessageNumberClicked(event);">
        <asp:ListItem Value="10"></asp:ListItem>
        <asp:ListItem Value="20"></asp:ListItem>
        <asp:ListItem Value="50"></asp:ListItem>
    </asp:DropDownList>
</div>
<div class="clearFix">
</div>
<asp:HiddenField runat="server" ID="_ctrlSMErrorMessage" />
<asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
