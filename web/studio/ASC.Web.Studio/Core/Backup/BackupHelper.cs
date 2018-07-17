using System;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Backup
{
    public class BackupHelper
    {
        public const long AvailableZipSize = 10 * 1024 * 1024 * 1024L;
        private static readonly Guid mailStorageTag = new Guid("666ceac1-4532-4f8c-9cba-8f510eca2fd1");

        public static BackupAvailableSize GetAvailableSize()
        {
            if (CoreContext.Configuration.Standalone)
                return BackupAvailableSize.Available;

            var size = CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(TenantProvider.CurrentTenantID))
                      .Where(r => !string.IsNullOrEmpty(r.Tag) && new Guid(r.Tag) != Guid.Empty && !new Guid(r.Tag).Equals(mailStorageTag))
                      .Sum(r => r.Counter);
            if (size > AvailableZipSize)
            {
                return BackupAvailableSize.NotAvailable;
            }
            
            size = TenantStatisticsProvider.GetUsedSize();
            if (size > AvailableZipSize)
            {
                return BackupAvailableSize.WithoutMail;
            }

            return BackupAvailableSize.Available;
        }

        public static bool ExceedsMaxAvailableSize
        {
            get
            {
                return GetAvailableSize() != BackupAvailableSize.Available;
            }
        }
    }

    public enum BackupAvailableSize
    {
        Available,
        WithoutMail,
        NotAvailable,
    }
}