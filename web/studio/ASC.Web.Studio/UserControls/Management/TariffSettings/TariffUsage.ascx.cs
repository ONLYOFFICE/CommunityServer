/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Web;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Web.Configuration;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.Voip;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Resources;
using log4net;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TariffUsageController")]
    public partial class TariffUsage : UserControl
    {
        [Serializable]
        [DataContract]
        internal class TariffSettings : ISettings
        {
            [DataMember(Name = "HideRecommendation")]
            public bool HideBuyRecommendation { get; set; }

            public ISettings GetDefault()
            {
                return new TariffSettings {HideBuyRecommendation = false};
            }

            public Guid ID
            {
                get { return new Guid("{07956D46-86F7-433b-A657-226768EF9B0D}"); }
            }

            public static bool GetTariffSettings()
            {
                return SettingsManager.Instance.LoadSettingsFor<TariffSettings>(SecurityContext.CurrentAccount.ID).HideBuyRecommendation;
            }

            public static void SaveHideBuyRecommendation(bool hide)
            {
                var tariffSettings = new TariffSettings {HideBuyRecommendation = hide};
                SettingsManager.Instance.SaveSettingsFor(tariffSettings, SecurityContext.CurrentAccount.ID);
            }
        }

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
        protected RegionInfo Region = new RegionInfo("US");

        protected string CurrencySymbol
        {
            get { return Region.CurrencySymbol; }
        }

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
                    || CoreContext.Configuration.Standalone && CurrentTariff.QuotaId.Equals(Tenant.DEFAULT_TENANT))
                {
                    quota = _quotaList.FirstOrDefault(q => q.Id == TenantExtra.GetRightQuotaId());
                }
                _quotaForDisplay = quota ?? CurrentQuota;
                return _quotaForDisplay;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
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
                    Region = new RegionInfo(Partner.Currency);
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

            HideBuyRecommendation = CurrentTariff.Autorenewal || TariffSettings.GetTariffSettings() || Partner != null;

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

            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "ru")
            {
                var cur = SetupInfo.ExchangeRateRuble;
                SetStar(string.Format(Resource.TariffsCurrencyRu, cur));
            }
        }

        protected string TariffDescription()
        {
            if (CoreContext.Configuration.Standalone && CurrentTariff.QuotaId.Equals(Tenant.DEFAULT_TENANT))
            {
                return string.Format(UserControlsCommonResource.TariffServerCE, "<b>", "</b>")
                       + "</b><br/><br>"
                       + string.Format(UserControlsCommonResource.TariffLicenseOverReason,
                                       "<br/>",
                                       "<a href=\"http://www.onlyoffice.com/online-editor.aspx\" class=\"link underline medium\" >",
                                       "</a>",
                                       SetStar(UserControlsCommonResource.TariffServerCEstar));
            }

            if (CurrentQuota.Trial)
            {
                if (CurrentTariff.State == TariffState.Trial)
                {
                    return "<b>" + Resource.TariffTrial + "</b> "
                           + (CurrentTariff.DueDate.Date != DateTime.MaxValue.Date
                                  ? string.Format(Resource.TariffExpiredDate, CurrentTariff.DueDate.ToLongDateString())
                                  : "")
                           + "<br />" + Resource.TariffChooseLabel;
                }
                return String.Format(Resource.TariffTrialOverdue,
                                     "<span>",
                                     "</span>",
                                     "<br />", string.Empty, string.Empty);
            }

            if (CurrentQuota.Free)
            {
                return "<b>" + Resource.TariffFree + "</b><br />" + Resource.TariffChooseLabel;
            }

            if (CurrentTariff.State == TariffState.Paid)
            {
                if (CurrentQuota.NonProfit)
                {
                    return "<b>" + UserControlsCommonResource.TariffNonProfit + "</b>";
                }

                var str = "<b>"
                          + String.Format(UserControlsCommonResource.TariffPaid,
                                          (int)CurrentQuota.Price + CurrencySymbol,
                                          CurrentQuota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth,
                                          SetStar(CurrentQuota.Visible ? Resource.TariffRemarkPrice : Resource.TariffPeriodExpired))
                          + "</b> ";
                if (CurrentTariff.DueDate.Date != DateTime.MaxValue.Date)
                    str += string.Format(Resource.TariffExpiredDate, CurrentTariff.DueDate.ToLongDateString());

                if (CurrentTariff.Autorenewal) return str;
                str += "<br />" + Resource.TariffCanProlong;
                return str;
            }

            return String.Format(UserControlsCommonResource.TariffOverdue,
                                 (int)CurrentQuota.Price + CurrencySymbol,
                                 CurrentQuota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth,
                                 SetStar(CurrentQuota.Visible ? Resource.TariffRemarkPrice : Resource.TariffPeriodExpired),
                                 "<span>",
                                 "</span>",
                                 "<br />");
        }

        protected TenantQuota GetQuotaMonth(TenantQuota quota)
        {
            return _quotaList.FirstOrDefault(r =>
                                             r.ActiveUsers == quota.ActiveUsers
                                             && (!r.Year || quota.Free));
        }

        protected string GetTypeLink(TenantQuota quota)
        {
            return quota.ActiveUsers >= UsersCount
                   && quota.MaxTotalSize >= UsedSize
                       ? !quota.Free
                             ? (CurrentQuota.Trial
                                || CurrentQuota.Free
                                || CoreContext.Configuration.Standalone && CurrentTariff.QuotaId.Equals(Tenant.DEFAULT_TENANT)
                                || Equals(quota.Id, CurrentQuota.Id))
                                   ? "pay"
                                   : "change"
                             : CurrentTariff.State == TariffState.NotPaid
                                   ? "free"
                                   : "stopfree"
                       : "limit";
        }

        protected string GetShoppingUri(TenantQuota quota)
        {
            var uri = string.Empty;
            if (quota != null
                && quota.ActiveUsers >= TenantStatisticsProvider.GetUsersCount()
                && quota.MaxTotalSize >= TenantStatisticsProvider.GetUsedSize())
            {
                if (Partner == null)
                {
                    var link = CoreContext.PaymentManager.GetShoppingUri(TenantProvider.CurrentTenantID, quota.Id);
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
                        .Replace("{currency}", Region.ISOCurrencySymbol)
                        .Replace("{price}", ((int)quota.Price).ToString());
                }
            }
            return uri;
        }

        protected string WithFullRemarks()
        {
            if (CurrentTariff.Autorenewal)
                return SetStar(Resource.TariffRemarkProlongEnable);
            if (CurrentTariff.Prolongable)
                return SetStar(Resource.TariffRemarkProlongDisable);
            return "";
        }

        protected string SetStar(string starType)
        {
            return SetStar(starType, false);
        }

        protected string SetStar(string starType, bool withHighlight)
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

        protected bool MonthIsDisable()
        {
            return !CurrentQuota.Free && CurrentQuota.Year && CurrentTariff.State == TariffState.Paid;
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

        protected DateTime GetSaleDate()
        {
            DateTime date;
            if (!DateTime.TryParse(WebConfigurationManager.AppSettings["web.payment-sale"], out date))
            {
                date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1);
            }
            return date;
        }

        [AjaxMethod]
        public void SaveHideRecommendation(bool hide)
        {
            TariffSettings.SaveHideBuyRecommendation(hide);
        }

        [AjaxMethod]
        public void GetFree()
        {
            var quota = TenantExtra.GetTenantQuota();
            var usersCount = TenantStatisticsProvider.GetUsersCount();
            var usedSize = TenantStatisticsProvider.GetUsedSize();
            if (!quota.Free
                && quota.ActiveUsers >= usersCount
                && quota.MaxTotalSize >= usedSize)
                TenantExtra.FreeRequest();
        }

        //[AjaxMethod]
        //public void GetTrial()
        //{
        //    if (CoreContext.Configuration.Standalone && string.IsNullOrEmpty(StudioKeySettings.GetSKey()))
        //        TenantExtra.TrialRequest();
        //}

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