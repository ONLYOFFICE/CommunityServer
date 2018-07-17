using System.Linq;
using System.Threading;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Data.Storage;

namespace ASC.Web.Studio.Core.Quota
{
    public class QuotaSync
    {
        public const string TenantIdKey = "tenantID";
        protected DistributedTask TaskInfo { get; private set; }
        private int TenantId { get; set; }

        public QuotaSync(int tenantId)
        {
            TenantId = tenantId;
            TaskInfo = new DistributedTask();
        }

        public void RunJob(DistributedTask _, CancellationToken cancellationToken)
        {
            CoreContext.TenantManager.SetCurrentTenant(TenantId);

            var storageModules = StorageFactory.GetModuleList(string.Empty).ToList();

            foreach (var module in storageModules)
            {
                var storage = StorageFactory.GetStorage(TenantId.ToString(), module);
                storage.ResetQuota("");

                var domains = StorageFactory.GetDomainList(string.Empty, module).ToList();
                foreach (var domain in domains)
                {
                    storage.ResetQuota(domain);
                }

            }
        }


        public virtual DistributedTask GetDistributedTask()
        {
            TaskInfo.SetProperty(TenantIdKey, TenantId);
            return TaskInfo;
        }
    }
}