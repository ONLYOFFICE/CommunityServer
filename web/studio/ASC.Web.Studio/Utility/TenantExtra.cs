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
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Statistics;

namespace ASC.Web.Studio.Utility
{
    public static class TenantExtra
    {
        public static bool EnableTarrifSettings
        {
            get
            {
                return
                    SetupInfo.IsVisibleSettings<TariffSettings>()
                    && !TenantAccessSettings.Load().Anyone
                    && (!CoreContext.Configuration.Standalone || !string.IsNullOrEmpty(LicenseReader.LicensePath));
            }
        }

        public static bool Saas
        {
            get { return !CoreContext.Configuration.Standalone; }
        }

        public static bool Enterprise
        {
            get { return CoreContext.Configuration.Standalone && !String.IsNullOrEmpty(LicenseReader.LicensePath); }
        }

        public static bool Opensource
        {
            get { return CoreContext.Configuration.Standalone && String.IsNullOrEmpty(LicenseReader.LicensePath); }
        }

        public static bool EnterprisePaid
        {
            get { return Enterprise && GetCurrentTariff().State < TariffState.NotPaid; }
        }

        public static bool EnableControlPanel
        {
            get
            {
                return CoreContext.Configuration.Standalone && !String.IsNullOrEmpty(SetupInfo.ControlPanelUrl)
                  && GetTenantQuota().ControlPanel && GetCurrentTariff().State < TariffState.NotPaid
                  && CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();
            }
        }

        public static string GetAppsPageLink()
        {
            return VirtualPathUtility.ToAbsolute("~/AppInstall.aspx");
        }

        public static string GetTariffPageLink()
        {
            return EnableControlPanel
                ? CommonLinkUtility.GetFullAbsolutePath(SetupInfo.ControlPanelUrl.TrimEnd('/') + "/activate")
                : VirtualPathUtility.ToAbsolute("~/Tariffs.aspx");
        }

        public static Tariff GetCurrentTariff()
        {
            return CoreContext.PaymentManager.GetTariff(TenantProvider.CurrentTenantID);
        }

        public static TenantQuota GetTenantQuota()
        {
            return GetTenantQuota(TenantProvider.CurrentTenantID);
        }

        public static TenantQuota GetTenantQuota(int tenant)
        {
            return CoreContext.TenantManager.GetTenantQuota(tenant);
        }

        public static IEnumerable<TenantQuota> GetTenantQuotas()
        {
            return CoreContext.TenantManager.GetTenantQuotas();
        }

        private static TenantQuota GetPrevQuota(TenantQuota curQuota)
        {
            TenantQuota prev = null;
            foreach (var quota in GetTenantQuotas().OrderBy(r => r.ActiveUsers).Where(r => r.Year == curQuota.Year && r.Year3 == curQuota.Year3))
            {
                if (quota.Id == curQuota.Id)
                    return prev;

                prev = quota;
            }
            return null;
        }

        public static int GetPrevUsersCount(TenantQuota quota)
        {
            var prevQuota = GetPrevQuota(quota);
            if (prevQuota == null || prevQuota.Trial)
                return 1;
            return prevQuota.ActiveUsers + 1;
        }

        public static int GetRightQuotaId()
        {
            var q = GetRightQuota();
            return q != null ? q.Id : 0;
        }

        public static TenantQuota GetRightQuota()
        {
            var usedSpace = TenantStatisticsProvider.GetUsedSize();
            var needUsersCount = TenantStatisticsProvider.GetUsersCount();
            var quotas = GetTenantQuotas();

            return quotas.OrderBy(q => q.ActiveUsers)
                         .ThenBy(q => q.Year)
                         .FirstOrDefault(q =>
                                         q.ActiveUsers > needUsersCount
                                         && q.MaxTotalSize > usedSpace
                                         && !q.Free
                                         && !q.Trial);
        }

        public static int GetRemainingCountUsers()
        {
            return GetTenantQuota().ActiveUsers - TenantStatisticsProvider.GetUsersCount();
        }

        public static bool UpdatedWithoutLicense
        {
            get
            {
                DateTime licenseDay;
                return CoreContext.Configuration.Standalone
                       && (licenseDay = GetCurrentTariff().LicenseDate.Date) < DateTime.Today
                       && licenseDay < LicenseReader.VersionReleaseDate;
            }
        }

        public static void DemandControlPanelPermission()
        {
            if (!CoreContext.Configuration.Standalone || TenantControlPanelSettings.Instance.LimitedAccess)
            {
                throw new System.Security.SecurityException(Resources.Resource.ErrorAccessDenied);
            }
        }
    }
}