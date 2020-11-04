<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceDetailsView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.Invoices.InvoiceDetailsView" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>


<div class="invoice-container"></div>

<div id="invoiceDetailsMenuPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <% if (Global.CanDownloadInvoices) { %>
        <li>
            <a class="dropdown-item download-btn"><%= CRMInvoiceResource.Download %></a>
        </li>
        <% } %>
        <li>
            <a class="dropdown-item print-btn"><%= CRMInvoiceResource.Print %></a>
        </li>
        <% if (Global.CanDownloadInvoices) { %>
        <li>
            <a class="dropdown-item mail-btn"><%= CRMInvoiceResource.SendByEmail %></a>
        </li>
        <% } %>
        <li>
            <a class="dropdown-item duplicate-btn" href="<%= String.Format("Invoices.aspx?id={0}&action=duplicate", TargetInvoice.ID) %>">
                <%= CRMInvoiceResource.DuplicateInvoice %>
            </a>
        </li>
        <li>
            <a class="dropdown-item edit-btn" href="<%= String.Format("Invoices.aspx?id={0}&action=edit", TargetInvoice.ID) %>">
                <%= CRMInvoiceResource.EditInvoice %>
            </a>
        </li>
        <li>
            <a class="dropdown-item delete-btn"><%= CRMInvoiceResource.DeleteThisInvoice %></a>
        </li>
    </ul>
</div>
