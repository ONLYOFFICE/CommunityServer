<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffPartner.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffPartner" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<% switch (CurPartner.PaymentMethod)
   {
       case PartnerPaymentMethod.Keys: %>

<a class="tariff-pay-key button huge <%= (TariffNotPaid ? " red " : " blue ") + (TariffProlongable ? "" : " tariff-pay-key-prolongable ") %>">
    <%= Resource.PartnerPayKey %>
</a>

<div id="partnerPayKeyDialog" class="display-none">
    <sc:Container runat="server" ID="PartnerPayKeyContainer">
        <Header><%= Resource.PartnerPayKey %></Header>
        <Body>
            <label>
                <input type="radio" id="registrationKeyOption" name="partnerKeyOption" /><%= Resource.PartnerKeyEnter %></label>
            <input type="text" id="registrationKeyValue" class="textEdit" />

            <br />
            <label>
                <input type="radio" id="registrationRequestOption" name="partnerKeyOption" /><%= Resource.PartnerKeyRequest %></label>
            <div class="error-popup display-none"></div>
            <div class="big-button-container">
                <a class="tariff-key-aplly button blue middle disable">
                    <%= Resource.PartnerKeyApply %>
                </a>
                <a class="tariff-key-request button blue middle">
                    <%= Resource.PartnerRequestSend %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="tariff-key-cancel button gray middle">
                    <%= Resource.CancelButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>

<div id="partnerApplyDialog" class="display-none">
    <sc:Container runat="server" ID="PartnerApplyContainer">
        <Header><%=Resource.PartnerCodeWait %></Header>
        <Body>
            <%: Resource.PartnerCodeWaitInfo %>
            <div class="middle-button-container">
                <a class="button blue middle" onclick="PopupKeyUpActionProvider.CloseDialog();">
                    <%= Resource.OKButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>

<div id="partnerRequestDialog" class="display-none">
    <sc:Container runat="server" ID="PartnerRequestContainer">
        <Header><%= Resource.PartnerRequestSent %></Header>
        <Body>
            <%: Resource.PartnerRequestInfo %>
            <div class="middle-button-container">
                <a class="button blue middle" onclick="PopupKeyUpActionProvider.CloseDialog();">
                    <%= Resource.OKButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>

<% break;
       case PartnerPaymentMethod.PayPal: %>

<a class="tariff-pay-pal tariff-buy-action button huge green <%= (TariffProlongable ? "" : " tariff-pay-key-prolongable ") %>">
    <%= Resource.PartnerPayPal %>
</a>
<a class="tariff-buy-action tariff-buy-limit button huge green">
    <%= Resource.PartnerPayPal %>
</a>

<div id="partnerPayExceptionDialog" class="display-none">
    <sc:Container runat="server" ID="PartnerPayExceptionContainer">
        <Header><%= Resource.PartnerPayExceptionHeader %></Header>
        <Body>
            <span id="partnerPayExceptionText"></span>
            <div class="middle-button-container">
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();">
                    <%= Resource.CloseButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>

<% break;
       case PartnerPaymentMethod.External: %>

<a class="tariff-buy-pay tariff-buy-change tariff-buy-action button huge blue <%= (TariffProlongable ? "" : " tariff-pay-key-prolongable ") %>">
    <%= Resource.ButtonBuy %>
</a>
<a class="tariff-buy-action tariff-buy-limit button huge blue">
    <%= Resource.ButtonBuy %>
</a>

<% break;
   } %>

<div class="partner-area">
    <% if (!string.IsNullOrEmpty(CurPartner.SupportEmail))
       { %>
    <span class="describe-text"><%= Resource.PartnerSupport %>:</span>
    <a href="mailto:<%= CurPartner.SupportEmail %>" class="link underline" target="_blank"><%= CurPartner.SupportEmail %></a>
    <br />
    <% }
       if (!string.IsNullOrEmpty(CurPartner.SalesEmail))
       { %>
    <span class="describe-text"><%= Resource.PartnerSales %>:</span>
    <a href="mailto:<%= CurPartner.SalesEmail %>" class="link underline" target="_blank"><%= CurPartner.SalesEmail %></a>
    <br />
    <% }
       if (!string.IsNullOrEmpty(CurPartner.SupportPhone))
       { %>
    <span class="describe-text"><%= Resource.PartnerPhone %>:</span>
    <%: CurPartner.SupportPhone %>
    <% } %>
</div>
