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
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Notify.Signalr;
using ASC.Mail.Clients;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Core.Dao.Expressions.Contact;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Contracts.Base;
using ASC.Mail.Data.Search;
using ASC.Mail.Data.Storage;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;
using ASC.Mail.Extensions;
using ASC.Mail.Utils;
using MimeKit;
using MailMessage = ASC.Mail.Data.Contracts.MailMessageData;

namespace ASC.Mail.Core.Engine
{
    public class TemplateEngine : ComposeEngineBase
    {
        public TemplateEngine(int tenant, string user, DeliveryFailureMessageTranslates daemonLabels = null, ILog log = null)
            : base(tenant, user, daemonLabels, log)
        {
            Log = log ?? LogManager.GetLogger("ASC.Mail.TemplateEngine");
        }

        public override MailMessage Save(int id, string from, List<string> to, List<string> cc, List<string> bcc, string mimeReplyToId,
            bool importance, string subject, List<int> tags, string body, List<MailAttachmentData> attachments, string calendarIcs,
            DeliveryFailureMessageTranslates translates = null)
        {
            var mailAddress = new MailAddress(from);

            var engine = new EngineFactory(Tenant, User);

            var accounts = engine.AccountEngine.GetAccountInfoList().ToAccountData();

            var account = accounts.FirstOrDefault(a => a.Email.ToLower().Equals(mailAddress.Address));

            if (account == null)
                throw new ArgumentException("Mailbox not found");

            if (account.IsGroup)
                throw new InvalidOperationException("Saving emails from a group address is forbidden");

            var mbox = engine.MailboxEngine.GetMailboxData(
                new СoncreteUserMailboxExp(account.MailboxId, Tenant, User));

            if (mbox == null)
                throw new ArgumentException("No such mailbox");

            string mimeMessageId, streamId;

            var previousMailboxId = mbox.MailBoxId;

            if (id > 0)
            {
                var message = engine.MessageEngine.GetMessage(id, new MailMessage.Options
                {
                    LoadImages = false,
                    LoadBody = true,
                    NeedProxyHttp = Defines.NeedProxyHttp,
                    NeedSanitizer = false
                });

                if (message.Folder != FolderType.Templates)
                {
                    throw new InvalidOperationException("Saving emails is permitted only in the Templates folder");
                }

                if (message.HtmlBody.Length > Defines.MaximumMessageBodySize)
                {
                    throw new InvalidOperationException("Message body exceeded limit (" + Defines.MaximumMessageBodySize / 1024 + " KB)");
                }

                mimeMessageId = message.MimeMessageId;

                streamId = message.StreamId;

                /*
                if (attachments != null && attachments.Any())
                {
                    foreach (var attachment in attachments)
                    {
                        attachment.streamId = streamId;
                    }
                }
                 */

                previousMailboxId = message.MailboxId;
            }
            else
            {
                mimeMessageId = MailUtil.CreateMessageId();
                streamId = MailUtil.CreateStreamId();
            }

            var fromAddress = MailUtil.CreateFullEmail(mbox.Name, mbox.EMail.Address);

            var template = new MailTemplateData(id, mbox, fromAddress, to, cc, bcc, subject, mimeMessageId, mimeReplyToId, importance,
                    tags, body, streamId, attachments, calendarIcs) { PreviousMailboxId = previousMailboxId };

            DaemonLabels = translates ?? DeliveryFailureMessageTranslates.Defauilt;

            return Save(template);
        }
    }
}
