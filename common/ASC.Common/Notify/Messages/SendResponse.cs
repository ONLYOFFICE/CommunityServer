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
using ASC.Notify.Model;
using ASC.Notify.Recipients;

namespace ASC.Notify.Messages
{
    [Serializable]
    public class SendResponse
    {
        public SendResponse()
        {
            Result = SendResult.OK;
        }

        public SendResponse(INotifyAction action, IRecipient recipient, Exception exc)
        {
            Result = SendResult.Impossible;
            Exception = exc;
            Recipient = recipient;
            NotifyAction = action;
        }

        public SendResponse(INotifyAction action, string senderName, IRecipient recipient, Exception exc)
        {
            Result = SendResult.Impossible;
            SenderName = senderName;
            Exception = exc;
            Recipient = recipient;
            NotifyAction = action;
        }

        public SendResponse(INotifyAction action, string senderName, IRecipient recipient, SendResult sendResult)
        {
            SenderName = senderName;
            Recipient = recipient;
            Result = sendResult;
            NotifyAction = action;
        }

        public SendResponse(INoticeMessage message, string sender, SendResult result)
        {
            NoticeMessage = message;
            SenderName = sender;
            Result = result;
            if (message != null)
            {
                Recipient = message.Recipient;
                NotifyAction = message.Action;
            }
        }

        public SendResponse(INoticeMessage message, string sender, Exception exc)
        {
            NoticeMessage = message;
            SenderName = sender;
            Result = SendResult.Impossible;
            Exception = exc;
            if (message != null)
            {
                Recipient = message.Recipient;
                NotifyAction = message.Action;
            }
        }

        public INoticeMessage NoticeMessage { get; internal set; }

        public INotifyAction NotifyAction { get; internal set; }

        public SendResult Result { get; set; }

        public Exception Exception { get; set; }

        public string SenderName { get; internal set; }

        public IRecipient Recipient { get; internal set; }
    }
}