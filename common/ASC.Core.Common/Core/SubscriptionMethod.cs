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

namespace ASC.Core
{
    [Serializable]
    public class SubscriptionMethod
    {
        public int Tenant
        {
            get;
            set;
        }

        public string SourceId
        {
            get;
            set;
        }

        public string ActionId
        {
            get;
            set;
        }

        public string RecipientId
        {
            get;
            set;
        }

        public string[] Methods
        {
            get;
            set;
        }

        public static implicit operator SubscriptionMethod(SubscriptionMethodCache cache)
        {
            return new SubscriptionMethod()
            {
                Tenant = cache.Tenant,
                SourceId = cache.SourceId,
                ActionId = cache.ActionId,
                RecipientId = cache.RecipientId
            };
        }

        public static implicit operator SubscriptionMethodCache(SubscriptionMethod cache)
        {
            return new SubscriptionMethodCache
            {
                Tenant = cache.Tenant,
                SourceId = cache.SourceId,
                ActionId = cache.ActionId,
                RecipientId = cache.RecipientId
            };
        }

    }



    public class SubscriptionMethodCache
    {
        public string RecipientId;
        public string ActionId;
        public string SourceId;
        public string[] Methods;
        public int Tenant;
    }
}
