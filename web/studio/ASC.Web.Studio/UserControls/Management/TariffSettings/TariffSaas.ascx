<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffSaas.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffSaas" %>

<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<div class="saas-tariff-page display-none">
    <% if (CurrentQuota.NonProfit)
        { %>
    <h1 class="header-1 main-header"><%= UserControlsCommonResource.TariffNonProfit %></h1>
    <% } else { %>
    <% if (IsStartup)
        { %>
    <h1 class="header-1 main-header"><%= UserControlsCommonResource.SaasTariffMainHeaderStartup %></h1>
    <% } %>
    <% if (IsBusiness && IsTrial)
        { %>
    <h1 class="header-1 main-header"><%: IsExpired ? UserControlsCommonResource.SaasTariffMainHeaderTrialExpired : UserControlsCommonResource.SaasTariffMainHeaderTrial %></h1>
    <% if (!IsExpired)
        { %>
    <div class="trial-days-left"><%= UserControlsCommonResource.SaasTariffInfoFreeDaysLeft %>&nbsp;<%= CurrentTariff.DueDate.Date.Subtract(DateTime.Today).Days %></div>
    <% } %>
    <% } %>
    <% if (IsBusiness && !IsTrial)
        { %>
    <h1 class="header-1 main-header"><%: IsExpired ? UserControlsCommonResource.SaasTariffMainHeaderBusinessExpired : UserControlsCommonResource.SaasTariffMainHeaderBusiness %></h1>
    <% } %>
    <% } %>
    <ul class="current-tariff">
        <li>
            <span><%= UserControlsCommonResource.SaasTariffInfoUsersCount %>:&nbsp;</span>
            <b title="<%= UserControlsCommonResource.SaasTariffInfoUsersCountTitle %>"><%= string.Format(IsPeopleModuleAvailable ? "<a target=\"_blank\" href=\"{2}\">{0}{1}</a>" : "{0}{1}",
                        CurrentUsersCount,
                        CurrentQuota.NonProfit ? string.Empty : "/" + CurrentQuota.ActiveUsers,
                        CommonLinkUtility.GetEmployees()) %></b>
            <% if (!CurrentQuota.NonProfit)
                { %>
            <div class="progress-wrapper"><div class="progress-value" <%= string.Format("style=\"width: {0}%;\"", ActiveUsersPercent) %>></div></div>
            <% } %>
        </li>
        <li>
            <span><%= UserControlsCommonResource.SaasTariffInfoStorageSpace %>:&nbsp;</span>
            <b title="<%= UserControlsCommonResource.SaasTariffInfoStorageSpaceTitle %>"><%= string.Format(UserControlsCommonResource.SaasTariffInfoStorageSpaceValue,
                        string.Format(IsAdmin ? "<a target=\"_blank\" href=\"{2}\">{0}/{1}</a>" : "{0}/{1}",
                            FileSizeComment.FilesSizeToString(CurrentUsedSize),
                            FileSizeComment.FilesSizeToString(CurrentQuota.MaxTotalSize),
                            CommonLinkUtility.GetAdministration(ManagementType.Statistic)),
                        StorageSpacePercent) %></b>
            <div class="progress-wrapper"><div class="progress-value" <%= string.Format("style=\"width: {0}%;\"", StorageSpacePercent) %>></div></div>
        </li>
        <% if (IsBusiness && !IsTrial &&!CurrentQuota.NonProfit && !IsExpired)
            { %>
        <li class="last"><%= string.Format(UserControlsCommonResource.SaasTariffInfoExpiresOnDate, "<b>" + CurrentTariff.DueDate.Date.ToLongDateString() + "</b>") %></li>
        <% } %>
    </ul>
    <% if (IsTrial && IsExpired)
        { %>
    <div class="available-tariff-plans display-none">
        <h1 class="header-1"><%= UserControlsCommonResource.SaasTariffChoosePlan %></h1>
        <div class="available-tariffs">
            <div class="available-tariff">
                <h2 class="header-2"><%= UserControlsCommonResource.SaasTariffStartup %></h2>
                <h4 class="header-4"><%= string.Format(UserControlsCommonResource.SaasTariffStartupCost, "<span class=\"tariff-price-cur\">" + GetPricePerMonthString(StartupQuota) + "</span>") %></h4>
                <div class="line"></div>
                <ul class="available-tariff-features">
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffStartupFeature1 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffStartupFeature2 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffStartupFeature3 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffStartupFeature4 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffStartupFeature5 %></li>
                </ul>
                <% if (StartupEnabled)
                    { %>
                <a id="continueStartupBtn" class="button gray"><%= Resource.ContinueButton %></a>
                <% }
                    else
                    { %>
                <a class="button gray" href="mailto:<%= SalesEmail %>"><%= UserControlsCommonResource.SaasTariffContactSales %></a>
                <% } %>
            </div>
            <div class="available-tariff active">
                <h2 class="header-2"><%= UserControlsCommonResource.SaasTariffBusiness %></h2>
                <h4 class="header-4"><%= string.Format(UserControlsCommonResource.SaasTariffBusinessCost, "<span class=\"tariff-price-cur\">" + GetPricePerMonthString(ThreeYearsQuota) + "</span>") %></h4>
                <div class="line"></div>
                <ul class="available-tariff-features">
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffBusinessFeature1 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffBusinessFeature2 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffBusinessFeature3 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffBusinessFeature4 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffBusinessFeature5 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffBusinessFeature6 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffBusinessFeature7 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffBusinessFeature8 %></li>
                    <li class="tariff-feature"><%= UserControlsCommonResource.SaasTariffBusinessFeature9 %></li>
                </ul>
                <a id="buyBusinessBtn" class="button green"><%= Resource.TariffButtonBuy %></a>
            </div>
        </div>
    </div>
    <div id="backBtn" class="back-button display-none">
        <%= UserControlsCommonResource.SaasTariffBackButton %>
    </div>
    <% } %>

    <% if (Regions.Count > 1)
        { %>
    <div id="currencyPanel" class="display-none">
        <div id="currencySelector">
            <span class="baseLinkAction"><%= CurrentRegion.ISOCurrencySymbol + " - " + CurrentRegion.CurrencyNativeName %></span>
        </div>

        <div id="currencyList" class="studio-action-panel">
            <ul class="dropdown-content">
                <% for (var i = 0; i < Regions.Count; i++)
                    { %>
                <li>
                    <a href="<%= TenantExtra.GetTariffPageLink() + "?cur=" + Regions[i].Name %>" class="dropdown-item <%= CurrentRegion.Equals(Regions[i]) ? "active" : string.Empty %>">
                        <%= Regions[i].ISOCurrencySymbol + " - " + Regions[i].CurrencyNativeName %>
                    </a>
                </li>
                <% } %>
            </ul>
        </div>

        <span id="currencyHelpSwitcher" class="HelpCenterSwitcher"></span>
        <div id="currencyHelp" class="popup_helper">
            <p><%= UserControlsCommonResource.TariffCurrencyHelp %></p>
        </div>
    </div>
    <% } %>

    <div class="subscription display-none">
        <div class="subscription-dscr">
            <% if (IsStartup)
                { %>
            <h1 class="header-1"><%= UserControlsCommonResource.SaasTariffBuyBusinessSubscriptionHeaderStartup %></h1>
            <% } %>
            <% if (IsBusiness && IsTrial)
                { %>
            <h1 class="header-1"><%: UserControlsCommonResource.SaasTariffBuyBusinessSubscriptionHeaderTrial %></h1>
            <% } %>
            <% if (IsBusiness && !IsTrial)
                { %>
            <h1 class="header-1"><%: UserControlsCommonResource.SaasTariffBuyBusinessSubscriptionHeaderBusiness %></h1>
            <% } %>
            <h4 class="header-4"><%= string.Format(UserControlsCommonResource.SaasTariffBuyBusinessSubscriptionSubHeader, "<span class=\"tariff-price-cur\">" + GetPricePerMonthString(ThreeYearsQuota) + "</span>") %></h4>
            <div class="line"></div>
            <h3 class="header-3"><%= UserControlsCommonResource.SaasTariffBenefits %></h3>
            <ul class="subscription-dscr-items">
                <li class="subscription-dscr-item people"><%= UserControlsCommonResource.SaasTariffBusinessFeature1 %></li>
                <li class="subscription-dscr-item storage"><%= UserControlsCommonResource.SaasTariffBusinessFeature2 %></li>
                <li class="subscription-dscr-item infinity"><%= UserControlsCommonResource.SaasTariffBusinessFeature3 %></li>
                <li class="subscription-dscr-item branding"><%= UserControlsCommonResource.SaasTariffBusinessFeature4 %></li>
                <li class="subscription-dscr-item ldap"><%= UserControlsCommonResource.SaasTariffBusinessFeature5 %></li>
                <li class="subscription-dscr-item cloud"><%= UserControlsCommonResource.SaasTariffBusinessFeature6 %></li>
                <li class="subscription-dscr-item history"><%= UserControlsCommonResource.SaasTariffBusinessFeature7 %></li>
                <li class="subscription-dscr-item puzzle"><%= UserControlsCommonResource.SaasTariffBusinessFeature8 %></li>
                <li class="subscription-dscr-item mail"><%= UserControlsCommonResource.SaasTariffBusinessFeature9 %></li>
            </ul>
        </div>
        <div class="subscription-calc requiredField">
            <div class="subscription-period">
                <div class="subscription-period-item" data-quotaid="<%= MonthQuota.Id %>">
                    <span><%= UserControlsCommonResource.SaasTariffOneMonth %></span>
                </div>
                <div class="subscription-period-item center active" data-quotaid="<%= YearQuota.Id %>">
                    <span><%= UserControlsCommonResource.SaasTariffOneYear %></span>
                    <div class="sale"><%= string.Format(UserControlsCommonResource.SaasTariffSale,  (int)(100 - (100 * YearQuota.Price / 12) / MonthQuota.Price)) %></div>
                </div>
                <div class="subscription-period-item" data-quotaid="<%= ThreeYearsQuota.Id %>">
                    <span><%= UserControlsCommonResource.SaasTariffThreeYears %></span>
                    <div class="sale"><%= string.Format(UserControlsCommonResource.SaasTariffSale,  (int)(100 - (100 * ThreeYearsQuota.Price / 36) / MonthQuota.Price)) %></div>
                </div>
            </div>
            <label><%= UserControlsCommonResource.SaasTariffSelectNumberOfUsers %></label>
            <input id="tariffSelector" type="number" min="<%= MinUsersCount %>" max="<%= MaxUsersCount %>" step="1" value="<%= Math.Min(Math.Max(CurrentUsersCount, MinUsersCount), MaxUsersCount) %>" class="textEdit" />
            <h3 class="header-3"><%= UserControlsCommonResource.SaasTariffSelectedTariffDetails %></h3>
            <ul class="selected-tariff">
                <li>
                    <span><%= UserControlsCommonResource.SaasTariffSelectedTariffPrice %>:</span>
                    <b><%= string.Format(UserControlsCommonResource.SaasTariffSelectedTariffPriceValue, "<span id=\"tariffUserPrice\" class=\"tariff-price-cur\"></span>") %></b>
                </li>
                <li>
                    <span><%= UserControlsCommonResource.SaasTariffInfoUsersCount %>:</span>
                    <b id="tariffUsersCount"></b>
                </li>
                <li>
                    <span><%= UserControlsCommonResource.SaasTariffSelectedTariffLength %>:</span>
                    <b id="tariffLength"></b>
                </li>
            </ul>
            <div class="line"></div>
            <div class="selected-tariff-total">
                <div><%= UserControlsCommonResource.SaasTariffTotalPrice %>:</div>
                <div>
                    <span class="sale tariff-price-cur" id="tariffSale"></span>
                    <b class="total tariff-price-cur" id="tariffTotalPrice"></b>
                </div>
            </div>
            <span class="requiredErrorText">
                <span id="tariffErrorMsg" class="display-none">
                    <% if (IsExpired)
                        { %>
                    <%= string.Format(UserControlsCommonResource.SaasTariffErrorExpired, "<span>" + CurrentUsersCount + "</span>", string.Format("<a href=\"mailto:{0}\">", SalesEmail), "</a>") %>
                    <% }
                        else
                        { %>
                    <%= string.Format(UserControlsCommonResource.SaasTariffError,
                            "<span>" + CurrentUsersCount + "</span>",
                            IsPeopleModuleAvailable ? string.Format("<a target=\"_blank\" href=\"{0}\">", CommonLinkUtility.GetEmployees()) : "",
                            IsPeopleModuleAvailable ? "</a>" : "") %>
                    <% } %>
                </span>
                <span id="minMaxErrorMsg" class="display-none">
                    <%= string.Format(UserControlsCommonResource.SaasTariffErrorMinMax, MinUsersCount, MaxUsersCount) %>
                </span>
            </span>
            <a id="buyNow" class="button green"><%= IsBusiness && !IsTrial && !IsExpired ? UserControlsCommonResource.SaasTariffChangePlanBtn : Resource.TariffButtonBuy %></a>
            <div class="subscription-customer-info display-none"></div>
            <div class="blocker display-none"></div>
        </div>
    </div>
    <div class="contact-info">
        <% if ((IsTrial && !IsExpired) || IsStartup)
            { %>
        <%= string.Format(UserControlsCommonResource.SaasTariffTContactSupport, string.Format("<a href=\"mailto:{0}\">", SalesEmail), SalesEmail, "</a>") %>
        <% } %>
        <% if ((IsTrial && IsExpired) || (IsBusiness && !IsTrial))
            { %>
        <%= string.Format(UserControlsCommonResource.SaasTariffTContactUs, string.Format("<a href=\"mailto:{0}\">", SalesEmail), "</a>") %>
        <% } %>
    </div>

    <input id="usersCount" type="hidden" value="<%= CurrentUsersCount %>" />
    <input id="regionName" type="hidden" value="<%= CurrentRegion.Name %>" />
</div>
