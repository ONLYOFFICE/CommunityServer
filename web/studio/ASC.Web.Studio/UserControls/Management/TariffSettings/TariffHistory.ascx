<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffHistory.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffHistory" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<asp:Repeater runat="server" ID="PaymentsRepeater">
    <HeaderTemplate>
        <div class="tabs-section payments-history">
            <span class="header-base"><%= Resource.PaymentsTitle %></span>
            <span id="switcherPayments" class="toggle-button" data-switcher="0" 
                data-showtext="<%=Resource.Show%>" data-hidetext="<%=Resource.Hide%>">
                <%=Resource.Hide%>
            </span>
        </div>

        <table id="paymentsContainer" class="payments-table">
            <tbody>
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
            </tbody>
        </table>
    </FooterTemplate>
</asp:Repeater>