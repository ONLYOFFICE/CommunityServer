/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Linq;

namespace ASC.Core
{
    class ClientSubscriptionManager : ISubscriptionManagerClient
    {
        private readonly ISubscriptionService service;


        public ClientSubscriptionManager(ISubscriptionService service)
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

        public string[] GetSubscriptions(string sourceID, string actionID, string recipientID)
        {
            return service.GetSubscriptions(GetTenant(), sourceID, actionID, recipientID, null)
                .Where(s => s.Subscribed)
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
