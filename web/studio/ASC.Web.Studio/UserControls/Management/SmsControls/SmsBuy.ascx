<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SmsBuy.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SmsBuy" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.SMS" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<%= string.Format(Resource.SmsCount, "<b>" + StudioSmsNotificationSettings.LeftSms + "</b>", StudioSmsNotificationSettings.PaidSms) %>

<% if (ShowLink)
   { %>
&nbsp;&nbsp;&nbsp;<a class="link gray underline" href="<%= CommonLinkUtility.GetAdministration(ManagementType.PortalSecurity) %>#sms-auth"><%= Resource.SmsAuthTurnOn %></a>
<% }
   else
   { %>
<% if (CanBuy)
   { %>
<div style="display: inline-block; margin-left: 8px;">
    <a id="smsBuy" class="baseLinkAction link plus gray"><%= Resource.SmsBuyHeader %></a>
    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'SmsPackageHelper'});"></span>
    <div id="SmsPackageHelper" class="popup_helper">
        <p>
            <%= Resource.SmsPackageHelper %>
        </p>
    </div>
</div>

<div id="smsBuyDialog" style="display: none;">
    <sc:Container ID="SmsBuyContainer" runat="server">
        <Header><%= Resource.SmsBuyHeader %></Header>
        <Body>
            <asp:Repeater ID="SmsQuotas" runat="server">
                <ItemTemplate>
                    <label class="text">
                        <input type="radio" name="smsPackageOption" <%# Container.ItemIndex == 0 ? "checked" : "" %>
                            data-quota-link="<%# CoreContext.PaymentManager.GetShoppingUri(((ASC.Core.Tenants.TenantQuota)Container.DataItem).Id) %>" />
                        <%# string.Format(Resource.SmsPackage,
                                "<span class=\"header-base medium bold\">" + ((ASC.Core.Tenants.TenantQuota)Container.DataItem).ActiveUsers + "</span>",
                                "<span class=\"header-base medium\">" + (int) ((ASC.Core.Tenants.TenantQuota)Container.DataItem).Price + CurrencySymbol + "</span>") %>
                    </label>
                </ItemTemplate>
                <SeparatorTemplate>
                    <br />
                    <br />
                </SeparatorTemplate>
            </asp:Repeater>

            <div class="middle-button-container">
                <a id="smsPay" class="button blue middle">
                    <%= Resource.ButtonBuy %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= Resource.CancelButton %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>
<% } %>
<% } %>