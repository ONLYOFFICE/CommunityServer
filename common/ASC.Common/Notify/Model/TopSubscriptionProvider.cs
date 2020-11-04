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
using ASC.Notify.Recipients;


namespace ASC.Notify.Model
{
    public class TopSubscriptionProvider : ISubscriptionProvider
    {
        private readonly string[] defaultSenderMethods = new string[0];
        private readonly ISubscriptionProvider subscriptionProvider;
        private readonly IRecipientProvider recipientProvider;


        public TopSubscriptionProvider(IRecipientProvider recipientProvider, ISubscriptionProvider directSubscriptionProvider)
        {
            if (recipientProvider == null) throw new ArgumentNullException("recipientProvider");
            if (directSubscriptionProvider == null) throw new ArgumentNullException("directSubscriptionProvider");

            this.recipientProvider = recipientProvider;
            subscriptionProvider = directSubscriptionProvider;
        }

        public TopSubscriptionProvider(IRecipientProvider recipientProvider, ISubscriptionProvider directSubscriptionProvider, string[] defaultSenderMethods)
            : this(recipientProvider, directSubscriptionProvider)
        {
            this.defaultSenderMethods = defaultSenderMethods;
        }


        public virtual string[] GetSubscriptionMethod(INotifyAction action, IRecipient recipient)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");

            var senders = subscriptionProvider.GetSubscriptionMethod(action, recipient);
            if (senders == null || senders.Length == 0)
            {
                var parents = WalkUp(recipient);
                foreach (var parent in parents)
                {
                    senders = subscriptionProvider.GetSubscriptionMethod(action, parent);
                    if (senders != null && senders.Length != 0) break;
                }
            }

            return senders != null && 0 < senders.Length ? senders : defaultSenderMethods;
        }

        public virtual IRecipient[] GetRecipients(INotifyAction action, string objectID)
        {
            if (action == null) throw new ArgumentNullException("action");

            var recipents = new List<IRecipient>(5);
            var directRecipients = subscriptionProvider.GetRecipients(action, objectID) ?? new IRecipient[0];
            recipents.AddRange(directRecipients);
            return recipents.ToArray();
        }

        public virtual bool IsUnsubscribe(IDirectRecipient recipient, INotifyAction action, string objectID)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");

            return subscriptionProvider.IsUnsubscribe(recipient, action, objectID);
        }


        public virtual void Subscribe(INotifyAction action, string objectID, IRecipient recipient)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");

            subscriptionProvider.Subscribe(action, objectID, recipient);
        }

        public virtual void UnSubscribe(INotifyAction action, string objectID, IRecipient recipient)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");

            subscriptionProvider.UnSubscribe(action, objectID, recipient);
        }

        public void UnSubscribe(INotifyAction action, string objectID)
        {
            if (action == null) throw new ArgumentNullException("action");

            subscriptionProvider.UnSubscribe(action, objectID);
        }

        public void UnSubscribe(INotifyAction action)
        {
            if (action == null) throw new ArgumentNullException("action");

            subscriptionProvider.UnSubscribe(action);
        }

        public virtual void UnSubscribe(INotifyAction action, IRecipient recipient)
        {
            var objects = GetSubscriptions(action, recipient);
            foreach (string objectID in objects)
            {
                subscriptionProvider.UnSubscribe(action, objectID, recipient);
            }
        }

        public virtual void UpdateSubscriptionMethod(INotifyAction action, IRecipient recipient, params string[] senderNames)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");
            if (senderNames == null) throw new ArgumentNullException("senderNames");

            subscriptionProvider.UpdateSubscriptionMethod(action, recipient, senderNames);
        }

        public virtual object GetSubscriptionRecord(INotifyAction action, IRecipient recipient, string objectID)
        {
            if (recipient == null) throw new ArgumentNullException("recipient");
            if (action == null) throw new ArgumentNullException("action");

            var subscriptionRecord = subscriptionProvider.GetSubscriptionRecord(action, recipient, objectID);

            if (subscriptionRecord != null) return subscriptionRecord;

            var parents = WalkUp(recipient);

            foreach (var parent in parents)
            {
                subscriptionRecord = subscriptionProvider.GetSubscriptionRecord(action, parent, objectID);

                if (subscriptionRecord != null) break;
            }

            return subscriptionRecord;
        }

        public virtual string[] GetSubscriptions(INotifyAction action, IRecipient recipient, bool checkSubscription = true)
        {
            if (recipient == null) throw new ArgumentNullException("recipient");
            if (action == null) throw new ArgumentNullException("action");

            var objects = new List<string>();
            var direct = subscriptionProvider.GetSubscriptions(action, recipient, checkSubscription) ?? new string[0];
            MergeObjects(objects, direct);
            var parents = WalkUp(recipient);
            foreach (var parent in parents)
            {
                direct = subscriptionProvider.GetSubscriptions(action, parent, checkSubscription) ?? new string[0];
                if (recipient is IDirectRecipient)
                {
                    foreach (var groupsubscr in direct)
                    {
                        if (!objects.Contains(groupsubscr) && !subscriptionProvider.IsUnsubscribe(recipient as IDirectRecipient, action, groupsubscr))
                        {
                            objects.Add(groupsubscr);
                        }
                    }
                }
                else
                {
                    MergeObjects(objects, direct);
                }
            }
            return objects.ToArray();
        }


        private List<IRecipient> WalkUp(IRecipient recipient)
        {
            var parents = new List<IRecipient>();
            var groups = recipientProvider.GetGroups(recipient) ?? new IRecipientsGroup[0];
            foreach (var group in groups)
            {
                parents.Add(group);
                parents.AddRange(WalkUp(group));
            }
            return parents;
        }

        private void MergeActions(List<INotifyAction> result, IEnumerable<INotifyAction> additions)
        {
            foreach (var addition in additions)
            {
                if (!result.Contains(addition)) result.Add(addition);
            }
        }

        private void MergeObjects(List<string> result, IEnumerable<string> additions)
        {
            foreach (var addition in additions)
            {
                if (!result.Contains(addition)) result.Add(addition);
            }
        }
    }
}