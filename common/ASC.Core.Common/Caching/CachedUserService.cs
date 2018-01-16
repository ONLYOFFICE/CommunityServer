/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Linq;
using System.Threading;
using ASC.Common.Caching;
using ASC.Core.Tenants;
using ASC.Core.Users;

namespace ASC.Core.Caching
{
    public class CachedUserService : IUserService, ICachedService
    {
        private const string USERS = "users";
        private const string GROUPS = "groups";
        private const string REFS = "refs";

        private readonly IUserService service;
        private readonly ICache cache;
        private readonly ICacheNotify cacheNotify;
        private readonly TrustInterval trustInterval;
        private int getchanges;


        public TimeSpan CacheExpiration { get; set; }

        public TimeSpan DbExpiration { get; set; }

        public TimeSpan PhotoExpiration { get; set; }


        public CachedUserService(IUserService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            this.service = service;
            cache = AscCache.Memory;
            trustInterval = new TrustInterval();

            CacheExpiration = TimeSpan.FromMinutes(20);
            DbExpiration = TimeSpan.FromMinutes(1);
            PhotoExpiration = TimeSpan.FromMinutes(10);

            cacheNotify = AscCache.Notify;
            cacheNotify.Subscribe<UserInfo>((u, a) => InvalidateCache());
            cacheNotify.Subscribe<UserPhoto>((p, a) => cache.Remove(p.Key));
            cacheNotify.Subscribe<Group>((g, a) => InvalidateCache());
            cacheNotify.Subscribe<UserGroupRef>((r, a) => UpdateUserGroupRefCache(r, a == CacheNotifyAction.Remove));
        }


        public IDictionary<Guid, UserInfo> GetUsers(int tenant, DateTime from)
        {
            var users = GetUsers(tenant);
            lock (users)
            {
                return (from == default(DateTime) ? users.Values : users.Values.Where(u => u.LastModified >= from)).ToDictionary(u => u.ID);
            }
        }

        public UserInfo GetUser(int tenant, Guid id)
        {
            if (CoreContext.Configuration.Personal)
            {
                return GetUserForPersonal(tenant, id);
            }

            var users = GetUsers(tenant);
            lock (users)
            {
                UserInfo u;
                users.TryGetValue(id, out u);
                return u;
            }
        }

        /// <summary>
        /// For Personal only
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private UserInfo GetUserForPersonal(int tenant, Guid id)
        {
            var key = GetUserCacheKeyForPersonal(tenant, id);
            var user = cache.Get<UserInfo>(key);

            if (user == null)
            {
                user = service.GetUser(tenant, id);

                if (user != null)
                {
                    cache.Insert(key, user, CacheExpiration);
                }
            }

            return user;
        }

        public UserInfo GetUser(int tenant, string login, string passwordHash)
        {
            return service.GetUser(tenant, login, passwordHash);
        }

        public UserInfo SaveUser(int tenant, UserInfo user)
        {
            user = service.SaveUser(tenant, user);
            cacheNotify.Publish(user, CacheNotifyAction.InsertOrUpdate);
            return user;
        }

        public void RemoveUser(int tenant, Guid id)
        {
            service.RemoveUser(tenant, id);
            cacheNotify.Publish(new UserInfo { ID = id }, CacheNotifyAction.Remove);
        }

        public byte[] GetUserPhoto(int tenant, Guid id)
        {
            var photo = cache.Get<byte[]>(GetUserPhotoCacheKey(tenant, id));
            if (photo == null)
            {
                photo = service.GetUserPhoto(tenant, id);
                cache.Insert(GetUserPhotoCacheKey(tenant, id), photo, PhotoExpiration);
            }
            return photo;
        }

        public void SetUserPhoto(int tenant, Guid id, byte[] photo)
        {
            service.SetUserPhoto(tenant, id, photo);
            cacheNotify.Publish(new UserPhoto { Key = GetUserPhotoCacheKey(tenant, id) }, CacheNotifyAction.Remove);
        }

        public string GetUserPassword(int tenant, Guid id)
        {
            return service.GetUserPassword(tenant, id);
        }

        public void SetUserPassword(int tenant, Guid id, string password)
        {
            service.SetUserPassword(tenant, id, password);
        }


        public IDictionary<Guid, Group> GetGroups(int tenant, DateTime from)
        {
            var groups = GetGroups(tenant);
            lock (groups)
            {
                return (from == default(DateTime) ? groups.Values : groups.Values.Where(g => g.LastModified >= from)).ToDictionary(g => g.Id);
            }
        }

        public Group GetGroup(int tenant, Guid id)
        {
            var groups = GetGroups(tenant);
            lock (groups)
            {
                Group g;
                groups.TryGetValue(id, out g);
                return g;
            }
        }

        public Group SaveGroup(int tenant, Group group)
        {
            group = service.SaveGroup(tenant, group);
            cacheNotify.Publish(group, CacheNotifyAction.InsertOrUpdate);
            return group;
        }

        public void RemoveGroup(int tenant, Guid id)
        {
            service.RemoveGroup(tenant, id);
            cacheNotify.Publish(new Group { Id = id }, CacheNotifyAction.Remove);
        }


        public IDictionary<string, UserGroupRef> GetUserGroupRefs(int tenant, DateTime from)
        {
            GetChangesFromDb();

            var key = GetRefCacheKey(tenant);
            var refs = cache.Get<UserGroupRefStore>(key) as IDictionary<string, UserGroupRef>;
            if (refs == null)
            {
                refs = service.GetUserGroupRefs(tenant, default(DateTime));
                cache.Insert(key, new UserGroupRefStore(refs), CacheExpiration);
            }
            lock (refs)
            {
                return from == default(DateTime) ? refs : refs.Values.Where(r => r.LastModified >= from).ToDictionary(r => r.CreateKey());
            }
        }

        public UserGroupRef SaveUserGroupRef(int tenant, UserGroupRef r)
        {
            r = service.SaveUserGroupRef(tenant, r);
            cacheNotify.Publish(r, CacheNotifyAction.InsertOrUpdate);
            return r;
        }

        public void RemoveUserGroupRef(int tenant, Guid userId, Guid groupId, UserGroupRefType refType)
        {
            service.RemoveUserGroupRef(tenant, userId, groupId, refType);

            var r = new UserGroupRef(userId, groupId, refType) { Tenant = tenant };
            cacheNotify.Publish(r, CacheNotifyAction.Remove);
        }


        public void InvalidateCache()
        {
            trustInterval.Expire();
        }


        private IDictionary<Guid, UserInfo> GetUsers(int tenant)
        {
            GetChangesFromDb();

            var key = GetUserCacheKey(tenant);
            var users = cache.Get<IDictionary<Guid, UserInfo>>(key);
            if (users == null)
            {
                users = service.GetUsers(tenant, default(DateTime));

                cache.Insert(key, users, CacheExpiration);
            }
            return users;
        }

        private IDictionary<Guid, Group> GetGroups(int tenant)
        {
            GetChangesFromDb();

            var key = GetGroupCacheKey(tenant);
            var groups = cache.Get<IDictionary<Guid, Group>>(key);
            if (groups == null)
            {
                groups = service.GetGroups(tenant, default(DateTime));
                cache.Insert(key, groups, CacheExpiration);
            }
            return groups;
        }

        private void GetChangesFromDb()
        {
            if (!trustInterval.Expired)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref getchanges, 1, 0) == 0)
            {
                try
                {
                    if (!trustInterval.Expired)
                    {
                        return;
                    }

                    var starttime = trustInterval.StartTime;
                    if (starttime != default(DateTime))
                    {
                        var correction = TimeSpan.FromTicks(DbExpiration.Ticks * 3);
                        starttime = trustInterval.StartTime.Subtract(correction);
                    }

                    trustInterval.Start(DbExpiration);

                    //get and merge changes in cached tenants
                    foreach (var tenantGroup in service.GetUsers(Tenant.DEFAULT_TENANT, starttime).Values.GroupBy(u => u.Tenant))
                    {
                        var users = cache.Get<IDictionary<Guid, UserInfo>>(GetUserCacheKey(tenantGroup.Key));
                        if (users != null)
                        {
                            lock (users)
                            {
                                foreach (var u in tenantGroup)
                                {
                                    users[u.ID] = u;
                                }
                            }
                        }
                    }

                    foreach (var tenantGroup in service.GetGroups(Tenant.DEFAULT_TENANT, starttime).Values.GroupBy(g => g.Tenant))
                    {
                        var groups = cache.Get<IDictionary<Guid, Group>>(GetGroupCacheKey(tenantGroup.Key));
                        if (groups != null)
                        {
                            lock (groups)
                            {
                                foreach (var g in tenantGroup)
                                {
                                    groups[g.Id] = g;
                                }
                            }
                        }
                    }

                    foreach (var tenantGroup in service.GetUserGroupRefs(Tenant.DEFAULT_TENANT, starttime).Values.GroupBy(r => r.Tenant))
                    {
                        var refs = cache.Get<UserGroupRefStore>(GetRefCacheKey(tenantGroup.Key));
                        if (refs != null)
                        {
                            lock (refs)
                            {
                                foreach (var r in tenantGroup)
                                {
                                    refs[r.CreateKey()] = r;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Volatile.Write(ref getchanges, 0);
                }
            }
        }

        private void UpdateUserGroupRefCache(UserGroupRef r, bool remove)
        {
            var key = GetRefCacheKey(r.Tenant);
            var refs = cache.Get<UserGroupRefStore>(key);
            if (!remove && refs != null)
            {
                lock (refs)
                {
                    refs[r.CreateKey()] = r;
                }
            }
            else
            {
                InvalidateCache();
            }
        }


        private static string GetUserPhotoCacheKey(int tenant, Guid userId)
        {
            return tenant.ToString() + "userphoto" + userId.ToString();
        }

        private static string GetGroupCacheKey(int tenant)
        {
            return tenant.ToString() + GROUPS;
        }

        private static string GetRefCacheKey(int tenant)
        {
            return tenant.ToString() + REFS;
        }
        private static string GetUserCacheKey(int tenant)
        {
            return tenant.ToString() + USERS;
        }

        private static string GetUserCacheKeyForPersonal(int tenant, Guid userId)
        {
            return tenant.ToString() + USERS + userId;
        }


        [Serializable]
        class UserPhoto
        {
            public string Key { get; set; }
        }
    }
}
