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
using System.Linq;
using ASC.Notify.Model;
using ASC.Notify.Recipients;

namespace ASC.Core.Notify
{
    public class DirectSubscriptionProvider : ISubscriptionProvider
    {
        private readonly IRecipientProvider recipientProvider;
        private readonly SubscriptionManager subscriptionManager;
        private readonly string sourceID;


        public DirectSubscriptionProvider(string sourceID, SubscriptionManager subscriptionManager, IRecipientProvider recipientProvider)
        {
            if (string.IsNullOrEmpty(sourceID)) throw new ArgumentNullException("sourceID");
            if (subscriptionManager == null) throw new ArgumentNullException("subscriptionManager");
            if (recipientProvider == null) throw new ArgumentNullException("recipientProvider");
            
            this.sourceID = sourceID;
            this.subscriptionManager = subscriptionManager;
            this.recipientProvider = recipientProvider;
        }


        public object GetSubscriptionRecord(INotifyAction action, IRecipient recipient, string objectID)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");

            return subscriptionManager.GetSubscriptionRecord(sourceID, action.ID, recipient.ID, objectID);
        }

        public string[] GetSubscriptions(INotifyAction action, IRecipient recipient, bool checkSubscribe = true)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");

            return subscriptionManager.GetSubscriptions(sourceID, action.ID, recipient.ID, checkSubscribe);
        }

        public IRecipient[] GetRecipients(INotifyAction action, string objectID)
        {
            if (action == null) throw new ArgumentNullException("action");

            return subscriptionManager.GetRecipients(sourceID, action.ID, objectID)
                .Select(r => recipientProvider.GetRecipient(r))
                .Where(r => r != null)
                .ToArray();
        }

        public string[] GetSubscriptionMethod(INotifyAction action, IRecipient recipient)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");

            return subscriptionManager.GetSubscriptionMethod(sourceID, action.ID, recipient.ID);
        }
        
        public void UpdateSubscriptionMethod(INotifyAction action, IRecipient recipient, params string[] senderNames)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");
            subscriptionManager.UpdateSubscriptionMethod(sourceID, action.ID, recipient.ID, senderNames);
        }
        
        public bool IsUnsubscribe(IDirectRecipient recipient, INotifyAction action, string objectID)
        {
            if (recipient == null) throw new ArgumentNullException("recipient");
            if (action == null) throw new ArgumentNullException("action");
            
            return subscriptionManager.IsUnsubscribe(sourceID, recipient.ID, action.ID, objectID);
        }        

        public void Subscribe(INotifyAction action, string objectID, IRecipient recipient)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");

            subscriptionManager.Subscribe(sourceID, action.ID, objectID, recipient.ID);
        }

        public void UnSubscribe(INotifyAction action, string objectID, IRecipient recipient)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");
            
            subscriptionManager.Unsubscribe(sourceID, action.ID, objectID, recipient.ID);
        }

        public void UnSubscribe(INotifyAction action)
        {
            if (action == null) throw new ArgumentNullException("action");
            
            subscriptionManager.UnsubscribeAll(sourceID, action.ID);
        }

        public void UnSubscribe(INotifyAction action, string objectID)
        {
            if (action == null) throw new ArgumentNullException("action");
            
            subscriptionManager.UnsubscribeAll(sourceID, action.ID, objectID);
        }

        [Obsolete("Use UnSubscribe(INotifyAction, string, IRecipient)", true)]
        public void UnSubscribe(INotifyAction action, IRecipient recipient)
        {
            throw new NotSupportedException("use UnSubscribe(INotifyAction, string, IRecipient )");
        }
    }
}