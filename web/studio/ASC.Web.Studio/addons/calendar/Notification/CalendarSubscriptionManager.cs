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
using ASC.Web.Core.Subscriptions;
using ASC.Api.Calendar.Notification;

namespace ASC.Web.Calendar.Notification
{
    public class CalendarSubscriptionManager : IProductSubscriptionManager
    {
        public SubscriptionType CalendarSharingSubscription { get {
            return new SubscriptionType {
                ID = new Guid("{1CE43C40-72F4-4265-A4C6-8B55E29DB447}"),
                Name = Resources.CalendarAddonResource.CalendarSharingSubscription,
                NotifyAction = CalendarNotifySource.CalendarSharing,
                Single = true,
                CanSubscribe = true};
        } }

        public SubscriptionType EventAlertSubscription
        {
            get
            {
                return new SubscriptionType
                {
                    ID = new Guid("{862D17FE-7119-4aa0-B1AA-E3C23097CB69}"),
                    Name = Resources.CalendarAddonResource.EventAlertSubscription,
                    NotifyAction = CalendarNotifySource.EventAlert,
                    Single = true,
                    CanSubscribe = true
                };
            }
        }

        #region ISubscriptionManager Members

        public List<SubscriptionObject> GetSubscriptionObjects(Guid subItem)
        {
            return new List<SubscriptionObject>();
        }

        public List<SubscriptionType> GetSubscriptionTypes()
        {
            return new List<SubscriptionType> { CalendarSharingSubscription, EventAlertSubscription};
        }

        public Notify.Model.ISubscriptionProvider SubscriptionProvider
        {
            get { return CalendarNotifySource.Instance.GetSubscriptionProvider(); }
        }

        #endregion

        #region IProductSubscriptionManager Members

        public List<SubscriptionGroup> GetSubscriptionGroups()
        {
            return new List<SubscriptionGroup>();
        }

        public GroupByType GroupByType
        {
            get { return GroupByType.Simple; }
        }

        #endregion
    }
}
