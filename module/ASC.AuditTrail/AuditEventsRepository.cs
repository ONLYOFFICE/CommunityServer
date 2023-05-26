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
using System.Text;

using ASC.AuditTrail.Mappers;
using ASC.AuditTrail.Types;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json;

namespace ASC.AuditTrail
{
    public class AuditEventsRepository
    {
        private const string dbid = "default";
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

        public static IEnumerable<AuditEvent> GetByFilter(
            Guid? userId = null,
            ProductType? productType = null,
            ModuleType? moduleType = null,
            ActionType? actionType = null,
            MessageAction? action = null,
            EntryType? entry = null,
            string target = null,
            DateTime? from = null,
            DateTime? to = null,
            int startIndex = 0,
            int limit = 0)
        {
            var q = new SqlQuery("audit_events a")
                .Select(auditColumns.Select(x => "a." + x).ToArray())
                .LeftOuterJoin("core_user u", Exp.EqColumns("a.user_id", "u.id"))
                .Select("u.firstname", "u.lastname")
                .Where("a.tenant_id", TenantProvider.CurrentTenantID)
                .OrderBy("a.date", false);

            if (startIndex > 0)
            {
                q.SetFirstResult(startIndex);
            }
            if (limit > 0)
            {
                q.SetMaxResults(limit);
            }

            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                q.Where("a.user_id", userId.Value.ToString());
            }

            var isNeedFindEntry = entry.HasValue && entry.Value != EntryType.None && target != null;


            if (action.HasValue && action.Value != MessageAction.None)
            {
                q.Where("a.action", (int)action);
            }
            else
            {
                IEnumerable<KeyValuePair<MessageAction, MessageMaps>> actions = new List<KeyValuePair<MessageAction, MessageMaps>>();

                var isFindActionType = actionType.HasValue && actionType.Value != ActionType.None;

                if (productType.HasValue && productType.Value != ProductType.None)
                {
                    var productMapper = AuditActionMapper.Mappers.FirstOrDefault(m => m.Product == productType.Value);

                    if (productMapper != null)
                    {
                        if (moduleType.HasValue && moduleType.Value != ModuleType.None)
                        {
                            var moduleMapper = productMapper.Mappers.FirstOrDefault(m => m.Module == moduleType.Value);
                            if (moduleMapper != null)
                            {
                                actions = moduleMapper.Actions;
                            }
                        }
                        else
                        {
                            actions = productMapper.Mappers.SelectMany(r => r.Actions);
                        }
                    }
                }
                else
                {
                    actions = AuditActionMapper.Mappers
                            .SelectMany(r => r.Mappers)
                            .SelectMany(r => r.Actions);
                }

                if (isFindActionType || isNeedFindEntry)
                {
                    actions = actions
                            .Where(a => (!isFindActionType || a.Value.ActionType == actionType.Value) && (!isNeedFindEntry || (entry.Value == a.Value.EntryType1) || entry.Value == a.Value.EntryType2))
                            .ToList();
                }

                if (isNeedFindEntry)
                {
                    FindByEntry(q, entry.Value, target, actions);
                }
                else
                {
                    var keys = actions.Select(x => (int)x.Key).ToList();
                    q.Where(Exp.In("a.action", keys));
                }
            }


            var hasFromFilter = (from.HasValue && from.Value != DateTime.MinValue);
            var hasToFilter = (to.HasValue && to.Value != DateTime.MinValue);

            if (hasFromFilter || hasToFilter)
            {
                if (hasFromFilter)
                {
                    if (hasToFilter)
                    {
                        q.Where(Exp.Between("a.date", from, to));
                    }
                    else
                    {
                        q.Where(Exp.Ge("a.date", from));
                    }
                }
                else if (hasToFilter)
                {
                    q.Where(Exp.Le("a.date", to));
                }
            }

            using (var db = new DbManager(dbid))
            {
                return db
                    .ExecuteList(q)
                    .Select(ToAuditEvent)
                    .Where(x => x != null);
            }
        }

        private static void FindByEntry(SqlQuery q, EntryType entry, string target, IEnumerable<KeyValuePair<MessageAction, MessageMaps>> actions)
        {
            var sb = new StringBuilder();

            foreach (var action in actions)
            {
                if (action.Value.EntryType1 == entry)
                {
                    sb.Append(string.Format("(a.action = {0} AND SUBSTRING_INDEX(SUBSTRING_INDEX(a.target,',',{1}),',',1) = {2}) OR ", (int)action.Key, -2, target));
                }
                if (action.Value.EntryType2 == entry)
                {
                    sb.Append(string.Format("(a.action = {0} AND SUBSTRING_INDEX(SUBSTRING_INDEX(a.target,',',{1}),',',1) = {2}) OR ", (int)action.Key, -1, target));
                }
            }

            sb.Remove(sb.Length - 3, 3);
            q.Where(sb.ToString());
        }

        public static int GetCount(int tenant, DateTime? from = null, DateTime? to = null)
        {
            var q = new SqlQuery("audit_events a")
                .SelectCount()
                .Where("a.tenant_id", tenant);

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
            var firstName = Convert.ToString(row[12]);
            var lastName = Convert.ToString(row[13]);

            if (evt.UserId == Core.Configuration.Constants.CoreSystem.ID)
            {
                evt.UserName = AuditReportResource.SystemAccount;
            }
            else if (evt.UserId == Core.Configuration.Constants.Guest.ID)
            {
                evt.UserName = AuditReportResource.GuestAccount;
            }
            else if (!(string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)))
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
            else
            {
                evt.UserName = evt.Initiator ?? AuditReportResource.UnknownAccount;
            }

            var map = AuditActionMapper.GetMessageMaps(evt.Action);
            if (map != null)
            {
                evt.ActionText = map.GetActionText(evt);
                evt.ActionTypeText = map.GetActionTypeText();
                evt.Product = map.GetProductText();
                evt.Module = map.GetModuleText();
            }

            return evt;
        }
    }
}