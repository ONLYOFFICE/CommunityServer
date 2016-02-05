/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using AjaxPro;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.Voip;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using Constants = ASC.Core.Users.Constants;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TariffUsageController")]
    public partial class TariffUsage : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TariffSettings/TariffUsage.ascx"; }
        }

        protected Partner Partner;

        protected bool HideBuyRecommendation;
        protected Dictionary<string, bool> ListStarRemark = new Dictionary<string, bool>();

        protected int UsersCount;
        protected long UsedSize;
        protected bool SmsEnable;
        protected bool VoipEnable;

        protected Tariff CurrentTariff;
        protected TenantQuota CurrentQuota;

        protected bool MonthIsDisable;
        protected bool YearIsDisable;
        protected int MinActiveUser;

        private RegionInfo _region = new RegionInfo("US");
        protected decimal RateRuble;

        private string CurrencySymbol
        {
            get { return _region.CurrencySymbol; }
        }

        private string _currencyFormat = "{currency}{price}";

        private IEnumerable<TenantQuota> _quotaList;
        protected List<TenantQuota> QuotasYear;

        private TenantQuota _quotaForDisplay;

        protected TenantQuota QuotaForDisplay
        {
            get
            {
                if (_quotaForDisplay != null) return _quotaForDisplay;
                TenantQuota quota = null;
                if (CurrentQuota.Trial
                    || CurrentQuota.Free
                    || !CurrentQuota.Visible
                    || CoreContext.Configuration.Standalone && CurrentTariff.QuotaId.Equals(Tenant.DEFAULT_TENANT))
                {
                    var rightQuotaId = TenantExtra.GetRightQuotaId();
                    quota = _quotaList.FirstOrDefault(q => q.Id == rightQuotaId);
                }
                _quotaForDisplay = quota ?? CurrentQuota;
                return _quotaForDisplay;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/usercontrols/management/tariffsettings/js/tariffusage.js");
            Page.RegisterStyle("~/usercontrols/management/tariffsettings/css/tariff.less");
            Page.RegisterStyle("~/usercontrols/management/tariffsettings/css/tariffusage.less");

            UsersCount = TenantStatisticsProvider.GetUsersCount();
            UsedSize = TenantStatisticsProvider.GetUsedSize();
            CurrentTariff = TenantExtra.GetCurrentTariff();
            CurrentQuota = TenantExtra.GetTenantQuota();

            var partner = CoreContext.PaymentManager.GetApprovedPartner();
            if (partner != null)
            {
                Partner = partner;

                _quotaList = CoreContext.PaymentManager.GetPartnerTariffs(Partner.Id);

                if (!string.IsNullOrEmpty(Partner.Currency))
                {
                    _region = new RegionInfo(Partner.Currency);
                }

                var control = (TariffPartner)LoadControl(TariffPartner.Location);
                control.CurPartner = Partner;
                control.TariffNotPaid = CurrentTariff.State >= TariffState.NotPaid;
                control.TariffProlongable = CurrentTariff.Prolongable;
                PaymentsCodeHolder.Controls.Add(control);
            }

            if (_quotaList == null || !_quotaList.Any())
            {
                _quotaList = TenantExtra.GetTenantQuotas();
            }
            else if (!CurrentQuota.Trial)
            {
                CurrentQuota = _quotaList.FirstOrDefault(q => q.Id == CurrentQuota.Id) ?? CurrentQuota;
            }
            _quotaList = _quotaList.OrderBy(r => r.ActiveUsers).ToList().Where(r => !r.Trial);
            QuotasYear = _quotaList.Where(r => r.Year).ToList();

            MonthIsDisable = !CurrentQuota.Free && (CurrentQuota.Year || CurrentQuota.Year3) && CurrentTariff.State == TariffState.Paid;
            YearIsDisable = !CurrentQuota.Free && CurrentQuota.Year3 && CurrentTariff.State == TariffState.Paid;

            var minYearQuota = QuotasYear.FirstOrDefault(q => q.ActiveUsers >= UsersCount && q.MaxTotalSize >= UsedSize);
            MinActiveUser = minYearQuota != null ? minYearQuota.ActiveUsers : (QuotasYear.Last().ActiveUsers + 1);

            HideBuyRecommendation = CurrentTariff.Autorenewal || TariffSettings.HideRecommendation || Partner != null;

            downgradeInfoContainer.Options.IsPopup = true;
            buyRecommendationContainer.Options.IsPopup = true;
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            if (StudioSmsNotificationSettings.IsVisibleSettings
                && (SettingsManager.Instance.LoadSettings<StudioSmsNotificationSettings>(TenantProvider.CurrentTenantID).EnableSetting
                    || CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID))
                && Partner == null)
            {
                SmsEnable = true;
                var smsBuy = (SmsBuy)LoadControl(SmsBuy.Location);
                smsBuy.ShowLink = !SettingsManager.Instance.LoadSettings<StudioSmsNotificationSettings>(TenantProvider.CurrentTenantID).EnableSetting;
                SmsBuyHolder.Controls.Add(smsBuy);
            }

            if (VoipPaymentSettings.IsVisibleSettings
                && CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID)
                && Partner == null)
            {
                VoipEnable = true;
                var voipBuy = (VoipBuy)LoadControl(VoipBuy.Location);
                VoipBuyHolder.Controls.Add(voipBuy);
            }

            if (Partner == null)
            {
                RegisterScript();
            }

            if (Partner == null && Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "ru")
            {
                _region = new RegionInfo("RU");
                _currencyFormat = "{price}{currency}";
                RateRuble = SetupInfo.ExchangeRateRuble;

                SetStar(string.Format(Resource.TariffsCurrencyRu, RateRuble));
            }
        }

        protected string TariffDescription()
        {
            if (CurrentQuota.Trial)
            {
                if (CurrentTariff.State == TariffState.Trial)
                {
                    return "<b>" + Resource.TariffTrial + "</b> "
                           + (CurrentTariff.DueDate.Date != DateTime.MaxValue.Date
                                  ? string.Format(Resource.TariffExpiredDate, CurrentTariff.DueDate.Date.ToLongDateString())
                                  : "")
                           + "<br />" + Resource.TariffChooseLabel;
                }
                return String.Format(Resource.TariffTrialOverdue.HtmlEncode(),
                                     "<span class='tarifff-marked'>",
                                     "</span>",
                                     "<br />", string.Empty, string.Empty);
            }

            if (CurrentQuota.Free)
            {
                return "<b>" + Resource.TariffFree + "</b><br />" + Resource.TariffChooseLabel;
            }

            if (CurrentTariff.State == TariffState.Paid
                && CurrentTariff.DueDate.Date >= DateTime.Today)
            {
                if (CurrentQuota.NonProfit)
                {
                    return "<b>" + UserControlsCommonResource.TariffNonProfit + "</b>";
                }

                var str = "<b>"
                          + String.Format(UserControlsCommonResource.TariffPaid,
                                          GetPriceString(CurrentQuota.Price),
                                          CurrentQuota.Year3 ? UserControlsCommonResource.TariffPerYear3 : CurrentQuota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth,
                                          SetStar(CurrentQuota.Visible ? Resource.TariffRemarkPrice : Resource.TariffPeriodExpired))
                          + "</b> ";
                if (CurrentTariff.DueDate.Date != DateTime.MaxValue.Date)
                    str += string.Format(Resource.TariffExpiredDate, CurrentTariff.DueDate.Date.ToLongDateString());

                if (CurrentTariff.Autorenewal) return str;
                str += "<br />" + Resource.TariffCanProlong;
                return str;
            }

            return String.Format(UserControlsCommonResource.TariffOverdue.HtmlEncode(),
                                 GetPriceString(CurrentQuota.Price),
                                 CurrentQuota.Year3 ? UserControlsCommonResource.TariffPerYear3 : CurrentQuota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth,
                                 SetStar(CurrentQuota.Visible ? Resource.TariffRemarkPrice : Resource.TariffPeriodExpired),
                                 "<span class='tariff-marked'>",
                                 "</span>",
                                 "<br />");
        }

        protected TenantQuota GetQuotaMonth(TenantQuota quota)
        {
            return _quotaList.FirstOrDefault(r =>
                                             r.ActiveUsers == quota.ActiveUsers
                                             && (!r.Year || quota.Free)
                                             && !r.Year3);
        }

        protected TenantQuota GetQuotaYear3(TenantQuota quota)
        {
            return _quotaList.FirstOrDefault(r =>
                                             r.ActiveUsers == quota.ActiveUsers
                                             && r.Year3);
        }

        protected Tuple<string, string, string> GetBuyAttr(TenantQuota quota)
        {
            var cssList = new List<string>();
            var getHref = true;
            var text = Resource.TariffButtonBuy;

            if (quota != null)
            {
                cssList.Add(CurrentTariff.State >= TariffState.NotPaid ? "red" : "blue");

                if (quota.ActiveUsers < UsersCount || quota.MaxTotalSize < UsedSize)
                {
                    cssList.Add("disable");
                    getHref = false;
                }
                else if (Equals(quota.Id, CurrentQuota.Id))
                {
                    text = Resource.TariffButtonExtend;
                    if (!CurrentTariff.Prolongable)
                    {
                        cssList.Add("disable");
                        getHref = false;
                    }
                    else if (CurrentTariff.Autorenewal)
                    {
                        cssList.Add("disable");
                        getHref = false;
                        text = Resource.TariffEnabledAutorenew + SetStar(Resource.TariffRemarkProlongEnable);
                    }
                }
                else if (CurrentTariff.Prolongable)
                {
                    text = Resource.TariffButtonBuy + SetStar(Resource.TariffRemarkProlongDisable);
                }

                if (!quota.Year3)
                {
                    if (quota.Year && YearIsDisable || !quota.Year && MonthIsDisable)
                    {
                        cssList.Add("disable");
                        getHref = false;
                        text = Resource.TariffButtonBuy;
                    }
                }
            }
            else
            {
                cssList.Add("disable");
                getHref = false;
            }

            var href = getHref ? GetShoppingUri(quota) : string.Empty;
            var cssClass = string.Join(" ", cssList.Distinct());
            var result = new Tuple<string, string, string>(cssClass, href, text);
            return result;
        }

        private string GetShoppingUri(TenantQuota quota)
        {
            var uri = string.Empty;
            if (quota != null)
            {
                if (Partner == null)
                {
                    var link = CoreContext.PaymentManager.GetShoppingUri(quota.Id);
                    if (link == null)
                    {
                        LogManager.GetLogger("ASC.Web.Billing").Error(string.Format("GetShoppingUri return null for tenant {0} and quota {1}", TenantProvider.CurrentTenantID, quota == null ? 0 : quota.Id));
                    }
                    else
                    {
                        uri = link.ToString();
                    }
                }
                else if (Partner.PaymentMethod == PartnerPaymentMethod.External)
                {
                    uri = (Partner.PaymentUrl ?? "")
                        .ToLower()
                        .Replace("{partnerid}", Partner.Id)
                        .Replace("{tariffid}", quota.ActiveUsers + (quota.Year ? "year" : "month"))
                        .Replace("{portal}", CoreContext.TenantManager.GetCurrentTenant().TenantAlias)
                        .Replace("{currency}", _region.ISOCurrencySymbol)
                        .Replace("{price}", ((int)quota.Price).ToString());
                }
            }
            return uri;
        }

        protected string SetStar(string starType, bool withHighlight = false)
        {
            if (Partner != null) return string.Empty;
            if (!ListStarRemark.Keys.Contains(starType))
            {
                ListStarRemark.Add(starType, withHighlight);
            }

            return GetStar(starType);
        }

        protected string GetStar(string starType)
        {
            if (!ListStarRemark.Keys.Contains(starType))
            {
                return null;
            }

            var result = string.Empty;
            for (var i = 0; i < ListStarRemark.Keys.ToList().IndexOf(starType) + 1; i++)
            {
                result += "*";
            }

            return result;
        }

        protected string GetRemarks()
        {
            var result = string.Empty;

            foreach (var starType in ListStarRemark)
            {
                if (!string.IsNullOrEmpty(result))
                    result += "<br />";

                if (starType.Value)
                    result += "<span class=\"tariff-remark-highlight\">";
                result += GetStar(starType.Key) + " ";
                result += starType.Key;
                if (starType.Value)
                    result += "</span>";
            }

            return result;
        }

        protected string GetChatBannerPath()
        {
            var lng = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToLower();
            var cult = string.Empty;
            var cultArray = new[] {"de", "es", "fr", "it", "lv", "ru"};
            if (cultArray.Contains(lng))
            {
                cult = "_" + lng;
            }
            var imgName = "support/live_support_banner" + cult + ".png";

            return WebImageSupplier.GetAbsoluteWebPath(imgName);
        }

        protected string GetSaleDate()
        {
            DateTime date;
            if (!DateTime.TryParse(WebConfigurationManager.AppSettings["web.payment-sale"], out date))
            {
                date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddDays(-1);
            }

            return date.ToString(Thread.CurrentThread.CurrentCulture.DateTimeFormat.MonthDayPattern);
        }

        protected string GetPriceString(decimal price)
        {
            if ((int) RateRuble != 0)
            {
                price = price*RateRuble;
            }
            return _currencyFormat
                .Replace("{currency}", "<span class='tariff-price-cur'>" + CurrencySymbol + "</span>")
                .Replace("{price}", ((int) price).ToString());
        }

        [AjaxMethod]
        public void SaveHideRecommendation(bool hide)
        {
            TariffSettings.HideRecommendation = hide;
        }

        [AjaxMethod]
        public void RequestTariff(string name, string email, string message)
        {
            var key = HttpContext.Current.Request.UserHostAddress + "requesttariff";
            var count = Convert.ToInt32(HttpContext.Current.Cache[key]);
            if (2 < count)
            {
                throw new ArgumentOutOfRangeException("Messages count", "Rate limit exceeded.");
            }
            HttpContext.Current.Cache.Insert(key, ++count, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(2));

            StudioNotifyService.Instance.SendRequestTariff(name, email, message);
        }

        private void RegisterScript()
        {
            Page.RegisterInlineScript(@"
                var __lc = {};
                __lc.license = 2673891;

                (function () {
                    var lc = document.createElement('script'); lc.type = 'text/javascript'; lc.async = true;
                    lc.src = ('https:' == document.location.protocol ? 'https://' : 'http://') + 'cdn.livechatinc.com/tracking.js';
                    var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(lc, s);

                    (function loopForCorrectLoading(i) {
                        setTimeout(function () {
                            if (--i && typeof(window.LC_API) === 'undefined') {
                                loopForCorrectLoading(i);
                            }
                            if (typeof(window.LC_API) === 'object') {
                                window.LC_API.on_after_load = function () {
                                    if (window.LC_API.agents_are_available()) {
                                        jq('.support-chat-btn').show();
                                    }
                                };
                            }
                        }, 100);
                    })(500);
                })();", onReady: false);
        }
    }
}