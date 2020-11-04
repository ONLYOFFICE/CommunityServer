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
using ASC.Notify.Model;
using ASC.Web.Community.Birthdays.Resources;
using ASC.Web.Core.Subscriptions;

namespace ASC.Web.Community.Birthdays
{
    public class BirthdaysSubscriptionManager : ISubscriptionManager
    {
        public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
        {
            return new List<SubscriptionObject>();
        }

        public List<SubscriptionType> GetSubscriptionTypes()
        {
            var subscriptionTypes = new List<SubscriptionType>();

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = new Guid("{3177E937-9189-45db-BA7C-916C69C4A574}"),
                Name = BirthdaysResource.BirthdaysSubscribeAll,
                NotifyAction = BirthdaysNotifyClient.Event_BirthdayReminder,
                Single = true,
                IsEmptySubscriptionType = new IsEmptySubscriptionTypeDelegate(IsEmptySubscriptionType),
                CanSubscribe = true
            });
            return subscriptionTypes;
        }

        private bool IsEmptySubscriptionType(Guid productID, Guid moduleID, Guid typeID)
        {
            return false;
        }

        public ISubscriptionProvider SubscriptionProvider
        {
            get { return BirthdaysNotifyClient.NotifySource.GetSubscriptionProvider(); }
        }
    }
}
