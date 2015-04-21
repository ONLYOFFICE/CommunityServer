<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffUsage.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffUsage" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Billing" %>
<%@ Import Namespace="ASC.Core.Tenants" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<% if ((int) RateRuble != 0)
   { %>
<style>
    @font-face {
        font-family: rub;
        src: url('<%= VirtualPathUtility.ToAbsolute("~/UserControls/Management/TariffSettings/css/rub.eot") %>');
        src:
            url("<%= VirtualPathUtility.ToAbsolute("~/UserControls/Management/TariffSettings/css/rub.eot") %>") format("embedded-opentype"),
            url("<%= VirtualPathUtility.ToAbsolute("~/UserControls/Management/TariffSettings/css/rub.woff") %>") format("woff"),
            url("<%= VirtualPathUtility.ToAbsolute("~/UserControls/Management/TariffSettings/css/rub.ttf") %>") format("truetype"),
            url("<%= VirtualPathUtility.ToAbsolute("~/UserControls/Management/TariffSettings/css/rub.svg") %>") format("svg");
    }
    .tariff-price-cur {
        font-family: rub, calibri;
    }
</style>
<% } %>

<% if (Partner != null)
   { %>
<div class="partner-is-label">
    <% if (Partner.DisplayType == PartnerDisplayType.LogoOnly && !string.IsNullOrEmpty(Partner.LogoUrl)) %>
    <% { %>
        <%= Resource.PartnerIs %>
        <img src="<%= Partner.LogoUrl %>" align="middle"/>
    <% } %>

    <% if (Partner.DisplayType == PartnerDisplayType.DisplayNameOnly && !string.IsNullOrEmpty(Partner.DisplayName)) %>
    <% { %>
        <%= Resource.PartnerIs %>
        <%= Partner.DisplayName.HtmlEncode() %>
    <% } %>

    <% if (Partner.DisplayType == PartnerDisplayType.All && (!string.IsNullOrEmpty(Partner.LogoUrl) || !string.IsNullOrEmpty(Partner.DisplayName))) %>
    <% { %>
        <%= Resource.PartnerIs %>
        <% if (!string.IsNullOrEmpty(Partner.LogoUrl)) %>
        <% { %>
            <img src="<%= Partner.LogoUrl %>" align="middle"/>
        <% } %>
        <% if (!string.IsNullOrEmpty(Partner.DisplayName)) %>
        <% { %>
            <%= Partner.DisplayName.HtmlEncode() %>
        <% } %>
    <% } %>
</div>
<% } %>

<div class="current-tariff-desc">
    <%= TariffDescription() %>
    <br />
    <br />
    <%= String.Format(Resource.TariffStatistics, "<a class=\"link-black-14\" href=\"" + CommonLinkUtility.GetEmployees() + "\">" + UsersCount + "</a>") %>
    <br />
    <%= CoreContext.Configuration.Standalone 
            ? ""
            : String.Format(Resource.TariffStatisticsStorage, "<a class=\"link-black-14\" href=\"" + CommonLinkUtility.GetAdministration(ManagementType.Statistic) + "\">" + FileSizeComment.FilesSizeToString(UsedSize) + "</a>") %>
    <% if (SmsEnable)
       { %>
    <br />
    <asp:PlaceHolder runat="server" ID="SmsBuyHolder"></asp:PlaceHolder>
    <% } %>
    <% if (VoipEnable)
       { %>
    <br />
    <asp:PlaceHolder runat="server" ID="VoipBuyHolder"></asp:PlaceHolder>
    <% } %>
</div>

<table class="tariffs-panel" cols="3" cellspacing="0" cellpadding="0" frame="void" data-id="<%= CoreContext.TenantManager.GetCurrentTenant().OwnerId %>">
    <thead>
        <% if (AnnualDiscount)
           { %>
        <tr valign="middle">
            <td colspan="3">
                <span class="sale-ticket"><%= GetAnnualSaleDate() %></span>
                <span class="header-base medium bold"><%= string.Format(UserControlsCommonResource.AnnualDiscountDescr, 33) %></span>
                <span class="sale-baloon"><%= string.Format(UserControlsCommonResource.AnnualDiscountMonth, 8, "<br/>") %></span>
            </td>
        </tr>
        <% } %>
        <tr valign="middle">
            <th>
                <span class="tariffs-header tariffs-header-descr"><%= UserControlsCommonResource.TariffNameUsers %></span>
            </th>
            <th width="35%">
                <span class="tariffs-header">
                    <%= string.Format(UserControlsCommonResource.TariffNameMonth, "", "", "") + (MonthIsDisable() ? SetStar(Resource.TariffRemarkDisabledMonth) : "")%>
                </span>
            </th>
            <th width="35%">
                <span class="tariffs-header"><%= string.Format(UserControlsCommonResource.TariffNameYear, "", "") %></span>
            </th>
        </tr>
    </thead>
    <tbody>
        <style>
            .tariff-price-sale-box::after {
                content: "<%= GetSaleDateStar() %>";
                right: -<%= 3+7*GetSaleDateStar().Length %>px;
            }
        </style>

        <% for (var i = 0; i < QuotasYear.Count; i++)
           {
               var quotaMonth = GetQuotaMonth(QuotasYear[i]); %>

        <tr class="tariff-item" valign="middle">
            <td>
                <div class="tariff-descr <%= CoreContext.Configuration.Standalone ? "tariff-descr-standalone" : "" %>">
                    <span class="tariff-users">
                        <%= QuotasYear[i].ActiveUsers %>
                    </span>
                    <span class="bold">
                        <%= Resource.TariffActiveEmployees %>
                    </span>
                    <% if (!CoreContext.Configuration.Standalone)
                       { %>
                    + <%= FileSizeComment.FilesSizeToString(QuotasYear[i].MaxTotalSize) %>
                    <% } %>
                </div>
            </td>

            <% if (quotaMonth != null || !QuotasYear[i].Free)
               { %>
            <td>
                <% if (quotaMonth == null)
                   { %>
                <label class="tariff-price-empty">
                    <span class="tariffs-block tariffs-block-center">
                        <span class="tariff-price"><%= UserControlsCommonResource.TariffPerYearOnly %></span>
                    </span>
                </label>
                <% }
                   else
                   { %>
                <label class="tariff-price-block <%= CurrentQuota.Equals(quotaMonth) && !CurrentQuota.Free ? " tariffs-current " : " " %> <%= QuotaForDisplay.Equals(quotaMonth) ? " tariffs-selected " : " " %>" data-id="<%= quotaMonth.Id %>">
                    <span class="tariffs-block">
                        <input type="radio" name="tariff" <%= CurrentQuota.Equals(quotaMonth) ? "checked=\"checked\"" : " " %>
                            <%= MonthIsDisable() ? "disabled=\"disabled\"" : "" %> />
                        <% if (!MonthIsDisable())
                           { %>
                        <input type="hidden" class="tariff-hidden-link tariff-hidden-<%= GetTypeLink(quotaMonth) %>" value="<%= GetShoppingUri(quotaMonth) %>" />
                        <input type="hidden" class="tariff-hidden-users" value="<%= quotaMonth.ActiveUsers %>" />
                        <input type="hidden" class="tariff-hidden-storage" value="<%= FileSizeComment.FilesSizeToString(quotaMonth.MaxTotalSize) %>" />
                        <input type="hidden" class="tariff-hidden-price" value="<%= (int) quotaMonth.Price %>" />
                        <% } %>

                        <% if (quotaMonth.Price == decimal.Zero)
                           { %>
                        <span class="tariff-price"><%= UserControlsCommonResource.TariffFree %></span>
                        <% }
                           else
                           { %>

                        <% if (quotaMonth.Price2 != decimal.Zero)
                           { %>
                        <span class="tariff-price-lost"><span class="price-line"></span><%= GetPriceString(quotaMonth.Price2) %></span>
                        <span class="tariff-price tariff-price-sale-box">
                            <span class="tariff-price-sale"><%= GetPriceString(quotaMonth.Price) %></span>
                            <span class="tariff-price-sale-percent"><%= (int) (quotaMonth.Price/quotaMonth.Price2*100) - 100 %>%</span>
                        </span>
                        <% }
                           else
                           { %>
                        <span class="tariff-price"><%= GetPriceString(quotaMonth.Price) %></span>
                        <% } %>
                        <% } %>

                    </span>
                </label>
                <% } %>
            </td>
            <% } %>

            <td>
                <label class="tariff-price-block <%= CurrentQuota.Equals(QuotasYear[i]) && !CurrentQuota.Free ? " tariffs-current " : " " %> <%= QuotaForDisplay.Equals(QuotasYear[i]) ? " tariffs-selected " : " " %>" data-id="<%= QuotasYear[i].Id %>">
                    <span class="tariffs-block">
                        <input type="radio" name="tariff" <%= CurrentQuota.Equals(QuotasYear[i]) ? "checked=\"checked\"" : " " %> />
                        <input type="hidden" class="tariff-hidden-link tariff-hidden-<%= GetTypeLink(QuotasYear[i]) %>" value="<%= GetShoppingUri(QuotasYear[i]) %>" />
                        <input type="hidden" class="tariff-hidden-users" value="<%= QuotasYear[i].ActiveUsers %>" />
                        <input type="hidden" class="tariff-hidden-storage" value="<%= FileSizeComment.FilesSizeToString(QuotasYear[i].MaxTotalSize) %>" />
                        <input type="hidden" class="tariff-hidden-price" value="<%= (int) QuotasYear[i].Price %>" />

                        <% if (QuotasYear[i].Price == decimal.Zero)
                           { %>
                        <span class="tariff-price"><%= UserControlsCommonResource.TariffFree %></span>
                        <% }
                           else
                           { %>

                        <% if (QuotasYear[i].Price2 != decimal.Zero)
                           { %>
                        <span class="tariff-price-lost"><span class="price-line"></span><%= GetPriceString(QuotasYear[i].Price2) %></span>
                        <% } %>

                        <% if (!AnnualDiscount && QuotasYear[i].Price2 != decimal.Zero)
                           { %>
                        <span class="tariff-price tariff-price-sale-box">
                            <span class="tariff-price-sale"><%= GetPriceString(QuotasYear[i].Price) %></span>
                            <span class="tariff-price-sale-percent"><%= (int) (QuotasYear[i].Price/QuotasYear[i].Price2*100) - 100 %>%</span>
                        </span>
                        <% }
                           else
                           { %>
                        <span class="tariff-price"><%= GetPriceString(QuotasYear[i].Price) %></span>
                        <% } %>
                        <% } %>

                    </span>
                </label>
            </td>

        </tr>
        <% } %>
    </tbody>
</table>

<div class="tariffs-button-block clearFix">

    <asp:PlaceHolder runat="server" ID="PaymentsCodeHolder"></asp:PlaceHolder>

    <% if (Partner == null)
       { %>
    
    <div class="button-block">

    <% if (CurrentQuota.Trial || CurrentQuota.Free || CoreContext.Configuration.Standalone && CurrentTariff.QuotaId.Equals(Tenant.DEFAULT_TENANT))
       { %>
    <a class="tariff-buy-action tariff-buy-limit button huge blue" href="">
        <%= Resource.TariffButtonBuy %>
    </a>
    <a class="tariff-buy-action tariff-buy-pay button huge <%= CurrentTariff.State >= TariffState.NotPaid ? " red " : " blue " %>" href="">
        <%= Resource.TariffButtonBuy %>
    </a>
    <% }
       else
       { %>
    <a class="tariff-buy-action tariff-buy-free button huge blue" href="">
        <%= Resource.TariffButtonFree %>
    </a>
    <a class="tariff-buy-action tariff-buy-limit button huge blue" href="">
        <%= Resource.TariffButtonUpgrade + WithFullRemarks() %>
    </a>
    <a class="tariff-buy-action tariff-buy-change button huge blue" href="">
        <%= Resource.TariffButtonUpgrade + WithFullRemarks() %>
    </a>
    <% if (CurrentTariff.Prolongable)
       { %>
    <a class="tariff-buy-action tariff-buy-pay button huge <%= CurrentTariff.Autorenewal ? " blue disable " : (CurrentTariff.State >= TariffState.NotPaid ? " red " : " blue ") %>" href="">
        <%= (CurrentTariff.Autorenewal ? Resource.TariffEnabledAutorenew : Resource.TariffButtonExtend) + WithFullRemarks() %>
    </a>
    <% }
       else
       { %>
    <a class="tariff-buy-action tariff-buy-pay button huge blue disable">
        <%= Resource.TariffButtonExtend %>
    </a>

    <% } %>
    <% } %>
    &nbsp;

    <%--<% if (CoreContext.Configuration.Standalone && string.IsNullOrEmpty(StudioKeySettings.GetSKey()))
       { %>
    <a class="tariff-buy-try button huge green" href="">
        <%= Resource.TariffButtonTryNow %>
    </a>
    <% } %>--%>
    
    </div>

    <div class="support-block clearFix">
        <div class="support-photo"></div>
        <div class="support-actions">
            <div class="support-title"><%= Resource.SupportBlockTitle %></div>
            <div>
                <span class="support-mail-btn">
                    <a class="link dotline" href="mailto:support@onlyoffice.com"><%= Resource.SupportBlockEmailBth %></a>
                </span>
                <span class="support-chat-btn">
                    <a class="link dotline" onclick="window.LC_API.open_chat_window()"><%= Resource.SupportBlockChatBtn %></a>
                </span>    
            </div>
        </div> 
    </div>

    <% } %>
</div>

<div class="tariff-remark">
    <%= GetRemarks() %>
</div>

<%-- Dialog --%>
<div id="tafirrDowngradeDialog" class="display-none">
    <sc:Container ID="downgradeInfoContainer" runat="server">
        <Header>
            <%= Resource.TariffDowngrade %>
        </Header>
        <Body>
            <span>
                <%= Resource.TariffDowngradeErrorTitle %>
            </span>
            <br />
            <br />
            <span>
                <%= String.Format(Resource.TariffDowngradeErrorStatisticsUsers, "<span id=\"downgradeUsers\" class=\"header-base-small\"></span>", "<span class=\"header-base-small\">" + UsersCount + "</span>") %>
            </span>
            <br />
            <span>
                <%= String.Format(Resource.TariffDowngradeErrorStatisticsStorage, "<span id=\"downgradeStorage\" class=\"header-base-small\"></span>", "<span class=\"header-base-small\">" + FileSizeComment.FilesSizeToString(UsedSize) + "</span>")%>
            </span>
            <br />
            <br />
            <span>
                <%= Resource.TarffDowngradeErrorDescription %>
            </span>
            <div class="middle-button-container">
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= Resource.OKButton %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>

<% if (!HideBuyRecommendation)
   { %>
<div id="buyRecommendationDialog" class="display-none">
    <sc:Container runat="server" ID="buyRecommendationContainer">
        <Header><%= Resource.TariffBuyRecommendationTitle%></Header>
        <Body>
            <span><%= Resource.TariffBuyRecommendation %></span>
            <br />
            <br />
            <label>
                <input type="checkbox" id="buyRecommendationDisplay" class="checkbox" />
                <%= Resource.LabelDontShowMessage %>
            </label>
            <div class="middle-button-container">
                <a id="buyRecommendationOk" class="button gray middle">
                    <%= Resource.OKButton %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>
<% } %>

<% if (!HideAnnualRecomendation)
   { %>
<div id="annualRecomendationDialog" class="display-none">
    <sc:Container runat="server" ID="annualRecomendationContainer">
        <Header><%= UserControlsCommonResource.AnnualDiscountHeader %></Header>
        <Body>
            <span class="header-base medium bold"><%= string.Format(UserControlsCommonResource.AnnualDiscountDescr, 33) %></span>
            <div class="annual-box clearFix">
                <span class="annual-descr"><%= string.Format(UserControlsCommonResource.AnnualDiscount, 
                                           "<span>4</span>",
                                           8, "<span class=\"annual-descr-bold\">", "</span>", "<br/>") %></span>
                <span class="annual-arrow"><span></span></span>
                <span class="annual-value">
                    <%= string.Format(UserControlsCommonResource.AnnualDiscountValue, "<br/>",
                            "<b>" + UsersCount + "</b>",
                            "<b>" + string.Format(UserControlsCommonResource.PerYear, GetPriceString(AnnualQuotaForDisplay.Price2)) + "</b>",
                            "<b>" + GetPriceString(AnnualQuotaForDisplay.Price) + "</b>",
                            "<b>" + GetPriceString(AnnualQuotaForDisplay.Price2 - AnnualQuotaForDisplay.Price) + "</b>") %>
                </span>
            </div>
            <div class="middle-button-container">
                <a id="buttonYearSubscribe" class="button blue middle" data-id="<%= AnnualQuotaForDisplay.Id %>">
                    <%= UserControlsCommonResource.AnnualDiscountSubscribe %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();">
                    <%= UserControlsCommonResource.AnnualDiscountNo %>
                </a>
            </div>
            <br />
            <label>
                <input type="checkbox" id="annualRecomendationDisplay" class="checkbox" />
                <%= Resource.LabelDontShowMessage %>
            </label>
        </Body>
    </sc:Container>
</div>
<% } %>

<% if (Partner == null)
   { %>
<!-- BEGIN livechatinc.com Code -->
<div class="livechat online" style="display: none; margin-top: 24px;">
    <a href="javascript:window.LC_API.open_chat_window();">
        <img style="max-width: 983px;" title="<%= Resource.SupportByChat %>" src="<%= GetChatBannerPath() %>" />
    </a>
</div>
<!-- END livechatinc.com Code -->
<% } %>