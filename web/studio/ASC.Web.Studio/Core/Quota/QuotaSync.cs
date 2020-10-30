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