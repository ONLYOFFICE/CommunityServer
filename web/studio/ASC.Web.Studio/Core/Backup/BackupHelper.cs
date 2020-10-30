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
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Studio.UserControls.Statistics;

namespace ASC.Web.Studio.Core.Backup
{
    public class BackupHelper
    {
        public const long AvailableZipSize = 10 * 1024 * 1024 * 1024L;
        private static readonly Guid mailStorageTag = new Guid("666ceac1-4532-4f8c-9cba-8f510eca2fd1");

        public static BackupAvailableSize GetAvailableSize(int tenantId)
        {
            if (CoreContext.Configuration.Standalone)
                return BackupAvailableSize.Available;

            var size = CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(tenantId))
                      .Where(r => !string.IsNullOrEmpty(r.Tag) && new Guid(r.Tag) != Guid.Empty && !new Guid(r.Tag).Equals(mailStorageTag))
                      .Sum(r => r.Counter);
            if (size > AvailableZipSize)
            {
                return BackupAvailableSize.NotAvailable;
            }

            size = TenantStatisticsProvider.GetUsedSize(tenantId);
            if (size > AvailableZipSize)
            {
                return BackupAvailableSize.WithoutMail;
            }

            return BackupAvailableSize.Available;
        }

        public static bool ExceedsMaxAvailableSize(int tenantId)
        {
            return GetAvailableSize(tenantId) != BackupAvailableSize.Available;
        }
    }

    public enum BackupAvailableSize
    {
        Available,
        WithoutMail,
        NotAvailable,
    }
}