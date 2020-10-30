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

using ASC.Core.Notify;
using ASC.Core.Notify.Senders;
using ASC.Core.Tenants;
using ASC.Notify.Messages;
using ASC.Notify.Sinks;

namespace ASC.Core.Notify
{
    class TelegramSenderSink : Sink
    {
        private string senderName = ASC.Core.Configuration.Constants.NotifyTelegramSenderSysName;
        private readonly INotifySender sender;

        public TelegramSenderSink(INotifySender sender)
        {
            if (sender == null) throw new ArgumentNullException("sender");

            this.sender = sender;
        }


        public override SendResponse ProcessMessage(INoticeMessage message)
        {
            try
            {
                var result = SendResult.OK;
                var m = new NotifyMessage
                {
                    To = message.Recipient.ID,
                    Subject = message.Subject,
                    ContentType = message.ContentType,
                    Content = message.Body,
                    Sender = senderName,
                    CreationDate = DateTime.UtcNow,
                };

                var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                m.Tenant = tenant == null ? Tenant.DEFAULT_TENANT : tenant.TenantId;

                sender.Send(m);

                return new SendResponse(message, senderName, result);
            }
            catch (Exception ex)
            {
                return new SendResponse(message, senderName, ex);
            }
        }
    }
}