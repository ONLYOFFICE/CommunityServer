<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<asp:PlaceHolder runat="server" ID="tariffHolder"></asp:PlaceHolder>

<asp:Repeater runat="server" ID="PaymentsRepeater">
    <HeaderTemplate>
        <div class="tabs-section payments-history">
            <span id="paymentsTitle" class="header-base"><%= Resource.PaymentsTitle %></span>
            <span id="switcherPayments" class="toggle-button" data-switcher="0" 
                data-showtext="<%=Resource.Show%>" data-hidetext="<%=Resource.Hide%>">
                <%=Resource.Hide%>
            </span>
        </div>

        <table id="paymentsContainer" class="payments-table">
    </HeaderTemplate>
    <ItemTemplate>
            <tr class="borderBase">
                <td>#<%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).CartId %></td> 
                <td><%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).Method %></td>
                <td><%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).Name %></td>
                <td><%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).Email %></td>
                <td><%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).Date.ToShortDateString() %> <%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).Date.ToShortTimeString() %></td>
                <td><%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).Price.ToString("###,##") %> <%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).Currency %></td>
                <td><a class="link bold" href="<%# CommonLinkUtility.ToAbsolute("~/tariffs/invoice.ashx") + "?pid=" + ((ASC.Core.Billing.PaymentInfo)Container.DataItem).CartId %>" target="_blank"><%= Resource.TariffInvoice %></a></td>
            </tr>
    </ItemTemplate>
    <FooterTemplate>
        </table>
    </FooterTemplate>
</asp:Repeater>