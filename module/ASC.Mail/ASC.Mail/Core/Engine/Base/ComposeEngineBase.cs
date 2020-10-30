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
    public class ComposeEngineBase
    {
        public ILog Log { get; set; }
        public static SignalrServiceClient _signalrServiceClient;
        public readonly bool _isAutoreply;
        public readonly bool _sslCertificatePermit;
        public const string EMPTY_HTML_BODY = "<div dir=\"ltr\"><br></div>"; // GMail style

        public int Tenant { get; private set; }
        public string User { get; private set; }

        public class DeliveryFailureMessageTranslates
        {
            public string DaemonEmail { get; set; }
            public string SubjectLabel { get; set; }
            public string AutomaticMessageLabel { get; set; }
            public string MessageIdentificator { get; set; }
            public string RecipientsLabel { get; set; }
            public string RecommendationsLabel { get; set; }
            public string TryAgainButtonLabel { get; set; }
            public string FaqInformationLabel { get; set; }
            public string ReasonLabel { get; set; }

            public DeliveryFailureMessageTranslates(string daemonEmail,
                string subjectLabel,
                string automaticMessageLabel,
                string messageIdentificator,
                string recipientsLabel,
                string recommendationsLabel,
                string tryAgainButtonLabel,
                string faqInformationLabel,
                string reasonLabel
                )
            {
                DaemonEmail = daemonEmail;
                SubjectLabel = subjectLabel;
                AutomaticMessageLabel = automaticMessageLabel;
                MessageIdentificator = messageIdentificator;
                RecipientsLabel = recipientsLabel;
                RecommendationsLabel = recommendationsLabel;
                TryAgainButtonLabel = tryAgainButtonLabel;
                FaqInformationLabel = faqInformationLabel;
                ReasonLabel = reasonLabel;
            }

            public static DeliveryFailureMessageTranslates Defauilt
            {
                get
                {
                    return new DeliveryFailureMessageTranslates("mail-daemon@onlyoffice.com",
                        "Message Delivery Failure",
                        "This message was created automatically by mail delivery software.",
                        "Delivery failed for message with subject \"{subject}\" from {date}.",
                        "Message could not be delivered to recipient(s)",
                        "Please, check your message recipients addresses and message format. " +
                        "If you are sure your message is correct, check all the {account_name} account settings, " +
                        "and, if everything is correct, sign in to the mail service you use and confirm any " +
                        "verification questions, in case there are some. After then try again.",
                        "Change your message",
                        "In case the error persists, please read the {url_begin}FAQ section{url_end} " +
                        "to learn more about the problem.",
                        "Reason");
                }
            }
        }

        public DeliveryFailureMessageTranslates DaemonLabels { get; internal set; }

        public ComposeEngineBase(int tenant, string user, DeliveryFailureMessageTranslates daemonLabels = null, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.ComposeEngineBase");

            DaemonLabels = daemonLabels ?? DeliveryFailureMessageTranslates.Defauilt;

            _sslCertificatePermit = Defines.SslCertificatesErrorPermit;

            if (_signalrServiceClient != null) return;
            _signalrServiceClient = new SignalrServiceClient("mail");
        }

        #region .Public

        public virtual MailMessage Save(
            int id,
            string from,
            List<string> to,
            List<string> cc,
            List<string> bcc,
            string mimeReplyToId,
            bool importance,
            string subject,
            List<int> tags,
            string body,
            List<MailAttachmentData> attachments,
            string calendarIcs,
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

                if (message.Folder != FolderType.Draft)
                {
                    throw new InvalidOperationException("Saving emails is permitted only in the Drafts folder");
                }

                if (message.HtmlBody.Length > Defines.MaximumMessageBodySize)
                {
                    throw new InvalidOperationException("Message body exceeded limit (" + Defines.MaximumMessageBodySize / 1024 + " KB)");
                }

                mimeMessageId = message.MimeMessageId;

                streamId = message.StreamId;

                previousMailboxId = message.MailboxId;
            }
            else
            {
                mimeMessageId = MailUtil.CreateMessageId();
                streamId = MailUtil.CreateStreamId();
            }

            var fromAddress = MailUtil.CreateFullEmail(mbox.Name, mbox.EMail.Address);

            var compose = new MailDraftData(id, mbox, fromAddress, to, cc, bcc, subject, mimeMessageId, mimeReplyToId, importance,
                    tags, body, streamId, attachments, calendarIcs) { PreviousMailboxId = previousMailboxId };

            DaemonLabels = translates ?? DeliveryFailureMessageTranslates.Defauilt;

            return Save(compose);
        }

        public MailMessage Save(MailComposeBase compose)
        {
            var embededAttachmentsForSaving = FixHtmlBodyWithEmbeddedAttachments(compose);

            var message = compose.ToMailMessage();

            var engine = new EngineFactory(compose.Mailbox.TenantId, compose.Mailbox.UserId);

            var addIndex = compose.Id == 0;

            var attachmentsToRestore = message.Attachments.Where(att => att.streamId != message.StreamId || att.isTemp).ToList();

            var needRestoreAttachments = attachmentsToRestore.Any();

            if (needRestoreAttachments)
            {
                message.Attachments.ForEach(
                    attachment =>
                        engine.AttachmentEngine.StoreAttachmentCopy(compose.Mailbox.TenantId, compose.Mailbox.UserId,
                            attachment, compose.StreamId));
            }

            MessageEngine.StoreMailBody(compose.Mailbox, message, Log);

            long usedQuota;

            using (var daoFactory = new DaoFactory())
            {
                var db = daoFactory.DbManager;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    compose.Id = engine.MessageEngine.MailSave(daoFactory, compose.Mailbox, message, compose.Id, message.Folder, message.Folder, null,
                        string.Empty, string.Empty, false, out usedQuota);

                    message.Id = compose.Id;

                    if (compose.AccountChanged)
                    {
                        engine.ChainEngine.UpdateChain(daoFactory, message.ChainId, message.Folder, null, compose.PreviousMailboxId,
                            compose.Mailbox.TenantId, compose.Mailbox.UserId);
                    }

                    var daoMailInfo = daoFactory.CreateMailInfoDao(compose.Mailbox.TenantId, compose.Mailbox.UserId);

                    if (compose.Id > 0 && needRestoreAttachments)
                    {
                        var daoAttachment = daoFactory.CreateAttachmentDao(compose.Mailbox.TenantId, compose.Mailbox.UserId);
                        var existingAttachments = daoAttachment.GetAttachments(
                            new ConcreteMessageAttachmentsExp(compose.Id, compose.Mailbox.TenantId, compose.Mailbox.UserId));

                        foreach (var attachment in message.Attachments)
                        {
                            if (existingAttachments.Any(x => x.Id == attachment.fileId ||
                            (x.Stream == attachment.streamId && x.StoredName == attachment.storedName)))
                            {
                                continue;
                            }

                            var attach = attachment.ToAttachmnet(compose.Id);
                            attach.Id = 0;

                            var newId = daoAttachment.SaveAttachment(attach);
                            attachment.fileId = newId;
                        }

                        if (message.Attachments.Any())
                        {
                            var count = daoAttachment.GetAttachmentsCount(
                                new ConcreteMessageAttachmentsExp(compose.Id, compose.Mailbox.TenantId, compose.Mailbox.UserId));

                            daoMailInfo.SetFieldValue(
                                SimpleMessagesExp.CreateBuilder(compose.Mailbox.TenantId, compose.Mailbox.UserId)
                                    .SetMessageId(compose.Id)
                                    .Build(),
                                MailTable.Columns.AttachCount,
                                count);
                        }
                    }

                    if (compose.Id > 0 && embededAttachmentsForSaving.Any())
                    {
                        var daoAttachment = daoFactory.CreateAttachmentDao(compose.Mailbox.TenantId, compose.Mailbox.UserId);

                        foreach (var attachment in embededAttachmentsForSaving)
                        {
                            var newId = daoAttachment.SaveAttachment(attachment.ToAttachmnet(compose.Id));
                            attachment.fileId = newId;
                        }

                        if (message.Attachments.Any())
                        {
                            var count = daoAttachment.GetAttachmentsCount(
                                new ConcreteMessageAttachmentsExp(compose.Id, compose.Mailbox.TenantId, compose.Mailbox.UserId));

                            daoMailInfo.SetFieldValue(
                                SimpleMessagesExp.CreateBuilder(compose.Mailbox.TenantId, compose.Mailbox.UserId)
                                    .SetMessageId(compose.Id)
                                    .Build(),
                                MailTable.Columns.AttachCount,
                                count);
                        }
                    }

                    engine.ChainEngine.UpdateChain(daoFactory, message.ChainId, message.Folder, null, compose.Mailbox.MailBoxId, compose.Mailbox.TenantId,
                        compose.Mailbox.UserId);

                    if (compose.AccountChanged)
                    {
                        var daoCrmLink = daoFactory.CreateCrmLinkDao(compose.Mailbox.TenantId, compose.Mailbox.UserId);

                        daoCrmLink.UpdateCrmLinkedMailboxId(message.ChainId, compose.PreviousMailboxId,
                            compose.Mailbox.MailBoxId);
                    }

                    tx.Commit();

                }
            }

            if (usedQuota > 0)
            {
                engine.QuotaEngine.QuotaUsedDelete(usedQuota);
            }

            if (addIndex)
            {
                engine.IndexEngine.Add(message.ToMailWrapper(compose.Mailbox.TenantId, new Guid(compose.Mailbox.UserId)));
            }
            else
            {
                engine.IndexEngine.Update(new List<MailWrapper>
                {
                    message.ToMailWrapper(compose.Mailbox.TenantId,
                        new Guid(compose.Mailbox.UserId))
                });
            }

            try
            {
                var tempStorage = MailDataStore.GetDataStore(compose.Mailbox.TenantId);

                tempStorage.DeleteDirectory("attachments_temp", compose.Mailbox.UserId + "/" + compose.StreamId + "/");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Clearing temp storage failed with exception: {0}", ex.ToString());
            }

            return message;
        }

        public MailMessage GetTemplate()
        {
            var template = new MailMessage
            {
                Attachments = new List<MailAttachmentData>(),
                Bcc = "",
                Cc = "",
                Subject = "",
                From = "",
                HtmlBody = "",
                Important = false,
                ReplyTo = "",
                MimeMessageId = "",
                MimeReplyToId = "",
                To = "",
                StreamId = MailUtil.CreateStreamId()
            };

            return template;
        }

        #endregion

        #region .Private

        private List<MailAttachmentData> FixHtmlBodyWithEmbeddedAttachments(MailComposeBase compose)
        {
            var embededAttachmentsForSaving = new List<MailAttachmentData>();

            var embeddedLinks = compose.GetEmbeddedAttachmentLinks();
            if (!embeddedLinks.Any())
                return embededAttachmentsForSaving;

            var fckStorage = StorageManager.GetDataStoreForCkImages(compose.Mailbox.TenantId);
            var attachmentStorage = StorageManager.GetDataStoreForAttachments(compose.Mailbox.TenantId);
            var currentMailFckeditorUrl = fckStorage.GetUri(StorageManager.CKEDITOR_IMAGES_DOMAIN, "").ToString();
            var currentUserStorageUrl = MailStoragePathCombiner.GetUserMailsDirectory(compose.Mailbox.UserId);

            var engine = new EngineFactory(compose.Mailbox.TenantId, compose.Mailbox.UserId);

            StorageManager storage = null;
            foreach (var embeddedLink in embeddedLinks)
            {
                try
                {
                    var isFckImage = embeddedLink.StartsWith(currentMailFckeditorUrl);
                    var prefixLength = isFckImage
                        ? currentMailFckeditorUrl.Length
                        : embeddedLink.IndexOf(currentUserStorageUrl, StringComparison.Ordinal) +
                          currentUserStorageUrl.Length + 1;
                    var fileLink = HttpUtility.UrlDecode(embeddedLink.Substring(prefixLength));
                    var fileName = Path.GetFileName(fileLink);
                    var attach = new MailAttachmentData
                    {
                        fileName = fileName,
                        storedName = fileName,
                        contentId = embeddedLink.GetMd5(),
                        storedFileUrl = fileLink,
                        streamId = compose.StreamId,
                        user = compose.Mailbox.UserId,
                        tenant = compose.Mailbox.TenantId,
                        mailboxId = compose.Mailbox.MailBoxId
                    };

                    var savedAttachment =
                        engine.AttachmentEngine.GetAttachment(
                            new ConcreteContentAttachmentExp(compose.Id, attach.contentId));

                    var savedAttachmentId = savedAttachment == null ? 0 : savedAttachment.fileId;

                    var attachmentWasSaved = savedAttachmentId != 0;
                    var currentImgStorage = isFckImage ? fckStorage : attachmentStorage;
                    var domain = isFckImage ? StorageManager.CKEDITOR_IMAGES_DOMAIN : compose.Mailbox.UserId;

                    if (compose.Id == 0 || !attachmentWasSaved)
                    {
                        attach.data = StorageManager.LoadDataStoreItemData(domain, fileLink, currentImgStorage);

                        if (storage == null)
                        {
                            storage = new StorageManager(compose.Mailbox.TenantId, compose.Mailbox.UserId);
                        }

                        storage.StoreAttachmentWithoutQuota(attach);

                        embededAttachmentsForSaving.Add(attach);
                    }

                    if (attachmentWasSaved)
                    {
                        attach = engine.AttachmentEngine.GetAttachment(
                            new ConcreteUserAttachmentExp(savedAttachmentId, compose.Mailbox.TenantId, compose.Mailbox.UserId));

                        var path = MailStoragePathCombiner.GerStoredFilePath(attach);
                        currentImgStorage = attachmentStorage;
                        attach.storedFileUrl =
                            MailStoragePathCombiner.GetStoredUrl(currentImgStorage.GetUri(path));
                    }

                    compose.HtmlBody = compose.HtmlBody.Replace(embeddedLink, attach.storedFileUrl);

                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("ChangeEmbededAttachmentLinksForStoring() failed with exception: {0}", ex.ToString());
                }
            }

            return embededAttachmentsForSaving;
        }

        #endregion
    }
}
