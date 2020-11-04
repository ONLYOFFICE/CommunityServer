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


using ASC.Common.Caching;
using ASC.Core;
using ASC.Projects.Core.Domain;
using System;

namespace ASC.Projects.Engine
{
    public class CachedProjectEngine : ProjectEngine
    {
        private static readonly ICache cache = AscCache.Memory;
        private static readonly ICacheNotify notify = AscCache.Notify;
        private static readonly TimeSpan expiration = TimeSpan.FromMinutes(10);


        static CachedProjectEngine()
        {
            notify.Subscribe<ProjectCacheItem>((i, a) => cache.Remove(GetCountKey(i.Tenant)));
        }

        public CachedProjectEngine(bool disableNotificationParameter)
            : base(disableNotificationParameter)
        {
            
        }

        public override int CountOpen()
        {
            var key = GetCountKey(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            var value = cache.Get<string>(key);

            if (!String.IsNullOrEmpty(value))
            {
                return Convert.ToInt32(value);
            }
            var count = base.CountOpen();
            cache.Insert(key, count, DateTime.UtcNow.Add(expiration));
            return count;
        }

        public override Project SaveOrUpdate(Project project, bool notifyManager, bool isImport)
        {
            var p = base.SaveOrUpdate(project, notifyManager, isImport);
            notify.Publish(new ProjectCacheItem { Tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId }, CacheNotifyAction.InsertOrUpdate);
            return p;
        }

        public override void Delete(int projectId)
        {
            base.Delete(projectId);
            notify.Publish(new ProjectCacheItem { Tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId }, CacheNotifyAction.Remove);
        }

        private static string GetCountKey(int tenant)
        {
            return tenant + "/projects/count";
        }


        [Serializable]
        class ProjectCacheItem
        {
            public int Tenant { get; set; }
        }
    }
}
