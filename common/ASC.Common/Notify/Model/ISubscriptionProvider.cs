/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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