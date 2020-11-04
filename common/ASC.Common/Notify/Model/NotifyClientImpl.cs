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



        public INotifyClient RegisterSendMethod(Action<DateTime> method, string cron)
        {
            ctx.NotifyEngine.RegisterSendMethod(method, cron);
            return this;
        }

        public INotifyClient UnregisterSendMethod(Action<DateTime> method)
        {
            ctx.NotifyEngine.UnregisterSendMethod(method);
            return this;

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