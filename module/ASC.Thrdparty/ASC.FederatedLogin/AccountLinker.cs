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


using ASC.Collections;
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
        private readonly string _dbid;
        private const string LinkTable = "account_links";

        private static readonly CachedDictionary<IEnumerable<LoginProfile>> CacheEntry = new CachedDictionary<IEnumerable<LoginProfile>>("account_links", x => true);

        public AccountLinker(string dbid)
        {
            _dbid = dbid;
        }

        public IEnumerable<string> GetLinkedObjects(string id, string provider)
        {
            return GetLinkedObjects(new LoginProfile {Id = id, Provider = provider});
        }

        public IEnumerable<string> GetLinkedObjects(LoginProfile profile)
        {
            //Retrieve by uinque id
            return GetLinkedObjectsByHashId(profile.HashId);
        }

        public IEnumerable<string> GetLinkedObjectsByHashId(string hashid)
        {
            //Retrieve by uinque id
            using (var db = new DbManager(_dbid))
            {
                var query = new SqlQuery(LinkTable)
                    .Select("id").Where("uid", hashid).Where(!Exp.Eq("provider", string.Empty));
                return db.ExecuteList(query).ConvertAll(x => (string) x[0]);
            }
        }

        public IEnumerable<LoginProfile> GetLinkedProfiles(string obj)
        {
            return CacheEntry.Get(obj, () => GetLinkedProfilesFromDB(obj));
        }

        private IEnumerable<LoginProfile> GetLinkedProfilesFromDB(string obj)
        {
            //Retrieve by uinque id
            using (var db = new DbManager(_dbid))
            {
                var query = new SqlQuery(LinkTable)
                    .Select("profile").Where("id", obj);
                return db.ExecuteList(query).ConvertAll(x => LoginProfile.CreateFromSerializedString((string) x[0]));
            }
        }

        public void AddLink(string obj, LoginProfile profile)
        {
            CacheEntry.Reset(obj);

            using (var db = new DbManager(_dbid))
            {
                db.ExecuteScalar<int>(
                    new SqlInsert(LinkTable, true)
                        .InColumnValue("id", obj)
                        .InColumnValue("uid", profile.HashId)
                        .InColumnValue("provider", profile.Provider)
                        .InColumnValue("profile", profile.ToSerializedString())
                        .InColumnValue("linked", DateTime.UtcNow)
                    );
            }
        }

        public void AddLink(string obj, string id, string provider)
        {
            AddLink(obj, new LoginProfile {Id = id, Provider = provider});
        }

        public void RemoveLink(string obj, string id, string provider)
        {
            RemoveLink(obj, new LoginProfile {Id = id, Provider = provider});
        }

        public void RemoveLink(string obj, LoginProfile profile)
        {
            RemoveProvider(obj, hashId: profile.HashId);
        }

        public void RemoveProvider(string obj, string provider = null, string hashId = null)
        {
            CacheEntry.Reset(obj);

            var sql = new SqlDelete(LinkTable).Where("id", obj);

            if (!string.IsNullOrEmpty(provider)) sql.Where("provider", provider);
            if (!string.IsNullOrEmpty(hashId)) sql.Where("uid", hashId);

            using (var db = new DbManager(_dbid))
            {
                db.ExecuteScalar<int>(sql);
            }
        }
    }
}