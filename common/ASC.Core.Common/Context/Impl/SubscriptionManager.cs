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
using ASC.Common.Security.Authorizing;

namespace ASC.Core
{
    public class SubscriptionManager
    {
        private readonly ISubscriptionService service;
        public static readonly ICache Cache;
        public static readonly object CacheLocker;
        public static readonly List<Guid> Groups;

        static SubscriptionManager()
        {
            Cache = AscCache.Default;
            CacheLocker = new object();
            Groups = new List<Guid>
            {
                Constants.Admin.ID,
                Constants.Everyone.ID,
                Constants.User.ID
            };
        }

        public SubscriptionManager(ISubscriptionService service)
        {
            if (service == null) throw new ArgumentNullException("subscriptionManager");
            this.service = service;
        }


        public void Subscribe(string sourceID, string actionID, string objectID, string recipientID)
        {
            var s = new SubscriptionRecord
            {
                Tenant = GetTenant(),
                SourceId = sourceID,
                ActionId = actionID,
                RecipientId = recipientID,
                ObjectId = objectID,
                Subscribed = true,
            };
            service.SaveSubscription(s);
        }

        public void Unsubscribe(string sourceID, string actionID, string objectID, string recipientID)
        {
            var s = new SubscriptionRecord
            {
                Tenant = GetTenant(),
                SourceId = sourceID,
                ActionId = actionID,
                RecipientId = recipientID,
                ObjectId = objectID,
                Subscribed = false,
            };
            service.SaveSubscription(s);
        }

        public void UnsubscribeAll(string sourceID, string actionID, string objectID)
        {
            service.RemoveSubscriptions(GetTenant(), sourceID, actionID, objectID);
        }

        public void UnsubscribeAll(string sourceID, string actionID)
        {
            service.RemoveSubscriptions(GetTenant(), sourceID, actionID);
        }

        public string[] GetSubscriptionMethod(string sourceID, string actionID, string recipientID)
        {
            IEnumerable<SubscriptionMethod> methods;

            if (Groups.Any(r => r.ToString() == recipientID))
            {
                methods = GetDefaultSubscriptionMethodsFromCache(sourceID, actionID, recipientID);
            }
            else
            {
                methods = service.GetSubscriptionMethods(GetTenant(), sourceID, actionID, recipientID);
            }

            var m = methods
                .FirstOrDefault(x => x.ActionId.Equals(actionID, StringComparison.OrdinalIgnoreCase));

            if (m == null)
            {
                m = methods.FirstOrDefault();
            }

            return m != null ? m.Methods : new string[0];
        }

        public string[] GetRecipients(string sourceID, string actionID, string objectID)
        {
            return service.GetRecipients(GetTenant(), sourceID, actionID, objectID);
        }

        public object GetSubscriptionRecord(string sourceID, string actionID, string recipientID, string objectID)
        {
            return service.GetSubscription(GetTenant(), sourceID, actionID, recipientID, objectID);
        }

        public string[] GetSubscriptions(string sourceID, string actionID, string recipientID, bool checkSubscribe = true)
        {
            return service.GetSubscriptions(GetTenant(), sourceID, actionID, recipientID, checkSubscribe);
        }

        public bool IsUnsubscribe(string sourceID, string recipientID, string actionID, string objectID)
        {
            return service.IsUnsubscribe(GetTenant(), sourceID, actionID, recipientID, objectID);
        }

        public void UpdateSubscriptionMethod(string sourceID, string actionID, string recipientID, string[] senderNames)
        {
            var m = new SubscriptionMethod
            {
                Tenant = GetTenant(),
                SourceId = sourceID,
                ActionId = actionID,
                RecipientId = recipientID,
                Methods = senderNames,
            };
            service.SetSubscriptionMethod(m);
        }

        private IEnumerable<SubscriptionMethod> GetDefaultSubscriptionMethodsFromCache(string sourceID, string actionID, string recepient)
        {
            lock (CacheLocker)
            {
                var key = $"subs|-1{sourceID}{actionID}{recepient}";
                var result = Cache.Get<IEnumerable<SubscriptionMethod>>(key);
                if (result == null)
                {
                    result = service.GetSubscriptionMethods(-1, sourceID, actionID, recepient);
                    Cache.Insert(key, result, DateTime.UtcNow.AddDays(1));
                }

                return result;
            }
        }

        private int GetTenant()
        {
            return CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }
    }
}
