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
using ASC.Notify.Model;
using System.Collections.Generic;

namespace ASC.Web.Core.Subscriptions
{
    public delegate bool IsEmptySubscriptionTypeDelegate(Guid productID, Guid moduleOrGroupID, Guid typeID);

    public delegate List<SubscriptionObject> GetSubscriptionObjectsDelegate(Guid productID, Guid moduleOrGroupID, Guid typeID);

    public class SubscriptionType
    {
        public INotifyAction NotifyAction { get; set; }

        public Guid ID { get; set; }

        public string Name { get; set; }

        public bool Single { get; set; }

        public bool CanSubscribe { get; set; }

        public IsEmptySubscriptionTypeDelegate IsEmptySubscriptionType;

        public GetSubscriptionObjectsDelegate GetSubscriptionObjects;
        
    }
}
