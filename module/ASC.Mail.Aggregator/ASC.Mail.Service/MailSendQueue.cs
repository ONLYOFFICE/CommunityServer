/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading;
using ASC.Mail.Aggregator;
using ASC.Mail.Service.DAO;
using ASC.Core;
using ASC.Mail.Aggregator.Collection;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Service
{
    public class MailSendQueue
    {
        private readonly MailBoxManager _manager;

        public MailSendQueue(MailBoxManager manager)
        {
            _manager = manager;
        }

        public int Send(int tenantId, string username, MailSendItem item, int mailId)
        {
            var mbox = _manager.GetMailBox(tenantId, username, new MailAddress(item.From));
            if (mbox.Name != "") item.DisplayName = (mbox.Name != "") ? mbox.Name : null;
            
            if (mbox == null)
                throw new ArgumentException("no such mailbox");

            string mime_message_id, in_reply_to;
            mailId = Save(tenantId, username, item, mailId, out mime_message_id, out in_reply_to);

            if (mailId != -1)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    ActiveUp.Net.Mail.Message message = null;
                    try
                    {
                        message = item.CreateMimeMessage(tenantId, username, true);
                        message.MessageId = mime_message_id;
                        message.InReplyTo = in_reply_to;

                        if (!mbox.UseSsl)
                        {
                            if (!mbox.SmtpAuth)
                                ActiveUp.Net.Mail.SmtpClient.Send(message, mbox.SmtpServer, mbox.SmtpPort);
                            else
                                ActiveUp.Net.Mail.SmtpClient.Send(message, mbox.SmtpServer, mbox.SmtpPort, mbox.SmtpAccount, mbox.SmtpPassword, SaslMechanism.Login);
                        }
                        else
                        {
                            if (!mbox.SmtpAuth)
                                ActiveUp.Net.Mail.SmtpClient.SendSsl(message, mbox.SmtpServer, mbox.SmtpPort);
                            else
                                ActiveUp.Net.Mail.SmtpClient.SendSsl(message, mbox.SmtpServer, mbox.SmtpPort, mbox.SmtpAccount, mbox.SmtpPassword, SaslMechanism.Login);
                        }
                    }
                    catch (Exception ex)
                    {
                        #region Error notification message
                        StringBuilder sbMessage = new StringBuilder(1024);
                        MailSendItem messageDelivery = new MailSendItem();
                        messageDelivery.Subject = "Message Delivery Failure";
                        messageDelivery.To = new ASC.Mail.Aggregator.Collection.ItemList<string>() { item.From };
                        messageDelivery.From = MailBoxManager.MAIL_DAEMON_EMAIL;
                        messageDelivery.Important = true;
                        messageDelivery.StreamId = Guid.NewGuid().ToString("N").ToLower();
                        sbMessage.Append("<span style=\"font-size: 15px;color:red;\">Message could not be delivered to_addresses recipient(s):</span><br/><ul>");
                        messageDelivery.NeedReSaveAttachments = false;
                        messageDelivery.Labels = new ASC.Mail.Aggregator.Collection.ItemList<int>();

                        for (int i = 0; i < item.To.Count; i++)
                        {
                            sbMessage.Append("<li>" + item.To[i] + "</li>");
                        }

                        sbMessage.AppendFormat("</ul><span style=\"font-size: 15px;color:red;\">Reason: {0}</span><br/>", ex.Message);
                        sbMessage.AppendFormat("<a id=\"id_delivery_failure_button\" href=\"#draftitem/{0}/\" class=\"baseLinkButton\" target=\"_self\" style=\"margin-top:10px;margin-bottom: 10px; color:white; text-decoration: none;\" style=\"margin-right:8px;\">Change your message and try again</a>", mailId.ToString());

                        if (message != null)
                        {
                            sbMessage.Append("<br/><span>Message headers follow:</span><br/><ul>");
                            var headers = message.HeaderFields;
                            for (int i = 0; i < headers.Count; i++)
                            {
                                var head = headers[i].ToString();
                                if (!String.IsNullOrEmpty(head))
                                    sbMessage.AppendFormat("<li>{0}</li>", head);
                            }
                            sbMessage.Append("</ul>");
                        }

                        messageDelivery.HtmlBody = sbMessage.ToString();
                        // Save To Inbox
                        MailMessageItem notify_message_item = messageDelivery.ToMailMessageItem(tenantId, username, false);
                        notify_message_item.IsNew = true;
                        notify_message_item.IsFromCRM = false;
                        notify_message_item.IsFromTL = false;

                        _manager.MailSave(mbox, notify_message_item, 0, MailFolder.Inbox.Id, string.Empty, string.Empty, item.NeedReSaveAttachments);
                        #endregion
                    }
                });

                //Move to_addresses sent
                _manager.SetMessageFolder(tenantId, username, MailFolder.Sent.Id, new[] { mailId });
            }
            else
            {
                throw new ArgumentException("Failed to_addresses save draft");
            }

            return mailId;
        }

        public int Save(int tenantId, string username, MailSendItem item, int mailId)
        {
            string mime_message_id;
            string in_reply_to;
            return Save(tenantId, username, item, mailId, out mime_message_id, out in_reply_to);
        }

        public int Save(int tenantId, string username, MailSendItem item, int mailId, out string mime_message_id, out string in_reply_to)
        {
            MailBox mbox = _manager.GetMailBox(tenantId, username, new MailAddress(item.From));
            item.DisplayName = mbox.Name != "" ? mbox.Name : "";

            if (mbox == null)
                throw new ArgumentException("no such mailbox");

            MailMessageItem message_item = item.ToMailMessageItem(tenantId, username, false);

            #region Mime ids determination
            mime_message_id = mailId == 0 ?
                MailSendItem.CreateMessageID() :
                _manager.GetMimeMessageIdByMessageId(mailId);

            in_reply_to = item.ReplyToId != 0 ? _manager.GetMimeMessageIdByMessageId(item.ReplyToId) : "";

            if (!string.IsNullOrEmpty(mime_message_id)) message_item.MessageId = mime_message_id;
            if (!string.IsNullOrEmpty(in_reply_to)) message_item.InReplyTo = in_reply_to;
            #endregion

            _manager.DetectChain(message_item, mbox);

            mailId = _manager.MailSave(mbox, message_item, mailId, MailFolder.Drafts.Id, string.Empty, string.Empty, item.NeedReSaveAttachments);

            return mailId;
        }
        
    }
}