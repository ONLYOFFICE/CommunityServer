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
using System.Linq;
using System.Threading;

using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Web.Core;

namespace ASC.Web.Studio.Core.Quota
{
    public class UserQuotaSync: DistributedTask
    {
        public const string TenantIdKey = "tenantID";
        protected DistributedTask TaskInfo { get; private set; }
        private int TenantId { get; set; }

        public UserQuotaSync(int tenantId)
        {
            TenantId = tenantId;
            TaskInfo = new DistributedTask();
        }

        public void RunJob(DistributedTask _, CancellationToken cancellationToken)
        {
            CoreContext.TenantManager.SetCurrentTenant(TenantId);

            var storageModules = StorageFactory.GetModuleList(string.Empty).ToList();

            var users = CoreContext.UserManager.GetUsers();
            var webItems = WebItemManager.Instance.GetItems(Web.Core.WebZones.WebZoneType.All, ItemAvailableState.All);

            foreach (var user in users)
            {
                SecurityContext.AuthenticateMe(user.ID);
                
                foreach (var item in webItems)
                {
                    IUserSpaceUsage manager;

                    if (item.ID == WebItemManager.DocumentsProductID || item.ID == WebItemManager.MailProductID || item.ID == WebItemManager.TalkProductID)
                    {
                        manager = item.Context.SpaceUsageStatManager as IUserSpaceUsage;
                        if (manager == null) continue;
                        manager.RecalculateUserQuota(TenantId, user.ID);
                    }
                }
            }

            var tenantUserQuotaSetting = TenantUserQuotaSettings.Load();
            tenantUserQuotaSetting.LastRecalculateDate = DateTime.UtcNow;

            tenantUserQuotaSetting.Save();

        }


        public virtual DistributedTask GetDistributedTask()
        {
            TaskInfo.SetProperty(TenantIdKey, TenantId);
            return TaskInfo;
        }
    }
}