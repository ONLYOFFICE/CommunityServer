/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Geolocation;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using PhoneNumbers;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TariffUsageController")]
    public partial class TariffCustom : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TariffSettings/TariffCustom.ascx"; }
        }

        protected int UsersCount;
        protected long UsedSize;

        protected Tariff CurrentTariff;
        protected TenantQuota CurrentQuota;

        protected bool MonthIsDisable;
        protected bool YearIsDisable;
        protected int MinActiveUser;

        protected RegionInfo RegionDefault = new RegionInfo("RU");
        protected RegionInfo CurrentRegion;
        protected string PhoneCountry = "RU";

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
                .RegisterBodyScripts("~/UserControls/Management/TariffSettings/js/tariffcustom.js",
                    "~/js/asc/plugins/countries.js",
                    "~/js/asc/plugins/phonecontroller.js")
                .RegisterStyle(
                    "~/skins/default/phonecontroller.css",
                    "~/UserControls/Management/TariffSettings/css/tariff.less",
                    "~/UserControls/Management/TariffSettings/css/tariffusage.less",
                    "~/UserControls/Management/TariffSettings/css/tariffcustom.less")
                .RegisterClientScript(new CountriesResources());

            CurrentRegion = RegionDefault;

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

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            CurrencyCheck();
        }

        private void CurrencyCheck()
        {
            var findRegion = FindRegionInfo();

            CurrentRegion = findRegion ?? CurrentRegion;
            PhoneCountry = (findRegion ?? new RegionInfo(Thread.CurrentThread.CurrentCulture.LCID)).TwoLetterISORegionName;

            var requestCur = Request["cur"];
            if (!string.IsNullOrEmpty(requestCur))
            {
                try
                {
                    CurrentRegion = new RegionInfo(requestCur);
                }
                catch
                {
                }
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
    }
}