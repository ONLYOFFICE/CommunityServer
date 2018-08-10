/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using ASC.Web.Calendar.Notification;
using ASC.Web.Core.Subscriptions;
using ASC.Api.Calendar.Notification;

namespace ASC.Web.Calendar.Notification
{
    public class CalendarSubscriptionManager : IProductSubscriptionManager
    {
        public SubscriptionType CalendarSharingSubscription { get {
            return new SubscriptionType() {
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
                return new SubscriptionType()
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
            return new List<SubscriptionType>() { CalendarSharingSubscription, EventAlertSubscription};
        }

        public ASC.Notify.Model.ISubscriptionProvider SubscriptionProvider
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
