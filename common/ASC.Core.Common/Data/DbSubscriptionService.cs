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
using System.Configuration;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;

namespace ASC.Core.Data
{
    public class DbSubscriptionService : DbBaseService, ISubscriptionService
    {
        public DbSubscriptionService(ConnectionStringSettings connectionString)
            : base(connectionString, "tenant")
        {

        }


        public IEnumerable<SubscriptionRecord> GetSubscriptions(int tenant, string sourceId, string actionId)
        {
            if (sourceId == null) throw new ArgumentNullException("sourceId");
            if (actionId == null) throw new ArgumentNullException("actionId");

            var q = GetQuery(tenant, sourceId, actionId);
            return GetSubscriptions(q, tenant);
        }

        public IEnumerable<SubscriptionRecord> GetSubscriptions(int tenant, string sourceId, string actionId, string recipientId, string objectId)
        {
            var q = GetQuery(tenant, sourceId, actionId);
            if (!string.IsNullOrEmpty(recipientId))
            {
                q.Where("recipient", recipientId);
            }
            else
            {
                q.Where("object", objectId ?? string.Empty);
            }

            return GetSubscriptions(q, tenant);
        }

        public SubscriptionRecord GetSubscription(int tenant, string sourceId, string actionId, string recipientId, string objectId)
        {
            if (recipientId == null) throw new ArgumentNullException("recipientId");

            var q = GetQuery(tenant, sourceId, actionId)
                .Where("recipient", recipientId)
                .Where("object", objectId ?? string.Empty);

            return GetSubscriptions(q, tenant).FirstOrDefault();
        }

        public void SaveSubscription(SubscriptionRecord s)
        {
            if (s == null) throw new ArgumentNullException("s");

            var i = Insert("core_subscription", s.Tenant)
                .InColumnValue("source", s.SourceId)
                .InColumnValue("action", s.ActionId)
                .InColumnValue("recipient", s.RecipientId)
                .InColumnValue("object", s.ObjectId ?? string.Empty)
                .InColumnValue("unsubscribed", !s.Subscribed);

            ExecNonQuery(i);
        }

        public void RemoveSubscriptions(int tenant, string sourceId, string actionId)
        {
            RemoveSubscriptions(tenant, sourceId, actionId, string.Empty);
        }

        public void RemoveSubscriptions(int tenant, string sourceId, string actionId, string objectId)
        {
            if (sourceId == null) throw new ArgumentNullException("sourceId");
            if (actionId == null) throw new ArgumentNullException("actionId");

            var d = Delete("core_subscription", tenant).Where("source", sourceId).Where("action", actionId);
            if (objectId != string.Empty)
            {
                d.Where("object", objectId ?? string.Empty);
            }
            ExecNonQuery(d);
        }


        public IEnumerable<SubscriptionMethod> GetSubscriptionMethods(int tenant, string sourceId, string actionId, string recipientId)
        {
            if (sourceId == null) throw new ArgumentNullException("sourceId");
            if (actionId == null) throw new ArgumentNullException("actionId");

            var q = new SqlQuery("core_subscriptionmethod")
                .Select("tenant", "recipient", "sender", "action")
                .Where(Exp.Eq("tenant", -1) | Exp.Eq("tenant", tenant))
                .Where("source", sourceId)
                .GroupBy("recipient", "action")
                .OrderBy("tenant", true);

            if (recipientId != null) q.Where("recipient", recipientId);

            var methods = ExecList(q)
                .ConvertAll(r => new SubscriptionMethod
                                     {
                                         Tenant = Convert.ToInt32(r[0]),
                                         SourceId = sourceId,
                                         ActionId = Convert.ToString(r[3]),
                                         RecipientId = (string)r[1],
                                         Methods = Convert.ToString(r[2]).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries),
                                     });

            var result = methods.ToList();
            var common = new Dictionary<string, SubscriptionMethod>();
            foreach (var m in methods)
            {
                var key = m.SourceId + m.ActionId + m.RecipientId;
                if (m.Tenant == Tenant.DEFAULT_TENANT)
                {
                    m.Tenant = tenant;
                    common.Add(key, m);
                }
                else
                {
                    SubscriptionMethod r;
                    if (common.TryGetValue(key, out r))
                    {
                        result.Remove(r);
                    }
                }
            }
            return result;
        }

        public void SetSubscriptionMethod(SubscriptionMethod m)
        {
            if (m == null) throw new ArgumentNullException("m");

            ISqlInstruction i = null;
            if (m.Methods == null || m.Methods.Length == 0)
            {
                i = Delete("core_subscriptionmethod", m.Tenant)
                    .Where("source", m.SourceId)
                    .Where("action", m.ActionId)
                    .Where("recipient", m.RecipientId);
            }
            else
            {
                i = Insert("core_subscriptionmethod", m.Tenant)
                    .InColumnValue("source", m.SourceId)
                    .InColumnValue("action", m.ActionId)
                    .InColumnValue("recipient", m.RecipientId)
                    .InColumnValue("sender", string.Join("|", m.Methods));
            }
            ExecNonQuery(i);
        }


        private SqlQuery GetQuery(int tenant, string sourceId, string actionId)
        {
            if (sourceId == null) throw new ArgumentNullException("sourceId");
            if (actionId == null) throw new ArgumentNullException("actionId");

            return new SqlQuery("core_subscription")
                .Select("tenant", "source", "action", "recipient", "object", "unsubscribed")
                .Where(Exp.Eq("tenant", -1) | Exp.Eq("tenant", tenant))
                .Where("source", sourceId)
                .Where("action", actionId)
                .OrderBy("tenant", true);
        }

        private IEnumerable<SubscriptionRecord> GetSubscriptions(ISqlInstruction q, int tenant)
        {
            var subs = ExecList(q)
                .ConvertAll(r => new SubscriptionRecord
                {
                    Tenant = Convert.ToInt32(r[0]),
                    SourceId = (string)r[1],
                    ActionId = (string)r[2],
                    RecipientId = (string)r[3],
                    ObjectId = string.Empty.Equals(r[4]) ? null : (string)r[4],
                    Subscribed = !Convert.ToBoolean(r[5]),
                });

            var result = subs.ToList();
            var common = new Dictionary<string, SubscriptionRecord>();
            foreach (var s in subs)
            {
                var key = s.SourceId + s.ActionId + s.RecipientId + s.ObjectId;
                if (s.Tenant == Tenant.DEFAULT_TENANT)
                {
                    s.Tenant = tenant;
                    common.Add(key, s);
                }
                else
                {
                    SubscriptionRecord r;
                    if (common.TryGetValue(key, out r))
                    {
                        result.Remove(r);
                    }
                }
            }
            return result;
        }
    }
}
