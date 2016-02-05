using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using ActiveUp.Net.Mail;
using ASC.Core;
using ASC.Core.Notify.Signalr;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Exceptions;
using ASC.Mail.Aggregator.Extension;

namespace ASC.Mail.Aggregator.Managers
{
    public class DraftManager
    {
        public readonly MailBoxManager manager;
        public readonly ILogger log;
        private static SignalrServiceClient _signalrServiceClient;
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

        public DraftManager(MailBoxManager manager, ILogger log, DeliveryFailureMessageTranslates daemonLabels = null)
        {
            DaemonLabels = daemonLabels ?? DeliveryFailureMessageTranslates.Defauilt;
            this.manager = manager;
            this.log = log;
            if (_signalrServiceClient != null) return;
            var enableSignalr = string.IsNullOrEmpty(ConfigurationManager.AppSettings["web.hub"]) ? "false" : "true";
            _signalrServiceClient = new SignalrServiceClient(enableSignalr);
        }

        #region .Public

        public MailMessage Save(MailDraft draft)
        {
           return manager.SaveDraft(draft);
        }

        public int Send(MailDraft draft)
        {
            if (string.IsNullOrEmpty(draft.HtmlBody))
                draft.HtmlBody = EMPTY_HTML_BODY;

            var message = Save(draft);

            if (message.Id > 0)
            {
                ValidateAddresses(DraftFieldTypes.From, new[] { draft.From }, true);
                ValidateAddresses(DraftFieldTypes.To, draft.To, true);
                ValidateAddresses(DraftFieldTypes.Cc, draft.Cc, false);
                ValidateAddresses(DraftFieldTypes.Bcc, draft.Bcc, false);

                var scheme = HttpContext.Current == null ? Uri.UriSchemeHttp : HttpContext.Current.Request.GetUrlRewriter().Scheme;

                manager.SetDraftSending(draft);

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        CoreContext.TenantManager.SetCurrentTenant(draft.Mailbox.TenantId);

                        SecurityContext.AuthenticateMe(new Guid(draft.Mailbox.UserId));

                        ApiHelper.SetupScheme(scheme);

                        draft.ChangeEmbededAttachmentLinks(log);

                        draft.ChangeSmileLinks(log);

                        draft.ChangeAttachedFileLinksAddresses(log);

                        draft.ChangeAttachedFileLinksImages(log);

                        var mimeMessage = draft.ToMimeMessage(true);

                        var smtp = MailClientBuilder.Smtp();

                        smtp.Send(draft.Mailbox, mimeMessage, log);

                        try
                        {
                            SendMailNotification(draft.Mailbox.TenantId, draft.Mailbox.UserId, 0);

                            manager.ReleaseSendingDraftOnSuccess(draft, message);

                            manager.AddRelationshipEventForLinkedAccounts(draft.Mailbox, message, log);

                            manager.SaveEmailInData(draft.Mailbox, message, log);

                            manager.SaveMailContacts(draft.Mailbox.TenantId, draft.Mailbox.UserId, mimeMessage);

                            StoreMessageToImapSentFolder(draft.Mailbox, mimeMessage);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Unexpected Error in Send() Id = {0}\r\nException: {1}",
                                message.Id, ex.ToString());
                        }

                    }
                    catch (Exception ex)
                    {
                        AddNotificationAlertToMailbox(draft, ex);

                        manager.ReleaseSendingDraftOnFailure(draft);

                        SendMailNotification(draft.Mailbox.TenantId, draft.Mailbox.UserId, 1);
                    }

                });
            }
            else
            {
                throw new ArgumentException("Failed to_addresses save draft");
            }

            return message.Id > 0 ? (Int32)message.Id : 1; // Callback in api will be raised if value > 0
        }

        #endregion

        #region .Private

        private static void ValidateAddresses(DraftFieldTypes fieldType, IEnumerable<string> addresses, bool strongValidation)
        {
            var invalidEmailFound = false;
            if (addresses != null)
            {
                if (addresses.Any(addr => !Validator.ValidateSyntax(addr)))
                    invalidEmailFound = true;

                if (invalidEmailFound)
                    throw new DraftException(DraftException.ErrorTypes.IncorrectField, "Incorrect email address", fieldType);
            }
            else if (strongValidation)
                throw new DraftException(DraftException.ErrorTypes.EmptyField, "Empty email address in {0} field", fieldType);
        }

        private void SendMailNotification(int tenant, string userId, int state)
        {
            try
            {
                // send success notification
                _signalrServiceClient.SendMailNotification(tenant, userId, state);
            }
            catch (Exception ex)
            {
                log.Error("Unexpected error with wcf signalrServiceClient: {0}, {1}", ex.Message, ex.StackTrace);
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

        private void StoreMessageToImapSentFolder(MailBox mbox, Message mimeMessage)
        {
            if (mimeMessage == null || !mbox.Imap || DisableImapSendSyncServers.Contains(mbox.Server))
                return;

            var imap = MailClientBuilder.Imap();
            try
            {
                imap.AuthenticateImap(mbox, log);

                // reverse folders and order them to download tagged incoming messages first
                // gmail returns tagged letters in mailboxes & duplicate them in inbox
                // to retrieve tags - first we need to download files from "sub" mailboxes
                var sentFolder =
                    imap.GetImapMailboxes(mbox.Server, MailQueueItemSettings.DefaultFolders, MailQueueItemSettings.SpecialDomainFolders,
                                          MailQueueItemSettings.SkipImapFlags, MailQueueItemSettings.ImapFlags)
                        .FirstOrDefault(m => m.folder_id == MailFolder.Ids.sent);

                if (sentFolder == null)
                    throw new InvalidDataException(String.Format("Cannot find Sent folder over Imap. MailboxId={0}",
                                                                 mbox.MailBoxId));

                var mailbox = imap.SelectMailbox(sentFolder.name);

                var flags = new FlagCollection { "Seen" };

                var response = mailbox.Append(mimeMessage, flags);

                log.Info("StoreMessageToImapSentFolder() in MailboxId={0} succeed! Returned: '{0}'", response);
            }
            catch (Exception ex)
            {
                log.Error("StoreMessageToSentFolder() in MailboxId={0} failed with exception:\r\n{1}",
                           mbox.MailBoxId, ex.ToString());
            }
            finally
            {
                try
                {
                    if (imap.IsConnected)
                    {
                        imap.Disconnect();
                    }
                }
                catch
                {
                }
            }
        }

        private void AddNotificationAlertToMailbox(MailDraft draft, Exception exOnSanding)
        {
            try
            {
                var sbMessage = new StringBuilder(1024);

                sbMessage.Append(@"<style>
                                            .button.blue:hover {
                                            color: white;
                                            background: #57A7D3;
                                            background: linear-gradient(top, #78BFE8, #57A7D3 50%, #57A7D3 51%, #3F96C3);
                                            background: -o-linear-gradient(top, #78BFE8, #57A7D3 50%, #57A7D3 51%, #3F96C3);
                                            background: -moz-linear-gradient(top, #78BFE8, #57A7D3 50%, #57A7D3 51%, #3F96C3);
                                            background: -webkit-linear-gradient(top, #78BFE8, #57A7D3 50%, #57A7D3 51%, #3F96C3);
                                            border: 1px solid #5EAAD5;
                                            }
                                            .button.blue {
                                            color: white;
                                            background: #3D96C6;
                                            background: linear-gradient(top, #59B1E2, #3D96C6 50%, #3D96C6 51%, #1A76A6);
                                            background: -o-linear-gradient(top, #59B1E2, #3D96C6 50%, #3D96C6 51%, #1A76A6);
                                            background: -moz-linear-gradient(top, #59B1E2, #3D96C6 50%, #3D96C6 51%, #1A76A6);
                                            background: -webkit-linear-gradient(top, #59B1E2, #3D96C6 50%, #3D96C6 51%, #1A76A6);
                                            border-width: 1px;
                                            border-style: solid;
                                            border-color: #4DA9DC #4098C9 #2D7399 #4098C9;
                                            }
                                            .button, .button:visited, .button:hover, .button:active {
                                            display: inline-block;
                                            font-weight: normal;
                                            text-align: center;
                                            text-decoration: none;
                                            vertical-align: middle;
                                            cursor: pointer;
                                            border-radius: 3px;
                                            -moz-border-radius: 3px;
                                            -webkit-border-radius: 3px;
                                            touch-callout: none;
                                            -o-touch-callout: none;
                                            -moz-touch-callout: none;
                                            -webkit-touch-callout: none;
                                            user-select: none;
                                            -o-user-select: none;
                                            -moz-user-select: none;
                                            -webkit-user-select: none;
                                            font-size: 12px;
                                            line-height: 14px;
                                            padding: 2px 12px 3px;
                                            color: white;
                                            background: #3D96C6;
                                            background: linear-gradient(top, #59B1E2, #3D96C6 50%, #3D96C6 51%, #1A76A6);
                                            background: -o-linear-gradient(top, #59B1E2, #3D96C6 50%, #3D96C6 51%, #1A76A6);
                                            background: -moz-linear-gradient(top, #59B1E2, #3D96C6 50%, #3D96C6 51%, #1A76A6);
                                            background: -webkit-linear-gradient(top, #59B1E2, #3D96C6 50%, #3D96C6 51%, #1A76A6);
                                            border-width: 1px;
                                            border-style: solid;
                                            border-color: #4DA9DC #4098C9 #2D7399 #4098C9;
                                            }
                                            body {
                                            color: #333;
                                            font: normal 12px Arial, Tahoma,sans-serif;
                                            }
                                            </style>")
                    .AppendFormat("<div style=\"max-width:500px;\"><p style=\"color:gray;\">{0}</p>",
                        DaemonLabels.AutomaticMessageLabel)
                    .AppendFormat("<p>{0}</p>", DaemonLabels.MessageIdentificator
                        .Replace("{subject}", draft.Subject)
                        .Replace("{date}", DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                    .AppendFormat("<div><p>{0}:</p><ul style=\"color:#333;\">",
                        DaemonLabels.RecipientsLabel);

                draft.To.ForEach(rcpt => sbMessage.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(rcpt)));
                draft.Cc.ForEach(rcpt => sbMessage.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(rcpt)));
                draft.Bcc.ForEach(rcpt => sbMessage.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(rcpt)));

                sbMessage
                    .AppendFormat("</ul>")
                    .AppendFormat("<p>{0}</p>",
                        DaemonLabels.RecommendationsLabel
                            .Replace("{account_name}", "<b>" + draft.From + "</b>"))
                    .AppendFormat(
                        "<a id=\"delivery_failure_button\" mailid={0} class=\"button blue\" style=\"margin-right:8px;\">{1}</a></div>",
                        draft.Id, DaemonLabels.TryAgainButtonLabel)
                    .AppendFormat("<p>{0}</p>",
                        DaemonLabels.FaqInformationLabel
                            .Replace("{url_begin}",
                                "<a id=\"delivery_failure_faq_link\" target=\"blank\" href=\"#\">")
                            .Replace("{url_end}", "</a>"));

                var lastDotIndex = exOnSanding.Message.LastIndexOf('.');
                var smtpResponse = exOnSanding.Message;

                if (lastDotIndex != -1 && lastDotIndex != smtpResponse.Length)
                {
                    try
                    {
                        smtpResponse = smtpResponse.Remove(lastDotIndex + 1, smtpResponse.Length - lastDotIndex - 1);
                    }
                    catch (Exception)
                    {
                    }
                }

                sbMessage.AppendFormat("<p style=\"color:gray;\">{0}: \"{1}\"</p></div>", DaemonLabels.ReasonLabel,
                    smtpResponse);

                draft.Mailbox.Name = "";

                var messageDelivery = new MailDraft(0, draft.Mailbox, DaemonLabels.DaemonEmail,
                    new List<string>() {draft.From}, new List<string>(), new List<string>(),
                    DaemonLabels.SubjectLabel,
                    manager.CreateStreamId(), "", true, new List<int>(), sbMessage.ToString(), manager.CreateStreamId(),
                    new List<MailAttachment>());

                // SaveToDraft To Inbox
                var notifyMessageItem = messageDelivery.ToMailMessage();
                notifyMessageItem.ChainId = notifyMessageItem.MimeMessageId;
                notifyMessageItem.IsNew = true;
                notifyMessageItem.IsFromCRM = false;
                notifyMessageItem.IsFromTL = false;

                manager.StoreMailBody(draft.Mailbox.TenantId, draft.Mailbox.UserId, notifyMessageItem);

                manager.MailSave(draft.Mailbox, notifyMessageItem, 0, MailFolder.Ids.inbox, MailFolder.Ids.inbox,
                    string.Empty, string.Empty, false);

                manager.CreateDeliveryFailureAlert(draft.Mailbox.TenantId,
                    draft.Mailbox.UserId,
                    draft.Subject,
                    draft.From,
                    draft.Id);
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
