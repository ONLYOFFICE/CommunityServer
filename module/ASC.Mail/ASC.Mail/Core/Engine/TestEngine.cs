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
using System.IO;
using System.Linq;
using ASC.Common.Logging;
using ASC.Common.Web;
using ASC.Mail.Clients;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;
using ASC.Mail.Extensions;
using ASC.Mail.Utils;
using MailMessage = ASC.Mail.Data.Contracts.MailMessageData;

namespace ASC.Mail.Core.Engine
{
    public class TestEngine
    {
        public int TenantId { get; private set; }
        public string Username { get; private set; }

        public ILog Log { get; private set; }

        private readonly EngineFactory _engineFactory;

        private const string SAMPLE_UIDL = "api sample";
        private const string SAMPLE_REPLY_UIDL = "api sample reply";
        private const string LOREM_IPSUM_SUBJECT = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
        private const string LOREM_IPSUM_INTRO = "Lorem ipsum introduction";

        private const string LOREM_IPSUM_BODY =
            "<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce vestibulum luctus mauris, " +
            "eget blandit libero auctor quis. Vestibulum quam ex, euismod sit amet luctus eget, condimentum " +
            "vel nulla. Etiam pretium justo tortor, gravida scelerisque augue porttitor sed. Sed in purus neque. " +
            "Sed eget efficitur erat. Ut lobortis eros vitae urna lacinia, mattis efficitur felis accumsan. " +
            "Nullam at dapibus tortor, ut vulputate libero. Fusce ac auctor eros. Aenean justo quam, sodales nec " +
            "risus eget, cursus semper lacus. Nullam mattis neque ac felis euismod aliquet. Donec id eros " +
            "condimentum, egestas sapien vitae, tempor tortor. Nam vehicula ligula eget congue egestas. " +
            "Nulla facilisi. Aenean sodales gravida arcu, a volutpat nulla accumsan ac. Duis leo enim, condimentum " +
            "in malesuada at, rhoncus sed ex. Quisque fringilla scelerisque lacus.</p>";

        public TestEngine(int tenant, string user, ILog log = null)
        {
            TenantId = tenant;
            Username = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.TestEngine");

            _engineFactory = new EngineFactory(TenantId, Username, Log);
        }

        public int CreateSampleMessage(
            int? folderId,
            int? mailboxId,
            List<string> to,
            List<string> cc,
            List<string> bcc,
            bool importance,
            bool unread,
            string subject,
            string body, 
            string calendarUid = null,
            DateTime? date = null,
            List<int> tagIds = null,
            string fromAddress = null,
            bool add2Index = false,
            string mimeMessageId = null,
            uint? userFolderId = null)
        {
            var folder = folderId.HasValue ? (FolderType) folderId.Value : FolderType.Inbox;

            if (!MailFolder.IsIdOk(folder))
                throw new ArgumentException(@"Invalid folder id", "folderId");

            if (!mailboxId.HasValue)
                throw new ArgumentException(@"Invalid mailbox id", "mailboxId");

            var accounts = _engineFactory.AccountEngine.GetAccountInfoList().ToAccountData().ToList();

            var account = mailboxId.HasValue
                ? accounts.FirstOrDefault(a => a.MailboxId == mailboxId)
                : accounts.FirstOrDefault(a => a.IsDefault) ?? accounts.FirstOrDefault();

            if (account == null)
                throw new ArgumentException("Mailbox not found");

            var mbox = _engineFactory.MailboxEngine.GetMailboxData(
                new СoncreteUserMailboxExp(account.MailboxId, TenantId, Username));

            if (mbox == null)
                throw new ArgumentException("no such mailbox");

            var internalId = string.IsNullOrEmpty(mimeMessageId) ? MailUtil.CreateMessageId() : mimeMessageId;

            var restoreFolder = folder == FolderType.Spam || folder == FolderType.Trash
                ? FolderType.Inbox
                : folder;

            string sampleBody;
            string sampleIntro;

            if (!to.Any())
            {
                to = new List<string> {mbox.EMail.Address};
            }

            if (!string.IsNullOrEmpty(body))
            {
                sampleBody = body;
                sampleIntro = MailUtil.GetIntroduction(body);
            }
            else
            {
                sampleBody = LOREM_IPSUM_BODY;
                sampleIntro = LOREM_IPSUM_INTRO;
            }

            var sampleMessage = new MailMessage
            {
                Date = date ?? DateTime.UtcNow,
                MimeMessageId = internalId,
                MimeReplyToId = null,
                ChainId = internalId,
                ReplyTo = "",
                To = string.Join(", ", to.ToArray()),
                Cc = cc.Any() ? string.Join(", ", cc.ToArray()) : "",
                Bcc = bcc.Any() ? string.Join(", ", bcc.ToArray()) : "",
                Subject = string.IsNullOrEmpty(subject) ? LOREM_IPSUM_SUBJECT : subject,
                Important = importance,
                TextBodyOnly = false,
                Attachments = new List<MailAttachmentData>(),
                Size = sampleBody.Length,
                MailboxId = mbox.MailBoxId,
                HtmlBody = sampleBody,
                Introduction = sampleIntro,
                Folder = folder,
                RestoreFolderId = restoreFolder,
                IsNew = unread,
                StreamId = MailUtil.CreateStreamId(),
                CalendarUid = calendarUid
            };

            if (!string.IsNullOrEmpty(fromAddress))
            {
                var from = Parser.ParseAddress(fromAddress);

                sampleMessage.From = from.ToString();
                sampleMessage.FromEmail = from.Email;
            }
            else
            {
                sampleMessage.From = MailUtil.CreateFullEmail(mbox.Name, mbox.EMail.Address);
                sampleMessage.FromEmail = mbox.EMail.Address;
            }

            if (tagIds != null && tagIds.Any())
            {
                sampleMessage.TagIds = tagIds;
            }

            MessageEngine.StoreMailBody(mbox, sampleMessage, Log);

            var id = _engineFactory.MessageEngine.MailSave(mbox, sampleMessage, 0, folder, restoreFolder, userFolderId,
                SAMPLE_UIDL, "", false);

            if (!add2Index) 
                return id;

            var message = _engineFactory.MessageEngine.GetMessage(id, new MailMessageData.Options());

            message.IsNew = unread;

            var wrapper = message.ToMailWrapper(mbox.TenantId, new Guid(mbox.UserId));

            _engineFactory.IndexEngine.Add(wrapper);

            return id;
        }

        public int CreateReplyToSampleMessage(int id, string body, bool add2Index = false)
        {
            var message = _engineFactory.MessageEngine.GetMessage(id, new MailMessage.Options());

            if (message == null)
                throw new ArgumentException("Message with id not found");

            var mbox = _engineFactory.MailboxEngine.GetMailboxData(
                new СoncreteUserMailboxExp(message.MailboxId, TenantId, Username));

            if (mbox == null)
                throw new ArgumentException("Mailbox not found");

            var mimeMessageId = MailUtil.CreateMessageId();

            var sampleMessage = new MailMessage
            {
                Date = DateTime.UtcNow,
                MimeMessageId = mimeMessageId,
                MimeReplyToId = message.MimeMessageId,
                ChainId = message.MimeMessageId,
                ReplyTo = message.FromEmail,
                To = message.FromEmail,
                Cc = "",
                Bcc = "",
                Subject = "Re: " + message.Subject,
                Important = message.Important,
                TextBodyOnly = false,
                Attachments = new List<MailAttachmentData>(),
                Size = body.Length,
                MailboxId = mbox.MailBoxId,
                HtmlBody = body,
                Introduction = body,
                Folder = FolderType.Sent,
                RestoreFolderId = FolderType.Sent,
                IsNew = false,
                StreamId = MailUtil.CreateStreamId(),
                From = MailUtil.CreateFullEmail(mbox.Name, mbox.EMail.Address),
                FromEmail = mbox.EMail.Address
            };

            MessageEngine.StoreMailBody(mbox, sampleMessage, Log);

            var replyId = _engineFactory.MessageEngine.MailSave(mbox, sampleMessage, 0, FolderType.Sent, FolderType.Sent, null,
                SAMPLE_REPLY_UIDL, "", false);

            if (!add2Index)
                return replyId;

            var replyMessage = _engineFactory.MessageEngine.GetMessage(replyId, new MailMessageData.Options());

            var wrapper = replyMessage.ToMailWrapper(mbox.TenantId, new Guid(mbox.UserId));

            _engineFactory.IndexEngine.Add(wrapper);

            return replyId;
        }

        public MailAttachmentData AppendAttachmentsToSampleMessage(
            int? messageId, string filename, Stream stream, string contentType)
        {
            if (!messageId.HasValue || messageId.Value <= 0)
                throw new ArgumentException(@"Invalid message id", "messageId");

            var message = _engineFactory.MessageEngine.GetMessage(messageId.Value, new MailMessage.Options());

            if (message == null)
                throw new AttachmentsException(AttachmentsException.Types.MessageNotFound, "Message not found.");

            if (!message.Uidl.Equals(SAMPLE_UIDL))
                throw new Exception("Message is not api sample.");

            if (string.IsNullOrEmpty(filename))
                throw new Exception("File name is empty.");

            if (stream == null)
                throw new Exception("File stream is empty.");

            contentType = string.IsNullOrEmpty(contentType) ? MimeMapping.GetMimeMapping(filename) : contentType;

            return _engineFactory.AttachmentEngine.AttachFile(TenantId, Username, message, filename, stream, stream.Length, contentType);
        }

        public int LoadSampleMessage(
            int? folderId,
            uint? userFolderId,
            int? mailboxId,
            bool unread,
            Stream emlStream, 
            bool add2Index = false)
        {
            var folder = folderId.HasValue ? (FolderType) folderId.Value : FolderType.Inbox;

            if (!MailFolder.IsIdOk(folder))
                throw new ArgumentException(@"Invalid folder id", "folderId");

            if (!mailboxId.HasValue)
                throw new ArgumentException(@"Invalid mailbox id", "mailboxId");

            var accounts = _engineFactory.AccountEngine.GetAccountInfoList().ToAccountData().ToList();

            var account = mailboxId.HasValue
                ? accounts.FirstOrDefault(a => a.MailboxId == mailboxId)
                : accounts.FirstOrDefault(a => a.IsDefault) ?? accounts.FirstOrDefault();

            if (account == null)
                throw new ArgumentException("Mailbox not found");

            var mbox = _engineFactory.MailboxEngine.GetMailboxData(
                new СoncreteUserMailboxExp(account.MailboxId, TenantId, Username));

            if (mbox == null)
                throw new ArgumentException("no such mailbox");

            var mimeMessage = MailClient.ParseMimeMessage(emlStream);

            var message = MessageEngine.Save(mbox, mimeMessage, SAMPLE_UIDL, new MailFolder(folder, ""), userFolderId,
                unread, Log);

            if (message == null)
                return -1;

            if (!add2Index)
                return message.Id;

            _engineFactory.IndexEngine.Add(message.ToMailWrapper(mbox.TenantId, new Guid(mbox.UserId)));

            return message.Id;
        }
    }
}
