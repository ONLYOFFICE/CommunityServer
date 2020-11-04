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


using ASC.Common.Caching;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Core.Caching
{
    public class CachedSubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionService service;
        private readonly ICache cache;
        private readonly ICacheNotify notify;


        public TimeSpan CacheExpiration
        {
            get;
            set;
        }


        public CachedSubscriptionService(ISubscriptionService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            this.service = service;
            cache = AscCache.Memory;
            notify = AscCache.Notify;
            CacheExpiration = TimeSpan.FromMinutes(5);

            notify.Subscribe<SubscriptionRecord>((s, a) =>
            {
                var store = GetSubsciptionsStore(s.Tenant, s.SourceId, s.ActionId);
                lock (store)
                {
                    if (a == CacheNotifyAction.InsertOrUpdate)
                    {
                        store.SaveSubscription(s);
                    }
                    else if (a == CacheNotifyAction.Remove)
                    {
                        if (s.ObjectId == null)
                        {
                            store.RemoveSubscriptions();
                        }
                        else
                        {
                            store.RemoveSubscriptions(s.ObjectId);
                        }
                    }
                }
            });
            notify.Subscribe<SubscriptionMethod>((m, a) =>
            {
                var store = GetSubsciptionsStore(m.Tenant, m.SourceId, m.ActionId);
                lock (store)
                {
                    store.SetSubscriptionMethod(m);
                }
            });
        }


        public IEnumerable<SubscriptionRecord> GetSubscriptions(int tenant, string sourceId, string actionId)
        {
            var store = GetSubsciptionsStore(tenant, sourceId, actionId);
            lock (store)
            {
                return store.GetSubscriptions();
            }
        }

        public IEnumerable<SubscriptionRecord> GetSubscriptions(int tenant, string sourceId, string actionId, string recipientId, string objectId)
        {
            var store = GetSubsciptionsStore(tenant, sourceId, actionId);
            lock (store)
            {
                return store.GetSubscriptions(recipientId, objectId);
            }
        }

        public SubscriptionRecord GetSubscription(int tenant, string sourceId, string actionId, string recipientId, string objectId)
        {
            var store = GetSubsciptionsStore(tenant, sourceId, actionId);
            lock (store)
            {
                return store.GetSubscription(recipientId, objectId);
            }
        }

        public void SaveSubscription(SubscriptionRecord s)
        {
            service.SaveSubscription(s);
            notify.Publish(s, CacheNotifyAction.InsertOrUpdate);
        }

        public void RemoveSubscriptions(int tenant, string sourceId, string actionId)
        {
            service.RemoveSubscriptions(tenant, sourceId, actionId);
            notify.Publish(new SubscriptionRecord { Tenant = tenant, SourceId = sourceId, ActionId = actionId }, CacheNotifyAction.Remove);
        }

        public void RemoveSubscriptions(int tenant, string sourceId, string actionId, string objectId)
        {
            service.RemoveSubscriptions(tenant, sourceId, actionId, objectId);
            notify.Publish(new SubscriptionRecord { Tenant = tenant, SourceId = sourceId, ActionId = actionId, ObjectId = objectId }, CacheNotifyAction.Remove);
        }

        public IEnumerable<SubscriptionMethod> GetSubscriptionMethods(int tenant, string sourceId, string actionId, string recipientId)
        {
            var store = GetSubsciptionsStore(tenant, sourceId, actionId);
            lock (store)
            {
                return store.GetSubscriptionMethods(recipientId);
            }
        }

        public void SetSubscriptionMethod(SubscriptionMethod m)
        {
            service.SetSubscriptionMethod(m);
            notify.Publish(m, CacheNotifyAction.Any);
        }


        private SubsciptionsStore GetSubsciptionsStore(int tenant, string sourceId, string actionId)
        {
            var key = string.Format("sub/{0}/{1}/{2}", tenant, sourceId, actionId);
            var store = cache.Get<SubsciptionsStore>(key);
            if (store == null)
            {
                var records = service.GetSubscriptions(tenant, sourceId, actionId);
                var methods = service.GetSubscriptionMethods(tenant, sourceId, actionId, null);
                cache.Insert(key, store = new SubsciptionsStore(records, methods), DateTime.UtcNow.Add(CacheExpiration));
            }
            return store;
        }


        private class SubsciptionsStore
        {
            private readonly List<SubscriptionRecord> records;
            private IDictionary<string, List<SubscriptionRecord>> recordsByRec;
            private IDictionary<string, List<SubscriptionRecord>> recordsByObj;

            private readonly List<SubscriptionMethod> methods;
            private IDictionary<string, List<SubscriptionMethod>> methodsByRec;

            public SubsciptionsStore(IEnumerable<SubscriptionRecord> records, IEnumerable<SubscriptionMethod> methods)
            {
                this.records = records.ToList();
                this.methods = methods.ToList();
                BuildSubscriptionsIndex(records);
                BuildMethodsIndex(methods);
            }

            public IEnumerable<SubscriptionRecord> GetSubscriptions()
            {
                return records.ToList();
            }

            public IEnumerable<SubscriptionRecord> GetSubscriptions(string recipientId, string objectId)
            {
                return recipientId != null ?
                    recordsByRec.ContainsKey(recipientId) ? recordsByRec[recipientId].ToList() : new List<SubscriptionRecord>() :
                    recordsByObj.ContainsKey(objectId ?? string.Empty) ? recordsByObj[objectId ?? string.Empty].ToList() : new List<SubscriptionRecord>();
            }

            public SubscriptionRecord GetSubscription(string recipientId, string objectId)
            {
                return recordsByRec.ContainsKey(recipientId) ?
                    recordsByRec[recipientId].Where(s => s.ObjectId == objectId).FirstOrDefault() :
                    null;
            }

            public void SaveSubscription(SubscriptionRecord s)
            {
                var old = GetSubscription(s.RecipientId, s.ObjectId);
                if (old != null)
                {
                    old.Subscribed = s.Subscribed;
                }
                else
                {
                    records.Add(s);
                    BuildSubscriptionsIndex(records);
                }
            }

            public void RemoveSubscriptions()
            {
                records.Clear();
                BuildSubscriptionsIndex(records);
            }

            public void RemoveSubscriptions(string objectId)
            {
                records.RemoveAll(s => s.ObjectId == objectId);
                BuildSubscriptionsIndex(records);
            }

            public IEnumerable<SubscriptionMethod> GetSubscriptionMethods(string recipientId)
            {
                return string.IsNullOrEmpty(recipientId) ?
                    methods.ToList() :
                    methodsByRec.ContainsKey(recipientId) ? methodsByRec[recipientId].ToList() : new List<SubscriptionMethod>();
            }

            public void SetSubscriptionMethod(SubscriptionMethod m)
            {
                methods.RemoveAll(r => r.Tenant == m.Tenant && r.SourceId == m.SourceId && r.ActionId == m.ActionId && r.RecipientId == m.RecipientId);
                if (m.Methods != null && 0 < m.Methods.Length)
                {
                    methods.Add(m);
                }
                BuildMethodsIndex(methods);
            }

            private void BuildSubscriptionsIndex(IEnumerable<SubscriptionRecord> records)
            {
                recordsByRec = records.GroupBy(r => r.RecipientId).ToDictionary(g => g.Key, g => g.ToList());
                recordsByObj = records.GroupBy(r => r.ObjectId ?? string.Empty).ToDictionary(g => g.Key, g => g.ToList());
            }

            private void BuildMethodsIndex(IEnumerable<SubscriptionMethod> methods)
            {
                methodsByRec = methods.GroupBy(r => r.RecipientId).ToDictionary(g => g.Key, g => g.ToList());
            }
        }
    }
}
