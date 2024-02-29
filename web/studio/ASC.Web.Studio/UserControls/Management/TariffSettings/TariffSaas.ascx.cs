/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Contracts;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.FederatedLogin;
using ASC.Geolocation;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Management.SingleSignOnSettings;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;

using PhoneNumbers;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TariffSaasController")]
    public partial class TariffSaas : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TariffSettings/TariffSaas.ascx"; }
        }

        protected readonly int MinUsersCount = TariffService.ACTIVE_USERS_MIN;
        protected readonly int MaxUsersCount = TariffService.ACTIVE_USERS_MAX;
        protected readonly string SalesEmail = AdditionalWhiteLabelSettings.Instance.SalesEmail;
        protected readonly RegionInfo RegionDefault = new RegionInfo("US");

        protected readonly int CurrentUsersCount = TenantStatisticsProvider.GetUsersCount();
        protected readonly long CurrentUsedSize = TenantStatisticsProvider.GetUsedSize();
        protected readonly Tariff CurrentTariff = TenantExtra.GetCurrentTariff();
        protected readonly TenantQuota CurrentQuota = TenantExtra.GetTenantQuota();
        protected readonly Guid CurrentOwnerId = CoreContext.TenantManager.GetCurrentTenant().OwnerId;

        protected TenantQuota StartupQuota;
        protected TenantQuota MonthQuota;
        protected TenantQuota YearQuota;
        protected TenantQuota ThreeYearsQuota;

        protected bool IsStartup;
        protected bool IsTrial;
        protected bool IsBusiness;
        protected bool IsExpired;

        protected bool StartupEnabled;

        protected RegionInfo CurrentRegion;
        protected List<RegionInfo> Regions = new List<RegionInfo>();

        private IDictionary<string, Dictionary<string, decimal>> priceInfo;
        private IEnumerable<TenantQuota> quotaList;
        private readonly ILog Log = LogManager.GetLogger("ASC.Web.Billing");

        protected bool IsAdmin;
        protected bool IsPeopleModuleAvailable;
        protected int ActiveUsersPercent;
        protected int StorageSpacePercent;

        protected void Page_Load(object sender, EventArgs e)
        {
            Page
                .RegisterBodyScripts(
                    "~/UserControls/Management/TariffSettings/js/tariffsaas.js"
                );
            if (ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle(
                    "~/UserControls/Management/TariffSettings/css/dark-tariff.less",
                    "~/UserControls/Management/TariffSettings/css/dark-tariffsaas.less"
                );
            }
            else
            {
                Page.RegisterStyle(
                    "~/UserControls/Management/TariffSettings/css/tariff.less",
                    "~/UserControls/Management/TariffSettings/css/tariffsaas.less"
                );
            }
            priceInfo = CoreContext.TenantManager.GetProductPriceInfo(false);
            quotaList = TenantExtra.GetTenantQuotas();

            InitRegionInfo();

            StartupQuota = GetStartupQuota();
            MonthQuota = GetMonthQuota();
            YearQuota = GetYearQuota();
            ThreeYearsQuota = GetThreeYearsQuota();

            IsStartup = CurrentQuota.Free;
            IsTrial = CurrentQuota.Trial;
            IsBusiness = !CurrentQuota.Free;
            IsExpired = CurrentTariff.DueDate.Date != DateTime.MaxValue.Date && CurrentTariff.State >= TariffState.NotPaid;

            StartupEnabled = CheckStartupEnabled(CurrentQuota, StartupQuota, out string _);

            IsAdmin = CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID);

            var peopleProduct = WebItemManager.Instance[WebItemManager.PeopleProductID];
            IsPeopleModuleAvailable = peopleProduct != null && !peopleProduct.IsDisabled();

            ActiveUsersPercent = 100 * CurrentUsersCount / CurrentQuota.ActiveUsers;
            StorageSpacePercent = (int)(100 * CurrentUsedSize / CurrentQuota.MaxTotalSize);

            AjaxPro.Utility.RegisterTypeForAjax(GetType());
        }

        private void InitRegionInfo()
        {
            Regions.Add(RegionDefault);

            CurrentRegion = GetRegionInfoByRequestCurrency() ?? FindRegionInfo() ?? RegionDefault;

            if (!CurrentRegion.Equals(RegionDefault))
            {
                if (priceInfo.Values.Any(value => value.ContainsKey(CurrentRegion.ISOCurrencySymbol)))
                {
                    Regions.Add(CurrentRegion);
                }
                else
                {
                    CurrentRegion = RegionDefault;
                }
            }
        }

        private RegionInfo GetRegionInfoByRequestCurrency()
        {
            RegionInfo regionInfo = null;

            var requestCurrency = Request["cur"];

            if (!string.IsNullOrEmpty(requestCurrency))
            {
                try
                {
                    regionInfo = new RegionInfo(requestCurrency);
                }
                catch
                {
                }
            }

            return regionInfo;
        }

        private RegionInfo FindRegionInfo()
        {
            RegionInfo regionInfo = null;

            var owner = CoreContext.UserManager.GetUsers(CurrentOwnerId);

            if (!string.IsNullOrEmpty(owner.MobilePhone))
            {
                try
                {
                    var phoneUtil = PhoneNumberUtil.GetInstance();
                    var number = phoneUtil.Parse("+" + owner.MobilePhone.TrimStart('+'), "en-US");
                    var regionCode = phoneUtil.GetRegionCodeForNumber(number);
                    if (!string.IsNullOrEmpty(regionCode))
                    {
                        try
                        {
                            regionInfo = new RegionInfo(regionCode);
                        }
                        catch
                        {
                        }
                    }
                }
                catch (Exception err)
                {
                    LogManager.GetLogger("ASC.Web.Tariff").WarnFormat("Can not find country by phone {0}: {1}", owner.MobilePhone, err);
                }
            }

            if (regionInfo == null)
            {
                var geoinfo = new GeolocationHelper("teamlabsite").GetIPGeolocationFromHttpContext();
                if (!string.IsNullOrEmpty(geoinfo.Key))
                {
                    try
                    {
                        regionInfo = new RegionInfo(geoinfo.Key);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return regionInfo;
        }

        private bool CheckStartupEnabled(TenantQuota currentQuota, TenantQuota startupQuota, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!currentQuota.Trial)
            {
                errorMessage = UserControlsCommonResource.SaasTariffErrorTrial;
                return false;
            }

            if (TenantStatisticsProvider.GetUsersCount() > startupQuota.ActiveUsers)
            {
                errorMessage = string.Format(UserControlsCommonResource.SaasTariffErrorUsers, startupQuota.ActiveUsers);
                return false;
            }

            if (TenantStatisticsProvider.GetVisitorsCount() > 0)
            {
                errorMessage = string.Format(UserControlsCommonResource.SaasTariffErrorGuests, 0);
                return false;
            }

            var currentTenant = CoreContext.TenantManager.GetCurrentTenant();

            var admins = WebItemSecurity.GetProductAdministrators(Guid.Empty);

            if (admins.Any(admin => admin.ID != currentTenant.OwnerId))
            {
                errorMessage = string.Format(UserControlsCommonResource.SaasTariffErrorAdmins, 1);
                return false;
            }

            if (TenantStatisticsProvider.GetUsedSize() > startupQuota.MaxTotalSize)
            {
                errorMessage = string.Format(UserControlsCommonResource.SaasTariffErrorStorage, FileSizeComment.FilesSizeToString(startupQuota.MaxTotalSize));
                return false;
            }

            var authServiceList = new AuthorizationKeys().AuthServiceList.Where(x => x.CanSet);

            foreach (var service in authServiceList)
            {
                if (service.Props.Any(r => !string.IsNullOrEmpty(r.Value)))
                {
                    errorMessage = UserControlsCommonResource.SaasTariffErrorThirparty;
                    return false;
                }
            }

            if (!TenantWhiteLabelSettings.Load().IsDefault)
            {
                errorMessage = UserControlsCommonResource.SaasTariffErrorWhiteLabel;
                return false;
            }

            if (!string.IsNullOrEmpty(currentTenant.MappedDomain))
            {
                errorMessage = UserControlsCommonResource.SaasTariffErrorDomain;
                return false;
            }

            var accountLinker = new AccountLinker("webstudio");

            foreach (var user in CoreContext.UserManager.GetUsers(EmployeeStatus.All))
            {
                var linkedAccounts = accountLinker.GetLinkedProfiles(user.ID.ToString());

                if (linkedAccounts.Any())
                {
                    errorMessage = UserControlsCommonResource.SaasTariffErrorOauth;
                    return false;
                }
            }

            if (SsoSettingsV2.Load().EnableSso)
            {
                errorMessage = UserControlsCommonResource.SaasTariffErrorSso;
                return false;
            }

            if (ActiveDirectory.Base.Settings.LdapSettings.Load().EnableLdapAuthentication)
            {
                errorMessage = UserControlsCommonResource.SaasTariffErrorLdap;
                return false;
            }

            using (var service = new BackupServiceClient())
            {
                var scheduleResponse = service.GetSchedule(currentTenant.TenantId);

                if (scheduleResponse != null)
                {
                    errorMessage = UserControlsCommonResource.SaasTariffErrorAutoBackup;
                    return false;
                }
            }

            using (var db = new DbManager("default"))
            {
                var hasMailServerDomain = db.ExecuteScalar<bool>(@"select exists(select 1 from mail_server_domain where tenant = @tid)",
                    new { tid = currentTenant.TenantId });

                if (hasMailServerDomain)
                {
                    errorMessage = UserControlsCommonResource.SaasTariffErrorMailServerDomain;
                    return false;
                }
            }

            return true;
        }

        private TenantQuota GetStartupQuota()
        {
            var startupQuota = CoreContext.TenantManager.GetTenantQuotas(true).FirstOrDefault(q => q.Free && !q.Open);

            if (startupQuota == null)
            {
                throw new NotSupportedException(UserControlsCommonResource.SaasTariffErrorStartupTariffNotFound);
            }

            return startupQuota;
        }

        private TenantQuota GetMonthQuota()
        {
            var monthQuota = quotaList.FirstOrDefault(q => !q.Year && !q.Year3 && !q.Trial && !q.Free && !q.Open);

            if (monthQuota == null)
            {
                throw new NotSupportedException(UserControlsCommonResource.SaasTariffErrorMonthTariffNotFound);
            }

            return monthQuota;
        }

        private TenantQuota GetYearQuota()
        {
            var yearQuota = quotaList.FirstOrDefault(q => q.Year && !q.Trial && !q.Free && !q.Open);

            //if (yearQuota == null)
            //{
            //    throw new NotSupportedException(UserControlsCommonResource.SaasTariffErrorYearTariffNotFound);
            //}

            return yearQuota;
        }

        private TenantQuota GetThreeYearsQuota()
        {
            var threeYearsQuota = quotaList.FirstOrDefault(q => q.Year3 && !q.Trial && !q.Free && !q.Open);

            //if (threeYearsQuota == null)
            //{
            //    throw new NotSupportedException(UserControlsCommonResource.SaasTariffErrorThreeYearsTariffNotFound);
            //}

            return threeYearsQuota;
        }

        protected Tuple<string, string, string> GetShoppingData(TenantQuota quota, string currency, int quantity)
        {
            var getLink = true;
            var buttonText = Resource.TariffButtonBuy;
            var infoText = string.Empty;

            var currentUsersCount = TenantStatisticsProvider.GetUsersCount();
            var currentVisitorsCount = TenantStatisticsProvider.GetVisitorsCount();
            var currentUsedSize = TenantStatisticsProvider.GetUsedSize();
            var currentTariff = TenantExtra.GetCurrentTariff();
            var currentQuota = TenantExtra.GetTenantQuota();
            var language = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            var customerId = CoreContext.TenantManager.GetCurrentTenant().OwnerId.ToString();

            var quotaActiveUsers = quota.ActiveUsers == -1 ? quantity : quota.ActiveUsers;
            var quotaMaxTotalSize = quota.ActiveUsers == -1 ? quota.MaxTotalSize * quantity : quota.MaxTotalSize;

            if (quotaActiveUsers < currentUsersCount
                || quotaMaxTotalSize < currentUsedSize
                || (!currentQuota.Free && (quotaActiveUsers * Constants.CoefficientOfVisitors) < currentVisitorsCount))
            {
                getLink = false;
            }
            else if (Equals(quota.Id, currentQuota.Id)
                && quantity == currentTariff.Quantity)
            {
                buttonText = Resource.TariffButtonExtend;
                if (!currentTariff.Prolongable)
                {
                    getLink = false;
                }
                else if (currentTariff.Autorenewal)
                {
                    getLink = false;
                    buttonText = Resource.TariffEnabledAutorenew;
                    infoText = Resource.TariffRemarkProlongEnable;
                }
            }
            else if (currentTariff.Prolongable)
            {
                buttonText = Resource.TariffButtonBuy;
                infoText = Resource.TariffRemarkProlongDisable;
            }
            else if (currentTariff.State == TariffState.Paid && quotaActiveUsers < currentQuota.ActiveUsers)
            {
                getLink = false;
                buttonText = Resource.TariffButtonBuy;
                infoText = currentQuota.Year3 ? Resource.TariffRemarkDisabledYear : Resource.TariffRemarkDisabledMonth;
            }

            var link = getLink ? GetShoppingLink(quota, currency, language, customerId, quantity) : string.Empty;

            return new Tuple<string, string, string>(buttonText, infoText, link);
        }

        private string GetShoppingLink(TenantQuota quota, string currency, string language, string customerId, int quantity)
        {
            var link = string.Empty;
            if (quota != null)
            {
                try
                {
                    var uri = CoreContext.PaymentManager.GetShoppingUri(quota.Id, true, null, currency, language, customerId, quantity.ToString());
                    if (uri == null)
                    {
                        var message = string.Format("GetShoppingLink: return null for tenant {0} quota {1} currency {2} language {3} customerId {4} quantity {5}", TenantProvider.CurrentTenantID, quota.Id, currency, language, customerId, quantity);
                        Log.Info(message);
                    }
                    else
                    {
                        link = uri.ToString();
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("GetShoppingLink: {0} tenant {1} quota {2} currency {3} language {4} customerId {5} quantity {6}", ex.Message, TenantProvider.CurrentTenantID, quota.Id, currency, language, customerId, quantity);
                    Log.Info(message, ex);
                }
            }
            return link;
        }

        private static string GetPriceString(decimal price, RegionInfo region)
        {
            var inEuro = "EUR".Equals(region.ISOCurrencySymbol);

            var priceString = inEuro && Math.Truncate(price) != price
                                  ? price.ToString(CultureInfo.InvariantCulture)
                                  : ((int)price).ToString(CultureInfo.InvariantCulture);

            return string.Format("{0}{1}", region.CurrencySymbol, priceString);
        }

        protected string GetPricePerMonthString(TenantQuota quota)
        {
            var length = quota.Year3 ? 36 : quota.Year ? 12 : 1;

            if (!string.IsNullOrEmpty(quota.AvangateId) && priceInfo.ContainsKey(quota.AvangateId))
            {
                var prices = priceInfo[quota.AvangateId];
                if (prices.ContainsKey(CurrentRegion.ISOCurrencySymbol))
                {
                    return GetPriceString(prices[CurrentRegion.ISOCurrencySymbol] / length, CurrentRegion);
                }
                return GetPriceString(quota.Price / length, RegionDefault);
            }
            return GetPriceString(quota.Price / length, CurrentRegion);
        }

        protected decimal GetPrice(TenantQuota quota, RegionInfo region)
        {
            if (priceInfo == null)
            {
                priceInfo = CoreContext.TenantManager.GetProductPriceInfo(false);
            }

            if (!string.IsNullOrEmpty(quota.AvangateId) && priceInfo.ContainsKey(quota.AvangateId))
            {
                var prices = priceInfo[quota.AvangateId];
                if (prices.ContainsKey(region.ISOCurrencySymbol))
                {
                    return prices[region.ISOCurrencySymbol];
                }
            }
            return quota.Price;
        }

        [AjaxMethod]
        public TariffDetails GetTariffDetails(int quotaId, int usersCount, string regionName)
        {
            var quotas = TenantExtra.GetTenantQuotas();
            var selectedQuota = quotas.FirstOrDefault(q => q.Id == quotaId);
            var monthQuota = quotas.FirstOrDefault(q => !q.Year && !q.Year3 && !q.Trial && !q.Free && !q.Open);

            if (selectedQuota == null || monthQuota == null)
            {
                throw new NotSupportedException(UserControlsCommonResource.SaasTariffErrorTariffNotFound);
            }

            if (usersCount < TariffService.ACTIVE_USERS_MIN || usersCount > TariffService.ACTIVE_USERS_MAX)
            {
                throw new NotSupportedException(UserControlsCommonResource.SaasTariffErrorInvalidNumberOfActiveUsers);
            }

            RegionInfo region = null;

            try
            {
                region = new RegionInfo(regionName);
            }
            catch
            {
            }

            if (region == null)
            {
                throw new NotSupportedException(UserControlsCommonResource.SaasTariffErrorRegionNotFound);
            }

            var period = selectedQuota.Year3
                    ? UserControlsCommonResource.SaasTariffThreeYears
                    : selectedQuota.Year
                        ? UserControlsCommonResource.SaasTariffOneYear
                        : UserControlsCommonResource.SaasTariffOneMonth;

            var length = selectedQuota.Year3 ? 36 : selectedQuota.Year ? 12 : 1;

            var sale = GetPrice(monthQuota, region) * usersCount * (selectedQuota.Year3 ? 36 : selectedQuota.Year ? 12 : 0);

            var total = GetPrice(selectedQuota, region) * usersCount;

            var result = new TariffDetails()
            {
                Price = GetPriceString(GetPrice(selectedQuota, region) / length, region),
                UsersCount = usersCount,
                Period = period,
                Sale = sale > 0 ? GetPriceString(sale, region) : string.Empty,
                TotalPrice = GetPriceString(total, region),
            };

            var shoppingData = GetShoppingData(selectedQuota, region.ISOCurrencySymbol, usersCount);

            result.ButtonText = shoppingData.Item1;
            result.InfoText = shoppingData.Item2;
            result.ShoppingUrl = shoppingData.Item3;

            return result;
        }

        [AjaxMethod]
        public TenantQuota ContinueStartup()
        {
            var currentQuota = TenantExtra.GetTenantQuota();
            var startupQuota = GetStartupQuota();

            if (!CheckStartupEnabled(currentQuota, startupQuota, out string message))
            {
                throw new NotSupportedException(message);
            }

            CoreContext.PaymentManager.SetTariff(TenantProvider.CurrentTenantID, new Tariff
            {
                QuotaId = startupQuota.Id,
                DueDate = DateTime.MaxValue
            });

            return startupQuota;
        }

        [Serializable]
        public class TariffDetails
        {
            public string Price;
            public int UsersCount;
            public string Period;
            public string Sale;
            public string TotalPrice;
            public string ButtonText;
            public string InfoText;
            public string ShoppingUrl;
        }
    }
}