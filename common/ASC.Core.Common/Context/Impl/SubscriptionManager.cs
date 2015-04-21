/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Linq;

namespace ASC.Core
{
    public class SubscriptionManager
    {
        private readonly ISubscriptionService service;


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
            var m = service.GetSubscriptionMethods(GetTenant(), sourceID, actionID, recipientID)
                .FirstOrDefault(x => x.ActionId.Equals(actionID, StringComparison.OrdinalIgnoreCase));
            if (m == null)
            {
                m = service.GetSubscriptionMethods(GetTenant(), sourceID, actionID, recipientID).FirstOrDefault();
            }
            if (m == null)
            {
                m = service.GetSubscriptionMethods(GetTenant(), sourceID, actionID, Guid.Empty.ToString()).FirstOrDefault();
            }
            return m != null ? m.Methods : new string[0];
        }

        public string[] GetRecipients(string sourceID, string actionID, string objectID)
        {
            return service.GetSubscriptions(GetTenant(), sourceID, actionID, null, objectID)
                .Where(s => s.Subscribed)
                .Select(s => s.RecipientId)
                .ToArray();
        }

        public string[] GetSubscriptions(string sourceID, string actionID, string recipientID, bool checkSubscribe = true)
        {
            return service.GetSubscriptions(GetTenant(), sourceID, actionID, recipientID, null)
                .Where(s => !checkSubscribe || s.Subscribed)
                .Select(s => s.ObjectId)
                .ToArray();
        }

        public bool IsUnsubscribe(string sourceID, string recipientID, string actionID, string objectID)
        {
            var s = service.GetSubscription(GetTenant(), sourceID, actionID, recipientID, objectID);
            if (s == null && !string.IsNullOrEmpty(objectID))
            {
                s = service.GetSubscription(GetTenant(), sourceID, actionID, recipientID, null);
            }
            return s != null && !s.Subscribed;
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


        private int GetTenant()
        {
            return CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }
    }
}
