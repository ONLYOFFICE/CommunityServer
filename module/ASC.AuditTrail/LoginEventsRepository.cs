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

using ASC.AuditTrail.Mappers;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json;

namespace ASC.AuditTrail.Data
{
    public class LoginEventsRepository
    {
        private const string auditDbId = "default";

        private static readonly List<string> auditColumns = new List<string>
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

        private static IDbManager GetDbManager()
        {
            return new DbManager(auditDbId);
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

            using (var db = GetDbManager())
            {
                return db.ExecuteScalar<int>(q);
            }
        }

        public static IEnumerable<LoginEvent> GetByFilter(
            Guid? login = null,
            MessageAction? action = null,
            DateTime? from = null,
            DateTime? to = null,
            int startIndex = 0,
            int limit = 0)
        {
            var q = new SqlQuery("login_events l")
                .Select(auditColumns.Select(x => "l." + x).ToArray())
                .LeftOuterJoin("core_user u", Exp.EqColumns("l.user_id", "u.id"))
                .Select("u.firstname", "u.lastname")
                .Where("l.tenant_id", TenantProvider.CurrentTenantID)
                .OrderBy("l.date", false);

            if (startIndex > 0)
            {
                q.SetFirstResult(startIndex);
            }
            if (limit > 0)
            {
                q.SetMaxResults(limit);
            }

            if (login.HasValue && login.Value != Guid.Empty)
            {
                q.Where("l.user_id", login.Value.ToString());
            }

            if (action.HasValue && action.Value != MessageAction.None)
            {
                q.Where("l.action", (int)action);
            }

            var hasFromFilter = (from.HasValue && from.Value != DateTime.MinValue);
            var hasToFilter = (to.HasValue && to.Value != DateTime.MinValue);

            if (hasFromFilter || hasToFilter)
            {
                if (hasFromFilter)
                {
                    if (hasToFilter)
                    {
                        q.Where(Exp.Between("l.date", from, to));
                    }
                    else
                    {
                        q.Where(Exp.Ge("l.date", from));
                    }
                }
                else if (hasToFilter)
                {
                    q.Where(Exp.Le("l.date", to));
                }
            }

            using (var db = GetDbManager())
            {
                return db
                    .ExecuteList(q)
                    .Select(ToLoginEvent)
                    .Where(x => x != null);
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

                var firstName = Convert.ToString(row[11]);
                var lastName = Convert.ToString(row[12]);

                if (!(string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)))
                {
                    evt.UserName = UserFormatter.GetUserName(firstName, lastName);
                }
                else if (!string.IsNullOrEmpty(firstName))
                {
                    evt.UserName = firstName;
                }
                else if (!string.IsNullOrEmpty(lastName))
                {
                    evt.UserName = lastName;
                }
                else if (!string.IsNullOrWhiteSpace(evt.Login))
                {
                    evt.UserName = evt.Login;
                }
                else if (evt.UserId == Core.Configuration.Constants.Guest.ID)
                {
                    evt.UserName = AuditReportResource.GuestAccount;
                }
                else
                {
                    evt.UserName = AuditReportResource.UnknownAccount;
                }

                evt.ActionText = AuditActionMapper.GetMessageMaps(evt.Action).GetActionText(evt);

                return evt;
            }
            catch (Exception)
            {
                //log.Error("Error while forming event from db: " + ex);
                return null;
            }
        }
    }
}