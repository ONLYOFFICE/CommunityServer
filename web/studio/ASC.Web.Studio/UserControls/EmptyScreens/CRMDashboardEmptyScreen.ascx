<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CRMDashboardEmptyScreen.ascx.cs" Inherits="ASC.Web.Studio.UserControls.EmptyScreens.CRMDashboardEmptyScreen" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<% if (!UseStaticPosition)
   { %>
    <div class="backdrop" blank-page=""></div>
<% } %>

<div id="content" blank-page="" class="dashboard-center-box crm <%= UseStaticPosition ? "static" : "" %>">
    <div class="header">
        <% if (!UseStaticPosition)
           { %>
            <span class="close"></span>
        <% } %>
        <%= CRMCommonResource.DashboardTitle %>
    </div>
    <div class="content clearFix">
        <div class="module-block">
            <div class="img contacts"></div>
            <div class="title"><%= string.Format(CRMCommonResource.ManageContacts, "<br />") %></div>
            <ul>
                <li><%= CRMCommonResource.DashContactsImport %></li>
                <li><%= CRMCommonResource.DashContactsDetails %></li>
                <li><%= CRMCommonResource.DashContactsMail %></li>
            </ul>
        </div>
        <div class="module-block">
            <div class="img sales"></div>
            <div class="title"><%= string.Format(CRMCommonResource.TrackSales, "<br />") %></div>
            <ul>
                <li><%= CRMCommonResource.DashSalesTasks %></li>
                <li><%= CRMCommonResource.DashSalesOpportunities %></li>
                <li><%= CRMCommonResource.DashSalesParticipants %></li>
            </ul>
        </div>
        <div class="module-block">
            <div class="img invoice"></div>
            <div class="title"><%= string.Format(CRMCommonResource.CreateInvoices, "<br />") %></div>
            <ul>
                <li><%= CRMCommonResource.DashIssueInvoices %></li>
                <li><%= CRMCommonResource.DashInvoiceProductsAndServices %></li>
                <li><%= CRMCommonResource.DashInvoiceTaxes %></li>
            </ul>
        </div>
        <div class="module-block">
            <div class="img customize"></div>
            <div class="title"><%= string.Format(CRMCommonResource.SetUpCustomize, "<br />") %></div>
            <ul>
                <li><%= CRMCommonResource.DashCustomizeCRM %></li>
                <li><%= CRMCommonResource.DashCustomizeFields %></li>
                <li><%= CRMCommonResource.DashCustomizeWebsite %></li>
            </ul>
        </div>
    </div>
    <div class="dashboard-buttons">
        <a class="button huge create-button" href="<%= VirtualPathUtility.ToAbsolute("~/products/crm/default.aspx?action=manage&type=people") %>" >
            <%= CRMCommonResource.CreateNewContact %>          
        </a>
    </div>
</div>