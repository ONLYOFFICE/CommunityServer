<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffCustom.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffCustom" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>


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

        <div class="tariff-slider-container" data-default="<%= UsersCount %>" data-min="<%= MinActiveUser %>">
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
                              "") %>
            </div>
            <% } %>

            <div class="tariff-user-warn-min">
                <%= UserControlsCommonResource.TariffUserWarnDescr %>
                <span class="tariff-user-warn-link baseLinkAction"><%= UserControlsCommonResource.TariffUserWarnRead %></span>
            </div>

            <div class="tariff-user-warn-max">
                <%= string.Format(UserControlsCommonResource.TariffUserRequestDescr,
                                  QuotasYear.Count > 0 ? QuotasYear.Last().ActiveUsers : 0,
                                  "") %>
            </div>
        </div>
    </div>
</div>

<table class="tariffs-panel" cols="3" cellspacing="0" cellpadding="0" frame="void">
    <thead>
        <tr valign="middle">
            <td width="50%" valign="bottom">
                <div class="tariffs-header-month tariffs-header">
                    <%= string.Format(UserControlsCommonResource.TariffNameSMonth,
                                      "<div class=\"tariffs-name\">",
                                      "</div>",
                                      "") %>
                </div>
            </td>
            <td width="50%" valign="bottom">
                <div class="tariffs-header-year tariffs-header">
                    <div class="tariffs-pop"><%= UserControlsCommonResource.TariffPop %></div>
                    <%= string.Format(UserControlsCommonResource.TariffNameSYear,
                                      "<div class=\"tariffs-name\">",
                                      "</div>",
                                      "") %>
                </div>
            </td>
        </tr>
    </thead>
    <tbody>
        <% 
            for (var i = 0; i < QuotasYear.Count; i++)
            {
                var quotaYear = QuotasYear[i];
        %>

        <tr class="tariff-item <%= QuotaForDisplay.ActiveUsers == quotaYear.ActiveUsers ? "tariff-item-selected" : string.Empty %>"
            valign="middle" data-users="<%= quotaYear.ActiveUsers %>" data-storage="<%= FileSizeComment.FilesSizeToString(quotaYear.MaxTotalSize) %>">
            <td>
                <div class="tariffs-body tariffs-body-month">

                    <% var quotaMonth = GetQuotaMonth(quotaYear);
                       if (quotaMonth != null || !quotaYear.Free)
                       { %>
                    <div class="tariffs-price-dscr">
                        <%= string.Format(UserControlsCommonResource.TariffPricePer,
                                          "<span class=\"price-string\">"
                                          + 0
                                          + "</span>") %>
                    </div>

                    <div class="tariffs-price">
                        0
                    </div>

                    <a class="tariffs-buy-action button huge tarrifs-button-month">
                        <%= Resource.TariffButtonBuy %>
                    </a>

                    <% } %>
                </div>
            </td>

            <td>
                <div class="tariffs-body tariffs-body-year">
                    <div class="tariffs-price-dscr">
                        <%= string.Format(UserControlsCommonResource.TariffPricePer,
                                          "<span class=\"price-string\">"
                                          + 0
                                          + "</span>") %>
                    </div>

                    <div class="tariffs-price">
                        0
                    </div>

                    <a class="tariffs-buy-action button huge tarrifs-button-year">
                        <%= Resource.TariffButtonBuy %>
                    </a>
                </div>
            </td>
        </tr>
        <% } %>
    </tbody>
</table>

<div class="tariff-request-panel clearFix">
    <div class="header-base bold"><%= UserControlsCommonResource.TariffRequestHeader %></div>
    <div class="request-form">
        <% var userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID); %>

        <p class="confirm-block-text gray-text">
            <%= Resource.FirstName %><span class="required-mark">*</span>
        </p>
        <input type="text" maxlength="64" tabindex="1" class="text-edit-fname text-edit" required="required" placeholder="<%= Resource.FirstName %>" value="<%= userInfo.FirstName %>">

        <p class="confirm-block-text gray-text">
            <%= Resource.LastName %><span class="required-mark">*</span>
        </p>
        <input type="text" maxlength="64" tabindex="2" class="text-edit-lname text-edit" required="required" placeholder="<%= Resource.LastName %>" value="<%= userInfo.LastName %>">

        <p class="confirm-block-text gray-text">
            <%= Resource.Email %><span class="required-mark">*</span>
        </p>
        <input type="email" maxlength="64" tabindex="4" class="text-edit-email text-edit" required="required" placeholder="<%= Resource.Email %>" value="<%= userInfo.Email %>">

        <p class="confirm-block-text gray-text">
            <%= Resource.TitlePhone %><span class="required-mark">*</span>
        </p>
        <input type="text" maxlength="64" tabindex="5" class="text-edit-phone text-edit" required="required" title="<%= Resource.TitlePhone %>"
             pattern="\+?\d{4,63}" placeholder="<%= Resource.TitlePhone %>" value="<%= userInfo.MobilePhone %>" data-country="<%= PhoneCountry %>">

        <p class="confirm-block-text gray-text">
            <%= UserControlsCommonResource.CompanySizeTitle %><span class="required-mark">*</span>
        </p>
        <input type="text" maxlength="64" tabindex="6" class="text-edit-csize text-edit" required="required">
        <input type="hidden" maxlength="64" tabindex="6" class="text-edit-message text-edit" required="required">

        <p class="confirm-block-text gray-text">
            <%= UserControlsCommonResource.CompanyTitle %><span class="required-mark">*</span>
        </p>
        <input type="text" maxlength="64" tabindex="6" class="text-edit-ctitle text-edit" required="required" placeholder="<%= UserControlsCommonResource.CompanyTitle %>">
    </div>

    <div class="middle-button-container">
        <a class="tariff-request button blue huge" tabindex="10">
            <%= UserControlsCommonResource.TariffRequestBtn %></a>
    </div>
</div>
