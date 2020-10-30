<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ButtonsSidePanel.ascx.cs" Inherits="ASC.Web.CRM.Controls.Common.ButtonsSidePanel" %>

<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>


<div class="page-menu">
    <ul class="menu-actions">
        <li id="menuCreateNewButton" class="menu-main-button without-separator middle" title="<%= CRMCommonResource.CreateNew %>">
            <span class="main-button-text"><%= CRMCommonResource.CreateNew %></span>
            <span class="white-combobox"></span>
        </li>
        <li id="menuOtherActionsButton" class="menu-gray-button" title="<%= Resources.Resource.MoreActions %>">
            <span class="btn_other-actions">...</span>
        </li>
    </ul>

    <%-- popup windows --%>
    <div id="createNewButton" class="studio-action-panel">
        <ul class="dropdown-content">
            <li><a class="dropdown-item" href="Default.aspx?action=manage"><%= CRMContactResource.Company %></a></li>
            <li><a class="dropdown-item" href="Default.aspx?action=manage&type=people"><%= CRMContactResource.Person %></a></li>
            <li><a id="menuCreateNewTask" class="dropdown-item" href="javascript:void(0);"><%= CRMTaskResource.Task %></a></li>
            <li><a id="menuCreateNewDeal" class="dropdown-item" href="Deals.aspx?action=manage"><%= CRMDealResource.Deal %></a></li>
            <li><a id="menuCreateNewInvoice" class="dropdown-item" href="Invoices.aspx?action=create"><%= CRMInvoiceResource.Invoice %></a></li>
            <li><a class="dropdown-item" href="Cases.aspx?action=manage"><%= CRMCasesResource.Case %></a></li>
        </ul>
    </div>

    <div id="otherActions" class="studio-action-panel">
        <ul class="dropdown-content">
            <% if (!MobileDetector.IsMobile)
               { %>
            <li><a id="importContactLink" class="dropdown-item" href="Default.aspx?action=import"><%= CRMContactResource.ImportContacts %></a></li>
            <li><a id="importTasksLink" class="dropdown-item" href="Tasks.aspx?action=import"><%= CRMTaskResource.ImportTasks %></a></li>
            <li><a id="importDealsLink" class="dropdown-item" href="Deals.aspx?action=import"><%= CRMDealResource.ImportDeals %></a></li>
            <li><a id="importCasesLink" class="dropdown-item" href="Cases.aspx?action=import"><%= CRMCasesResource.ImportCases %></a></li>
            <% } %>

            <% if (CRMSecurity.IsAdmin)
               { %>
            <li><a id="exportListToCSV" class="dropdown-item"><%= CRMCommonResource.ExportCurrentListToCsvFile %></a></li>
            <% } %>
        </ul>
    </div>

</div>