/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
            get { return SetupInfo.IsVisibleSettings<TariffSettings>(); }
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

        public static TenantQuota GetRightQuota(bool year = false)
        {
            var usedSpace = TenantStatisticsProvider.GetUsedSize();
            var needUsersCount = TenantStatisticsProvider.GetUsersCount();
            var quotas = GetTenantQuotas();
            TenantQuota result = null;

            if (!year)
            {
                result = quotas.OrderBy(q => q.Price).FirstOrDefault();
                if (result != null && result.DocsEdition && result.ActiveUsers > needUsersCount && result.MaxTotalSize > usedSpace)
                {
                    return result;
                }
            }

            result = quotas.OrderBy(r => r.ActiveUsers)
                                    .FirstOrDefault(quota =>
                                                    quota.ActiveUsers > needUsersCount
                                                    && quota.MaxTotalSize > usedSpace
                                                    && quota.DocsEdition
                                                    && quota.Year == year);
            if (year || result != null) return result;
            return GetRightQuota(true);
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
                var num = count % 100;

                if (num >= 11 && num <= 19)
                {
                    end = Resource.DaysTwo;
                }
                else
                {
                    var i = count % 10;
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
            String Text = "";
            if (tariff.State == TariffState.Trial)
            {
                Text = Resource.TrialPeriodInfoText;
            }
            if (tariff.State == TariffState.Paid)
            {
                Text = Resource.TariffExceedLimitInfoText;
            }
            return String.Format(Text, "<a href=\"" + TenantExtra.GetTariffPageLink() + "\">", "</a>");
        }
        public static void TrialRequest()
        {
            CoreContext.PaymentManager.SendTrialRequest(
                TenantProvider.CurrentTenantID,
                CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID));
        }

        public static int GetRemainingCountUsers()
        {
            return GetTenantQuota().ActiveUsers - TenantStatisticsProvider.GetUsersCount();
        }
    }
}