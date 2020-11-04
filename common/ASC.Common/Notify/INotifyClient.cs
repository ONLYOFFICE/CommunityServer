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
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;

namespace ASC.Notify
{
    public delegate void SendNoticeCallback(INotifyAction action, string objectID, IRecipient recipient, NotifyResult result);

    public interface INotifyClient
    {
        void SendNoticeToAsync(INotifyAction action, string objectID, IRecipient[] recipients, string[] senderNames, SendNoticeCallback sendCallback, params ITagValue[] args);

        void SendNoticeToAsync(INotifyAction action, string objectID, IRecipient[] recipients, SendNoticeCallback sendCallback, params ITagValue[] args);

        void SendNoticeToAsync(INotifyAction action, string objectID, IRecipient[] recipients, bool checkSubscription, params ITagValue[] args);

        void SendNoticeAsync(INotifyAction action, string objectID, SendNoticeCallback sendCallback, params ITagValue[] args);

        void SendNoticeAsync(INotifyAction action, string objectID, IRecipient recipient, SendNoticeCallback sendCallback, params ITagValue[] args);

        void SendNoticeAsync(INotifyAction action, string objectID, IRecipient recipient, bool checkSubscription, params ITagValue[] args);


        INotifyClient RegisterSendMethod(Action<DateTime> method, string cron);

        INotifyClient UnregisterSendMethod(Action<DateTime> method);


        void BeginSingleRecipientEvent(string name);

        void EndSingleRecipientEvent(string name);

        void AddInterceptor(ISendInterceptor interceptor);

        void RemoveInterceptor(string name);
    }
}