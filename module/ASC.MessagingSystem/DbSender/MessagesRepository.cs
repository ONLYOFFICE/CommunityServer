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
using System.Configuration;
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
        private DateTime lastSave = DateTime.UtcNow;
        private readonly TimeSpan CacheTime;
        private readonly int CacheLimit;
        private readonly IDictionary<string, EventMessage> Cache;
        private readonly Timer Timer;
        private bool timerStarted;

        private const string LoginEventsTable = "login_events";
        private const string AuditEventsTable = "audit_events";
        private readonly Timer ClearTimer;

        private static readonly ILog log = LogManager.GetLogger("ASC.Messaging");


        public MessagesRepository()
        {
            Cache = new Dictionary<string, EventMessage>();
            Timer = new Timer(FlushCache);
            timerStarted = false;

            ClearTimer = new Timer(DeleteOldEvents);
            ClearTimer.Change(new TimeSpan(0), TimeSpan.FromDays(1));

            var minutes = ConfigurationManagerExtension.AppSettings["messaging.CacheTimeFromMinutes"];
            var limit = ConfigurationManagerExtension.AppSettings["messaging.CacheLimit"];

            CacheTime = Int32.TryParse(minutes, out var cacheTime) ? TimeSpan.FromMinutes(cacheTime) : TimeSpan.FromMinutes(1);
            CacheLimit = Int32.TryParse(limit, out var cacheLimit) ? cacheLimit : 100;
        }

        ~MessagesRepository()
        {
            FlushCache(true);
        }

        private bool ForseSave(EventMessage message)
        {
            // messages with action code < 2000 are related to login-history
            if ((int)message.Action < 2000) return true;

            return message.Action == MessageAction.UserSentPasswordChangeInstructions;
        }


        public int Add(EventMessage message)
        {
            if (ForseSave(message))
            {
                int id = 0;
                if (!string.IsNullOrEmpty(message.UAHeader))
                {
                    try
                    {
                        MessageSettings.AddInfoMessage(message);
                    }
                    catch (Exception e)
                    {
                        LogManager.GetLogger("ASC").Error("Add " + message.Id, e);
                    }
                }
                using (var db = new DbManager(MessagesDbId))
                {
                    if ((int)message.Action < 2000)
                    {
                        id = AddLoginEvent(message, db);
                    }
                    else
                    {
                        id = AddAuditEvent(message, db);
                    }
                }
                return id;
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
            return 0;
        }

        private void FlushCache(object state)
        {
            FlushCache(false);
        }

        private void FlushCache(bool isDisposed = false)
        {
            List<EventMessage> events = null;

            if (DateTime.UtcNow > lastSave.Add(CacheTime) || Cache.Count > CacheLimit || isDisposed)
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

            using (var db = new DbManager(MessagesDbId))
            using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                var dict = new Dictionary<string, ClientInfo>();

                foreach (var message in events)
                {
                    if (!string.IsNullOrEmpty(message.UAHeader))
                    {
                        try
                        {
                            MessageSettings.AddInfoMessage(message, dict);
                        }
                        catch (Exception e)
                        {
                            LogManager.GetLogger("ASC").Error("FlushCache " + message.Id, e);
                        }
                    }

                    if (!ForseSave(message))
                    {
                        // messages with action code < 2000 are related to login-history
                        if ((int)message.Action < 2000)
                            AddLoginEvent(message, db);
                        else
                            AddAuditEvent(message, db);
                    }
                }

                tx.Commit();
            }
        }

        private int AddLoginEvent(EventMessage message, IDbManager dbManager)
        {
            var i = new SqlInsert("login_events")
                .InColumnValue("id", 0)
                .InColumnValue("ip", message.IP)
                .InColumnValue("login", message.Initiator)
                .InColumnValue("browser", message.Browser)
                .InColumnValue("platform", message.Platform)
                .InColumnValue("date", message.Date)
                .InColumnValue("tenant_id", message.TenantId)
                .InColumnValue("user_id", message.UserId)
                .InColumnValue("page", message.Page)
                .InColumnValue("action", message.Action)
                .InColumnValue("active", message.Active);

            if (message.Description != null && message.Description.Any())
            {
                i = i.InColumnValue("description",
                    JsonConvert.SerializeObject(message.Description, new JsonSerializerSettings
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    }));
            }

            i = i.Identity(0, 0, true);

            var id = dbManager.ExecuteScalar<int>(i);

            message.Id = id;

            log.TraceFormat("login event - {0}", JsonConvert.SerializeObject(message));

            return id;
        }

        private int AddAuditEvent(EventMessage message, IDbManager dbManager)
        {
            var i = new SqlInsert("audit_events")
                .InColumnValue("id", 0)
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

            i = i.Identity(0, 0, true);

            var id = dbManager.ExecuteScalar<int>(i);

            message.Id = id;

            log.TraceFormat("audit event - {0}", JsonConvert.SerializeObject(message));

            return id;
        }

        private IList<string> GetSafeDescription(IEnumerable<string> description)
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

        private void DeleteOldEvents(object state)
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

        private void GetOldEvents(string table, string settings)
        {
            var sqlQueryLimit = string.Format("(IFNULL((SELECT JSON_EXTRACT(`Data`, '$.{0}') from webstudio_settings where tt.id = TenantID and id='{1}'), {2})) as tout", settings, new TenantAuditSettings().ID, TenantAuditSettings.MaxLifeTime);
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