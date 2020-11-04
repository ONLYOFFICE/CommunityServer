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
using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.FederatedLogin.Profile;
using System;
using System.Collections.Generic;

namespace ASC.FederatedLogin
{
    public class AccountLinker
    {
        private static readonly ICache cache = AscCache.Memory;
        private static readonly ICacheNotify notify = AscCache.Notify;
        private readonly string dbid;


        static AccountLinker()
        {
            notify.Subscribe<LinkerCacheItem>((c, a) => cache.Remove(c.Obj));
        }


        public AccountLinker(string dbid)
        {
            this.dbid = dbid;
        }

        public IEnumerable<string> GetLinkedObjects(string id, string provider)
        {
            return GetLinkedObjects(new LoginProfile { Id = id, Provider = provider });
        }

        public IEnumerable<string> GetLinkedObjects(LoginProfile profile)
        {
            return GetLinkedObjectsByHashId(profile.HashId);
        }

        public IEnumerable<string> GetLinkedObjectsByHashId(string hashid)
        {
            using (var db = new DbManager(dbid))
            {
                var query = new SqlQuery("account_links")
                    .Select("id").Where("uid", hashid).Where(!Exp.Eq("provider", string.Empty));
                return db.ExecuteList(query).ConvertAll(x => (string)x[0]);
            }
        }

        public IEnumerable<LoginProfile> GetLinkedProfiles(string obj, string provider)
        {
            return GetLinkedProfiles(obj).Where(profile => profile.Provider.Equals(provider));
        }

        public IEnumerable<LoginProfile> GetLinkedProfiles(string obj)
        {
            var profiles = cache.Get<List<LoginProfile>>(obj);
            if (profiles == null)
            {
                cache.Insert(obj, profiles = GetLinkedProfilesFromDB(obj), DateTime.UtcNow + TimeSpan.FromMinutes(10));
            }
            return profiles;
        }

        private List<LoginProfile> GetLinkedProfilesFromDB(string obj)
        {
            //Retrieve by uinque id
            using (var db = new DbManager(dbid))
            {
                var query = new SqlQuery("account_links")
                    .Select("profile").Where("id", obj);
                return db.ExecuteList(query).ConvertAll(x => LoginProfile.CreateFromSerializedString((string)x[0]));
            }
        }

        public void AddLink(string obj, LoginProfile profile)
        {
            using (var db = new DbManager(dbid))
            {
                db.ExecuteScalar<int>(
                    new SqlInsert("account_links", true)
                        .InColumnValue("id", obj)
                        .InColumnValue("uid", profile.HashId)
                        .InColumnValue("provider", profile.Provider)
                        .InColumnValue("profile", profile.ToSerializedString())
                        .InColumnValue("linked", DateTime.UtcNow)
                    );
            }
            notify.Publish(new LinkerCacheItem { Obj = obj }, CacheNotifyAction.Remove);
        }

        public void AddLink(string obj, string id, string provider)
        {
            AddLink(obj, new LoginProfile { Id = id, Provider = provider });
        }

        public void RemoveLink(string obj, string id, string provider)
        {
            RemoveLink(obj, new LoginProfile { Id = id, Provider = provider });
        }

        public void RemoveLink(string obj, LoginProfile profile)
        {
            RemoveProvider(obj, hashId: profile.HashId);
        }

        public void RemoveProvider(string obj, string provider = null, string hashId = null)
        {
            var sql = new SqlDelete("account_links").Where("id", obj);

            if (!string.IsNullOrEmpty(provider)) sql.Where("provider", provider);
            if (!string.IsNullOrEmpty(hashId)) sql.Where("uid", hashId);

            using (var db = new DbManager(dbid))
            {
                db.ExecuteScalar<int>(sql);
            }
            notify.Publish(new LinkerCacheItem { Obj = obj }, CacheNotifyAction.Remove);
        }


        [Serializable]
        class LinkerCacheItem
        {
            public string Obj { get; set; }
        }
    }
}