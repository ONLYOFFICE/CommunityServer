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


using System;
using System.Collections.Generic;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;

namespace ASC.Feed.Data
{
    public class FeedReadedDataProvider
    {
        private const string dbId = Constants.FeedDbId;


        public DateTime GetTimeReaded()
        {
            return GetTimeReaded(GetUser(), "all", GetTenant());
        }

        public DateTime GetTimeReaded(string module)
        {
            return GetTimeReaded(GetUser(), module, GetTenant());
        }

        public DateTime GetTimeReaded(Guid user, string module, int tenant)
        {
            var query = new SqlQuery("feed_readed")
                .SelectMax("timestamp")
                .Where("tenant_id", tenant)
                .Where("user_id", user.ToString())
                .Where("module", module);

            using (var db = GetDb())
            {
                return db.ExecuteScalar<DateTime>(query);
            }
        }

        public void SetTimeReaded()
        {
            SetTimeReaded(GetUser(), DateTime.UtcNow, "all", GetTenant());
        }

        public void SetTimeReaded(string module)
        {
            SetTimeReaded(GetUser(), DateTime.UtcNow, module, GetTenant());
        }

        public void SetTimeReaded(Guid user)
        {
            SetTimeReaded(user, DateTime.UtcNow, "all", GetTenant());
        }

        public void SetTimeReaded(Guid user, DateTime time, string module, int tenant)
        {
            if (string.IsNullOrEmpty(module)) return;

            var query = new SqlInsert("feed_readed", true)
                .InColumns("user_id", "timestamp", "module", "tenant_id")
                .Values(user.ToString(), time, module, tenant);

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(query);
            }
        }

        public IEnumerable<string> GetReadedModules(DateTime fromTime)
        {
            return GetReadedModules(GetUser(), GetTenant(), fromTime);
        }

        public IEnumerable<string> GetReadedModules(Guid user, int tenant, DateTime fromTime)
        {
            var query = new SqlQuery("feed_readed")
                .Select("module")
                .Where("tenant_id", tenant)
                .Where("user_id", user)
                .Where(Exp.Gt("timestamp", fromTime));

            using (var db = GetDb())
            {
                return db.ExecuteList(query).ConvertAll(r => (string)r[0]);
            }
        }


        private static DbManager GetDb()
        {
            return new DbManager(dbId);
        }

        private static int GetTenant()
        {
            return CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }

        private static Guid GetUser()
        {
            return SecurityContext.CurrentAccount.ID;
        }
    }
}