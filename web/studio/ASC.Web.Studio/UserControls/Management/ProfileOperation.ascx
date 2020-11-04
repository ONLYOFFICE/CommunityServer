<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProfileOperation.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ProfileOperation" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="Resources" %>

<div id="operationBlock" runat="server">
    <div class="confirm-block-title header-base">
        <%= Resource.DeleteProfileConfirmation %>
    </div>
    <%if (!CoreContext.Configuration.CustomMode){%>
        <%= string.Format(Resource.DeleteProfileConfirmationInfo, "<a href='https://www.onlyoffice.com/legalterms.aspx' target='_blank'>", "</a>") %>
    <% } %>
    <div class="big-button-container">
        <asp:LinkButton ID="lblDelete" runat="server" CssClass="button blue big" OnClick="DeleteProfile"><%= Resource.DeleteProfileBtn%></asp:LinkButton>
    </div>
</div>
<div id="result" runat="server">
    <div class="confirm-block-title header-base">
        <%= Resource.DeleteProfileSuccessMessage %>
    </div>
    <%if (!CoreContext.Configuration.CustomMode){%>
        <%= string.Format(Resource.DeleteProfileSuccessMessageInfo, "<a href='https://www.onlyoffice.com/legalterms.aspx' target='_blank'>", "</a>")%>
    <% } %>
</div>
<div id="errorBlock" runat="server"></div>
