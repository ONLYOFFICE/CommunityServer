<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceTaxesView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.InvoiceTaxesView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>

<div id="invoiceTaxesList"class="clearFix">
    <p style="margin-bottom: 10px;"><%= CRMInvoiceResource.InvoiceTaxesDescriptionText %></p>
    <p style="margin-bottom: 20px;"><%= CRMInvoiceResource.InvoiceTaxesDescriptionTextEditDelete%></p>
    <div class="clearFix" style="margin-bottom: 8px;">
        <a class="gray button" id="createNewTax">
            <span class="plus"><%= CRMInvoiceResource.CreateInvoiceTax %></span>
        </a>
    </div>
    <table id="invoiceTaxesTable" class="tableBase" cellpadding="4" cellspacing="0">
        <colgroup>
            <col style="width: 1%;"/>
            <col style="width: 1%;"/>
            <col/>
            <col style="width: 40px;"/>
        </colgroup>
        <tbody>
        </tbody>
    </table>
</div>

<div id="invoiceTaxesActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="dropdown-item editInvoiceTaxLink"><%= CRMInvoiceResource.EditInvoiceTax %></a></li>
        <li><a class="dropdown-item deleteInvoiceTaxLink"><%= CRMCommonResource.Delete %></a></li>
    </ul>
</div>