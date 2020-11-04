<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginWithThirdParty.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.LoginWithThirdParty" %>

<input type="hidden" class="login-message" value="<%= LoginMessage %>"/>

<div id="social" class="social-login">
    <asp:PlaceHolder runat="server" ID="ThirdPartyList"></asp:PlaceHolder>
</div>
