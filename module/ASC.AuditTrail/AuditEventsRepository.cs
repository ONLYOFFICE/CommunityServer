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
using ASC.AuditTrail.Mappers;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using Newtonsoft.Json;

namespace ASC.AuditTrail
{
    public class AuditEventsRepository
    {
        private const string dbid = "core";
        private static readonly string[] auditColumns = new[]
                {
                    "id",
                    "ip",
                    "initiator",
                    "browser",
                    "platform",
                    "date",
                    "tenant_id",
                    "user_id",
                    "page",
                    "action",
                    "description",
                    "target"
                };


        public static IEnumerable<AuditEvent> GetLast(int tenant, int chunk)
        {
            return Get(tenant, null, null, chunk);
        }

        public static IEnumerable<AuditEvent> Get(int tenant, DateTime from, DateTime to)
        {
            return Get(tenant, from, to, null);
        }

        private static IEnumerable<AuditEvent> Get(int tenant, DateTime? from, DateTime? to, int? limit)
        {
            var q = new SqlQuery("audit_events a")
                .Select(auditColumns.Select(x => "a." + x).ToArray())
                .LeftOuterJoin("core_user u", Exp.EqColumns("a.user_id", "u.id"))
                .Select("u.firstname", "u.lastname")
                .Where("a.tenant_id", tenant)
                .OrderBy("a.date", false);

            if (from.HasValue && to.HasValue)
            {
                q.Where(Exp.Between("a.date", from.Value, to.Value));
            }
            if (limit.HasValue)
            {
                q.SetMaxResults(limit.Value);
            }

            using (var db = new DbManager(dbid))
            {
                return db.ExecuteList(q)
                    .Select(ToAuditEvent)
                    .Where(x => x != null)
                    .ToList();
            }
        }

        public static int GetCount(int tenant, DateTime? from = null, DateTime? to = null)
        {
            var q = new SqlQuery("audit_events a")
                .SelectCount()
                .Where("a.tenant_id", tenant)
                .OrderBy("a.date", false);

            if (from.HasValue && to.HasValue)
            {
                q.Where(Exp.Between("a.date", from.Value, to.Value));
            }

            using (var db = new DbManager(dbid))
            {
                return db.ExecuteScalar<int>(q);
            }
        }

        private static AuditEvent ToAuditEvent(object[] row)
        {
            try
            {
                var evt = new AuditEvent
                    {
                        Id = Convert.ToInt32(row[0]),
                        IP = Convert.ToString(row[1]),
                        Initiator = Convert.ToString(row[2]),
                        Browser = Convert.ToString(row[3]),
                        Platform = Convert.ToString(row[4]),
                        Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[5])),
                        TenantId = Convert.ToInt32(row[6]),
                        UserId = Guid.Parse(Convert.ToString(row[7])),
                        Page = Convert.ToString(row[8]),
                        Action = Convert.ToInt32(row[9])
                    };

                if (row[10] != null)
                {
                    evt.Description = JsonConvert.DeserializeObject<IList<string>>(
                        Convert.ToString(row[10]),
                        new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
                }

                evt.Target = MessageTarget.Parse(Convert.ToString(row[11]));

                evt.UserName = (row[12] != null && row[13] != null) ? UserFormatter.GetUserName(Convert.ToString(row[12]), Convert.ToString(row[13])) :
                    evt.UserId == Core.Configuration.Constants.CoreSystem.ID ? AuditReportResource.SystemAccount :
                        evt.UserId == Core.Configuration.Constants.Guest.ID ? AuditReportResource.GuestAccount : 
                            evt.Initiator ?? AuditReportResource.UnknownAccount;

                evt.ActionText = AuditActionMapper.GetActionText(evt);
                evt.ActionTypeText = AuditActionMapper.GetActionTypeText(evt);
                evt.Product = AuditActionMapper.GetProductText(evt);
                evt.Module = AuditActionMapper.GetModuleText(evt);

                return evt;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}