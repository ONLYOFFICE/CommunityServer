/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Geolocation;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using log4net;
using PhoneNumbers;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TariffUsageController")]
    public partial class TariffUsage : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TariffSettings/TariffUsage.ascx"; }
        }

        protected bool HideBuyRecommendation;
        protected Dictionary<string, bool> ListStarRemark = new Dictionary<string, bool>();

        protected int UsersCount;
        protected long UsedSize;

        protected Tariff CurrentTariff;
        protected TenantQuota CurrentQuota;

        protected bool MonthIsDisable;
        protected bool YearIsDisable;
        protected int MinActiveUser;

        protected RegionInfo RegionDefault = new RegionInfo("US");
        protected RegionInfo CurrentRegion;

        protected List<RegionInfo> Regions = new List<RegionInfo>();

        protected bool InRuble
        {
            get { return "RU".Equals(CurrentRegion.Name); }
        }

        protected bool InEuro
        {
            get { return "EUR".Equals(CurrentRegion.ISOCurrencySymbol); }
        }

        protected decimal[] PricesPerUser
        {
            get
            {
                return InEuro
                           ? new[] { 4.50m, 1.80m, 0.90m }
                           : new[] { 5.0m, 2.0m, 1.0m };
            }
        }

        protected decimal[] FakePrices
        {
            get
            {
                return InEuro
                           ? new[] { 9.0m, 18.0m }
                           : new[] { 10.0m, 20.0m };
            }
        }

        private string _currencyFormat = "{currency}{price}";
        private IDictionary<string, IEnumerable<Tuple<string, decimal>>> _priceInfo;

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
                    || !CurrentQuota.Visible)
                {
                    var rightQuotaId = TenantExtra.GetRightQuotaId();
                    quota = _quotaList.FirstOrDefault(q => q.Id == rightQuotaId);
                }
                _quotaForDisplay = quota ?? CurrentQuota;
                return _quotaForDisplay;
            }
        }

        protected bool PeopleModuleAvailable
        {
            get
            {
                var peopleProduct = WebItemManager.Instance[WebItemManager.PeopleProductID];
                return peopleProduct != null && !peopleProduct.IsDisabled();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page
                .RegisterBodyScripts("~/usercontrols/management/tariffsettings/js/tariffusage.js",
                    "~/js/asc/plugins/countries.js",
                    "~/js/asc/plugins/phonecontroller.js")
                .RegisterStyle(
                    "~/skins/default/phonecontroller.css",
                    "~/usercontrols/management/tariffsettings/css/tariff.less",
                    "~/usercontrols/management/tariffsettings/css/tariffusage.less")
                .RegisterClientScript(new CountriesResources());

            CurrentRegion = RegionDefault;
            Regions.Add(CurrentRegion);

            UsersCount = TenantStatisticsProvider.GetUsersCount();
            UsedSize = TenantStatisticsProvider.GetUsedSize();
            CurrentTariff = TenantExtra.GetCurrentTariff();
            CurrentQuota = TenantExtra.GetTenantQuota();

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
            MinActiveUser = minYearQuota != null ? minYearQuota.ActiveUsers : (QuotasYear.Count > 0 ? QuotasYear.Last().ActiveUsers : 0 + 1);

            HideBuyRecommendation = CurrentTariff.Autorenewal || TariffSettings.HideRecommendation;

            downgradeInfoContainer.Options.IsPopup = true;
            buyRecommendationContainer.Options.IsPopup = true;
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            RegisterScript();
            CurrencyCheck();
        }

        private void CurrencyCheck()
        {
            CurrentRegion = FindRegionInfo() ?? CurrentRegion;

            if (!CurrentRegion.Equals(RegionDefault))
            {
                Regions.Add(CurrentRegion);
            }

            var requestCur = Request["cur"];
            if (!string.IsNullOrEmpty(requestCur))
            {
                try
                {
                    CurrentRegion = new RegionInfo(requestCur);
                    if (!Regions.Contains(CurrentRegion))
                    {
                        Regions.Add(CurrentRegion);
                    }
                }
                catch
                {
                }
            }

            _priceInfo = CoreContext.TenantManager.GetProductPriceInfo(false);
            if (!_priceInfo.Values.Any(value => value.Any(item => item.Item1 == CurrentRegion.ISOCurrencySymbol)))
            {
                Regions.Remove(CurrentRegion);
                CurrentRegion = RegionDefault;
            }

            if (InRuble)
            {
                _currencyFormat = "{price}{currency}";

                SetStar(string.Format(Resource.TariffsCurrencyRu, SetupInfo.ExchangeRateRuble));
            }
        }

        private static RegionInfo FindRegionInfo()
        {
            RegionInfo ri = null;

            var ownerId = CoreContext.TenantManager.GetCurrentTenant().OwnerId;
            var owner = CoreContext.UserManager.GetUsers(ownerId);
            if (!string.IsNullOrEmpty(owner.MobilePhone))
            {
                try
                {
                    var phoneUtil = PhoneNumberUtil.GetInstance();
                    var number = phoneUtil.Parse("+" + owner.MobilePhone.TrimStart('+'), "en-US");
                    var regionCode = phoneUtil.GetRegionCodeForNumber(number);
                    if (!string.IsNullOrEmpty(regionCode))
                    {
                        ri = new RegionInfo(regionCode);
                    }
                }
                catch (Exception err)
                {
                    LogManager.GetLogger("ASC.Web.Tariff").WarnFormat("Can not find country by phone {0}: {1}", owner.MobilePhone, err);
                }
            }

            if (ri == null)
            {
                var geoinfo = new GeolocationHelper("teamlabsite").GetIPGeolocationFromHttpContext();
                if (!string.IsNullOrEmpty(geoinfo.Key))
                {
                    try
                    {
                        ri = new RegionInfo(geoinfo.Key);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
            }
            return ri;
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
                          + String.Format(UserControlsCommonResource.TariffPaidPlan,
                                          TenantExtra.GetPrevUsersCount(CurrentQuota),
                                          CurrentQuota.ActiveUsers)
                          + "</b> ";
                if (CurrentTariff.DueDate.Date != DateTime.MaxValue.Date)
                    str += string.Format(Resource.TariffExpiredDate, CurrentTariff.DueDate.Date.ToLongDateString());

                if (CurrentTariff.Autorenewal) return str;
                str += "<br />" + Resource.TariffCanProlong;
                return str;
            }

            return String.Format(UserControlsCommonResource.TariffOverduePlan.HtmlEncode(),
                                 TenantExtra.GetPrevUsersCount(CurrentQuota),
                                 CurrentQuota.ActiveUsers,
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
                var ownerId = CoreContext.TenantManager.GetCurrentTenant().OwnerId.ToString();
                var link = CoreContext.PaymentManager.GetShoppingUri(quota.Id, true, null, CurrentRegion.ISOCurrencySymbol, Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName, ownerId);
                if (link == null)
                {
                    LogManager.GetLogger("ASC.Web.Billing").Error(string.Format("GetShoppingUri return null for tenant {0} and quota {1}", TenantProvider.CurrentTenantID, quota.Id));
                }
                else
                {
                    uri = link.ToString();
                }
            }
            return uri;
        }

        protected string SetStar(string starType, bool withHighlight = false)
        {
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

        protected string GetPriceString(decimal price, bool rubleRate = true, string currencySymbol = null)
        {
            if (rubleRate && InRuble)
            {
                price = price*SetupInfo.ExchangeRateRuble;
            }

            var priceString = InEuro && Math.Truncate(price) != price
                                  ? price.ToString(CultureInfo.InvariantCulture)
                                  : ((int)price).ToString(CultureInfo.InvariantCulture);

            return _currencyFormat
                .Replace("{currency}", "<span class='tariff-price-cur'>" + (currencySymbol ?? CurrentRegion.CurrencySymbol) + "</span>")
                .Replace("{price}", priceString);
        }

        protected string GetPriceString(TenantQuota quota)
        {
            if (!string.IsNullOrEmpty(quota.AvangateId) && _priceInfo.ContainsKey(quota.AvangateId))
            {
                var prices = _priceInfo[quota.AvangateId];
                var price = prices.FirstOrDefault(p => p.Item1 == CurrentRegion.ISOCurrencySymbol);
                if (price != null)
                {
                    return GetPriceString(price.Item2, false);
                }
                return GetPriceString(quota.Price, false, RegionDefault.CurrencySymbol);
            }
            return GetPriceString(quota.Price);
        }

        protected decimal GetPrice(TenantQuota quota)
        {
            if (!string.IsNullOrEmpty(quota.AvangateId) && _priceInfo.ContainsKey(quota.AvangateId))
            {
                var prices = _priceInfo[quota.AvangateId];
                var price = prices.FirstOrDefault(p => p.Item1 == CurrentRegion.ISOCurrencySymbol);
                if (price != null)
                {
                    return price.Item2;
                }
                return quota.Price;
            }
            return quota.Price;
        }

        protected decimal GetSaleValue(decimal priceMonth, TenantQuota quota)
        {
            var price = GetPrice(quota);

            var period = quota.Year ? 12 : 36;
            return priceMonth*period - price;
        }

        protected string GetPerUserPrice(TenantQuota quota)
        {
            var price = PricesPerUser[quota == null ? 0 : quota.Year ? 1 : quota.Year3 ? 2 : 0];
            return GetPriceString(price, InRuble);
        }

        [AjaxMethod]
        public void SaveHideRecommendation(bool hide)
        {
            TariffSettings.HideRecommendation = hide;
        }

        [AjaxMethod]
        public void RequestTariff(string fname, string lname, string title, string email, string phone, string ctitle, string csize, string site, string message)
        {
            var key = HttpContext.Current.Request.UserHostAddress + "requesttariff";
            var count = Convert.ToInt32(HttpContext.Current.Cache[key]);
            if (2 < count)
            {
                throw new ArgumentOutOfRangeException("Messages count", "Rate limit exceeded.");
            }
            HttpContext.Current.Cache.Insert(key, ++count, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(2));

            StudioNotifyService.Instance.SendRequestTariff(false, fname, lname, title, email, phone, ctitle, csize, site, message);
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