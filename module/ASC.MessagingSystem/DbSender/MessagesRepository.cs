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
using System.Linq;
using System.Threading;

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core.Tenants;

using Newtonsoft.Json;

using UAParser;

using IsolationLevel = System.Data.IsolationLevel;

namespace ASC.MessagingSystem.DbSender
{
    internal class MessagesRepository
    {
        private const string MessagesDbId = "default";
        private static DateTime lastSave = DateTime.UtcNow;
        private static readonly TimeSpan CacheTime;
        private static readonly IDictionary<string, EventMessage> Cache;
        private static Parser Parser { get; set; }
        private static readonly Timer Timer;
        private static bool timerStarted;

        private const string LoginEventsTable = "login_events";
        private const string AuditEventsTable = "audit_events";
        private static readonly Timer ClearTimer;

        static MessagesRepository()
        {
            CacheTime = TimeSpan.FromMinutes(1);
            Cache = new Dictionary<string, EventMessage>();
            Parser = Parser.GetDefault();
            Timer = new Timer(FlushCache);
            timerStarted = false;

            ClearTimer = new Timer(DeleteOldEvents);
            ClearTimer.Change(new TimeSpan(0), TimeSpan.FromDays(1));
        }

        public static void Add(EventMessage message)
        {
            // messages with action code < 2000 are related to login-history
            if ((int)message.Action < 2000)
            {
                using (var db = DbManager.FromHttpContext(MessagesDbId))
                {
                    AddLoginEvent(message, db);
                }
                return;
            }

            var now = DateTime.UtcNow;
            var key = string.Format("{0}|{1}|{2}|{3}", message.TenantId, message.UserId, message.Id, now.Ticks);

            lock (Cache)
            {
                Cache[key] = message;

                if (!timerStarted)
                {
                    Timer.Change(0, 100);
                    timerStarted = true;
                }
            }

        }

        private static void FlushCache(object state)
        {
            List<EventMessage> events = null;

            if (CacheTime < DateTime.UtcNow - lastSave || Cache.Count > 100)
            {
                lock (Cache)
                {
                    Timer.Change(-1, -1);
                    timerStarted = false;

                    events = new List<EventMessage>(Cache.Values);
                    Cache.Clear();
                    lastSave = DateTime.UtcNow;
                }
            }

            if (events == null) return;

            using (var db = DbManager.FromHttpContext(MessagesDbId))
            using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                var dict = new Dictionary<string, ClientInfo>();

                foreach (var message in events)
                {
                    if (!string.IsNullOrEmpty(message.UAHeader))
                    {
                        try
                        {

                            ClientInfo clientInfo;

                            if (dict.ContainsKey(message.UAHeader))
                            {
                                clientInfo = dict[message.UAHeader];
                            }
                            else
                            {
                                clientInfo = Parser.Parse(message.UAHeader);
                                dict.Add(message.UAHeader, clientInfo);
                            }

                            if (clientInfo != null)
                            {
                                message.Browser = GetBrowser(clientInfo);
                                message.Platform = GetPlatform(clientInfo);
                            }
                        }
                        catch (Exception e)
                        {
                            LogManager.GetLogger("ASC").Error("FlushCache " + message.Id, e);
                        }
                    }

                    // messages with action code < 2000 are related to login-history
                    if ((int)message.Action >= 2000)
                    {
                        AddAuditEvent(message, db);
                    }
                }

                tx.Commit();
            }
        }

        private static void AddLoginEvent(EventMessage message, IDbManager dbManager)
        {
            var i = new SqlInsert("login_events")
                .InColumnValue("ip", message.IP)
                .InColumnValue("login", message.Initiator)
                .InColumnValue("browser", message.Browser)
                .InColumnValue("platform", message.Platform)
                .InColumnValue("date", message.Date)
                .InColumnValue("tenant_id", message.TenantId)
                .InColumnValue("user_id", message.UserId)
                .InColumnValue("page", message.Page)
                .InColumnValue("action", message.Action);

            if (message.Description != null && message.Description.Any())
            {
                i = i.InColumnValue("description",
                    JsonConvert.SerializeObject(message.Description, new JsonSerializerSettings
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    }));
            }

            dbManager.ExecuteNonQuery(i);
        }

        private static void AddAuditEvent(EventMessage message, IDbManager dbManager)
        {
            var i = new SqlInsert("audit_events")
                .InColumnValue("ip", message.IP)
                .InColumnValue("initiator", message.Initiator)
                .InColumnValue("browser", message.Browser)
                .InColumnValue("platform", message.Platform)
                .InColumnValue("date", message.Date)
                .InColumnValue("tenant_id", message.TenantId)
                .InColumnValue("user_id", message.UserId)
                .InColumnValue("page", message.Page)
                .InColumnValue("action", message.Action);

            if (message.Description != null && message.Description.Any())
            {
                i = i.InColumnValue("description",
                    JsonConvert.SerializeObject(GetSafeDescription(message.Description), new JsonSerializerSettings
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    }));
            }

            i.InColumnValue("target", message.Target == null ? null : message.Target.ToString());

            dbManager.ExecuteNonQuery(i);
        }

        private static IList<string> GetSafeDescription(IEnumerable<string> description)
        {
            const int maxLength = 15000;

            var currentLength = 0;
            var safe = new List<string>();

            foreach (var d in description.Where(r => r != null))
            {
                if (currentLength + d.Length <= maxLength)
                {
                    currentLength += d.Length;
                    safe.Add(d);
                }
                else
                {
                    safe.Add(d.Substring(0, maxLength - currentLength - 3) + "...");
                    break;
                }
            }

            return safe;
        }

        private static string GetBrowser(ClientInfo clientInfo)
        {
            return clientInfo == null
                       ? null
                       : string.Format("{0} {1}", clientInfo.UA.Family, clientInfo.UA.Major);
        }

        private static string GetPlatform(ClientInfo clientInfo)
        {
            return clientInfo == null
                       ? null
                       : string.Format("{0} {1}", clientInfo.OS.Family, clientInfo.OS.Major);
        }


        private static void DeleteOldEvents(object state)
        {
            try
            {
                GetOldEvents(LoginEventsTable, "LoginHistoryLifeTime");
                GetOldEvents(AuditEventsTable, "AuditTrailLifeTime");
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Messaging").Error(ex.Message, ex);
            }
        }

        private static void GetOldEvents(string table, string settings)
        {
            var sqlQueryLimit = string.Format("(IFNULL((SELECT JSON_EXTRACT(`Data`, '$.{0}') from webstudio_settings where tt.id = TenantID and id='{1}'), {2})) as tout", settings, TenantAuditSettings.LoadForDefaultTenant().ID, TenantAuditSettings.MaxLifeTime);
            var query = new SqlQuery(table + " t1")
                .Select("t1.id")
                .Select(sqlQueryLimit)
                .Select("t1.`date` AS dout")
                .InnerJoin("tenants_tenants tt", Exp.EqColumns("tt.id", "t1.tenant_id"))
                .Having(Exp.Sql("dout < ADDDATE(UTC_DATE(), INTERVAL -tout DAY)"))
                .SetMaxResults(1000);

            List<int> ids;

            do
            {
                using (var dbManager = new DbManager(MessagesDbId, 180000))
                {
                    ids = dbManager.ExecuteList(query).ConvertAll(r => Convert.ToInt32(r[0]));

                    if (!ids.Any()) return;

                    var deleteQuery = new SqlDelete(table).Where(Exp.In("id", ids));

                    dbManager.ExecuteNonQuery(deleteQuery);
                }
            } while (ids.Any());
        }
    }
}