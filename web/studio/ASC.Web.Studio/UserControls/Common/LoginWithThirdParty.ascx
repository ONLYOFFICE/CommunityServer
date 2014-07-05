<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginWithThirdParty.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.LoginWithThirdParty" %>
<%@ Import Namespace="Resources" %>

<div class="login-with-label">
    <%= FromEditor ? string.Format(UserControlsCommonResource.LoginWithFromTry, "<br />") : UserControlsCommonResource.LoginWith %>
</div>

<div>
    <%= LoginMessage %>
</div>

<div class="social-login">
    <asp:PlaceHolder runat="server" ID="ThirdPartyList"></asp:PlaceHolder>
</div>
