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
    public class DraftEngine : ComposeEngineBase
    {
        public DraftEngine(int tenant, string user, DeliveryFailureMessageTranslates daemonLabels = null, ILog log = null)
            : base(tenant, user, daemonLabels, log)
        {
            Log = log ?? LogManager.GetLogger("ASC.Mail.DraftEngine");
        }

        #region .Public

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
                throw new ArgumentException("No such mailbox");

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

                if (message.Folder != FolderType.Draft && message.Folder != FolderType.Templates)
                {
                    throw new InvalidOperationException("Sending emails is permitted only in the Drafts folder");
                }

                if (message.HtmlBody.Length > Defines.MaximumMessageBodySize)
                {
                    throw new InvalidOperationException("Message body exceeded limit (" + Defines.MaximumMessageBodySize / 1024 + " KB)");
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

            ValidateAddresses(DraftFieldTypes.From, new List<string> { draft.From }, true);

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
                            draft.Mailbox.Imap && !Defines.DisableImapSendSyncServers.Contains(draft.Mailbox.Server));
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

                    var listObjects = engine.ChainEngine.GetChainedMessagesInfo(daoFactory, new List<int> { draft.Id });

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
                    var listObjects = engine.ChainEngine.GetChainedMessagesInfo(daoFactory, new List<int> { draft.Id });

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
                            ContactType.FrequentlyContacted, new[] { email });

                        engine.ContactEngine.SaveContactCard(contactCard);
                    }
                }

                treatedAddresses.Add(email);
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
                    new List<string>() { draft.From }, new List<string>(), new List<string>(),
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

        #endregion
    }
}
