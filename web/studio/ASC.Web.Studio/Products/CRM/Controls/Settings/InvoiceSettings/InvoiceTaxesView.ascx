<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceTaxesView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.InvoiceTaxesView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>

<div id="invoiceTaxesList"class="clearFix">
    <table id="invoiceTaxesTable" class="table-list padding4" cellpadding="0" cellspacing="0">
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
        <li><a class="dropdown-item with-icon edit editInvoiceTaxLink"><%= CRMInvoiceResource.EditInvoiceTax %></a></li>
        <li><a class="dropdown-item with-icon delete deleteInvoiceTaxLink"><%= CRMCommonResource.Delete %></a></li>
    </ul>
</div>