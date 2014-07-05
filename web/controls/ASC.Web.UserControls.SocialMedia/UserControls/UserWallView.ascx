<%@ Assembly Name="ASC.Web.UserControls.SocialMedia" %>
<%@ Import Namespace="ASC.Web.UserControls.SocialMedia.Resources" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="ASC.Web.UserControls.SocialMedia.UserControls.UserWallView" %>
<div class="clearFix" style="float: right; margin: 0px 0 20px 0" runat="server" id="_ctrlMessageNumberContainer">
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
<div id="_ctrlErrorDescriptionContainer" runat="server" class="infoPanel sm_UserActivityView_ErrorDescription"
    style="display: none;">
    <div id="_ctrlErrorDescription" runat="server">
    </div>
</div>
