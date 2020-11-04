<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListBaseView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Common.ListBaseView" %>

<div class="main-list main-list-contacts display-none">
</div>

<div class="main-list main-list-tasks display-none">
</div>

<div class="main-list main-list-deals display-none">
</div>

<div class="main-list main-list-invoices display-none">
</div>

<div class="main-list main-list-cases display-none">
</div>


<div id="hiddenBlockForPrivatePanel" style="display:none;">
    <div id="privatePanelWrapper">
        <asp:PlaceHolder runat="server" ID="_phPrivatePanel"></asp:PlaceHolder>
    </div>
</div>

<asp:PlaceHolder ID="_phDashboardEmptyScreen" runat="server"></asp:PlaceHolder>

