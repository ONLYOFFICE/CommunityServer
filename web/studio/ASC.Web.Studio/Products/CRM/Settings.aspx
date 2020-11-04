<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/CRM/Masters/BasicTemplate.Master" CodeBehind="Settings.aspx.cs" Inherits="ASC.Web.CRM.Settings" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ MasterType TypeName="ASC.Web.CRM.BasicTemplate" %>


<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    <asp:PlaceHolder ID="TitleContentHolder" runat="server"></asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="FilterContent" runat="server">
    <% if(IsInvoiceItemsList) { %>
    <div id="invoiceItemsFilterContainer">
        <div id="invoiceItemsAdvansedFilter"></div>
    </div>
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
    <% } %>
</asp:Content>

<asp:Content ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="PagingContent" runat="server">
    <% if(IsInvoiceItemsList) { %>
    <table id="tableForInvoiceItemsNavigation" class="crm-navigationPanel" cellpadding="6" cellspacing="0" border="0">
        <tbody>
        <tr>
            <td>
                <div id="divForInvoiceItemsPager">
                </div>
            </td>
            <td style="text-align:right;">
                <span class="gray-text"><%= CRMCommonResource.Total %>:&nbsp;</span>
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
    <% } %>
</asp:Content>