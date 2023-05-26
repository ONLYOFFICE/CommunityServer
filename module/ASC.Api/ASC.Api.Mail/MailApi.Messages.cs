/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Threading;

using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Mail;
using ASC.Mail.Core.Engine;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;
using ASC.Mail.Utils;
using ASC.Web.Mail.Resources;

using FileShare = ASC.Files.Core.Security.FileShare;
using MailMessage = ASC.Mail.Data.Contracts.MailMessageData;
// ReSharper disable InconsistentNaming

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Returns the messages with the parameters specified in the request.
        /// </summary>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="folder">Folder ID</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="unread">Message status: unread (true), read (false), or all (null) messages</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="attachments">Defines if a message has attachments or not: with attachments (true), without attachments (false), or all (null) messages</param>
        /// <param type="System.Nullable{System.Int64}, System" method="url" optional="true" name="period_from">Start search period date</param>
        /// <param type="System.Nullable{System.Int64}, System" method="url" optional="true" name="period_to">End search period date</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="important">Specifies if only important messages will be displayed or not</param>
        /// <param type="System.String, System" method="url" optional="true" name="from_address">Mail address from which a letter was received</param>
        /// <param type="System.String, System" method="url" optional="true" name="to_address">Mail address to which a letter was sent</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="mailbox_id">Recipient mailbox ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" method="url" optional="true" name="tags">IDs of tags linked to the target messages</param>
        /// <param type="System.String, System" method="url" optional="true" name="search">Text to search in the message bodies and subjects</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="page">Page number</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="with_calendar">Specifies if the messages have a calendar or not</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="page_size">Number of messages on the page</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="user_folder_id">User folder ID</param>
        /// <param type="System.String, System" method="url" name="sortorder">Sort order by date: "ascending" or "descending"</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailMessageData, ASC.Mail">List of filtered messages</returns>
        /// <short>Get filtered messages</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"messages")]
        public IEnumerable<MailMessage> GetFilteredMessages(int? folder,
            bool? unread,
            bool? attachments,
            long? period_from,
            long? period_to,
            bool? important,
            string from_address,
            string to_address,
            int? mailbox_id,
            IEnumerable<int> tags,
            string search,
            int? page,
            int? page_size,
            string sortorder,
            bool? with_calendar,
            int? user_folder_id)
        {
            var primaryFolder = user_folder_id.HasValue
                ? FolderType.UserFolder
                : folder.HasValue ? (FolderType)folder.Value : FolderType.Inbox;

            ActionEngine.SendUserAlive(folder ?? -1, tags);

            var filter = new MailSearchFilterData
            {
                PrimaryFolder = primaryFolder,
                Unread = unread,
                Attachments = attachments,
                PeriodFrom = period_from,
                PeriodTo = period_to,
                Important = important,
                FromAddress = from_address,
                ToAddress = to_address,
                MailboxId = mailbox_id,
                CustomLabels = new List<int>(tags),
                SearchText = search,
                Page = page.HasValue ? (page.Value > 0 ? page.Value - 1 : 0) : 0,
                PageSize = page_size.GetValueOrDefault(25),
                Sort = Defines.ORDER_BY_DATE_SENT,
                SortOrder = sortorder,
                WithCalendar = with_calendar,
                UserFolderId = user_folder_id
            };

            long totalMessages;

            var messages = MailEngineFactory.MessageEngine.GetFilteredMessages(filter, out totalMessages);

            _context.SetTotalCount(totalMessages);

            return messages;
        }

        /// <summary>
        /// Returns the detailed information about a message with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Message ID</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="loadImages">Specifies whether to display images sent in a message or not</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="needSanitize">Specifies if HTML needs to be prepared for the FCK editor or not</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="markRead">Specifies if a message will be marked as read or not</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailMessageData, ASC.Mail">Message information</returns>
        /// <short>Get a message</short>
        /// <category>Messages</category>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        /// <exception cref="ItemNotFoundException">The exception occurs when a message with the specified ID wasn't found.</exception>
        /// <path>api/2.0/mail/messages/{id}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"messages/{id:[0-9]+}")]
        public MailMessage GetMessage(int id, bool? loadImages, bool? needSanitize, bool? markRead)
        {
            if (id <= 0)
                throw new ArgumentException(@"Invalid message id", "id");

            var needSanitizeHtml = needSanitize.GetValueOrDefault(false);
#if DEBUG
            var watch = new Stopwatch();
            watch.Start();
#endif
            var item = MailEngineFactory.MessageEngine.GetMessage(id, new MailMessage.Options
            {
                LoadImages = loadImages.GetValueOrDefault(false),
                LoadBody = true,
                NeedProxyHttp = Defines.NeedProxyHttp,
                NeedSanitizer = needSanitizeHtml
            });

            if (item == null)
            {
#if DEBUG
                watch.Stop();
                Logger.DebugFormat(
                    "Mail->GetMessage(id={0})->Elapsed {1}ms [NotFound] (NeedProxyHttp={2}, NeedSanitizer={3})", id,
                    watch.Elapsed.TotalMilliseconds, Defines.NeedProxyHttp, needSanitizeHtml);
#endif
                throw new ItemNotFoundException(string.Format("Message with {0} wasn't found.", id));
            }

            if (item.WasNew && markRead.HasValue && markRead.Value)
            {
                var ids = new List<int> { item.Id };

                MailEngineFactory.MessageEngine.SetUnread(ids, false);
                item.IsNew = false;

                ActionEngine.SendUserActivity(ids, MailUserAction.SetAsRead);
            }

            if (needSanitizeHtml)
            {
                item.HtmlBody = HtmlSanitizer.SanitizeHtmlForEditor(item.HtmlBody);
            }
#if DEBUG
            watch.Stop();
            Logger.DebugFormat("Mail->GetMessage(id={0})->Elapsed {1}ms (NeedProxyHttp={2}, NeedSanitizer={3})", id,
                watch.Elapsed.TotalMilliseconds, Defines.NeedProxyHttp, needSanitizeHtml);
#endif
            if (item.Folder != FolderType.UserFolder)
                return item;

            var userFoler = GetUserFolderByMailId((uint)item.Id);

            if (userFoler != null)
            {
                item.UserFolderId = userFoler.Id;
            }

            return item;
        }

        /// <summary>
        /// Reassigns drafts/templates to the selected email.
        /// </summary>
        /// <param type="System.Int32, System" name="folder">Folder ID</param>
        /// <param type="System.String, System" name="email">Email to which messages will be reassigned</param>
        /// <short>Reassign drafts/templates</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages/reassign</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update(@"messages/reassign")]
        public void ReassignMailMessages(int folder, string email)
        {
            var filter = new MailSearchFilterData
            {
                PrimaryFolder = (FolderType)folder
            };

            if (filter.PrimaryFolder != FolderType.Draft && filter.PrimaryFolder != FolderType.Templates)
            {
                throw new InvalidOperationException("Only folders Templates and Drafts are allowed.");
            }

            long totalMessages;

            var messages = MailEngineFactory.MessageEngine.GetFilteredMessages(filter, out totalMessages);

            _context.SetTotalCount(totalMessages);

            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i];

                if (message.Bcc == null)
                {
                    message.Bcc = "";
                }

                var to = message.To.Split(',').ToList<string>();
                var cc = message.Cc.Split(',').ToList<string>();
                var bcc = message.Bcc.Split(',').ToList<string>();

                if (filter.PrimaryFolder == FolderType.Draft)
                {
                    MailEngineFactory.DraftEngine.Save(message.Id, email, to, cc, bcc, message.MimeReplyToId, message.Important, message.Subject,
                        message.TagIds, message.HtmlBody, message.Attachments, message.CalendarEventIcs);
                }

                if (filter.PrimaryFolder == FolderType.Templates)
                {
                    MailEngineFactory.TemplateEngine.Save(message.Id, email, to, cc, bcc, message.MimeReplyToId, message.Important, message.Subject,
                            message.TagIds, message.HtmlBody, message.Attachments, message.CalendarEventIcs);
                }
            }

            if (messages.Any()) ActionEngine.SendUserActivity(messages.Select(x => x.Id).ToList(), MailUserAction.UpdateDrafts);
        }

        /// <summary>
        /// Returns the previous or next message ID filtered with the parameters specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Head message ID of the current conversation</param>
        /// <param type="System.String, System" method="url" name="direction">Defines if the previous or next message is needed: "prev" for previous, "next" for next</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="folder">Folder type: 1 - inbox, 2 - sent, 5 - spam</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="unread">Message status: unread (true), read (false), or all (null) messages</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="attachments">Defines if a message has attachments or not: with attachments (true), without attachments (false), or all (null) messages</param>
        /// <param type="System.Nullable{System.Int64}, System" method="url" optional="true" name="period_from">Start search period date</param>
        /// <param type="System.Nullable{System.Int64}, System" method="url" optional="true" name="period_to">End search period date</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="important">Specifies if only important messages will be displayed or not</param>
        /// <param type="System.String, System" method="url" optional="true" name="from_address">Mail address from which a letter was received</param>
        /// <param type="System.String, System" method="url" optional="true" name="to_address">Mail address to which a letter was sent</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="mailbox_id">Recipient mailbox ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" method="url" optional="true" name="tags">IDs of tags linked to the target message</param>
        /// <param type="System.String, System" method="url" optional="true" name="search">Text to search in the message bodies and subjects</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="page_size">Number of messages on the page</param>
        /// <param type="System.String, System" method="url" optional="true" name="sortorder">Sort order by date: "ascending" or "descending"</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="with_calendar">Specifies if the messages have a calendar or not</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="user_folder_id">User folder ID</param>
        /// <returns>Previous or next message ID</returns>
        /// <short>Get the previous or next message ID</short> 
        /// <category>Messages</category>
        /// <httpMethod>GET</httpMethod>
        /// <path>api/2.0/mail/messages/{id}/{direction}</path>
        [Read(@"messages/{id:[0-9]+}/{direction:(next|prev)}")]
        public long GetPrevNextMessageId(int id,
            string direction,
            int? folder,
            bool? unread,
            bool? attachments,
            long? period_from,
            long? period_to,
            bool? important,
            string from_address,
            string to_address,
            int? mailbox_id,
            IEnumerable<int> tags,
            string search,
            int? page_size,
            string sortorder,
            bool? with_calendar,
            int? user_folder_id)
        {
            // inverse sort order if prev message require
            if ("prev" == direction)
                sortorder = Defines.ASCENDING == sortorder ? Defines.DESCENDING : Defines.ASCENDING;

            var primaryFolder = folder.HasValue ? (FolderType)folder.Value : FolderType.Inbox;

            var filter = new MailSearchFilterData
            {
                PrimaryFolder = primaryFolder,
                Unread = unread,
                Attachments = attachments,
                PeriodFrom = period_from,
                PeriodTo = period_to,
                Important = important,
                FromAddress = from_address,
                ToAddress = to_address,
                MailboxId = mailbox_id,
                CustomLabels = new List<int>(tags),
                SearchText = search,
                Page = null,
                PageSize = 2,
                Sort = Defines.ORDER_BY_DATE_SENT,
                SortOrder = sortorder,
                WithCalendar = with_calendar,
                UserFolderId = user_folder_id
            };

            var nextId = MailEngineFactory.MessageEngine.GetNextFilteredMessageId(id, filter);

            return nextId;
        }

        /// <summary>
        /// Deletes the selected attachment from the message with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <param type="System.Int32, System" method="url" name="attachmentid">Attachment ID</param>
        /// <returns>The message ID which attachment was removed</returns>
        /// <short>Delete an attachment</short> 
        /// <category>Attachments</category>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        /// <path>api/2.0/mail/messages/{messageid}/attachments/{attachmentid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"messages/{messageid:[0-9]+}/attachments/{attachmentid:[0-9]+}")]
        public int DeleteMessageAttachment(int messageid, int attachmentid)
        {
            if (messageid <= 0)
                throw new ArgumentException(@"Invalid message id. Message id must be positive integer", "messageid");

            if (attachmentid <= 0)
                throw new ArgumentException(@"Invalid attachment id. Attachment id must be positive integer", "attachmentid");

            MailEngineFactory.AttachmentEngine
                .DeleteMessageAttachments(TenantId, Username, messageid, new List<int> { attachmentid });

            return messageid;
        }

        /// <summary>
        /// Sets a status to the messages with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="ids">List of message IDs</param>
        /// <param type="System.String, System" name="status">Message status: "read", "unread", "important", or "normal"</param>
        /// <returns>List of messages with changed status</returns>
        /// <short>Set a message status</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages/mark</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"messages/mark")]
        public IEnumerable<int> MarkMessages(List<int> ids, string status)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailUserAction mailUserAction = MailUserAction.Nothing;

            switch (status)
            {
                case "read":
                    MailEngineFactory.MessageEngine.SetUnread(ids, false);
                    mailUserAction = MailUserAction.SetAsRead;
                    break;

                case "unread":
                    MailEngineFactory.MessageEngine.SetUnread(ids, true);
                    mailUserAction = MailUserAction.SetAsUnread;
                    break;

                case "important":
                    MailEngineFactory.MessageEngine.SetImportant(ids, true);
                    mailUserAction = MailUserAction.SetAsImportant;
                    break;

                case "normal":
                    MailEngineFactory.MessageEngine.SetImportant(ids, false);
                    mailUserAction = MailUserAction.SetAsNotImpotant;
                    break;

                case "receiptProcessed":
                    MailEngineFactory.MessageEngine.ReceiptStatus(ids, false);
                    mailUserAction = MailUserAction.ReceiptStatusChanged;
                    break;
            }

            ActionEngine.SendUserActivity(ids, mailUserAction);

            return ids;
        }

        /// <summary>
        /// Restores the messages with the IDs specified in the request to their original folders.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="ids">List of message IDs</param>
        /// <returns>List of restored message IDs</returns>
        /// <short>Restore messages</short>
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages/restore</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"messages/restore")]
        public IEnumerable<int> RestoreMessages(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailEngineFactory.MessageEngine.Restore(ids);

            MailEngineFactory.OperationEngine.ApplyFilters(ids);

            ActionEngine.SendUserActivity(ids, MailUserAction.Restore);

            return ids;
        }

        /// <summary>
        ///  Moves the messages to a folder with the ID specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="ids">List of message IDs</param>
        /// <param type="System.Int32, System" name="folder">Folder type: 1 - inbox, 2 - sent, 3 - drafts, 4 - trash, 5 - spam</param>
        /// <param type="System.UInt32, System" optional="true" name="userFolderId">User folder ID</param>
        /// <returns>List of moved message IDs</returns>
        /// <short>Move messages to the folder</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages/move</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"messages/move")]
        public IEnumerable<int> MoveMessages(List<int> ids, int folder, uint? userFolderId = null)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            var toFolder = (FolderType)folder;

            if (!MailFolder.IsIdOk(toFolder))
                throw new ArgumentException(@"Invalid folder id", "folder");

            MailEngineFactory.MessageEngine.SetFolder(ids, toFolder, userFolderId);

            ActionEngine.SendUserActivity(ids, MailUserAction.MoveTo, folder, userFolderId);

            if (toFolder == FolderType.Spam || toFolder == FolderType.Sent || toFolder == FolderType.Inbox)
                MailEngineFactory.OperationEngine.ApplyFilters(ids);

            return ids;
        }

        /// <summary>
        /// Sends a message with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="id">Message ID which will be sent or 0</param>
        /// <param type="System.String, System" name="from">Mail address from which a letter will be sent. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="to">List of mail addresses to which a letter will be sent. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="cc">List of Cc (carbon copy) mail addresses. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="bcc">List of Bcc (blind carbon copy) mail addresses. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.String, System" name="mimeReplyToId">Message ID to which this message is replying to</param>
        /// <param type="System.Boolean, System" name="importance">Specifies if this message is important or not: true - important, false - not important</param>
        /// <param type="System.String, System" name="subject">Message subject</param>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="tags">List of tag IDs added to the message</param>
        /// <param type="System.String, System" name="body">Message body as the HTML string</param>
        /// <param type="System.Collections.Generic.List{ASC.Mail.Data.Contracts.MailAttachmentData}, System.Collections.Generic" file="ASC.Mail" name="attachments">List of message attachments</param>
        /// <param type="ASC.Files.Core.Security.FileShare, ASC.Files.Core.Security" name="fileLinksShareMode">Sharing mode for the links of the attached files</param>
        /// <param type="System.String, System" name="calendarIcs">Calendar event in the iCal format for sending</param>
        /// <param type="System.Boolean, System" name="isAutoreply">Specifies if this message is autoreply or not</param>
        /// <param type="System.Boolean, System" optional="true" name="requestReceipt">Specifies whether to add a request with the Return-Receipt-To header or not</param>
        /// <param type="System.Boolean, System" optional="true" name="requestRead">Specifies whether to add a request with the Disposition-Notification-To header or not</param>
        /// <returns>Message ID</returns>
        /// <short>Send a message</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages/send</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"messages/send")]
        public long SendMessage(int id,
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
            FileShare fileLinksShareMode,
            string calendarIcs,
            bool isAutoreply,
            bool requestReceipt,
            bool requestRead)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CurrentCulture;
                Thread.CurrentThread.CurrentUICulture = CurrentCulture;

                var daemonLabels =
                    new DraftEngine.DeliveryFailureMessageTranslates(
                        Defines.MailDaemonEmail,
                        MailApiResource.DeliveryFailureSubject,
                        MailApiResource.DeliveryFailureAutomaticMessage,
                        MailApiResource.DeliveryFailureMessageIdentificator,
                        MailApiResource.DeliveryFailureRecipients,
                        MailApiResource.DeliveryFailureRecommendations,
                        MailApiResource.DeliveryFailureBtn,
                        MailApiResource.DeliveryFailureFAQInformation,
                        MailApiResource.DeliveryFailureReason);

                long result = MailEngineFactory.DraftEngine.Send(id, from, to, cc, bcc, mimeReplyToId, importance, subject, tags, body,
                    attachments, fileLinksShareMode, calendarIcs, isAutoreply, requestReceipt, requestRead, daemonLabels);

                ActionEngine.SendUserActivity(new List<int>() { id }, MailUserAction.SendDraft);

                return result;
            }
            catch (DraftException ex)
            {
                string fieldName;

                switch (ex.FieldType)
                {
                    case DraftFieldTypes.From:
                        fieldName = MailApiResource.FieldNameFrom;
                        break;
                    case DraftFieldTypes.To:
                        fieldName = MailApiResource.FieldNameTo;
                        break;
                    case DraftFieldTypes.Cc:
                        fieldName = MailApiResource.FieldNameCc;
                        break;
                    case DraftFieldTypes.Bcc:
                        fieldName = MailApiResource.FieldNameBcc;
                        break;
                    default:
                        fieldName = "";
                        break;
                }
                switch (ex.ErrorType)
                {
                    case DraftException.ErrorTypes.IncorrectField:
                        throw new ArgumentException(MailApiResource.ErrorIncorrectEmailAddress.Replace("%1", fieldName));
                    case DraftException.ErrorTypes.EmptyField:
                        throw new ArgumentException(MailApiResource.ErrorEmptyField.Replace("%1", fieldName));
                    default:
                        throw;
                }
            }
        }

        /// <summary>
        /// Sends a message quickly specifying the sender, recipients, message subject, and message body.
        /// </summary>
        /// <param type="System.String, System" name="from">Mail address from which a letter will be sent. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="to">List of mail addresses to which a letter will be sent. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.String, System" name="subject">Message subject</param>
        /// <param type="System.String, System" name="body">Message body as the HTML string</param>
        /// <param type="System.Boolean, System" name="isReceipt">Specifies if the receipt is returned to the sender or not</param>
        /// <returns>Boolean value: true if a message was sent successfully</returns>
        /// <short>Send a message quickly</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages/simpleSend</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"messages/simpleSend")]
        public bool SimpleSend(
            string from,
            List<string> to,
            string subject,
            string body,
            bool isReceipt)
        {
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            var daemonLabels =
                    new DraftEngine.DeliveryFailureMessageTranslates(
                        Defines.MailDaemonEmail,
                        MailApiResource.DeliveryFailureSubject,
                        MailApiResource.DeliveryFailureAutomaticMessage,
                        MailApiResource.DeliveryFailureMessageIdentificator,
                        MailApiResource.DeliveryFailureRecipients,
                        MailApiResource.DeliveryFailureRecommendations,
                        MailApiResource.DeliveryFailureBtn,
                        MailApiResource.DeliveryFailureFAQInformation,
                        MailApiResource.DeliveryFailureReason);

            return MailEngineFactory.DraftEngine.SimpleSend(
                from,
                to,
                subject,
                body,
                isReceipt,
                daemonLabels);
        }

        /// <summary>
        /// Saves a message with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="id">Message ID which will be saved or 0</param>
        /// <param type="System.String, System" name="from">Mail address from which a letter will be sent. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="to">List of mail addresses to which a letter will be sent. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="cc">List of Cc (carbon copy) mail addresses. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="bcc">List of Bcc (blind carbon copy) mail addresses. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.String, System" name="mimeReplyToId">Message ID to which this message is replying to</param>
        /// <param type="System.Boolean, System" name="importance">Specifies if this message is important or not: true - important, false - not important</param>
        /// <param type="System.String, System" name="subject">Message subject</param>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="tags">List of tag IDs added to the message</param>
        /// <param type="System.String, System" name="body">Message body as the HTML string</param>
        /// <param type="System.Collections.Generic.List{ASC.Mail.Data.Contracts.MailAttachmentData}, System.Collections.Generic" name="attachments">List of message attachments</param>
        /// <param type="System.String, System" name="calendarIcs">Calendar event in the iCal format for sending</param>
        /// <returns>Saved message</returns>
        /// <short>Save a message</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages/save</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Obsolete]
        [Update(@"messages/save")]
        public MailMessage SaveMessageOld(int id,
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
                                          string calendarIcs)
        {
            return SaveMessage(id,
                               from,
                               to,
                               cc,
                               bcc,
                               mimeReplyToId,
                               importance,
                               subject,
                               tags,
                               body,
                               attachments,
                               calendarIcs);
        }

        /// <summary>
        /// Saves a message with the ID specified in the request as a draft.
        /// </summary>
        /// <param type="System.Int32, System" name="id">Message ID which will be saved or 0</param>
        /// <param type="System.String, System" name="from">Mail address from which a letter will be sent. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="to">List of mail addresses to which a letter will be sent. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="cc">List of Cc (carbon copy) mail addresses. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="bcc">List of Bcc (blind carbon copy) mail addresses. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.String, System" name="mimeReplyToId">Message ID to which this message is replying to</param>
        /// <param type="System.Boolean, System" name="importance">Specifies if this message is important or not: true - important, false - not important</param>
        /// <param type="System.String, System" name="subject">Message subject</param>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="tags">List of tag IDs added to the message</param>
        /// <param type="System.String, System" name="body">Message body as the HTML string</param>
        /// <param type="System.Collections.Generic.List{ASC.Mail.Data.Contracts.MailAttachmentData}, System.Collections.Generic" file="ASC.Mail" name="attachments">List of message attachments</param>
        /// <param type="System.String, System" name="calendarIcs">Calendar event in the iCal format for sending</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailMessageData, ASC.Mail">Saved message</returns>
        /// <short>Save a draft message</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/drafts/save</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"drafts/save")]
        public MailMessage SaveMessage(int id,
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
            string calendarIcs)
        {
            if (id < 1)
                id = 0;

            if (string.IsNullOrEmpty(from))
                throw new ArgumentNullException("from");

            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            try
            {
                var MailMessage = MailEngineFactory.DraftEngine.Save(id, from, to, cc, bcc, mimeReplyToId, importance, subject, tags,
                    body, attachments, calendarIcs);

                if (MailMessage != null)
                {
                    ActionEngine.SendUserActivity(new List<int>() { MailMessage.Id }, MailUserAction.UpdateDrafts);
                }

                return MailMessage;
            }
            catch (DraftException ex)
            {
                string fieldName;

                switch (ex.FieldType)
                {
                    case DraftFieldTypes.From:
                        fieldName = MailApiResource.FieldNameFrom;
                        break;
                    default:
                        fieldName = "";
                        break;
                }
                switch (ex.ErrorType)
                {
                    case DraftException.ErrorTypes.IncorrectField:
                        throw new ArgumentException(MailApiResource.ErrorIncorrectEmailAddress.Replace("%1", fieldName));
                    case DraftException.ErrorTypes.EmptyField:
                        throw new ArgumentException(MailApiResource.ErrorEmptyField.Replace("%1", fieldName));
                    case DraftException.ErrorTypes.TotalSizeExceeded:
                        throw new ArgumentException(MailScriptResource.AttachmentsTotalLimitError);
                    default:
                        throw;
                }
            }
        }

        /// <summary>
        /// Saves a template message with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="id">Template ID which will be saved</param>
        /// <param type="System.String, System" name="from">Mail address from which a letter will be sent. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="to">List of mail addresses to which a letter will be sent. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="cc">List of Cc (carbon copy) mail addresses. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="bcc">List of Bcc (blind carbon copy) mail addresses. <![CDATA[Format: Name &lt;name@domain&gt;]]></param>
        /// <param type="System.String, System" name="mimeReplyToId">Message ID to which this message is replying to</param>
        /// <param type="System.Boolean, System" name="importance">Specifies if this message is important or not: true - important, false - not important</param>
        /// <param type="System.String, System" name="subject">Message subject</param>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="tags">List of tag IDs added to the message</param>
        /// <param type="System.String, System" name="body">Message body as the HTML string</param>
        /// <param type="System.Collections.Generic.List{ASC.Mail.Data.Contracts.MailAttachmentData}, System.Collections.Generic" file="ASC.Mail" name="attachments">List of message attachments</param>
        /// <param type="System.String, System" name="calendarIcs">Calendar event in the iCal format for sending</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailMessageData, ASC.Mail">Saved template message</returns>
        /// <short>Save a template message</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/templates/save</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"templates/save")]
        public MailMessage SaveTemplate(int id, string from, List<string> to, List<string> cc, List<string> bcc, string mimeReplyToId, bool importance, string subject,
            List<int> tags, string body, List<MailAttachmentData> attachments, string calendarIcs)
        {
            if (string.IsNullOrEmpty(from))
                throw new ArgumentNullException("from");

            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            try
            {
                return MailEngineFactory.TemplateEngine.Save(id, from, to, cc, bcc, mimeReplyToId, importance, subject, tags,
                    body, attachments, calendarIcs);
            }
            catch (DraftException ex)
            {
                string fieldName;

                switch (ex.FieldType)
                {
                    case DraftFieldTypes.From:
                        fieldName = MailApiResource.FieldNameFrom;
                        break;
                    default:
                        fieldName = "";
                        break;
                }
                switch (ex.ErrorType)
                {
                    case DraftException.ErrorTypes.IncorrectField:
                        throw new ArgumentException(MailApiResource.ErrorIncorrectEmailAddress.Replace("%1", fieldName));
                    case DraftException.ErrorTypes.EmptyField:
                        throw new ArgumentException(MailApiResource.ErrorEmptyField.Replace("%1", fieldName));
                    case DraftException.ErrorTypes.TotalSizeExceeded:
                        throw new ArgumentException(MailScriptResource.AttachmentsTotalLimitError);
                    default:
                        throw;
                }
            }
        }

        /// <summary>
        /// Removes messages with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="ids">List of message IDs</param>
        /// <returns>List of removed message IDs</returns>
        /// <short>Remove messages</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages/remove</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"messages/remove")]
        public IEnumerable<int> RemoveMessages(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailEngineFactory.MessageEngine.SetRemoved(ids);

            ActionEngine.SendUserActivity(ids, MailUserAction.SetAsDeleted);

            return ids;
        }

        /// <summary>
        /// Returns a message template - empty message in the JSON format.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.MailMessageData, ASC.Mail">Empty message in the JSON format</returns>
        /// <short>Get a message template</short> 
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages/template</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"messages/template")]
        public MailMessage GetMessageTemplate()
        {
            return MailEngineFactory.DraftEngine.GetTemplate();
        }

        /// <summary>
        /// Attaches a document to the message with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Message ID</param>
        /// <param type="System.String, System" name="fileId">Document ID</param>
        /// <param type="System.String, System" name="version">Document version</param>
        /// <param type="System.Boolean, System" name="needSaveToTemp">Specifies if this document needs to be saved to a template message or not</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailAttachmentData, ASC.Mail">Attached document</returns>
        /// <short>Attach a document</short>
        /// <category>Attachments</category>
        /// <path>api/2.0/mail/messages/{id}/document</path>
        /// <httpMethod>POST</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Create(@"messages/{id:[0-9]+}/document")]
        public MailAttachmentData AttachDocument(int id, string fileId, string version, bool needSaveToTemp)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CurrentCulture;
                Thread.CurrentThread.CurrentUICulture = CurrentCulture;

                var attachment = MailEngineFactory.AttachmentEngine
                    .AttachFileFromDocuments(TenantId, Username, id, fileId, version, needSaveToTemp);

                return attachment;
            }
            catch (AttachmentsException e)
            {
                string errorMessage;
                switch (e.ErrorType)
                {
                    case AttachmentsException.Types.BadParams:
                        errorMessage = MailApiResource.AttachmentsBadInputParamsError;
                        break;
                    case AttachmentsException.Types.EmptyFile:
                        errorMessage = MailApiResource.AttachmentsEmptyFileNotSupportedError;
                        break;
                    case AttachmentsException.Types.MessageNotFound:
                        errorMessage = MailApiResource.AttachmentsMessageNotFoundError;
                        break;
                    case AttachmentsException.Types.TotalSizeExceeded:
                        errorMessage = MailApiResource.AttachmentsTotalLimitError;
                        break;
                    case AttachmentsException.Types.DocumentNotFound:
                        errorMessage = MailApiResource.AttachmentsDocumentNotFoundError;
                        break;
                    case AttachmentsException.Types.DocumentAccessDenied:
                        errorMessage = MailApiResource.AttachmentsDocumentAccessDeniedError;
                        break;
                    default:
                        errorMessage = MailApiResource.AttachmentsUnknownError;
                        break;
                }
                throw new Exception(errorMessage);
            }
            catch (Exception)
            {
                throw new Exception(MailApiResource.AttachmentsUnknownError);
            }
        }

        /// <summary>
        /// Exports a mail to the CRM relation history for some entities.
        /// </summary>
        /// <param type="System.Int32, System" name="id_message">ID of any message from the chain</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Mail.Data.Contracts.CrmContactData}, System.Collections.Generic" name="crm_contact_ids">List of CRM contact entity IDs in the following format: {entity_id: 0, entity_type: 0}.
        /// Entity types: 1 - Contact, 2 - Case, 3 - Opportunity
        /// </param>
        /// <short>Export a message to CRM</short>
        /// <category>Messages</category>
        /// <path>api/2.0/mail/messages/crm/export</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update(@"messages/crm/export")]
        public void ExportMessageToCrm(int id_message, IEnumerable<CrmContactData> crm_contact_ids)
        {
            if (id_message < 0)
                throw new ArgumentException(@"Invalid message id", "id_message");
            if (crm_contact_ids == null)
                throw new ArgumentException(@"Invalid contact ids list", "crm_contact_ids");

            MailEngineFactory.CrmLinkEngine.ExportMessageToCrm(id_message, crm_contact_ids);
        }
    }
}
