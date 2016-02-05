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


using ASC.Common.Caching;
using ASC.Core;
using ASC.Projects.Core.DataInterfaces;
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


        public CachedProjectEngine(IDaoFactory daoFactory, EngineFactory factory)
            : base(daoFactory, factory)
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
