<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProfileOperation.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ProfileOperation" %>
<%@ Import Namespace="Resources" %>

<div id="operationBlock" runat="server">
    <div class="confirm-block-title header-base">
        <%= Resource.DeleteProfileConfirmation %>
    </div>
    <%= string.Format(Resource.DeleteProfileConfirmationInfo, "<a href='https://www.onlyoffice.com/legalterms.aspx' target='_blank'>", "</a>")%>
    <div class="big-button-container">
        <asp:LinkButton ID="lblDelete" runat="server" CssClass="button blue big" OnClick="DeleteProfile"><%= Resource.DeleteProfileBtn%></asp:LinkButton>
    </div>
</div>
<div id="result" runat="server">
    <div class="confirm-block-title header-base">
        <%= Resource.DeleteProfileSuccessMessage %>
    </div>
    <%= string.Format(Resource.DeleteProfileSuccessMessageInfo, "<a href='https://www.onlyoffice.com/legalterms.aspx' target='_blank'>", "</a>")%>
</div>
<div id="errorBlock" runat="server"></div>
