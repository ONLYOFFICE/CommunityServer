<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffUsage.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffUsage" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<% if (InRuble)
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

<div class="current-tariff-desc">
    <%= TariffDescription() %>
    <br />
    <br />
    <%= String.Format(Resource.TariffStatistics,
                      (PeopleModuleAvailable
                           ? "<a class=\"link-black-14 bold\" href=\"" + CommonLinkUtility.GetEmployees() + "\">" + UsersCount + "</a>"
                           : "<span class=\"bold\">" + UsersCount + "</span>")
                      + (!CurrentQuota.NonProfit ? "/" + CurrentQuota.ActiveUsers : string.Empty)) %>
    <br />
    <%= String.Format(Resource.TariffStatisticsStorage,
                      (CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID)
                           ? "<a class=\"link-black-14 bold\" href=\"" + CommonLinkUtility.GetAdministration(ManagementType.Statistic) + "\">" + FileSizeComment.FilesSizeToString(UsedSize) + "</a>"
                           : "<span class=\"bold\">" + FileSizeComment.FilesSizeToString(UsedSize) + "</span>")
                      + "/" + FileSizeComment.FilesSizeToString(CurrentQuota.MaxTotalSize)) %>
</div>

<div class="tariff-header clearFix">
    <div class="tariff-user-panel">
        <div class="header-base bold"><%= UserControlsCommonResource.TariffUserChoose %></div>

        <div class="tariff-user-question"><%= UserControlsCommonResource.TariffUserChooseQuestion %></div>

        <div class="tariff-slider-container" data-default="<%= QuotaForDisplay.ActiveUsers %>" data-min="<%= MinActiveUser %>">
            <div id="pricingPlanSlider"></div>
        </div>

        <div class="tariff-user-descr">
            <% for (var i = 0; i < QuotasYear.Count; i++)
               { %>
            <div class="tariff-user-descr-item <%= QuotaForDisplay.ActiveUsers == QuotasYear[i].ActiveUsers ? "tariff-user-descr-item-selected" : string.Empty %>"
                data-users="<%= QuotasYear[i].ActiveUsers %>">
                <% var prevUserCount = TenantExtra.GetPrevUsersCount(QuotasYear[i]); %>
                <%= string.Format(UserControlsCommonResource.TariffUserDescr,
                              "<b>",
                              "</b>",
                              prevUserCount,
                              QuotasYear[i].ActiveUsers,
                              FileSizeComment.FilesSizeToString(QuotasYear[i].MaxTotalSize),
                              "",
                              "",
                              SetStar(UserControlsCommonResource.ActiveUserDescr)) %>
            </div>
            <% } %>

            <div class="tariff-user-warn-min">
                <%= UserControlsCommonResource.TariffUserWarnDescr %>
                <span class="tariff-user-warn-link baseLinkAction"><%= UserControlsCommonResource.TariffUserWarnRead %></span>
            </div>

            <div class="tariff-user-warn-max">
                <%= string.Format(UserControlsCommonResource.TariffUserRequestDescr,
                                  QuotasYear.Count > 0 ? QuotasYear.Last().ActiveUsers : 0,
                                  SetStar(UserControlsCommonResource.ActiveUserDescr)) %>
            </div>
        </div>
    </div>
</div>

<% if (Regions.Count > 1)
   { %>
<div id="currencyPanel">
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

<table class="tariffs-panel" cols="3" cellspacing="0" cellpadding="0" frame="void">
    <thead>
        <tr valign="middle">
            <td width="30%" valign="bottom">
                <div class="tariffs-header-month tariffs-header">
                    <%= string.Format(UserControlsCommonResource.TariffNameSMonth,
                                      "<div class=\"tariffs-name\">",
                                      "</div>",
                                      MonthIsDisable ? SetStar(CurrentQuota.Year3 ? Resource.TariffRemarkDisabledYear : Resource.TariffRemarkDisabledMonth) : "") %>
                </div>
            </td>
            <td width="30%" valign="bottom">
                <div class="tariffs-header-year tariffs-header">
                    <div class="tariffs-pop"><%= UserControlsCommonResource.TariffPop %></div>
                    <%= string.Format(UserControlsCommonResource.TariffNameSYear,
                                      "<div class=\"tariffs-name\">",
                                      "</div>",
                                      YearIsDisable ? SetStar(Resource.TariffRemarkDisabledYear) : "") %>
                </div>
            </td>
            <td width="30%" valign="bottom">
                <div class="tariffs-header-year3 tariffs-header">
                    <%= string.Format(UserControlsCommonResource.TariffNameSYear3,
                                      "<div class=\"tariffs-name\">",
                                      "</div>") %>
                </div>
            </td>
        </tr>
    </thead>
    <tbody>
        <% 
            for (var i = 0; i < QuotasYear.Count; i++)
            {
                Tuple<string, string, string> buyAttr;
                var quotaYear = QuotasYear[i];
        %>

        <tr class="tariff-item <%= QuotaForDisplay.ActiveUsers == quotaYear.ActiveUsers ? "tariff-item-selected" : string.Empty %>"
            valign="middle" data-users="<%= quotaYear.ActiveUsers %>" data-storage="<%= FileSizeComment.FilesSizeToString(quotaYear.MaxTotalSize) %>">
            <td>
                <div class="tariffs-body tariffs-body-month">

                    <% var quotaMonth = GetQuotaMonth(quotaYear);
                       if (quotaMonth != null || !quotaYear.Free)
                       { %>
                    <% if (quotaMonth == null)
                       {
                           var fakePrice = FakePrices[i]; %>
                    <div class="tariffs-price-dscr">
                        <%= string.Format(UserControlsCommonResource.TariffPricePer,
                                          "<span class=\"price-string\">"
                                          + GetPerUserPrice(null)
                                          + "</span>") %>
                    </div>

                    <div class="tariffs-price"><%= GetPriceString(fakePrice) %></div>

                    <div class="tariffs-price-per"><%= UserControlsCommonResource.TariffBasicPrice %></div>

                    <div class="tariffs-lost-descr"><%= UserControlsCommonResource.TariffPerYearOnly %></div>

                    <% }
                       else
                       { %>
                    <div class="tariffs-price-dscr">
                        <%= string.Format(UserControlsCommonResource.TariffPricePer,
                                          "<span class=\"price-string\">"
                                          + GetPerUserPrice(quotaMonth)
                                          + "</span>") %>
                    </div>

                    <div class="tariffs-price">
                        <%= quotaMonth.Price == decimal.Zero
                                ? UserControlsCommonResource.TariffFree
                                : GetPriceString(quotaMonth) %>
                    </div>

                    <div class="tariffs-price-per"><%= UserControlsCommonResource.TariffBasicPrice %></div>

                    <% buyAttr = GetBuyAttr(quotaMonth); %>
                    <a class="tariffs-buy-action button huge <%= buyAttr.Item1 %>"
                        <%= !string.IsNullOrEmpty(buyAttr.Item2) ? "href=\"" + buyAttr.Item2 + "\"" : string.Empty %>>
                        <%= buyAttr.Item3 %>
                    </a>

                    <% } %>
                    <% } %>
                </div>
            </td>

            <% var priceMonth = quotaMonth != null ? GetPrice(quotaMonth) : (FakePrices[i] * (InRuble ? SetupInfo.ExchangeRateRuble : 1)); %>
            <td>
                <div class="tariffs-body tariffs-body-year">
                    <div class="tariffs-price-dscr">
                        <%= string.Format(UserControlsCommonResource.TariffPricePer,
                                          "<span class=\"price-string\">"
                                          + GetPerUserPrice(quotaYear)
                                          + "</span>") %>
                    </div>

                    <div class="tariffs-price">
                        <%= quotaYear.Price == decimal.Zero
                                ? UserControlsCommonResource.TariffFree
                                : GetPriceString(quotaYear) %>
                    </div>

                    <div class="tariffs-price-per"><%= UserControlsCommonResource.TariffLimitedPrice %></div>

                    <div class="tariffs-price-sale">
                        <%= string.Format(UserControlsCommonResource.TariffPriceOffer, GetPriceString(GetSaleValue(priceMonth, quotaYear), false)) %>
                    </div>

                    <div class="tariffs-descr">
                        <%= string.Format(Resource.TariffRemarkSale, GetSaleDate()) %>
                    </div>

                    <% buyAttr = GetBuyAttr(quotaYear); %>
                    <a class="tariffs-buy-action button huge <%= buyAttr.Item1 %>"
                        <%= !string.IsNullOrEmpty(buyAttr.Item2) ? "href=\"" + buyAttr.Item2 + "\"" : string.Empty %>>
                        <%= buyAttr.Item3 %>
                    </a>
                </div>
            </td>

            <td>
                <div class="tariffs-body tariffs-body-year3">
                    <% var quotaYear3 = GetQuotaYear3(quotaYear);
                       if (quotaYear3 != null)
                       { %>

                    <div class="tariffs-price-dscr">
                        <%= string.Format(UserControlsCommonResource.TariffPricePer,
                                          "<span class=\"price-string\">"
                                          + GetPerUserPrice(quotaYear3)
                                          + "</span>") %>
                    </div>

                    <div class="tariffs-price">
                        <%= quotaYear3.Price == decimal.Zero
                                ? UserControlsCommonResource.TariffFree
                                : GetPriceString(quotaYear3) %>
                    </div>

                    <div class="tariffs-price-per"><%= UserControlsCommonResource.TariffLimitedPrice %></div>

                    <div class="tariffs-price-sale">

                        <%= string.Format(UserControlsCommonResource.TariffPriceOffer, GetPriceString(GetSaleValue(priceMonth, quotaYear3), false)) %>
                    </div>

                    <div class="tariffs-descr">
                        <%= string.Format(Resource.TariffRemarkSale, GetSaleDate()) %>
                    </div>

                    <% buyAttr = GetBuyAttr(quotaYear3); %>
                    <a class="tariffs-buy-action button huge <%= buyAttr.Item1 %>"
                        <%= !string.IsNullOrEmpty(buyAttr.Item2) ? "href=\"" + buyAttr.Item2 + "\"" : string.Empty %>>
                        <%= buyAttr.Item3 %>
                    </a>

                    <% } %>
                </div>
            </td>

        </tr>
        <% } %>
    </tbody>
</table>

<% var linkList = new Dictionary<string, string>
       {
           {"fr", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4577517&doc=azBvMUU2U0lKMjdqcVJQZVhWdHBMQ1g5UUZyc1dHbWUzaG1WRy9xa2RHUT0_IjQ1Nzc1MTci0"},
           {"de", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4577515&doc=WUphUzBkbW1lVGNKRDl3c01Vb2REdGRFWEN1WGI4OSs0UmdBWUU4ekpKaz0_IjQ1Nzc1MTUi0"},
           {"en", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4577516&doc=M1c2MGY3aXlYVzZxZGV6M014eFVRS21pSk52ZTNiWXBuYnNJYnpxdHlQVT0_IjQ1Nzc1MTYi0"},
           {"ru", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4577519&doc=QkpoRmJCSnJyYVo2UnAxcktxWFJqbWZvVjhtZVFodnd4UllCS0tweEcwbz0_IjQ1Nzc1MTki0"},
           {"it", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4577518&doc=TEFOU3NUcmtZTjl0eUN2WklaWkllMkI3U2t5Rm5va2lDSHJXeTZBK3A5ST0_IjQ1Nzc1MTgi0"}
       };
   string tariffLink;
   if (!linkList.TryGetValue(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, out tariffLink))
   {
       tariffLink = linkList["en"];
   }
%>

<a class="see-full-price link-black-12" target="_blank" href="<%= tariffLink %>"><%= UserControlsCommonResource.TariffFullPrice %></a>

<div class="tariff-request-panel clearFix">
    <div class="header-base bold"><%= UserControlsCommonResource.TariffRequestHeader %></div>
    <div class="request-form">
        <% var userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID); %>

        <p class="confirm-block-text gray-text">
            <%= Resource.FirstName %><span class="required-mark">*</span>
        </p>
        <input type="text" maxlength="64" tabindex="1" class="text-edit-fname text-edit" required="required" placeholder="<%= Resource.FirstName %>" value="<%= userInfo.FirstName.HtmlEncode() %>">

        <p class="confirm-block-text gray-text">
            <%= Resource.LastName %><span class="required-mark">*</span>
        </p>
        <input type="text" maxlength="64" tabindex="2" class="text-edit-lname text-edit" required="required" placeholder="<%= Resource.LastName %>" value="<%= userInfo.LastName.HtmlEncode() %>">

        <p class="confirm-block-text gray-text">
            <%= CustomNamingPeople.Substitute<Resource>("UserPost").HtmlEncode() %>
        </p>
        <input type="text" maxlength="64" tabindex="3" class="text-edit-title text-edit" required="required" placeholder="<%= CustomNamingPeople.Substitute<Resource>("UserPost").HtmlEncode() %>" value="<%= userInfo.Title %>">

        <p class="confirm-block-text gray-text">
            <%= Resource.Email %><span class="required-mark">*</span>
        </p>
        <input type="email" maxlength="64" tabindex="4" class="text-edit-email text-edit" required="required" placeholder="<%= Resource.Email %>" value="<%= userInfo.Email.HtmlEncode() %>">

        <p class="confirm-block-text gray-text">
            <%= Resource.TitlePhone %><span class="required-mark">*</span>
        </p>
        <input type="text" maxlength="64" tabindex="5" class="text-edit-phone text-edit" required="required" title="<%= Resource.TitlePhone %>"
             pattern="\+?\d{4,63}" placeholder="<%= Resource.TitlePhone %>" value="<%= userInfo.MobilePhone %>" data-country="<%= PhoneCountry %>">

        <p class="confirm-block-text gray-text">
            <%= UserControlsCommonResource.CompanyTitle %><span class="required-mark">*</span>
        </p>
        <input type="text" maxlength="64" tabindex="6" class="text-edit-ctitle text-edit" required="required" placeholder="<%= UserControlsCommonResource.CompanyTitle %>">

        <p class="confirm-block-text gray-text">
            <%= UserControlsCommonResource.CompanySizeTitle %><span class="required-mark">*</span>
        </p>
        <select class="text-edit-csize text-edit" required="required" tabindex="7">
            <% var usersCount = new[] { 2, 5, 10, 20, 30, 50, 100, 200, 300, 500, 700, 1000 };

               var selected = usersCount.FirstOrDefault(c => c >= UsersCount);
               for (var i = 0; i <= usersCount.Length; i++)
               {
                   var opt =
                       i == usersCount.Length
                           ? string.Format(UserControlsCommonResource.LicenseRequestQuotaMore, usersCount[i - 1])
                           : string.Format(UserControlsCommonResource.LicenseRequestQuota,
                                           i == 0 ? 1 : usersCount[i - 1] + 1,
                                           usersCount[i]); %>
            <option value="<%= opt %>"
                <%= i < usersCount.Length && usersCount[i] == selected
                    || i == usersCount.Length && selected == 0
                        ? "selected=\"selected\"" : "" %>>
                <%= opt %>
            </option>
            <% } %>
        </select>

        <p class="confirm-block-text gray-text">
            <%= UserControlsCommonResource.SiteTitle %><span class="required-mark">*</span>
        </p>
        <input type="text" maxlength="64" tabindex="8" class="text-edit-site text-edit" required="required" placeholder="<%= UserControlsCommonResource.SiteTitle %>">

        <p class="confirm-block-text gray-text">
            <%= UserControlsCommonResource.TariffRequestContent %><span class="required-mark">*</span>
        </p>
        <textarea rows="4" tabindex="9" class="text-edit-message text-edit" required="required" placeholder="<%= UserControlsCommonResource.TariffRequestContentHolder %>"></textarea>
    </div>

    <div class="middle-button-container">
        <a class="tariff-request button blue huge" tabindex="10">
            <%= UserControlsCommonResource.TariffRequestBtn %></a>
    </div>
</div>


<div class="support-block clearFix">
    <div class="support-photo"></div>
    <div class="support-actions">
        <div class="support-title"><%= Resource.SupportBlockTitle %></div>
        <div>
            <span class="support-mail-btn">
                <a class="link dotline" href="mailto:support@onlyoffice.com"><%= Resource.SupportBlockEmailBth %></a>
            </span>
        </div>
    </div>
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
                <%: Resource.TariffDowngradeErrorTitle %>
            </span>
            <br />
            <br />
            <span>
                <%= String.Format(Resource.TariffDowngradeErrorStatisticsUsers.HtmlEncode(), "<span id=\"downgradeUsers\" class=\"header-base-small\"></span>", "<span class=\"header-base-small\">" + UsersCount + "</span>") %>
            </span>
            <br />
            <span>
                <%= String.Format(Resource.TariffDowngradeErrorStatisticsStorage.HtmlEncode(), "<span id=\"downgradeStorage\" class=\"header-base-small\"></span>", "<span class=\"header-base-small\">" + FileSizeComment.FilesSizeToString(UsedSize) + "</span>")%>
            </span>
            <br />
            <br />
            <span>
                <%: Resource.TarffDowngradeErrorDescription %>
            </span>
            <div class="middle-button-container">
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= Resource.OKButton %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>
