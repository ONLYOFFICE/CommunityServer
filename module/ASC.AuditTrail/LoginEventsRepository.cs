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
using Newtonsoft.Json;

namespace ASC.AuditTrail.Data
{
    public class LoginEventsRepository
    {
        private const string auditDbId = "core";

        private static readonly List<string> auditColumns =
            new List<string>
                {
                    "id",
                    "ip",
                    "login",
                    "browser",
                    "platform",
                    "date",
                    "tenant_id",
                    "user_id",
                    "page",
                    "action",
                    "description"
                };

        public static IEnumerable<LoginEvent> GetLast(int tenant, int chunk)
        {
            var q = new SqlQuery("login_events au")
                .Select(auditColumns.Select(x => "au." + x).ToArray())
                .LeftOuterJoin("core_user u", Exp.EqColumns("au.user_id", "u.id"))
                .Select("u.firstname", "u.lastname")
                .Where("au.tenant_id", tenant)
                .OrderBy("au.date", false)
                .SetMaxResults(chunk);

            using (var db = new DbManager(auditDbId))
            {
                return db.ExecuteList(q).Select(ToLoginEvent).Where(x => x != null);
            }
        }

        public static IEnumerable<LoginEvent> Get(int tenant, DateTime from, DateTime to)
        {
            var q = new SqlQuery("login_events au")
                .Select(auditColumns.Select(x => "au." + x).ToArray())
                .LeftOuterJoin("core_user u", Exp.EqColumns("au.user_id", "u.id"))
                .Select("u.firstname", "u.lastname")
                .Where("au.tenant_id", tenant)
                .Where(Exp.Between("au.date", from, to))
                .OrderBy("au.date", false);

            using (var db = new DbManager(auditDbId))
            {
                return db.ExecuteList(q).Select(ToLoginEvent).Where(x => x != null);
            }
        }

        public static int GetCount(int tenant, DateTime? from = null, DateTime? to = null)
        {
            var q = new SqlQuery("login_events")
                .SelectCount()
                .Where("tenant_id", tenant);

            if (from.HasValue && to.HasValue)
            {
                q.Where(Exp.Between("date", from.Value, to.Value));
            }

            using (var db = new DbManager(auditDbId))
            {
                return db.ExecuteScalar<int>(q);
            }
        }

        private static LoginEvent ToLoginEvent(object[] row)
        {
            try
            {
                var evt = new LoginEvent
                    {
                        Id = Convert.ToInt32(row[0]),
                        IP = Convert.ToString(row[1]),
                        Login = Convert.ToString(row[2]),
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
                        new JsonSerializerSettings
                            {
                                DateTimeZoneHandling = DateTimeZoneHandling.Utc
                            });
                }
                evt.UserName = (row[11] != null && row[12] != null)
                                   ? UserFormatter.GetUserName(Convert.ToString(row[11]), Convert.ToString(row[12]))
                                   : !string.IsNullOrWhiteSpace(evt.Login)
                                         ? evt.Login
                                         : evt.UserId == Core.Configuration.Constants.Guest.ID
                                               ? AuditReportResource.GuestAccount
                                               : AuditReportResource.UnknownAccount;

                evt.ActionText = AuditActionMapper.GetActionText(evt);

                return evt;
            }
            catch(Exception)
            {
                //log.Error("Error while forming event from db: " + ex);
                return null;
            }
        }
    }
}