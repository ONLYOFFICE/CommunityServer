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
using System.Linq;
using ASC.Common.Logging;
using ASC.Common.Utils;
using ASC.Core.Notify.Senders;
using ASC.Core.Tenants;
using ASC.Notify.Messages;
using ASC.Notify.Sinks;

namespace ASC.Core.Notify
{
    class EmailSenderSink : Sink
    {
        private static readonly string senderName = ASC.Core.Configuration.Constants.NotifyEMailSenderSysName;
        private readonly INotifySender sender;


        public EmailSenderSink(INotifySender sender)
        {
            if (sender == null) throw new ArgumentNullException("sender");

            this.sender = sender;
        }


        public override SendResponse ProcessMessage(INoticeMessage message)
        {
            if (message.Recipient.Addresses == null || message.Recipient.Addresses.Length == 0)
            {
                return new SendResponse(message, senderName, SendResult.IncorrectRecipient);
            }

            var responce = new SendResponse(message, senderName, default(SendResult));
            try
            {
                var m = CreateNotifyMessage(message);
                var result = sender.Send(m);

                switch (result)
                {
                    case NoticeSendResult.TryOnceAgain:
                        responce.Result = SendResult.Inprogress;
                        break;
                    case NoticeSendResult.MessageIncorrect:
                        responce.Result = SendResult.IncorrectRecipient;
                        break;
                    case NoticeSendResult.SendingImpossible:
                        responce.Result = SendResult.Impossible;
                        break;
                    default:
                        responce.Result = SendResult.OK;
                        break;
                }
                return responce;
            }
            catch (Exception e)
            {
                return new SendResponse(message, senderName, e);
            }
        }


        private NotifyMessage CreateNotifyMessage(INoticeMessage message)
        {
            var m = new NotifyMessage
            {
                Subject = message.Subject.Trim(' ', '\t', '\n', '\r'),
                ContentType = message.ContentType,
                Content = message.Body,
                Sender = senderName,
                CreationDate = DateTime.UtcNow,
            };

            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            m.Tenant = tenant == null ? Tenant.DEFAULT_TENANT : tenant.TenantId;

            var from = MailAddressUtils.Create(CoreContext.Configuration.SmtpSettings.SenderAddress, CoreContext.Configuration.SmtpSettings.SenderDisplayName);
            var fromTag = message.Arguments.FirstOrDefault(x => x.Tag.Equals("MessageFrom"));
            if ((CoreContext.Configuration.SmtpSettings.IsDefaultSettings || string.IsNullOrEmpty(CoreContext.Configuration.SmtpSettings.SenderDisplayName)) && 
                fromTag != null && fromTag.Value != null)
            {
                try
                {
                    from = MailAddressUtils.Create(from.Address, fromTag.Value.ToString());
                }
                catch { }
            }
            m.From = from.ToString();

            var to = new List<string>();
            foreach (var address in message.Recipient.Addresses)
            {
                to.Add(MailAddressUtils.Create(address, message.Recipient.Name).ToString());
            }
            m.To = string.Join("|", to.ToArray());

            var replyTag = message.Arguments.FirstOrDefault(x => x.Tag == "replyto");
            if (replyTag != null && replyTag.Value is string)
            {
                try
                {
                    m.ReplyTo = MailAddressUtils.Create((string)replyTag.Value).ToString();
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC.Notify").Error("Error creating reply to tag for: " + replyTag.Value, e);
                }
            }

            var priority = message.Arguments.FirstOrDefault(a => a.Tag == "Priority");
            if (priority != null)
            {
                m.Priority = Convert.ToInt32(priority.Value);
            }

            var attachmentTag = message.Arguments.FirstOrDefault(x => x.Tag == "EmbeddedAttachments");
            if (attachmentTag != null && attachmentTag.Value != null)
            {
                m.EmbeddedAttachments = attachmentTag.Value as NotifyMessageAttachment[];
            }

            var autoSubmittedTag = message.Arguments.FirstOrDefault(x => x.Tag == "AutoSubmitted");
            if (autoSubmittedTag != null && autoSubmittedTag.Value is string)
            {
                try
                {
                    m.AutoSubmitted = autoSubmittedTag.Value.ToString();
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC.Notify").Error("Error creating AutoSubmitted tag for: " + autoSubmittedTag.Value, e);
                }
            }

            return m;
        }
    }
}