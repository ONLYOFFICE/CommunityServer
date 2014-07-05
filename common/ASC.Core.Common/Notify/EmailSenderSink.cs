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
using System.Linq;
using ASC.Common.Utils;
using ASC.Core.Notify.Senders;
using ASC.Core.Tenants;
using ASC.Notify.Messages;
using ASC.Notify.Sinks;
using log4net;

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
            if (fromTag != null && fromTag.Value != null)
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

            return m;
        }
    }
}