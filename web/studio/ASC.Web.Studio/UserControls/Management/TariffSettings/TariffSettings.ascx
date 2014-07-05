<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<asp:PlaceHolder runat="server" ID="tariffHolder"></asp:PlaceHolder>

<asp:Repeater runat="server" ID="PaymentsRepeater" ItemType="ASC.Core.Billing.PaymentInfo">
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
                <td>#<%# Item.CartId %></td> 
                <td><%# Item.Method %></td>
                <td><%# Item.Name %></td>
                <td><%# Item.Email %></td>
                <td><%# Item.Date.ToShortDateString() %> <%# Item.Date.ToShortTimeString() %></td>
                <td><%# Item.Price.ToString("###,##") %> <%# Item.Currency %></td>
                <td><a class="link bold" href="<%# CommonLinkUtility.ToAbsolute("~/tariffs/invoice.ashx") + "?pid=" + Item.CartId %>" target="_blank"><%= Resource.TariffInvoice %></a></td>
            </tr>
    </ItemTemplate>
    <FooterTemplate>
        </table>
    </FooterTemplate>
</asp:Repeater>