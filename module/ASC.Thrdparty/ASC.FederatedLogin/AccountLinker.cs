/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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