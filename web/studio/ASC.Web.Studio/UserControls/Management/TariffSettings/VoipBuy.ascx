<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipBuy.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.VoipBuy" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Voip" %>
<%@ Import Namespace="Resources" %>

<%= string.Format(Resource.VoipPackageSateMsg, "<b>" + VoipPaymentSettings.Left + "</b>", VoipPaymentSettings.Paid) %>

<% if (CanBuy)
   { %>
    <div style="display: inline-block; margin-left: 8px;">
        <a id="voipBuy" class="baseLinkAction link plus gray"><%= Resource.VoipBuyMsg %></a>
        <span class="HelpCenterSwitcher" onclick=" jq(this).helper({ BlockHelperID: 'VoipBalanceHelper' }); "></span>
        <div id="VoipBalanceHelper" class="popup_helper">
            <p>
                <%= Resource.VoipBuyHelperMsg %>
            </p>
        </div>
    </div>
<% } %>

<div id="voipBuyDialog" style="display: none;">
    <sc:Container ID="VoipBuyContainer" runat="server">
        <Header><%= Resource.VoipBuyMsg %></Header>
        <Body>
            <asp:Repeater ID="VoipQuotas" runat="server">
                <ItemTemplate>
                    <label class="text">
                        <input type="radio" name="voipBuyPackageOption" <%# Container.ItemIndex == 0 ? "checked" : "" %>
                               data-quota-link="<%# CoreContext.PaymentManager.GetShoppingUri(((ASC.Core.Tenants.TenantQuota)Container.DataItem).Id) %>" />
                        <%# string.Format(Resource.VoipPackageMsg,
                                          "<span class=\"header-base medium\">" + (int)((ASC.Core.Tenants.TenantQuota)Container.DataItem).Price + CurrencySymbol + "</span>") %>
                    </label>
                </ItemTemplate>
                <SeparatorTemplate>
                    <br />
                    <br />
                </SeparatorTemplate>
            </asp:Repeater>
            
            <div class="middle-button-container">
                <a id="voipPay" class="button blue middle">
                    <%= Resource.ButtonBuy %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick=" PopupKeyUpActionProvider.CloseDialog(); return false; ">
                    <%= Resource.CancelButton %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>