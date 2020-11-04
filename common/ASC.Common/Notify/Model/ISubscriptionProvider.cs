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


#region usings

using System;
using System.Linq;
using ASC.Common.Logging;
using ASC.Notify.Recipients;

#endregion

namespace ASC.Notify.Model
{
    public interface ISubscriptionProvider
    {
        string[] GetSubscriptions(INotifyAction action, IRecipient recipient, bool checkSubscribe = true);

        string[] GetSubscriptionMethod(INotifyAction action, IRecipient recipient);

        IRecipient[] GetRecipients(INotifyAction action, string objectID);

        object GetSubscriptionRecord(INotifyAction action, IRecipient recipient, string objectID);

        bool IsUnsubscribe(IDirectRecipient recipient, INotifyAction action, string objectID);

        void Subscribe(INotifyAction action, string objectID, IRecipient recipient);

        void UnSubscribe(INotifyAction action, string objectID, IRecipient recipient);

        void UnSubscribe(INotifyAction action, string objectID);

        void UnSubscribe(INotifyAction action);

        void UnSubscribe(INotifyAction action, IRecipient recipient);

        void UpdateSubscriptionMethod(INotifyAction action, IRecipient recipient, params string[] senderNames);
    }

    public static class SubscriptionProviderHelper
    {
        public static bool IsSubscribed(this ISubscriptionProvider provider, INotifyAction action, IRecipient recipient, string objectID)
        {
            var result = false;

            try
            {
                var subscriptionRecord = provider.GetSubscriptionRecord(action, recipient, objectID);
                if (subscriptionRecord != null)
                {
                    var properties = subscriptionRecord.GetType().GetProperties();
                    if (properties.Any())
                    {
                        var property = properties.Single(p => p.Name == "Subscribed");
                        if (property != null)
                        {
                            result = (bool)property.GetValue(subscriptionRecord, null);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogManager.GetLogger("ASC").Error(exception);
            }

            return result;
        }
    }
}