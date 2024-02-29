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
using System.Collections.Generic;
using System.Linq;

using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.AuditTrail;
using ASC.Core.Tenants;
using ASC.MessagingSystem;

namespace ASC.Core.Data
{
    public class DbLoginEventsManager
    {
        private const string guidLoginEvent = "F4D8BBF6-EB63-4781-B55E-5885EAB3D759";

        private static readonly ICache cache = AscCache.Memory;
        private static readonly TimeSpan expirationTimeout = TimeSpan.FromMinutes(5);
        private static string dbId = "default";

        private static readonly List<MessageAction> loginActions = new List<MessageAction>
        {
            MessageAction.LoginSuccess,
            MessageAction.LoginSuccessViaSocialAccount,
            MessageAction.LoginSuccessViaSms,
            MessageAction.LoginSuccessViaApi,
            MessageAction.LoginSuccessViaSocialApp,
            MessageAction.LoginSuccessViaApiSms,
            MessageAction.LoginSuccessViaSSO,
            MessageAction.LoginSuccessViaApiSocialAccount,
            MessageAction.LoginSuccesViaTfaApp,
            MessageAction.LoginSuccessViaApiTfa
        };

        public static bool IsActiveLoginEvent(int tenantId, Guid userId, int loginEventId)
        {
            if (loginEventId == 0) return true;

            var cacheKey = GetCacheKey(tenantId, userId);

            var cachedLoginEvents = cache.Get<Dictionary<int, bool>>(cacheKey);
            if (cachedLoginEvents != null)
            {
                if (cachedLoginEvents.ContainsKey(loginEventId))
                {
                    return cachedLoginEvents[loginEventId];
                }
            }
            else
            {
                cachedLoginEvents = new Dictionary<int, bool>();
            }

            using (var db = GetDbManager())
            {
                var query = new SqlQuery("login_events")
                    .Select("active")
                    .Where("id", loginEventId);

                var isActive = db.ExecuteScalar<bool>(query);

                cachedLoginEvents[loginEventId] = isActive;

                cache.Insert(cacheKey, cachedLoginEvents, expirationTimeout);

                return isActive;
            }
        }

        public static BaseEvent GetLoginEvent(int tenantId, int loginEventId)
        {
            using (var db = GetDbManager())
            {
                var query = new SqlQuery("login_events")
                    .Select("id", "substring_index(ip, ':', 1) ip", "platform", "browser", "date", "user_id")
                    .Where("tenant_id", tenantId)
                    .Where("id", loginEventId)
                    .Where("active", true);

                var loginEvent = db.ExecuteList(query).Select(row => new BaseEvent
                {
                    TenantId = tenantId,
                    Id = (int)row[0],
                    IP = (string)row[1],
                    Platform = (string)row[2],
                    Browser = (string)row[3],
                    Date = TenantUtil.DateTimeFromUtc((DateTime)row[4]),
                    UserId = Guid.Parse((string)row[5])
                }).FirstOrDefault();

                return loginEvent;
            }
        }

        public static List<BaseEvent> GetLoginEvents(int tenantId, Guid userId)
        {
            using (var db = GetDbManager())
            {
                var where = GetActiveConnectionsWhere(tenantId, userId);
                var query = new SqlQuery("login_events")
                    .Select("id", "substring_index(ip, ':', 1) ip", "platform", "browser", "date")
                    .Where(where)
                    .OrderBy("id", false);
                var loginInfo = db.ExecuteList(query).Select(row => new BaseEvent
                {
                    Id = (int)row[0],
                    IP = (string)row[1],
                    Platform = (string)row[2],
                    Browser = (string)row[3],
                    Date = TenantUtil.DateTimeFromUtc((DateTime)row[4])
                }).ToList();
                return loginInfo;
            }
        }

        private static Exp GetActiveConnectionsWhere(int tenantId, Guid userId)
        {
            var exp = Exp.Empty;
            exp &= Exp.Eq("tenant_id", tenantId);
            exp &= Exp.Eq("user_id", userId);
            exp &= Exp.In("action", loginActions);
            exp &= Exp.Gt("date", DateTime.UtcNow.AddYears(-1));
            exp &= Exp.Eq("active", true);
            return exp;
        }

        public static void LogOutEvent(int loginEventId)
        {
            using (var db = GetDbManager())
            {
                var query = new SqlUpdate("login_events")
                    .Set("active", false)
                    .Where("id", loginEventId);
                db.ExecuteNonQuery(query);
            }
            ResetCache();
        }

        public static void LogOutAllActiveConnections(int tenantId, Guid userId)
        {
            using (var db = GetDbManager())
            {
                var query = new SqlUpdate("login_events")
                    .Set("active", false)
                    .Where("tenant_id", tenantId)
                    .Where("user_id", userId)
                    .Where("active", true);
                db.ExecuteNonQuery(query);
            }
            ResetCache(tenantId, userId);
        }   

        public static void LogOutAllActiveConnectionsForTenant(int tenantId)
        {
            using (var db = GetDbManager())
            {
                var query = new SqlUpdate("login_events")
                    .Set("active", false)
                    .Where("tenant_id", tenantId)
                    .Where("active", true);
                db.ExecuteNonQuery(query);
            }
        }

        public static void LogOutAllActiveConnectionsExceptThis(int loginEventId, int tenantId, Guid userId)
        {
            using (var db = GetDbManager())
            {
                var query = new SqlUpdate("login_events")
                    .Set("active", false)
                    .Where("tenant_id", tenantId)
                    .Where("user_id", userId)
                    .Where(!Exp.Eq("id", loginEventId))
                    .Where("active", true);
                db.ExecuteNonQuery(query);
            }
            ResetCache(tenantId, userId);
        }

        public static void ResetCache(int tenantId, Guid userId)
        {
            var key = GetCacheKey(tenantId, userId);
            cache.Remove(key);
        }

        public static void ResetCache()
        {
            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var userId = SecurityContext.CurrentAccount.ID;
            ResetCache(tenantId, userId);
        }

        private static string GetCacheKey(int tenantId, Guid userId)
        {
            return string.Join("", guidLoginEvent, tenantId, userId);
        }

        private static IDbManager GetDbManager()
        {
            return new DbManager(dbId);
        }
    }
}
