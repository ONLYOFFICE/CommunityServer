using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ASC.Core;
using ASC.Core.Notify.Signalr;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Mail.Aggregator.Core.Clients;
using ASC.Mail.Aggregator.Exceptions;
using MimeKit;

namespace ASC.Mail.Aggregator.Core
{
    public class DraftManager
    {
        public readonly MailBoxManager manager;
        public readonly ILogger log;
        private static SignalrServiceClient _signalrServiceClient;
        private readonly bool _isAutoreply;
        private readonly bool _sslCertificatePermit;
        private const string EMPTY_HTML_BODY = "<div dir=\"ltr\"><br></div>"; // GMail style

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

        public DraftManager(MailBoxManager manager, ILogger log, DeliveryFailureMessageTranslates daemonLabels = null,
            bool isAutoreply = false)
        {
            DaemonLabels = daemonLabels ?? DeliveryFailureMessageTranslates.Defauilt;
            this.manager = manager;
            this.log = log;
            _isAutoreply = isAutoreply;
            _sslCertificatePermit = ConfigurationManager.AppSettings["mail.certificate-permit"] != null &&
                                    Convert.ToBoolean(ConfigurationManager.AppSettings["mail.certificate-permit"]);

            if (_signalrServiceClient != null) return;
            _signalrServiceClient = new SignalrServiceClient("mail");
        }

        #region .Public

        public MailMessage Save(MailDraft draft)
        {
           return manager.SaveDraft(draft);
        }

        public long Send(MailDraft draft)
        {
            if (string.IsNullOrEmpty(draft.HtmlBody))
                draft.HtmlBody = EMPTY_HTML_BODY;

            var message = Save(draft);

            if (message.Id <= 0)
                throw new ArgumentException(string.Format("DraftManager-Send: Invalid message.Id = {0}", message.Id));

            ValidateAddresses(DraftFieldTypes.From, new[] {draft.From}, true);
            ValidateAddresses(DraftFieldTypes.To, draft.To, true);
            ValidateAddresses(DraftFieldTypes.Cc, draft.Cc, false);
            ValidateAddresses(DraftFieldTypes.Bcc, draft.Bcc, false);

            var scheme = HttpContext.Current == null
                ? Uri.UriSchemeHttp
                : HttpContext.Current.Request.GetUrlRewriter().Scheme;

            manager.SetDraftSending(draft);

            Task.Run(() =>
            {
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(draft.Mailbox.TenantId);

                    SecurityContext.AuthenticateMe(new Guid(draft.Mailbox.UserId));

                    draft.ChangeEmbeddedAttachmentLinks(log);

                    draft.ChangeSmileLinks(log);

                    draft.ChangeAttachedFileLinksAddresses(log);

                    draft.ChangeAttachedFileLinksImages(log);

                    if (!string.IsNullOrEmpty(draft.CalendarIcs))
                    {
                        draft.ChangeAllImagesLinksToEmbedded(log);
                    }

                    draft.ChangeUrlProxyLinks(log);

                    var mimeMessage = draft.ToMimeMessage(!string.IsNullOrEmpty(draft.CalendarIcs), _isAutoreply);

                    using (
                        var mc = new MailClient(draft.Mailbox, CancellationToken.None,
                            certificatePermit: _sslCertificatePermit, log: log))
                    {
                        mc.Send(mimeMessage,
                            draft.Mailbox.Imap && !DisableImapSendSyncServers.Contains(draft.Mailbox.Server));
                    }

                    try
                    {
                        SaveIcsAttachment(draft, mimeMessage);

                        SendMailNotification(draft);

                        manager.ReleaseSendingDraftOnSuccess(draft, message);

                        manager.AddRelationshipEventForLinkedAccounts(draft.Mailbox, message, scheme, log);

                        manager.SaveEmailInData(draft.Mailbox, message, scheme, log);

                        SaveFrequentlyContactedAddress(draft.Mailbox.TenantId, draft.Mailbox.UserId, mimeMessage,
                            scheme);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Unexpected Error in Send() Id = {0}\r\nException: {1}",
                            message.Id, ex.ToString());
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Mail->Send failed: Exception: {0}", ex.ToString());

                    AddNotificationAlertToMailbox(draft, ex);

                    manager.ReleaseSendingDraftOnFailure(draft);

                    SendMailErrorNotification(draft);
                }
                finally
                {
                    if (_isAutoreply)
                    {
                        manager.SaveAutoreplyHistory(draft.Mailbox, message);
                    }
                }
            });

            return message.Id;
        }

        #endregion

        #region .Private

        private void SaveIcsAttachment(MailDraft draft, MimeMessage mimeMessage)
        {
            if (string.IsNullOrEmpty(draft.CalendarIcs)) return;

            try
            {
                var icsAttachment =
                    mimeMessage.Attachments.FirstOrDefault(
                        a => a.ContentType.IsMimeType("application", "ics"));

                if (icsAttachment == null)
                    return;

                using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(draft.CalendarIcs)))
                {                                                                                                          
                    manager.AttachFile(draft.Mailbox.TenantId, draft.Mailbox.UserId,
                        draft.Id, icsAttachment.ContentType.Name, memStream);
                }
            }
            catch (Exception ex)
            {
                log.Warn(string.Format("Problem with attach ICAL to message. mailId={0} Exception:\r\n{1}\r\n", draft.Id, ex));
            }
        }

        private static void ValidateAddresses(DraftFieldTypes fieldType, IEnumerable<string> addresses,
            bool strongValidation)
        {
            var invalidEmailFound = false;
            if (addresses != null)
            {
                if (addresses.Any(addr =>
                {
                    MailboxAddress address;
                    return !MailboxAddress.TryParse(ParserOptions.Default, addr, out address);
                }))
                    invalidEmailFound = true;

                if (invalidEmailFound)
                    throw new DraftException(DraftException.ErrorTypes.IncorrectField, "Incorrect email address",
                        fieldType);
            }
            else if (strongValidation)
                throw new DraftException(DraftException.ErrorTypes.EmptyField, "Empty email address in {0} field",
                    fieldType);
        }

        private void SendMailErrorNotification(MailDraft draft)
        {
            try
            {
                // send success notification
                _signalrServiceClient.SendMailNotification(draft.Mailbox.TenantId, draft.Mailbox.UserId, -1);
            }
            catch (Exception ex)
            {
                log.Error("Unexpected error with wcf signalrServiceClient: {0}, {1}", ex.Message, ex.StackTrace);
            }
        }

        private void SendMailNotification(MailDraft draft)
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
                log.Error("Unexpected error with wcf signalrServiceClient: {0}, {1}", ex.Message, ex.StackTrace);
            }
        }

        private void SaveFrequentlyContactedAddress(int tenant, string user, MimeMessage mimeMessage,
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

                var contacts = manager.GetContactsByContactInfo(tenant, user, ContactInfoType.Email, email, null);
                if (!contacts.Any())
                {
                    var emails = manager.SearchEmails(tenant, user, email, 1, scheme);
                    if (!emails.Any())
                    {
                        manager.SaveMailContact(tenant, user, recipient.Name, "",
                            new List<string> {email},
                            new List<string>(), ContactType.FrequentlyContacted);
                    }
                }

                treatedAddresses.Add(email);
            }
        }

        private List<string> DisableImapSendSyncServers
        {
            get
            {
                var config = ConfigurationManager.AppSettings["mail.disable-imap-send-sync-servers"] ?? "imap.googlemail.com|imap.gmail.com";
                return string.IsNullOrEmpty(config) ? new List<string>() : config.Split('|').ToList();
            }
        }

        private void AddNotificationAlertToMailbox(MailDraft draft, Exception exOnSanding)
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

                var messageDelivery = new MailDraft(0, draft.Mailbox, DaemonLabels.DaemonEmail,
                    new List<string>() {draft.From}, new List<string>(), new List<string>(),
                    DaemonLabels.SubjectLabel,
                    MailUtil.CreateStreamId(), "", true, new List<int>(), sbMessage.ToString(), MailUtil.CreateStreamId(),
                    new List<MailAttachment>());

                // SaveToDraft To Inbox
                var notifyMessageItem = messageDelivery.ToMailMessage();
                notifyMessageItem.ChainId = notifyMessageItem.MimeMessageId;
                notifyMessageItem.IsNew = true;

                manager.StoreMailBody(draft.Mailbox, notifyMessageItem);

                var mailDaemonMessageid = manager.MailSave(draft.Mailbox, notifyMessageItem, 0, MailFolder.Ids.inbox, MailFolder.Ids.inbox,
                    string.Empty, string.Empty, false);

                manager.CreateDeliveryFailureAlert(
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
                log.Error("AddNotificationAlertToMailbox() in MailboxId={0} failed with exception:\r\n{1}",
                    draft.Mailbox.MailBoxId, exError.ToString());
            }
        }

        #endregion
    }
}
