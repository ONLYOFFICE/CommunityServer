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
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Statistics
{
    public static class TenantStatisticsProvider
    {
        public static bool IsNotPaid()
        {
            Tariff tariff;
            return TenantExtra.EnableTarrifSettings
                   && ((tariff = TenantExtra.GetCurrentTariff()).State >= TariffState.NotPaid
                       || TenantExtra.Enterprise && !TenantExtra.EnterprisePaid && tariff.LicenseDate == DateTime.MaxValue);
        }

        public static int GetUsersCount()
        {
            return CoreContext.UserManager.GetUsersByGroup(Constants.GroupUser.ID).Length;
        }

        public static long GetUsedSize()
        {
            return GetUsedSize(TenantProvider.CurrentTenantID);
        }

        public static long GetUsedSize(int tenant)
        {
            return GetQuotaRows(tenant).Sum(r => r.Counter);
        }

        public static long GetUsedSize(Guid moduleId)
        {
            return GetQuotaRows(TenantProvider.CurrentTenantID).Where(r => new Guid(r.Tag).Equals(moduleId)).Sum(r => r.Counter);
        }

        public static IEnumerable<TenantQuotaRow> GetQuotaRows(int tenant)
        {
            return CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(tenant))
                .Where(r => !string.IsNullOrEmpty(r.Tag) && new Guid(r.Tag) != Guid.Empty);
        }
    }
}