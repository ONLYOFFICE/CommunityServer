<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceItemsView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.InvoiceItemsView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>


<div id="invoiceItemsFilterContainer">
    <div id="invoiceItemsAdvansedFilter"></div>
</div>

<div id="invoiceItemsList">
    <ul id="invoiceItemsHeaderMenu" class="clearFix contentMenu contentMenuDisplayAll">
        <%--<li class="menuAction menuActionSelectAll menuActionSelectLonely">
            <div class="menuActionSelect">
                <input type="checkbox" id="mainSelectAllInvoiceItems" title="<%=CRMCommonResource.SelectAll%>" onclick="ASC.CRM.InvoiceItemsView.selectAll(this);" />
            </div>
        </li>--%>

        <li class="menuAction menuActionCreateNew unlockAction plus" title="<%= CRMInvoiceResource.CreateInvoiceItem %>">
            <span><%= CRMInvoiceResource.CreateInvoiceItem %></span>
        </li>

        <li class="menu-action-simple-pagenav">
        </li>
        <li class="menu-action-checked-count">
            <span></span>
            <a class="linkDescribe baseLinkAction" style="margin-left:10px;" onclick="ASC.CRM.InvoiceItemsView.deselectAll();">
                <%= CRMCommonResource.DeselectAll%>
            </a>
        </li>
        <li class="menu-action-on-top">
            <a class="on-top-link" onclick="javascript:window.scrollTo(0, 0);">
                <%= CRMCommonResource.OnTop%>
            </a>
        </li>
    </ul>
    <div class="header-menu-spacer">&nbsp;</div>

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


    <table id="tableForInvoiceItemsNavigation" class="crm-navigationPanel" cellpadding="6" cellspacing="0" border="0">
        <tbody>
        <tr>
            <td>
                <div id="divForInvoiceItemsPager">
                </div>
            </td>
            <td style="text-align:right;">
                <span class="gray-text"><%= CRMCommonResource.Total %>:</span>
                <span class="gray-text" id="totalInvoiceItemsOnPage"></span>

                <span class="gray-text"><%= CRMCommonResource.ShowOnPage %>:&nbsp;</span>
                <select class="top-align">
                    <option value="25">25</option>
                    <option value="50">50</option>
                    <option value="75">75</option>
                    <option value="100">100</option>
                </select>
            </td>
        </tr>
        </tbody>
    </table>
</div>

<div id="invoiceItemsActionMenu" class="studio-action-panel">
    <ul class="dropdown-content">
        <li><a class="dropdown-item editInvoiceItemLink"><%= CRMInvoiceResource.EditInvoiceItem %></a></li>
        <li><a class="dropdown-item deleteInvoiceItemLink"><%= CRMCommonResource.Delete %></a></li>
    </ul>
</div>