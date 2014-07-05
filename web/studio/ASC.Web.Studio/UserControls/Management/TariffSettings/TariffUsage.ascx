<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffUsage.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffUsage" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Billing" %>
<%@ Import Namespace="ASC.Core.Tenants" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

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
</div>

<table class="tariffs-panel" cols="3" cellspacing="0" cellpadding="0" frame="void">
    <thead>
        <tr valign="middle">
            <td class="tariffs-header-empty"></td>
            <th width="35%">
                <div class="tariffs-header-basic tariffs-header">
                    <span class="tariffs-block tariffs-block-center">
                        <%= string.Format(UserControlsCommonResource.TariffNameMonth,
                                (MonthIsDisable() ? SetStar(Resource.TariffRemarkDisabledMonth) : ""),
                                "",
                                "")%>
                    </span>
                </div>
            </th>
            <th width="35%">
                <div class="tariffs-header-office tariffs-header">
                    <span class="tariffs-block tariffs-block-center">
                        <%= string.Format(UserControlsCommonResource.TariffNameYear, "", "") %>
                    </span>
                </div>
            </th>
        </tr>
    </thead>
    <tbody>

        <% for (var i = 0; i < QuotasYear.Count; i++)
           {
               var quotaMonth = GetQuotaMonth(QuotasYear[i]); %>

        <tr class="tariff-item" valign="middle">
            <td>
                <div class="tariff-descr <%= CoreContext.Configuration.Standalone ? "tariff-descr-standalone" : "" %>">
                    <span class="tariff-users">
                        <% var prevUsersCount = TenantExtra.GetPrevUsersCount(QuotasYear[i]); %>
                        <%= prevUsersCount
                            + (prevUsersCount < QuotasYear[i].ActiveUsers
                                                    ? "-" + QuotasYear[i].ActiveUsers
                                                    : "") %>
                    </span>
                    <span>
                        <%= Resource.TariffActiveEmployees %>
                    </span>
                    <% if (!CoreContext.Configuration.Standalone)
                       { %>
                    <span class="tariff-storage describe-text">
                        <%= string.Format(Resource.TariffDiskSize, FileSizeComment.FilesSizeToString(QuotasYear[i].MaxTotalSize)) %>
                    </span>
                    <% } %>
                </div>
            </td>

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
                <label class="tariff-price-block <%= CurrentQuota.Equals(quotaMonth) ? " tariffs-current " : " " %> <%= QuotaForDisplay.Equals(quotaMonth) ? " tariffs-selected " : " " %>" data="<%= quotaMonth.Id %>">
                    <span class="tariffs-block">
                        <input type="radio" name="tariff" <%= CurrentQuota.Equals(quotaMonth) ? "checked=\"checked\"" : " " %>
                            <%= MonthIsDisable() ? "disabled=\"disabled\"" : "" %> />
                        <% if (!MonthIsDisable())
                           { %>
                        <input type="hidden" class="tariff-hidden-link tariff-hidden-<%= GetTypeLink(quotaMonth) %>" value="<%= GetShoppingUri(quotaMonth) %>" />
                        <input type="hidden" class="tariff-hidden-users" value="<%= quotaMonth.ActiveUsers %>" />
                        <input type="hidden" class="tariff-hidden-storage" value="<%= FileSizeComment.FilesSizeToString(quotaMonth.MaxTotalSize) %>" />
                        <% } %>

                        <% if (quotaMonth.Price2 == decimal.Zero)
                           { %>

                        <% if (quotaMonth.Price == decimal.Zero)
                           { %>
                        <span class="tariff-price"><%= UserControlsCommonResource.TariffFree %></span>
                        <% }
                           else
                           { %>
                        <span class="tariff-price"><%= (int) quotaMonth.Price + CurrencySymbol %></span><span class="tariff-price-descr describe-text">/<%= UserControlsCommonResource.TariffPerMonth %></span>
                        <% } %>

                        <% }
                           else
                           { %>
                        <span class="tariff-price tariff-price-lost">
                            <span></span><%= (int) quotaMonth.Price2 + CurrencySymbol %></span><span class="tariff-price-descr describe-text">/<%= UserControlsCommonResource.TariffPerMonth %></span>
                        <span class="tariff-price-sale tariff-news">
                            <span class="tariff-news-arrow tariff-news-arrow-left"></span><%= (int) quotaMonth.Price + CurrencySymbol %><span class="tariff-price-sale-text"> SALE </span><%= SetStar(string.Format(Resource.TariffRemarkSale, GetSaleDate().ToString(Thread.CurrentThread.CurrentCulture.DateTimeFormat.MonthDayPattern)), true) %>
                        </span>
                        <% } %>

                    </span>
                </label>
                <% } %>
            </td>

            <td>
                <label class="tariff-price-block <%= CurrentQuota.Equals(QuotasYear[i]) ? " tariffs-current " : " " %> <%= QuotaForDisplay.Equals(QuotasYear[i]) ? " tariffs-selected " : " " %>" data="<%= QuotasYear[i].Id %>">
                    <span class="tariffs-block">
                        <input type="radio" name="tariff" <%= CurrentQuota.Equals(QuotasYear[i]) ? "checked=\"checked\"" : " " %> />
                        <input type="hidden" class="tariff-hidden-link tariff-hidden-<%= GetTypeLink(QuotasYear[i]) %>" value="<%= GetShoppingUri(QuotasYear[i]) %>" />
                        <input type="hidden" class="tariff-hidden-users" value="<%= QuotasYear[i].ActiveUsers %>" />
                        <input type="hidden" class="tariff-hidden-storage" value="<%= FileSizeComment.FilesSizeToString(QuotasYear[i].MaxTotalSize) %>" />

                        <% if (QuotasYear[i].Price2 == decimal.Zero)
                           { %>

                        <% if (QuotasYear[i].Price == decimal.Zero)
                           { %>
                        <span class="tariff-price"><%= UserControlsCommonResource.TariffFree %></span>
                        <% }
                           else
                           { %>
                        <span class="tariff-price"><%= (int) QuotasYear[i].Price + CurrencySymbol %></span><span class="tariff-price-descr describe-text">/<%= UserControlsCommonResource.TariffPerYear %></span>
                        <% } %>

                        <% }
                           else
                           { %>
                        <span class="tariff-price tariff-price-lost">
                            <span></span><%= (int) QuotasYear[i].Price2 + CurrencySymbol %></span><span class="tariff-price-descr describe-text">/<%= UserControlsCommonResource.TariffPerYear %></span>
                        <span class="tariff-price-sale tariff-news">
                            <span class="tariff-news-arrow tariff-news-arrow-left"></span><%= (int) QuotasYear[i].Price + CurrencySymbol %><span class="tariff-price-sale-text"> SALE </span><%= SetStar(string.Format(Resource.TariffRemarkSale, GetSaleDate().ToString(Thread.CurrentThread.CurrentCulture.DateTimeFormat.MonthDayPattern)), true) %>
                        </span>
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

    <% if (CurrentQuota.Trial || CoreContext.Configuration.Standalone && CurrentTariff.QuotaId.Equals(Tenant.DEFAULT_TENANT))
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
                <input type="checkbox" id="buyRecommendationDisplay" />
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