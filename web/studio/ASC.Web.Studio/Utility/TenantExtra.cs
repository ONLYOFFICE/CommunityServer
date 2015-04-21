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


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Statistics;
using Resources;

namespace ASC.Web.Studio.Utility
{
    public static class TenantExtra
    {
        public static bool EnableTarrifSettings
        {
            get
            {
                return SetupInfo.IsVisibleSettings<TariffSettings>() &&
                       !SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID).Anyone;
            }
        }

        public static string GetTariffPageLink()
        {
            return CoreContext.Configuration.Standalone
                       ? "https://www.onlyoffice.com/" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName + "/server-enterprise.aspx"
                       : VirtualPathUtility.ToAbsolute("~/tariffs.aspx");
        }

        public static Tariff GetCurrentTariff()
        {
            return CoreContext.PaymentManager.GetTariff(TenantProvider.CurrentTenantID);
        }

        public static TenantQuota GetTenantQuota()
        {
            var q = GetTenantQuota(TenantProvider.CurrentTenantID);
            if (CoreContext.Configuration.Standalone && LicenseReader.GetLicense() != null && LicenseReader.GetLicense().getUserQuota() > 0)
            {
                q.ActiveUsers = LicenseReader.GetLicense().getUserQuota();
            }
            return q;
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
            foreach (var quota in GetTenantQuotas().OrderBy(r => r.ActiveUsers).Where(r => r.DocsEdition && r.Year == curQuota.Year))
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
            if (prevQuota.DocsEdition != quota.DocsEdition)
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
                                         && q.DocsEdition
                                         && !q.Free
                                         && !q.Trial);
        }

        public static string GetTariffNotify()
        {
            var tariff = GetCurrentTariff();
            if (tariff.State == TariffState.Trial)
            {
                var count = tariff.DueDate.Subtract(DateTime.Today.Date).Days;
                if (count <= 0)
                    return Resource.TrialPeriodExpired;

                string end;
                var num = count%100;

                if (num >= 11 && num <= 19)
                {
                    end = Resource.DaysTwo;
                }
                else
                {
                    var i = count%10;
                    switch (i)
                    {
                        case (1):
                            end = Resource.Day;
                            break;
                        case (2):
                        case (3):
                        case (4):
                            end = Resource.DaysOne;
                            break;
                        default:
                            end = Resource.DaysTwo;
                            break;
                    }
                }
                return string.Format(Resource.TrialPeriod, count, end);
            }

            if (tariff.State == TariffState.Paid)
            {
                var quota = GetTenantQuota();
                long notifySize;
                long.TryParse(ConfigurationManager.AppSettings["web.tariff-notify.storage"] ?? "314572800", out notifySize); //300 MB
                if (notifySize > 0 && quota.MaxTotalSize - TenantStatisticsProvider.GetUsedSize() < notifySize)
                {
                    return string.Format(Resource.TariffExceedLimit, FileSizeComment.FilesSizeToString(quota.MaxTotalSize));
                }
            }

            return string.Empty;
        }

        public static string GetTariffNotifyText()
        {
            var tariff = GetCurrentTariff();
            var text = "";
            if (tariff.State == TariffState.Trial)
            {
                text = Resource.TrialPeriodInfoText;
            }
            if (tariff.State == TariffState.Paid)
            {
                text = Resource.TariffExceedLimitInfoText;
            }
            return String.Format(text, "<a href=\"" + GetTariffPageLink() + "\">", "</a>");
        }

        public static void TrialRequest()
        {
            CoreContext.PaymentManager.SendTrialRequest(
                TenantProvider.CurrentTenantID,
                CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID));
        }

        public static void FreeRequest()
        {
            var quota = GetTenantQuotas().FirstOrDefault(q => q.Free);
            if (quota != null)
                CoreContext.PaymentManager.SetTariff(TenantProvider.CurrentTenantID, new Tariff
                    {
                        QuotaId = quota.Id,
                        DueDate = DateTime.MaxValue
                    });
        }

        public static int GetRemainingCountUsers()
        {
            return GetTenantQuota().ActiveUsers - TenantStatisticsProvider.GetUsersCount();
        }
    }
}