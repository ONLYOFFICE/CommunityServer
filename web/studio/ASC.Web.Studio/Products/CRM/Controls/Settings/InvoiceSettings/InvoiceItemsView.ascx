<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceItemsView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.InvoiceItemsView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>

<div id="invoiceItemsList">

    <div id="changeStatusDialog" class="studio-action-panel changeStatusDialog">
        <ul class="dropdown-content mobile-overflow"></ul>
    </div>

    <table id="invoiceItemsTable" class="table-list padding4" cellpadding="0" cellspacing="0">
        <colgroup>
            <%--<col style="width: 30px;"/>--%>
            <col style="width: 1%;"/>
            <col/>
            <col style="width: 1%;"/>
            <col style="width: 1%;"/>
            <col style="width: 1%;"/>
            <col style="width: 1%;"/>
            <col style="width: 40px;"/>
        </colgroup>
        <tbody>
        </tbody>
    </table>

</div>

<div id="invoiceItemsActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="dropdown-item with-icon edit editInvoiceItemLink"><%= CRMInvoiceResource.EditInvoiceItem %></a></li>
        <li><a class="dropdown-item with-icon delete deleteInvoiceItemLink"><%= CRMCommonResource.Delete %></a></li>
    </ul>
</div>