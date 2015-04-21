/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using ASC.Core;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Authorization;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ActiveUp.Net.Mail;
using ASC.Api.Mail.DAO;
using System.Linq;
using ASC.Api.Mail.Resources;
using ASC.Core.Notify.Signalr;
using System.Configuration;

namespace ASC.Api.Mail
{
    class MailSendQueue
    {
        public readonly MailBoxManager manager;
        public readonly ILogger log;
        private static SignalrServiceClient _signalrServiceClient;
        private const string EMPTY_HTML_BODY = "<div dir=\"ltr\"><br></div>"; // GMail style

        public MailSendQueue(MailBoxManager manager, ILogger log)
        {
            this.manager = manager;
            this.log = log;
            if (_signalrServiceClient != null) return;
            var enableSignalr = string.IsNullOrEmpty(ConfigurationManager.AppSettings["web.hub"]) ? "false" : "true";
            _signalrServiceClient = new SignalrServiceClient(enableSignalr);
        }

        private string GetAccessToken(MailBox mbox)
        {
            var serviceType = (AuthorizationServiceType)mbox.ServiceType;

            switch (serviceType)
            {
                case AuthorizationServiceType.Google:
                    var grantedAccess = new GoogleOAuth2Authorization(log)
                        .RequestAccessToken(mbox.RefreshToken);

                    if (grantedAccess != null)
                        return grantedAccess.AccessToken;
                    break;
            }

            return "";
        }

        public int Send(int tenant, string username, MailSendItem originalMessage, int mailId, int mailboxId)
        {
            var mbox = manager.GetUnremovedMailBox(mailboxId);
            if (mbox == null)
                throw new ArgumentException("no such mailbox");

            originalMessage.MailboxId = mbox.MailBoxId;

            if (mbox.Name != "")
            {
                originalMessage.DisplayName = mbox.Name;
            }

            if (string.IsNullOrEmpty(originalMessage.HtmlBody))
                originalMessage.HtmlBody = EMPTY_HTML_BODY;

            var messageItem = SaveToDraft(originalMessage, mailId, mbox);

            if (messageItem.Id > 0)
            {
                var userCulture = Thread.CurrentThread.CurrentCulture;
                var userUiCulture = Thread.CurrentThread.CurrentUICulture;
                var scheme = HttpContext.Current.Request.GetUrlRewriter().Scheme;
                // move to_addresses temp
                manager.SetConversationsFolder(tenant, username, MailFolder.Ids.temp,
                                                new List<int> { (Int32)messageItem.Id });
                manager.SetMessageFolderRestore(tenant, username, MailFolder.Ids.drafts,
                                                (int)messageItem.Id);
                ThreadPool.QueueUserWorkItem(delegate
                    {
                        Message mimeMessage;
                        try
                        {
                            Thread.CurrentThread.CurrentCulture = userCulture;
                            Thread.CurrentThread.CurrentUICulture = userUiCulture;

                            CoreContext.TenantManager.SetCurrentTenant(tenant);
                            SecurityContext.AuthenticateMe(new Guid(username));

                            ApiHelper.SetupScheme(scheme);

                            originalMessage.ChangeEmbededAttachmentLinks(tenant, username);
                            originalMessage.ChangeSmileLinks();

                            originalMessage.ChangeAttachedFileLinksAddresses(tenant);
                            originalMessage.ChangeAttachedFileLinksImages();
                            
                            mimeMessage = originalMessage.ToMimeMessage(tenant, username, true);

                            var smptClient = MailClientBuilder.Smtp();

                            if (mbox.RefreshToken != null)
                            {
                                smptClient.SendSsl(mimeMessage, mbox.SmtpServer, mbox.SmtpPort,
                                                                        mbox.SmtpAccount, GetAccessToken(mbox),
                                                                        SaslMechanism.OAuth2);
                            }
                            else if (mbox.OutcomingEncryptionType == EncryptionType.None)
                            {
                                if (mbox.AuthenticationTypeSmtp == SaslMechanism.None)
                                    smptClient.Send(mimeMessage, mbox.SmtpServer, mbox.SmtpPort);
                                else
                                    smptClient.Send(mimeMessage, mbox.SmtpServer, mbox.SmtpPort,
                                                                        mbox.SmtpAccount, mbox.SmtpPassword,
                                                                        mbox.AuthenticationTypeSmtp);
                            }
                            else
                            {
                                if (mbox.AuthenticationTypeSmtp == SaslMechanism.None)
                                    smptClient.SendSsl(mimeMessage, mbox.SmtpServer, mbox.SmtpPort,
                                                                            mbox.OutcomingEncryptionType);
                                else
                                    smptClient.SendSsl(mimeMessage, mbox.SmtpServer, mbox.SmtpPort,
                                                                            mbox.SmtpAccount, mbox.SmtpPassword,
                                                                            mbox.AuthenticationTypeSmtp,
                                                                            mbox.OutcomingEncryptionType);
                            }
                        }
                        catch (Exception ex)
                        {
                            AddNotificationAlertToMailbox(originalMessage, (Int32)messageItem.Id, ex, mbox);

                            // move to_addresses drafts
                            manager.SetConversationsFolder(tenant, username, MailFolder.Ids.drafts,
                                                            new List<int> { (Int32)messageItem.Id });
                            manager.SetMessageFolderRestore(tenant, username, MailFolder.Ids.drafts,
                                                            (int)messageItem.Id);

                            // send unsuccess notification
                            SendMailNotification(tenant, username, 1);

                            return;
                        }

                        SendMailNotification(tenant, username, 0);

                        try
                        {
                            // message was correctly send - lets update its chains id
                            var draftChainId = messageItem.ChainId;
                            // before moving message from draft to sent folder - lets recalculate its correct chain id
                            messageItem.ChainId = manager.DetectChainId(mbox, messageItem);
                            // push new message correct chain id to db
                            manager.UpdateMessageChainId(mbox, messageItem.Id, MailFolder.Ids.temp, draftChainId,
                                                            messageItem.ChainId);

                            manager.UpdateCrmLinkedChainId(mbox.MailBoxId, tenant, draftChainId,
                                                            messageItem.ChainId);

                            // move to_addresses sent
                            manager.SetConversationsFolder(tenant, username, MailFolder.Ids.sent,
                                                            new List<int> { (Int32)messageItem.Id });
                            manager.SetMessageFolderRestore(tenant, username, MailFolder.Ids.sent,
                                                            (int)messageItem.Id);

                            manager.AddRelationshipEventForLinkedAccounts(mbox, messageItem, log);

                            manager.SaveEmailInData(mbox, messageItem, log);

                            manager.SaveMailContacts(mbox.TenantId, mbox.UserId, mimeMessage);

                            StoreMessageToImapSentFolder(mbox, mimeMessage);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Unexpected Error in Send(), message_item.Id = {0}, {1}, {2}",
                                messageItem.Id, ex.ToString(), ex.StackTrace);
                        }
                    });
            }
            else
            {
                throw new ArgumentException("Failed to_addresses save draft");
            }

            return messageItem.Id > 0 ? (Int32) messageItem.Id : 1; // Callback in api will be raised if value > 0
        }

        public void SendMailNotification(int tenant, string userId, int state)
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

        public List<string> NoImapSendSyncServers
        {
            get
            {
                var config = ConfigurationManager.AppSettings["mail.no-imap-send-sync-servers"] ?? "";
                return string.IsNullOrEmpty(config) ? new List<string>() : config.Split('|').ToList();
            }
        }

        private void StoreMessageToImapSentFolder(MailBox mbox, Message mimeMessage)
        {
            if (mimeMessage == null || !mbox.Imap || NoImapSendSyncServers.Contains(mbox.Server))
                return;
            
            var imap = MailClientBuilder.Imap();
            try
            {
                imap.AuthenticateImap(mbox, log);

                // reverse folders and order them to download tagged incoming messages first
                // gmail returns tagged letters in mailboxes & duplicate them in inbox
                // to retrieve tags - first we need to download files from "sub" mailboxes
                var sentFolder =
                    imap.GetImapMailboxes(mbox.Server, MailQueueItemSettings.SpecialDomainFolders,
                                          MailQueueItemSettings.SkipImapFlags, MailQueueItemSettings.ImapFlags)
                        .FirstOrDefault(m => m.folder_id == MailFolder.Ids.sent);

                if (sentFolder == null)
                    throw new InvalidDataException(String.Format("Cannot find Sent folder over Imap. MailboxId={0}",
                                                                 mbox.MailBoxId));
                
                var mailbox = imap.SelectMailbox(sentFolder.name);

                var flags = new FlagCollection {"Seen"};

                var response = mailbox.Append(mimeMessage, flags);

                log.Info("StoreMessageToImapSentFolder() in MailboxId={0} successed! Returned: '{0}'", response);
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

        private void AddNotificationAlertToMailbox(MailSendItem originalMessage, int mailId, Exception exOnSanding, MailBox mbox)
        {
            try
            {
                var sbMessage = new StringBuilder(1024);
                var messageDelivery = new MailSendItem {Subject = MailApiResource.DeliveryFailureSubject};
                messageDelivery.To.Add(originalMessage.From);
                messageDelivery.From = MailBoxManager.MAIL_DAEMON_EMAIL;
                messageDelivery.Important = true;
                messageDelivery.StreamId = manager.CreateNewStreamId();
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
                                            </style>");

                sbMessage.AppendFormat("<div style=\"max-width:500px;\"><p style=\"color:gray;\">{0}</p>",
                                        MailApiResource.DeliveryFailureAutomaticMessage);

                sbMessage.AppendFormat("<p>{0}</p>",
                                        MailApiResource.DeliveryFailureMessageIdentificator
                                                       .Replace("{subject}", originalMessage.Subject)
                                                       .Replace("{date}", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

                sbMessage.AppendFormat("<div><p>{0}:</p><ul style=\"color:#333;\">",
                                        MailApiResource.DeliveryFailureRecipients);

                originalMessage.To.ForEach(rcpt =>sbMessage.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(rcpt)));
                originalMessage.Cc.ForEach(rcpt =>sbMessage.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(rcpt)));
                originalMessage.Bcc.ForEach(rcpt =>sbMessage.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(rcpt)));

                sbMessage.AppendFormat("</ul>");

                sbMessage.AppendFormat("<p>{0}</p>",
                                        MailApiResource.DeliveryFailureRecommendations
                                                       .Replace("{account_name}", "<b>" + originalMessage.From + "</b>"));

                sbMessage.AppendFormat(
                    "<a id=\"delivery_failure_button\" mailid={0} class=\"button blue\" style=\"margin-right:8px;\">",
                    mailId);
                sbMessage.Append(MailApiResource.DeliveryFailureBtn + "</a></div>");

                sbMessage.AppendFormat("<p>{0}</p>",
                                        MailApiResource.DeliveryFailureFAQInformation
                                                       .Replace("{url_begin}",
                                                                "<a id=\"delivery_failure_faq_link\" target=\"blank\" href=\"#gmail\">")
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

                sbMessage.AppendFormat(
                    "<p style=\"color:gray;\">" + MailApiResource.DeliveryFailureReason + ": \"{0}\"</p></div>", smtpResponse);

                messageDelivery.HtmlBody = sbMessage.ToString();
                // SaveToDraft To Inbox
                var notifyMessageItem = messageDelivery.ToMailMessageItem(mbox.TenantId, mbox.UserId);
                notifyMessageItem.ChainId = notifyMessageItem.MimeMessageId;
                notifyMessageItem.IsNew = true;
                notifyMessageItem.IsFromCRM = false;
                notifyMessageItem.IsFromTL = false;

                manager.StoreMailBody(mbox.TenantId, mbox.UserId, notifyMessageItem);

                manager.MailSave(mbox, notifyMessageItem, 0, MailFolder.Ids.inbox, MailFolder.Ids.inbox, string.Empty, string.Empty, false);

                    manager.CreateDeliveryFailureAlert(mbox.TenantId,
                                                        mbox.UserId,
                                                        originalMessage.Subject,
                                                        originalMessage.From,
                                                        mailId);
            }
            catch(Exception exError)
            {
                log.Error("AddNotificationAlertToMailbox() in MailboxId={0} failed with exception:\r\n{1}",
                           mbox.MailBoxId, exError.ToString());
            }
        }

        public MailMessageItem SaveToDraft(int tenant, string user, MailSendItem originalMessage, int mailId, int mailboxId)
        {
            var mbox = manager.GetUnremovedMailBox(mailboxId);

            if (mbox == null)
                throw new ArgumentException("no such mailbox");

            originalMessage.MailboxId = mbox.MailBoxId;

            return SaveToDraft(originalMessage, mailId, mbox);
        }

        private MailMessageItem SaveToDraft(MailSendItem originalMessage, int mailId, MailBox mbox)
        {
            originalMessage.DisplayName = mbox.Name;
            var embededAttachmentsForSaving = originalMessage.ChangeEmbededAttachmentLinksForStoring(mbox.TenantId, mbox.UserId, mailId, manager);
            var messageItem = originalMessage.ToMailMessageItem(mbox.TenantId, mbox.UserId);
            messageItem.IsNew = false;
            messageItem.Folder = MailFolder.Ids.drafts;

            messageItem.ChainId = messageItem.MimeMessageId;

            var needToRestoreAttachmentsFromFckLocation = mailId == 0 && messageItem.Attachments.Any();
            if (needToRestoreAttachmentsFromFckLocation)
            {
                messageItem.Attachments.ForEach(attachment => manager.StoreAttachmentCopy(mbox.TenantId, mbox.UserId, attachment, originalMessage.StreamId));
            }

            manager.StoreMailBody(mbox.TenantId, mbox.UserId, messageItem);

            var previousMailboxId = mailId == 0
                                          ? mbox.MailBoxId
                                          : manager.GetMailInfo(mbox.TenantId, mbox.UserId, mailId, false, false).MailboxId;

            mailId = manager.MailSave(mbox, messageItem, mailId, messageItem.Folder, messageItem.Folder, string.Empty, string.Empty, false);
            messageItem.Id = mailId;

            if (previousMailboxId != mbox.MailBoxId)
            {
                manager.UpdateChain(messageItem.ChainId, messageItem.Folder, previousMailboxId, mbox.TenantId, mbox.UserId);
            }

            if (mailId > 0 && needToRestoreAttachmentsFromFckLocation)
            {
                foreach (var attachment in messageItem.Attachments)
                {
                    var newId = manager.SaveAttachment(mbox.TenantId, mailId, attachment);
                    attachment.fileId = newId;
                }
            }

            if (mailId > 0 && embededAttachmentsForSaving.Any())
            {
                manager.SaveAttachments(mbox.TenantId, mailId, embededAttachmentsForSaving);
            }

            manager.UpdateChain(messageItem.ChainId, messageItem.Folder, mbox.MailBoxId, mbox.TenantId, mbox.UserId);

            if (previousMailboxId != mbox.MailBoxId)
                manager.UpdateCrmLinkedMailboxId(messageItem.ChainId, mbox.TenantId, previousMailboxId, mbox.MailBoxId);

            return messageItem;
        }
    }
}
