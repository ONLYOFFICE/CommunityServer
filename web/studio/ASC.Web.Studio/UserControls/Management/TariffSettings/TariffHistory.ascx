<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffHistory.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffHistory" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

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
                    <td><%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).FName %> <%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).LName %></td>
                    <td><%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).Email %></td>
                    <td><%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).PaymentDate.ToShortDateString() %> <%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).PaymentDate.ToShortTimeString() %></td>
                    <td><%# (((ASC.Core.Billing.PaymentInfo)Container.DataItem).Qty * ((ASC.Core.Billing.PaymentInfo)Container.DataItem).Price).ToString("###,##") %> <%# ((ASC.Core.Billing.PaymentInfo)Container.DataItem).PaymentCurrency %></td>
                </tr>
    </ItemTemplate>
    <FooterTemplate>
            </tbody>
        </table>
    </FooterTemplate>
</asp:Repeater>