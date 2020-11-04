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
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Threading;
using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Mail;
using ASC.Mail.Core.Engine;
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
        ///    Returns the filtered messages in case there were changes since last check date
        /// </summary>
        /// <param optional="true" name="folder">Folder ID</param>
        /// <param optional="true" name="unread">Message unread status</param>
        /// <param optional="true" name="attachments">Message with attachments status</param>
        /// <param optional="true" name="period_from">Period start date</param>
        /// <param optional="true" name="period_to">Period end date</param>
        /// <param optional="true" name="important">Message with importance flag</param>
        /// <param optional="true" name="from_address">Address to find 'From' field</param>
        /// <param optional="true" name="to_address">Address to find 'To' field</param>
        /// <param optional="true" name="mailbox_id">Recipient mailbox id</param>
        /// <param optional="true" name="tags">Message tags</param>
        /// <param optional="true" name="search">Text to search in messages</param>
        /// <param optional="true" name="page">Page number</param>
        /// <param optional="true" name="with_calendar">Message has calendar flag. bool flag.</param>
        /// <param optional="true" name="page_size">Number of messages on page</param>
        /// <param optional="true" name="user_folder_id">id of user's folder</param>
        /// <param name="sortorder">Sort order</param>
        /// <returns>Messages list</returns>
        /// <short>Get filtered messages</short> 
        /// <category>Messages</category>
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
        ///    Returns the detailed information about message with the ID specified in the request
        /// </summary>
        /// <param name="id">Message ID</param>
        /// <param optional="true" name="loadImages">Unblock suspicious content or not</param>
        /// <param optional="true" name="needSanitize">Flag specifies is needed to prepare html for FCKeditor</param>
        /// <param optional="true" name="markRead">Mark message as read</param>
        /// <returns>MailMessageItem</returns>
        /// <short>Get message</short>
        /// <category>Messages</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="ItemNotFoundException">Exception happens when message with specified id wasn't found.</exception>
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
                MailEngineFactory.MessageEngine.SetUnread(new List<int> {item.Id}, false);
                item.IsNew = false;
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

            var userFoler = GetUserFolderByMailId((uint) item.Id);

            if (userFoler != null)
            {
                item.UserFolderId = userFoler.Id;
            }

            return item;
        }

        /// <summary>
        ///    Reassigns drafts/templates to selected email.
        /// </summary>
        /// <param name="folder">Folder id</param>
        /// <param name="email">Email to which messages will be reassigned</param>
        /// <returns>none</returns>
        /// <short>Reassign drafts/templates</short> 
        /// <category>Messages</category>
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
        }

        /// <summary>
        /// Get previous or next message id. U
        /// </summary>
        /// <param name="id">Head message id of current conversation.</param>
        /// <param name="direction">String parameter for determine prev or next conversation needed. "prev" for previous, "next" for next.</param>
        /// <param optional="true" name="folder">Folder ID - integer. 1 - inbox, 2 - sent, 5 - spam.</param>
        /// <param optional="true" name="unread">Message unread status. bool flag. Search in unread(true), read(false) or all(null) messages.</param>
        /// <param optional="true" name="attachments">Message attachments status. bool flag. Search messages with attachments(true), without attachments(false) or all(null) messages.</param>
        /// <param optional="true" name="period_from">Begin search period date</param>
        /// <param optional="true" name="period_to">End search period date</param>
        /// <param optional="true" name="important">Message has importance flag. bool flag.</param>
        /// <param optional="true" name="from_address">Address to find 'From' field</param>
        /// <param optional="true" name="to_address">Address to find 'To' field</param>
        /// <param optional="true" name="mailbox_id">Recipient mailbox id.</param>
        /// <param optional="true" name="tags">Messages tags. Id of tags linked with target messages.</param>
        /// <param optional="true" name="search">Text to search in messages body and subject.</param>
        /// <param optional="true" name="page_size">Count on messages on page</param>
        /// <param optional="true" name="sortorder">Sort order by date. String parameter: "ascending" - ascended, "descending" - descended.</param>
        /// <param optional="true" name="with_calendar">Message has with_calendar flag. bool flag.</param>
        /// <param optional="true" name="user_folder_id">id of user's folder</param>
        /// <returns>Previous or next message id</returns>
        /// <short>Get previous or next message id</short> 
        /// <category>Messages</category>
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
        ///    Deletes the selected attachment from the message with the ID specified in the request
        /// </summary>
        /// <param name="messageid">The message id which attachment will be removed.</param>
        /// <param name="attachmentid">Specifies attachment id for deleting.</param>
        /// <returns>The message id which removed an attachment</returns>
        /// <short>Delete attachment from message</short> 
        /// <category>Messages</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Delete(@"messages/{messageid:[0-9]+}/attachments/{attachmentid:[0-9]+}")]
        public int DeleteMessageAttachment(int messageid, int attachmentid)
        {
            if (messageid <= 0)
                throw new ArgumentException(@"Invalid message id. Message id must be positive integer", "messageid");

            if (attachmentid <= 0)
                throw new ArgumentException(@"Invalid attachment id. Attachment id must be positive integer", "attachmentid");

            MailEngineFactory.AttachmentEngine
                .DeleteMessageAttachments(TenantId, Username, messageid, new List<int> {attachmentid});

            return messageid;
        }

        /// <summary>
        ///    Sets the status for messages specified by ids.
        /// </summary>
        /// <param name="ids">List of messages ids for status changing.</param>
        /// <param name="status">String parameter specifies status for changing. Values: "read", "unread", "important" and "normal"</param>
        /// <returns>List of messages with changed status</returns>
        /// <short>Set message status</short> 
        /// <category>Messages</category>
        [Update(@"messages/mark")]
        public IEnumerable<int> MarkMessages(List<int> ids, string status)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            switch (status)
            {
                case "read":
                    MailEngineFactory.MessageEngine.SetUnread(ids, false);
                    break;

                case "unread":
                    MailEngineFactory.MessageEngine.SetUnread(ids, true);
                    break;

                case "important":
                    MailEngineFactory.MessageEngine.SetImportant(ids, true);
                    break;

                case "normal":
                    MailEngineFactory.MessageEngine.SetImportant(ids, false);
                    break;
            }
            return ids;
        }

        /// <summary>
        ///    Restores the messages to their original folders
        /// </summary>
        /// <param name="ids">List of conversation ids for restore.</param>
        /// <returns>List of restored messages ids</returns>
        /// <short>Restore messages to original folders</short>
        /// <category>Messages</category>
        [Update(@"messages/restore")]
        public IEnumerable<int> RestoreMessages(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailEngineFactory.MessageEngine.Restore(ids);

            MailEngineFactory.OperationEngine.ApplyFilters(ids);

            return ids;
        }

        /// <summary>
        ///    Moves the messages to the specified folder
        /// </summary>
        /// <param name="ids">List of mesasges ids.</param>
        /// <param name="folder">Folder ID - integer. 1 - inbox, 2 - sent, 3 - drafts, 4 - trash, 5 - spam.</param>
        /// <param optional="true" name="userFolderId">User Folder Id</param>
        /// <returns>List of moved messages ids.</returns>
        /// <short>Move message to folder</short> 
        /// <category>Messages</category>
        [Update(@"messages/move")]
        public IEnumerable<int> MoveMessages(List<int> ids, int folder, uint? userFolderId = null)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            var toFolder = (FolderType)folder;

            if (!MailFolder.IsIdOk(toFolder))
                throw new ArgumentException(@"Invalid folder id", "folder");

            MailEngineFactory.MessageEngine.SetFolder(ids, toFolder, userFolderId);

            if (toFolder == FolderType.Spam || toFolder == FolderType.Sent || toFolder == FolderType.Inbox)
                MailEngineFactory.OperationEngine.ApplyFilters(ids);

            return ids;
        }

        /// <summary>
        ///    Sends the message with the ID specified in the request
        /// </summary>
        /// <param name="id">Message id which will be saved or 0.</param>
        /// <param name="from">From email. Format: Name&lt;name@domain&gt;</param>
        /// <param name="to">List of "to" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="cc">List of "cc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="bcc">List of "bcc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="mimeReplyToId">Message id to which the reply answer</param>
        /// <param name="importance">Importanse flag. Values: true - important, false - not important.</param>
        /// <param name="subject">Message subject</param>
        /// <param name="tags">List of tags id added to message</param>
        /// <param name="body">Message body as html string.</param>
        /// <param name="attachments">List of attachments represented as MailAttachment object</param>
        /// <param name="fileLinksShareMode">Share mode for attached file links</param>
        /// <param name="calendarIcs">Calendar event ical-format for sending</param>
        /// <param name="isAutoreply">Indicate that message is autoreply or not</param>
        /// <param optional="true" name="requestReceipt">Add request Return-Receipt-To header</param>
        /// <param optional="true" name="requestRead">Add request Disposition-Notification-To header</param>
        /// <returns>message id</returns>
        /// <short>Send message</short> 
        /// <category>Messages</category>
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

                return MailEngineFactory.DraftEngine.Send(id, from, to, cc, bcc, mimeReplyToId, importance, subject, tags, body,
                    attachments, fileLinksShareMode, calendarIcs, isAutoreply, requestReceipt, requestRead, daemonLabels);
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
        ///    Saves the message with the ID specified in the request
        /// </summary>
        /// <param name="id">Message id which will be saved or 0.</param>
        /// <param name="from">From email. Format: Name&lt;name@domain&gt;</param>
        /// <param name="to">List of "to" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="cc">List of "cc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="bcc">List of "bcc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="mimeReplyToId">Message id to which the reply answer</param>
        /// <param name="importance">Importanse flag. Values: true - important, false - not important.</param>
        /// <param name="subject">Message subject</param>
        /// <param name="tags">List of tags id added to message</param>
        /// <param name="body">Message body as html string.</param>
        /// <param name="attachments">List of attachments represented as MailAttachment object</param>
        /// <param name="calendarIcs">Calendar event ical-format for sending</param>
        /// <returns>Saved message id</returns>
        /// <short>SaveToDraft message</short> 
        /// <category>Messages</category>
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
                return MailEngineFactory.DraftEngine.Save(id, from, to, cc, bcc, mimeReplyToId, importance, subject, tags,
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
        ///    Saves the template with the ID specified in the request
        /// </summary>
        /// <param name="id">Template id which will be saved.</param>
        /// <param name="from">From email. Format: Name&lt;name@domain&gt;</param>
        /// <param name="to">List of "to" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="cc">List of "cc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="bcc">List of "bcc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="mimeReplyToId">Template id to which the reply answer</param>
        /// <param name="importance">Importanse flag. Values: true - important, false - not important.</param>
        /// <param name="subject">Template subject</param>
        /// <param name="tags">List of tags id added to message</param>
        /// <param name="body">Template body as html string.</param>
        /// <param name="attachments">List of attachments represented as MailAttachment object</param>
        /// <param name="calendarIcs">Calendar event ical-format for sending</param>
        /// <returns>Saved template id</returns>
        /// <short>SaveToTemplate message</short> 
        /// <category>Templates</category>
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
        ///    Removes the selected messages
        /// </summary>
        /// <param name="ids">List of messages ids for remove.</param>
        /// <returns>List of removed messages ids</returns>
        /// <short>Remove messages</short> 
        /// <category>Messages</category>
        [Update(@"messages/remove")]
        public IEnumerable<int> RemoveMessages(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailEngineFactory.MessageEngine.SetRemoved(ids);

            return ids;
        }

        /// <summary>
        ///    Returns the message template. Message teplate - empty message JSON.
        /// </summary>
        /// <returns>Empty MailMessageItem</returns>
        /// <short>Get message template</short> 
        /// <category>Messages</category>
        [Read(@"messages/template")]
        public MailMessage GetMessageTemplate()
        {
            return MailEngineFactory.DraftEngine.GetTemplate();
        }

        /// <summary>
        ///    Attaches Teamlab document to the specified message
        /// </summary>
        /// <param name="id"> Message id for adding attachment</param>
        /// <param name="fileId">Teamlab document id.</param>
        /// <param name="version">Teamlab document version</param>
        /// <param name="needSaveToTemp">Need save to temp for templates</param>
        /// <returns>Attached document as MailAttachment object</returns>
        /// <short>Attach Teamlab document</short>
        /// <category>Messages</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
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
        /// Export mail to CRM relations history for some entities
        /// </summary>
        /// <param name="id_message">Id of any messages from the chain</param>
        /// <param name="crm_contact_ids">List of CrmContactEntity. List item format: {entity_id: 0, entity_type: 0}.
        /// Entity types: 1 - Contact, 2 - Case, 3 - Opportunity.
        /// </param>
        /// <returns>none</returns>
        /// <category>Messages</category>
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
