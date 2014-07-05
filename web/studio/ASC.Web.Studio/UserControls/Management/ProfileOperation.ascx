<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProfileOperation.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ProfileOperation" %>
<%@ Import Namespace="Resources" %>

<div id="operationBlock" runat="server">
    <div class="confirm-block-title header-base">
        <%= Resource.DeleteProfileConfirm%>
    </div>

    <div class="big-button-container">
        <asp:LinkButton ID="lblDelete" runat="server" CssClass="button blue big" OnClick="DeleteProfile"><%= Resource.DeleteProfileButton%></asp:LinkButton>
    </div>
</div>
<div class="confirm-block-text" id="result" runat="server"></div>
