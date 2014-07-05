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
using System.Collections.Generic;
using ASC.Notify.Engine;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;

namespace ASC.Notify.Model
{
    class NotifyClientImpl : INotifyClient
    {
        private readonly Context ctx;
        private readonly InterceptorStorage interceptors = new InterceptorStorage();
        private readonly INotifySource notifySource;


        public NotifyClientImpl(Context context, INotifySource notifySource)
        {
            if (notifySource == null) throw new ArgumentNullException("notifySource");
            if (context == null) throw new ArgumentNullException("context");

            this.notifySource = notifySource;
            ctx = context;
        }


        public void SendNoticeToAsync(INotifyAction action, string objectID, IRecipient[] recipients, string[] senderNames, SendNoticeCallback sendCallback, params ITagValue[] args)
        {
            SendNoticeToAsync(action, objectID, recipients, senderNames, sendCallback, false, args);
        }

        public void SendNoticeToAsync(INotifyAction action, string objectID, IRecipient[] recipients, SendNoticeCallback sendCallback, params ITagValue[] args)
        {
            SendNoticeToAsync(action, objectID, recipients, null, sendCallback, false, args);
        }

        public void SendNoticeToAsync(INotifyAction action, string objectID, IRecipient[] recipients, bool checkSubscription, params ITagValue[] args)
        {
            SendNoticeToAsync(action, objectID, recipients, null, null, checkSubscription, args);
        }

        public void SendNoticeAsync(INotifyAction action, string objectID, IRecipient recipient, SendNoticeCallback sendCallback, params ITagValue[] args)
        {
            SendNoticeToAsync(action, objectID, new[] { recipient }, null, sendCallback, false, args);
        }

        public void SendNoticeAsync(INotifyAction action, string objectID, SendNoticeCallback sendCallback, params ITagValue[] args)
        {
            var subscriptionSource = notifySource.GetSubscriptionProvider();
            var recipients = subscriptionSource.GetRecipients(action, objectID);
            SendNoticeToAsync(action, objectID, recipients, null, sendCallback, false, args);
        }

        public void SendNoticeAsync(INotifyAction action, string objectID, IRecipient recipient, bool checkSubscription, params ITagValue[] args)
        {
            SendNoticeToAsync(action, objectID, new[] { recipient }, null, null, checkSubscription, args);
        }



        public void RegisterSendMethod(Action<DateTime> method, string cron)
        {
            ctx.NotifyEngine.RegisterSendMethod(method, cron);
        }

        public void UnregisterSendMethod(Action<DateTime> method)
        {
            ctx.NotifyEngine.UnregisterSendMethod(method);
        }

        public void BeginSingleRecipientEvent(string name)
        {
            interceptors.Add(new SingleRecipientInterceptor(name));
        }

        public void EndSingleRecipientEvent(string name)
        {
            interceptors.Remove(name);
        }

        public void AddInterceptor(ISendInterceptor interceptor)
        {
            interceptors.Add(interceptor);
        }

        public void RemoveInterceptor(string name)
        {
            interceptors.Remove(name);
        }


        private void SendNoticeToAsync(INotifyAction action, string objectID, IRecipient[] recipients, string[] senderNames, SendNoticeCallback sendCallback, bool checkSubsciption, params ITagValue[] args)
        {
            if (recipients == null) throw new ArgumentNullException("recipients");

            BeginSingleRecipientEvent("__syspreventduplicateinterceptor");

            foreach (var recipient in recipients)
            {
                var r = CreateRequest(action, objectID, recipient, sendCallback, args, senderNames, checkSubsciption);
                SendAsync(r);
            }
        }

        private void SendAsync(NotifyRequest request)
        {
            request.Interceptors = interceptors.GetAll();
            ctx.NotifyEngine.QueueRequest(request);
        }

        private NotifyRequest CreateRequest(INotifyAction action, string objectID, IRecipient recipient, SendNoticeCallback sendCallback, ITagValue[] args, string[] senders, bool checkSubsciption)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (recipient == null) throw new ArgumentNullException("recipient");
            if (sendCallback != null) throw new NotImplementedException("sendCallback");

            var request = new NotifyRequest(notifySource, action, objectID, recipient);
            request.SenderNames = senders;
            request.IsNeedCheckSubscriptions = checkSubsciption;
            if (args != null) request.Arguments.AddRange(args);
            return request;
        }
    }
}