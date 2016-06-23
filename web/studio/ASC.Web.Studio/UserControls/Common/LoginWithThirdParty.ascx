<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginWithThirdParty.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.LoginWithThirdParty" %>
<%@ Import Namespace="Resources" %>

<div class="login-with-label"><%= UserControlsCommonResource.LoginWith %></div>

<input type="hidden" class="login-message" value="<%= LoginMessage %>"/>

<div class="social-login">
    <asp:PlaceHolder runat="server" ID="ThirdPartyList"></asp:PlaceHolder>
</div>
