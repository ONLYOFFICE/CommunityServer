/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
    public class DraftEngine
    {
        public ILog Log { get; private set; }
        private static SignalrServiceClient _signalrServiceClient;
        private readonly bool _sslCertificatePermit;
        private const string EMPTY_HTML_BODY = "<div dir=\"ltr\"><br></div>"; // GMail style

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

        public DraftEngine(int tenant, string user, DeliveryFailureMessageTranslates daemonLabels = null, ILog log = null)
        {
            Tenant = tenant;
            User = user;
            
            Log = log ?? LogManager.GetLogger("ASC.Mail.DraftEngine");
            
            DaemonLabels = daemonLabels ?? DeliveryFailureMessageTranslates.Defauilt;

            _sslCertificatePermit = Defines.SslCertificatesErrorPermit;

            if (_signalrServiceClient != null) return;
            _signalrServiceClient = new SignalrServiceClient("mail");
        }

        #region .Public

        public MailMessage Save(
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
                throw new ArgumentException("no such mailbox");

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

                mimeMessageId = message.MimeMessageId;

                streamId = message.StreamId;

                foreach (var attachment in attachments)
                {
                    attachment.streamId = streamId;
                }

                previousMailboxId = message.MailboxId;
            }
            else
            {
                mimeMessageId = MailUtil.CreateMessageId();
                streamId = MailUtil.CreateStreamId();
            }

            var fromAddress = MailUtil.CreateFullEmail(mbox.Name, mbox.EMail.Address);

            var draft = new MailDraftData(id, mbox, fromAddress, to, cc, bcc, subject, mimeMessageId, mimeReplyToId, importance,
                tags, body, streamId, attachments, calendarIcs)
            {
                PreviousMailboxId = previousMailboxId
            };

            DaemonLabels = translates ?? DeliveryFailureMessageTranslates.Defauilt;

            return Save(draft);
        }

        public MailMessage Save(MailComposeBase draft)
        {
            var embededAttachmentsForSaving = FixHtmlBodyWithEmbeddedAttachments(draft);

            var message = draft.ToMailMessage();

            var engine = new EngineFactory(draft.Mailbox.TenantId, draft.Mailbox.UserId);

            var addIndex = draft.Id == 0;

            var needRestoreAttachments = draft.Id == 0 && message.Attachments.Any();

            if (needRestoreAttachments)
            {
                message.Attachments.ForEach(
                    attachment =>
                        engine.AttachmentEngine.StoreAttachmentCopy(draft.Mailbox.TenantId, draft.Mailbox.UserId,
                            attachment, draft.StreamId));
            }

            MessageEngine.StoreMailBody(draft.Mailbox, message, Log);

            long usedQuota;

            using (var daoFactory = new DaoFactory())
            {
                var db = daoFactory.DbManager;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    draft.Id = engine.MessageEngine.MailSave(daoFactory, draft.Mailbox, message, draft.Id, message.Folder, message.Folder, null,
                        string.Empty, string.Empty, false, out usedQuota);

                    message.Id = draft.Id;

                    if (draft.AccountChanged)
                    {
                        engine.ChainEngine.UpdateChain(daoFactory, message.ChainId, message.Folder, null, draft.PreviousMailboxId,
                            draft.Mailbox.TenantId, draft.Mailbox.UserId);
                    }

                    var daoMailInfo = daoFactory.CreateMailInfoDao(draft.Mailbox.TenantId, draft.Mailbox.UserId);

                    if (draft.Id > 0 && needRestoreAttachments)
                    {
                        var daoAttachment = daoFactory.CreateAttachmentDao(draft.Mailbox.TenantId, draft.Mailbox.UserId);

                        foreach (var attachment in message.Attachments)
                        {
                            var attach = attachment.ToAttachmnet(draft.Id);
                            attach.Id = 0;

                            var newId = daoAttachment.SaveAttachment(attach);
                            attachment.fileId = newId;
                        }

                        if (message.Attachments.Any())
                        {
                            var count = daoAttachment.GetAttachmentsCount(
                                new ConcreteMessageAttachmentsExp(draft.Id, draft.Mailbox.TenantId, draft.Mailbox.UserId));

                            daoMailInfo.SetFieldValue(
                                SimpleMessagesExp.CreateBuilder(draft.Mailbox.TenantId, draft.Mailbox.UserId)
                                    .SetMessageId(draft.Id)
                                    .Build(),
                                MailTable.Columns.AttachCount,
                                count);
                        }
                    }

                    if (draft.Id > 0 && embededAttachmentsForSaving.Any())
                    {
                        var daoAttachment = daoFactory.CreateAttachmentDao(draft.Mailbox.TenantId, draft.Mailbox.UserId);

                        foreach (var attachment in embededAttachmentsForSaving)
                        {
                            var newId = daoAttachment.SaveAttachment(attachment.ToAttachmnet(draft.Id));
                            attachment.fileId = newId;
                        }

                        if (message.Attachments.Any())
                        {
                            var count = daoAttachment.GetAttachmentsCount(
                                new ConcreteMessageAttachmentsExp(draft.Id, draft.Mailbox.TenantId, draft.Mailbox.UserId));

                            daoMailInfo.SetFieldValue(
                                SimpleMessagesExp.CreateBuilder(draft.Mailbox.TenantId, draft.Mailbox.UserId)
                                    .SetMessageId(draft.Id)
                                    .Build(),
                                MailTable.Columns.AttachCount,
                                count);
                        }
                    }

                    engine.ChainEngine.UpdateChain(daoFactory, message.ChainId, message.Folder, null, draft.Mailbox.MailBoxId, draft.Mailbox.TenantId,
                        draft.Mailbox.UserId);

                    if (draft.AccountChanged)
                    {
                        var daoCrmLink = daoFactory.CreateCrmLinkDao(draft.Mailbox.TenantId, draft.Mailbox.UserId);

                        daoCrmLink.UpdateCrmLinkedMailboxId(message.ChainId, draft.PreviousMailboxId,
                            draft.Mailbox.MailBoxId);
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
                engine.IndexEngine.Add(message.ToMailWrapper(draft.Mailbox.TenantId, new Guid(draft.Mailbox.UserId)));
            }
            else
            {
                engine.IndexEngine.Update(new List<MailWrapper>
                {
                    message.ToMailWrapper(draft.Mailbox.TenantId,
                        new Guid(draft.Mailbox.UserId))
                });
            }

            return message;
        }

        public long Send(int id,
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
            Files.Core.Security.FileShare fileLinksShareMode,
            string calendarIcs,
            bool isAutoreply,
            bool requestReceipt,
            bool requestRead,
            DeliveryFailureMessageTranslates translates = null)
        {
            if (id < 1)
                id = 0;

            if (string.IsNullOrEmpty(from))
                throw new ArgumentNullException("from");

            if (!to.Any())
                throw new ArgumentNullException("to");

            var mailAddress = new MailAddress(from);

            var engine = new EngineFactory(Tenant, User);

            var accounts = engine.AccountEngine.GetAccountInfoList().ToAccountData();

            var account = accounts.FirstOrDefault(a => a.Email.ToLower().Equals(mailAddress.Address));

            if (account == null)
                throw new ArgumentException("Mailbox not found");

            if (account.IsGroup)
                throw new InvalidOperationException("Sending emails from a group address is forbidden");

            var mbox = engine.MailboxEngine.GetMailboxData(
                new СoncreteUserMailboxExp(account.MailboxId, Tenant, User));

            if (mbox == null)
                throw new ArgumentException("no such mailbox");

            if (!mbox.Enabled)
                throw new InvalidOperationException("Sending emails from a disabled account is forbidden");

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
                    throw new InvalidOperationException("Sending emails is permitted only in the Drafts folder");
                }

                mimeMessageId = message.MimeMessageId;

                streamId = message.StreamId;

                foreach (var attachment in attachments)
                {
                    attachment.streamId = streamId;
                }

                previousMailboxId = message.MailboxId;
            }
            else
            {
                mimeMessageId = MailUtil.CreateMessageId();
                streamId = MailUtil.CreateStreamId();
            }

            var fromAddress = MailUtil.CreateFullEmail(mbox.Name, mailAddress.Address);

            var draft = new MailDraftData(id, mbox, fromAddress, to, cc, bcc, subject, mimeMessageId, mimeReplyToId,
                importance, tags, body, streamId, attachments, calendarIcs)
            {
                FileLinksShareMode = fileLinksShareMode,
                PreviousMailboxId = previousMailboxId,
                RequestReceipt = requestReceipt,
                RequestRead = requestRead,
                IsAutogenerated =  !string.IsNullOrEmpty(calendarIcs),
                IsAutoreplied = isAutoreply
            };

            DaemonLabels = translates ?? DeliveryFailureMessageTranslates.Defauilt;

            return Send(draft);
        }

        public long Send(MailDraftData draft)
        {
            if (string.IsNullOrEmpty(draft.HtmlBody))
                draft.HtmlBody = EMPTY_HTML_BODY;

            var message = Save(draft);

            if (message.Id <= 0)
                throw new ArgumentException(string.Format("DraftManager-Send: Invalid message.Id = {0}", message.Id));

            ValidateAddresses(DraftFieldTypes.From, new List<string> {draft.From}, true);

            message.ToList = ValidateAddresses(DraftFieldTypes.To, draft.To, true);
            message.CcList = ValidateAddresses(DraftFieldTypes.Cc, draft.Cc, false);
            message.BccList = ValidateAddresses(DraftFieldTypes.Bcc, draft.Bcc, false);

            var scheme = HttpContext.Current == null
                ? Uri.UriSchemeHttp
                : HttpContext.Current.Request.GetUrlRewriter().Scheme;

            SetDraftSending(draft);

            Task.Run(() =>
            {
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(draft.Mailbox.TenantId);

                    SecurityContext.AuthenticateMe(new Guid(draft.Mailbox.UserId));

                    draft.ChangeEmbeddedAttachmentLinks(Log);

                    draft.ChangeSmileLinks(Log);

                    draft.ChangeAttachedFileLinksAddresses(Log);

                    draft.ChangeAttachedFileLinksImages(Log);

                    if (!string.IsNullOrEmpty(draft.CalendarIcs))
                    {
                        draft.ChangeAllImagesLinksToEmbedded(Log);
                    }

                    draft.ChangeUrlProxyLinks(Log);

                    var mimeMessage = draft.ToMimeMessage();

                    using (var mc = new MailClient(draft.Mailbox, CancellationToken.None,
                        certificatePermit: draft.Mailbox.IsTeamlab || _sslCertificatePermit, log: Log,
                        enableDsn: draft.RequestReceipt))
                    {
                        mc.Send(mimeMessage,
                            draft.Mailbox.Imap && !DisableImapSendSyncServers.Contains(draft.Mailbox.Server));
                    }

                    try
                    {
                        SaveIcsAttachment(draft, mimeMessage);

                        SendMailNotification(draft);

                        ReleaseSendingDraftOnSuccess(draft, message);

                        var factory = new EngineFactory(draft.Mailbox.TenantId, draft.Mailbox.UserId, Log);

                        factory.CrmLinkEngine.AddRelationshipEventForLinkedAccounts(draft.Mailbox, message, scheme);

                        factory.EmailInEngine.SaveEmailInData(draft.Mailbox, message, scheme);

                        SaveFrequentlyContactedAddress(draft.Mailbox.TenantId, draft.Mailbox.UserId, mimeMessage,
                            scheme);

                        var filters = factory.FilterEngine.GetList();

                        if (filters.Any())
                        {
                            factory.FilterEngine.ApplyFilters(message, draft.Mailbox, new MailFolder(FolderType.Sent, ""), filters);
                        }

                        factory.IndexEngine.Update(new List<MailWrapper>
                        {
                            message.ToMailWrapper(draft.Mailbox.TenantId,
                                new Guid(draft.Mailbox.UserId))
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat("Unexpected Error in Send() Id = {0}\r\nException: {1}",
                            message.Id, ex.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Mail->Send failed: Exception: {0}", ex.ToString());

                    AddNotificationAlertToMailbox(draft, ex);

                    ReleaseSendingDraftOnFailure(draft);

                    SendMailErrorNotification(draft);
                }
                finally
                {
                    if (draft.IsAutoreplied)
                    {
                        var engineFactory = new EngineFactory(draft.Mailbox.TenantId, draft.Mailbox.UserId, Log);

                        engineFactory
                            .AutoreplyEngine
                            .SaveAutoreplyHistory(draft.Mailbox, message);
                    }
                }
            });

            return message.Id;
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

        private void SetDraftSending(MailDraftData draft)
        {
            var engine = new EngineFactory(draft.Mailbox.TenantId, draft.Mailbox.UserId);

            engine.ChainEngine.SetConversationsFolder(new List<int> { draft.Id }, FolderType.Sending);
        }

        private void ReleaseSendingDraftOnSuccess(MailDraftData draft, MailMessage message)
        {
            var engine = new EngineFactory(draft.Mailbox.TenantId, draft.Mailbox.UserId);

            using (var daoFactory = new DaoFactory())
            {
                var db = daoFactory.DbManager;

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    // message was correctly send - lets update its chains id
                    var draftChainId = message.ChainId;
                    // before moving message from draft to sent folder - lets recalculate its correct chain id
                    var chainInfo = engine.MessageEngine.DetectChain(daoFactory, draft.Mailbox,
                        message.MimeMessageId, message.MimeReplyToId, message.Subject);

                    message.ChainId = chainInfo.Id;

                    if (message.ChainId.Equals(message.MimeMessageId))
                        message.MimeReplyToId = null;

                    var daoMailInfo = daoFactory.CreateMailInfoDao(draft.Mailbox.TenantId, draft.Mailbox.UserId);

                    if (!draftChainId.Equals(message.ChainId))
                    {
                        daoMailInfo.SetFieldValue(
                            SimpleMessagesExp.CreateBuilder(Tenant, User)
                                .SetMessageId(message.Id)
                                .Build(),
                            MailTable.Columns.ChainId,
                            message.ChainId);

                        engine.ChainEngine.UpdateChain(daoFactory, draftChainId, FolderType.Sending, null, draft.Mailbox.MailBoxId,
                            draft.Mailbox.TenantId, draft.Mailbox.UserId);

                        var daoCrmLink = daoFactory.CreateCrmLinkDao(draft.Mailbox.TenantId, draft.Mailbox.UserId);

                        daoCrmLink.UpdateCrmLinkedChainId(draftChainId, draft.Mailbox.MailBoxId, message.ChainId);
                    }

                    engine.ChainEngine.UpdateChain(daoFactory, message.ChainId, FolderType.Sending, null, draft.Mailbox.MailBoxId,
                        draft.Mailbox.TenantId, draft.Mailbox.UserId);

                    var listObjects = engine.ChainEngine.GetChainedMessagesInfo(daoFactory, new List<int> {draft.Id});

                    if (!listObjects.Any())
                        return;

                    engine.MessageEngine.SetFolder(daoFactory, listObjects, FolderType.Sent);

                    daoMailInfo.SetFieldValue(
                        SimpleMessagesExp.CreateBuilder(Tenant, User)
                            .SetMessageId(draft.Id)
                            .Build(),
                        MailTable.Columns.FolderRestore,
                        FolderType.Sent);

                    tx.Commit();
                }
            }
        }

        private void ReleaseSendingDraftOnFailure(MailDraftData draft)
        {
            var engine = new EngineFactory(draft.Mailbox.TenantId, draft.Mailbox.UserId);

            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var listObjects = engine.ChainEngine.GetChainedMessagesInfo(daoFactory, new List<int> {draft.Id});

                    if (!listObjects.Any())
                        return;

                    engine.MessageEngine.SetFolder(daoFactory, listObjects, FolderType.Draft);

                    tx.Commit();
                }
            }
        }

        private void SaveIcsAttachment(MailDraftData draft, MimeMessage mimeMessage)
        {
            if (string.IsNullOrEmpty(draft.CalendarIcs)) return;

            try
            {
                var icsAttachment =
                    mimeMessage.Attachments.FirstOrDefault(
                        a => a.ContentType.IsMimeType("application", "ics"));

                if (icsAttachment == null)
                    return;

                var engine = new EngineFactory(draft.Mailbox.TenantId, draft.Mailbox.UserId);

                using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(draft.CalendarIcs)))
                {
                    engine.AttachmentEngine
                        .AttachFileToDraft(draft.Mailbox.TenantId, draft.Mailbox.UserId, draft.Id,
                            icsAttachment.ContentType.Name, memStream, memStream.Length);
                }
            }
            catch (Exception ex)
            {
                Log.Warn(string.Format("Problem with attach ICAL to message. mailId={0} Exception:\r\n{1}\r\n", draft.Id, ex));
            }
        }

        private static List<MailAddress> ValidateAddresses(DraftFieldTypes fieldType, List<string> addresses,
            bool strongValidation)
        {
            if (addresses == null || !addresses.Any())
            {
                if (strongValidation)
                {
                    throw new DraftException(DraftException.ErrorTypes.EmptyField, "Empty email address in {0} field",
                        fieldType);
                }

                return null;
            }

            try
            {
                return addresses.ToMailAddresses();
            }
            catch (Exception ex)
            {
                throw new DraftException(DraftException.ErrorTypes.IncorrectField, ex.Message, fieldType);
            }
        }

        private void SendMailErrorNotification(MailDraftData draft)
        {
            try
            {
                // send success notification
                _signalrServiceClient.SendMailNotification(draft.Mailbox.TenantId, draft.Mailbox.UserId, -1);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error with wcf signalrServiceClient: {0}, {1}", ex.Message, ex.StackTrace);
            }
        }

        private void SendMailNotification(MailDraftData draft)
        {
            try
            {
                var state = 0;
                if (!string.IsNullOrEmpty(draft.CalendarIcs))
                {
                    switch (draft.CalendarMethod)
                    {
                        case Defines.ICAL_REQUEST:
                            state = 1;
                            break;
                        case Defines.ICAL_REPLY:
                            state = 2;
                            break;
                        case Defines.ICAL_CANCEL:
                            state = 3;
                            break;
                    }
                }

                // send success notification
                _signalrServiceClient.SendMailNotification(draft.Mailbox.TenantId, draft.Mailbox.UserId, state);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error with wcf signalrServiceClient: {0}, {1}", ex.Message, ex.StackTrace);
            }
        }

        private static void SaveFrequentlyContactedAddress(int tenant, string user, MimeMessage mimeMessage,
            string scheme)
        {
            var recipients = new List<MailboxAddress>();
            recipients.AddRange(mimeMessage.To.Mailboxes);
            recipients.AddRange(mimeMessage.Cc.Mailboxes);
            recipients.AddRange(mimeMessage.Bcc.Mailboxes);

            var treatedAddresses = new List<string>();
            foreach (var recipient in recipients)
            {
                var email = recipient.Address;
                if (treatedAddresses.Contains(email))
                    continue;

                var engine = new EngineFactory(tenant, user);

                var exp = new FullFilterContactsExp(tenant, user, searchTerm: email, infoType: ContactInfoType.Email);

                var contacts = engine.ContactEngine.GetContactCards(exp);

                if (!contacts.Any())
                {
                    var emails = engine.ContactEngine.SearchEmails(tenant, user, email, 1, scheme);
                    if (!emails.Any())
                    {
                        var contactCard = new ContactCard(0, tenant, user, recipient.Name, "",
                            ContactType.FrequentlyContacted, new[] {email});

                        engine.ContactEngine.SaveContactCard(contactCard);
                    }
                }

                treatedAddresses.Add(email);
            }
        }

        private static List<string> DisableImapSendSyncServers
        {
            get
            {
                var config = ConfigurationManager.AppSettings["mail.disable-imap-send-sync-servers"] ?? "imap.googlemail.com|imap.gmail.com";
                return string.IsNullOrEmpty(config) ? new List<string>() : config.Split('|').ToList();
            }
        }

        private void AddNotificationAlertToMailbox(MailDraftData draft, Exception exOnSanding)
        {
            try
            {
                var sbMessage = new StringBuilder(1024);

                sbMessage
                    .AppendFormat("<div style=\"max-width:500px;font: normal 12px Arial, Tahoma,sans-serif;\"><p style=\"color:gray;font: normal 12px Arial, Tahoma,sans-serif;\">{0}</p>",
                        DaemonLabels.AutomaticMessageLabel)
                    .AppendFormat("<p style=\"font: normal 12px Arial, Tahoma,sans-serif;\">{0}</p>", DaemonLabels.MessageIdentificator
                        .Replace("{subject}", draft.Subject)
                        .Replace("{date}", DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                    .AppendFormat("<div><p style=\"font: normal 12px Arial, Tahoma,sans-serif;\">{0}:</p><ul style=\"color:#333;font: normal 12px Arial, Tahoma,sans-serif;\">",
                        DaemonLabels.RecipientsLabel);

                draft.To.ForEach(rcpt => sbMessage.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(rcpt)));
                draft.Cc.ForEach(rcpt => sbMessage.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(rcpt)));
                draft.Bcc.ForEach(rcpt => sbMessage.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(rcpt)));

                sbMessage
                    .AppendFormat("</ul>")
                    .AppendFormat("<p style=\"font: normal 12px Arial, Tahoma,sans-serif;\">{0}</p>",
                        DaemonLabels.RecommendationsLabel
                            .Replace("{account_name}", "<b>" + draft.From + "</b>"))
                    .AppendFormat(
                        "<a id=\"delivery_failure_button\" mailid={0} class=\"button blue\" style=\"margin-right:8px;\">{1}</a></div>",
                        draft.Id, DaemonLabels.TryAgainButtonLabel)
                    .AppendFormat("<p style=\"font: normal 12px Arial, Tahoma,sans-serif;\">{0}</p>",
                        DaemonLabels.FaqInformationLabel
                            .Replace("{url_begin}",
                                "<a id=\"delivery_failure_faq_link\" target=\"blank\" href=\"#\" class=\"link underline\">")
                            .Replace("{url_end}", "</a>"));

                const int max_length = 300;

                var smtpResponse = string.IsNullOrEmpty(exOnSanding.Message)
                    ? "no response"
                    : exOnSanding.Message.Length > max_length
                        ? exOnSanding.Message.Substring(0, max_length)
                        : exOnSanding.Message;

                sbMessage.AppendFormat("<p style=\"color:gray;font: normal 12px Arial, Tahoma,sans-serif;\">{0}: \"{1}\"</p></div>", DaemonLabels.ReasonLabel,
                    smtpResponse);

                draft.Mailbox.Name = "";

                var messageDelivery = new MailDraftData(0, draft.Mailbox, DaemonLabels.DaemonEmail,
                    new List<string>() {draft.From}, new List<string>(), new List<string>(),
                    DaemonLabels.SubjectLabel,
                    MailUtil.CreateStreamId(), "", true, new List<int>(), sbMessage.ToString(), MailUtil.CreateStreamId(),
                    new List<MailAttachmentData>());

                // SaveToDraft To Inbox
                var notifyMessageItem = messageDelivery.ToMailMessage();
                notifyMessageItem.ChainId = notifyMessageItem.MimeMessageId;
                notifyMessageItem.IsNew = true;

                MessageEngine.StoreMailBody(draft.Mailbox, notifyMessageItem, Log);

                var engine = new EngineFactory(draft.Mailbox.TenantId, draft.Mailbox.UserId);

                var mailDaemonMessageid = engine.MessageEngine.MailSave(draft.Mailbox, notifyMessageItem, 0,
                    FolderType.Inbox, FolderType.Inbox, null,
                    string.Empty, string.Empty, false);

                engine.AlertEngine.CreateDeliveryFailureAlert(
                    draft.Mailbox.TenantId,
                    draft.Mailbox.UserId,
                    draft.Mailbox.MailBoxId,
                    draft.Subject,
                    draft.From,
                    draft.Id,
                    mailDaemonMessageid);
            }
            catch (Exception exError)
            {
                Log.ErrorFormat("AddNotificationAlertToMailbox() in MailboxId={0} failed with exception:\r\n{1}",
                    draft.Mailbox.MailBoxId, exError.ToString());
            }
        }

        private List<MailAttachmentData> FixHtmlBodyWithEmbeddedAttachments(MailComposeBase draft)
        {
            var embededAttachmentsForSaving = new List<MailAttachmentData>();

            var embeddedLinks = draft.GetEmbeddedAttachmentLinks();
            if (!embeddedLinks.Any())
                return embededAttachmentsForSaving;

            var fckStorage = StorageManager.GetDataStoreForCkImages(draft.Mailbox.TenantId);
            var attachmentStorage = StorageManager.GetDataStoreForAttachments(draft.Mailbox.TenantId);
            //todo: replace selector
            var currentMailFckeditorUrl = fckStorage.GetUri(StorageManager.CKEDITOR_IMAGES_DOMAIN, "").ToString();
            var currentUserStorageUrl = MailStoragePathCombiner.GetUserMailsDirectory(draft.Mailbox.UserId);

            var engine = new EngineFactory(draft.Mailbox.TenantId, draft.Mailbox.UserId);

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
                        streamId = draft.StreamId,
                        user = draft.Mailbox.UserId,
                        tenant = draft.Mailbox.TenantId,
                        mailboxId = draft.Mailbox.MailBoxId
                    };

                    var savedAttachment =
                        engine.AttachmentEngine.GetAttachment(
                            new ConcreteContentAttachmentExp(draft.Id, attach.contentId));

                    var savedAttachmentId = savedAttachment == null ? 0 : savedAttachment.fileId;

                    var attachmentWasSaved = savedAttachmentId != 0;
                    var currentImgStorage = isFckImage ? fckStorage : attachmentStorage;
                    var domain = isFckImage
                                     ? StorageManager.CKEDITOR_IMAGES_DOMAIN
                                     //todo: must be string.Empty
                                     : draft.Mailbox.UserId;

                    if (draft.Id == 0 || !attachmentWasSaved)
                    {
                        attach.data = StorageManager.LoadDataStoreItemData(domain, fileLink, currentImgStorage);

                        if (storage == null)
                        {
                            storage = new StorageManager(draft.Mailbox.TenantId, draft.Mailbox.UserId);
                        }

                        storage.StoreAttachmentWithoutQuota(attach);

                        embededAttachmentsForSaving.Add(attach);
                    }

                    if (attachmentWasSaved)
                    {
                        attach = engine.AttachmentEngine.GetAttachment(
                            new ConcreteUserAttachmentExp(savedAttachmentId, draft.Mailbox.TenantId, draft.Mailbox.UserId));

                        var path = MailStoragePathCombiner.GerStoredFilePath(attach);
                        currentImgStorage = attachmentStorage;
                        attach.storedFileUrl =
                            MailStoragePathCombiner.GetStoredUrl(currentImgStorage.GetUri(path));
                    }

                    draft.HtmlBody = draft.HtmlBody.Replace(embeddedLink, attach.storedFileUrl);

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
