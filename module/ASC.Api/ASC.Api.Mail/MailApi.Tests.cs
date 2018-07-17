using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Mail.Extensions;
using ASC.Common.Web;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Exceptions;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.Core;
using ASC.Mail.Aggregator.Core.Clients;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        private const string SAMPLE_UIDL = "api sample";
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

        /// <summary>
        /// Create sample message [Tests]
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="mailboxId"></param>
        /// <param name="to"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="importance"></param>
        /// <param name="unread"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns>Id message</returns>
        /// <category>Tests</category>
        /// <visible>false</visible>
        [Create(@"messages/sample/create")]
        public int CreateSampleMessage(
            int? folderId,
            int? mailboxId,
            List<string> to,
            List<string> cc,
            List<string> bcc,
            bool importance,
            bool unread,
            string subject,
            string body)
        {
            if (!folderId.HasValue)
                folderId = MailFolder.Ids.inbox;

            if (folderId < MailFolder.Ids.inbox || folderId > MailFolder.Ids.spam)
                throw new ArgumentException(@"Invalid folder id", "folderId");

            if (!mailboxId.HasValue)
                throw new ArgumentException(@"Invalid mailbox id", "mailboxId");

            var accounts = MailBoxManager.GetAccountInfo(TenantId, Username).ToAddressData();

            var account = mailboxId.HasValue
                ? accounts.FirstOrDefault(a => a.MailboxId == mailboxId)
                : accounts.FirstOrDefault(a => a.IsDefault) ?? accounts.FirstOrDefault();

            if (account == null)
                throw new ArgumentException("Mailbox not found");

            var mbox = MailBoxManager.GetUnremovedMailBox(account.MailboxId);

            if (mbox == null)
                throw new ArgumentException("no such mailbox");

            var mimeMessageId = MailUtil.CreateMessageId();

            var restoreFolderId = folderId.Value == MailFolder.Ids.spam || folderId.Value == MailFolder.Ids.trash
                ? MailFolder.Ids.inbox
                : folderId.Value;

            string sampleBody;
            string sampleIntro;

            if (!to.Any())
            {
                to = new List<string> { mbox.EMail.Address };
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
                Date = DateTime.UtcNow,
                MimeMessageId = mimeMessageId,
                MimeReplyToId = null,
                ChainId = mimeMessageId,
                ReplyTo = "",
                From = MailUtil.CreateFullEmail(mbox.Name, mbox.EMail.Address),
                FromEmail = mbox.EMail.Address,
                To = string.Join(", ", to.ToArray()),
                Cc = cc.Any() ? string.Join(", ", cc.ToArray()) : "",
                Bcc = bcc.Any() ? string.Join(", ", bcc.ToArray()) : "",
                Subject = string.IsNullOrEmpty(subject) ? LOREM_IPSUM_SUBJECT : subject,
                Important = importance,
                TextBodyOnly = false,
                Attachments = new List<MailAttachment>(),
                Size = sampleBody.Length,
                MailboxId = mbox.MailBoxId,
                HtmlBody = sampleBody,
                Introduction = sampleIntro,
                Folder = folderId.Value,
                RestoreFolderId = restoreFolderId,
                IsNew = unread,
                StreamId = MailUtil.CreateStreamId()
            };

            MailBoxManager.StoreMailBody(mbox, sampleMessage);

            var id = MailBoxManager.MailSave(mbox, sampleMessage, 0, folderId.Value, restoreFolderId, SAMPLE_UIDL, "",
                false);

            return id;
        }

        /// <summary>
        /// Append attachment to sample message [Tests]
        /// </summary>
        /// <param name="messageId">Id of any message</param>
        /// <param name="filename">File name</param>
        /// <param name="stream">File stream</param>
        /// <param name="contentType">File content type</param>
        /// <returns>Id message</returns>
        /// <category>Tests</category>
        /// <visible>false</visible>
        [Create(@"messages/sample/attachments/append")]
        public MailAttachment AppendAttachmentsToSampleMessage(
            int? messageId, string filename, Stream stream, string contentType)
        {
            if (!messageId.HasValue || messageId.Value <= 0)
                throw new ArgumentException(@"Invalid message id", "messageId");

            var message = MailBoxManager.GetMailInfo(TenantId, Username, messageId.Value, new MailMessage.Options());

            if (message == null)
                throw new AttachmentsException(AttachmentsException.Types.MessageNotFound, "Message not found.");

            if (!message.Uidl.Equals(SAMPLE_UIDL))
                throw new Exception("Message is not api sample.");

            if (string.IsNullOrEmpty(filename))
                throw new Exception("File name is empty.");

            if (stream == null)
                throw new Exception("File stream is empty.");

            contentType = string.IsNullOrEmpty(contentType) ? MimeMapping.GetMimeMapping(filename) : contentType;

            return MailBoxManager.AttachFile(TenantId, Username, message, filename, stream, contentType);
        }

        /// <summary>
        /// Load sample message from EML [Tests]
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="mailboxId"></param>
        /// <param name="unread"></param>
        /// <param name="emlStream"></param>
        /// <returns>Id message</returns>
        /// <category>Tests</category>
        /// <visible>false</visible>
        [Create(@"messages/sample/eml/load")]
        public int LoadSampleMessage(
            int? folderId,
            int? mailboxId,
            bool unread,
            Stream emlStream)
        {
            if (!folderId.HasValue)
                folderId = MailFolder.Ids.inbox;

            if (folderId < MailFolder.Ids.inbox || folderId > MailFolder.Ids.spam)
                throw new ArgumentException(@"Invalid folder id", "folderId");

            if (!mailboxId.HasValue)
                throw new ArgumentException(@"Invalid mailbox id", "mailboxId");

            var accounts = MailBoxManager.GetAccountInfo(TenantId, Username).ToAddressData();

            var account = mailboxId.HasValue
                ? accounts.FirstOrDefault(a => a.MailboxId == mailboxId)
                : accounts.FirstOrDefault(a => a.IsDefault) ?? accounts.FirstOrDefault();

            if (account == null)
                throw new ArgumentException("Mailbox not found");

            var mbox = MailBoxManager.GetUnremovedMailBox(account.MailboxId);

            if (mbox == null)
                throw new ArgumentException("no such mailbox");

            var mimeMessage = MailClient.ParseMimeMessage(emlStream);

            var message = MailRepository.Save(mbox, mimeMessage, SAMPLE_UIDL, new MailFolder(folderId.Value, ""), unread, _log);

            if (message == null)
                return -1;

            return (int) message.Id;
        }
    }
}
