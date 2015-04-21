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
using System.Linq;
using System.Net.Mail;
using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Mail.DAO;
using ASC.Api.Mail.Extensions;
using ASC.Files.Core.Security;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.Filter;
using ASC.Mail.Aggregator.Utils;
using ASC.Specific;
using ASC.Mail.Aggregator.Exceptions;
using ASC.Api.Mail.Resources;

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
        /// <param optional="true" name="find_address">Address to find</param>
        /// <param optional="true" name="mailbox_id">Recipient mailbox id</param>
        /// <param optional="true" name="tags">Message tags</param>
        /// <param optional="true" name="search">Text to search in messages</param>
        /// <param optional="true" name="page">Page number</param>
        /// <param optional="true" name="page_size">Number of messages on page</param>
        /// <param optional="true" name="last_check_date">Last message request date</param>
        /// <param name="sort">Sort</param>
        /// <param name="sortorder">Sort order</param>
        /// <returns>Messages list</returns>
        /// <short>Get filtered messages</short> 
        /// <category>Messages</category>
        [Read(@"messages")]
        public IEnumerable<MailMessageItem> GetFilteredMessages(int? folder,
            bool? unread,
            bool? attachments,
            long? period_from,
            long? period_to,
            bool? important,
            string find_address,
            int? mailbox_id,
            IEnumerable<int> tags,
            string search,
            int? page,
            int? page_size,
            ApiDateTime last_check_date,
            string sort,
            string sortorder
            )
        {
            var filter = new MailFilter
            {
                PrimaryFolder = folder.GetValueOrDefault(MailFolder.Ids.inbox),
                Unread = unread,
                Attachments = attachments.GetValueOrDefault(false),
                PeriodFrom = period_from.GetValueOrDefault(0),
                PeriodTo = period_to.GetValueOrDefault(0),
                Important = important.GetValueOrDefault(false),
                FindAddress = find_address,
                MailboxId = mailbox_id,
                CustomLabels = new ItemList<int>(tags),
                SearchFilter = search,
                PageSize = page_size.GetValueOrDefault(25),
                SortOrder = sortorder
            };

            if (null != last_check_date)
            {
                var dateTime = MailBoxManager.GetFolderModifyDate(TenantId, Username, filter.PrimaryFolder);
                var apiDate = new ApiDateTime(dateTime);

                var compareRez = apiDate.CompareTo(last_check_date);

                if (compareRez == 0) // equals
                    return null;
                if (compareRez == -1) // less
                    return new List<MailMessageItem>();
            }

            long totalMessages;
            var messages = GetFilteredMessages(filter, filter.Page, filter.PageSize, out totalMessages);
            CorrectPageValue(filter, totalMessages);
            _context.SetTotalCount(totalMessages);
            return messages;
        }

        /// <summary>
        ///    Returns the detailed information about message with the ID specified in the request
        /// </summary>
        /// <param name="id">Message ID</param>
        /// <param optional="true" name="unblocked">Unblock suspicious content or not</param>
        /// <param optional="true" name="is_need_to_sanitize_html">Flag specifies is needed to prepare html for FCKeditor</param>
        /// <param optional="true" name="mark_read">Mark message as read</param>
        /// <returns>MailMessageItem</returns>
        /// <short>Get message</short>
        /// <category>Messages</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="ItemNotFoundException">Exception happens when message with specified id wasn't founded.</exception>
        [Read(@"messages/{id:[0-9]+}")]
        public MailMessageItem GetMessage(int id, bool? unblocked, bool? is_need_to_sanitize_html, bool? mark_read)
        {
            if (id <= 0)
                throw new ArgumentException(@"Invalid message id", "id");

            var unblockedFlag = unblocked.GetValueOrDefault(false);
            var needSanitizeHtml = is_need_to_sanitize_html.GetValueOrDefault(false);

            var item = MailBoxManager.GetMailInfo(TenantId, Username, id, unblockedFlag, true);
            if (item == null)
                throw new ItemNotFoundException(String.Format("Message with {0} wasn't founded.", id));

            if (item.WasNew && mark_read.HasValue && mark_read.Value)
                MailBoxManager.SetMessagesReadFlags(TenantId, Username, new List<int> {(int) item.Id}, true);

            if (needSanitizeHtml)
                item.HtmlBody = HtmlSanitizer.SanitizeHtmlForEditor(item.HtmlBody);

            return item;
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
        /// <param optional="true" name="find_address">Address to find. Email for search in all mail fields: from, to</param>
        /// <param optional="true" name="mailbox_id">Recipient mailbox id.</param>
        /// <param optional="true" name="tags">Messages tags. Id of tags linked with target messages.</param>
        /// <param optional="true" name="search">Text to search in messages body and subject.</param>
        /// <param optional="true" name="page_size">Count on messages on page</param>
        /// <param name="sortorder">Sort order by date. String parameter: "ascending" - ascended, "descending" - descended.</param> 
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
            string find_address,
            int? mailbox_id,
            IEnumerable<int> tags,
            string search,
            int? page_size,
            string sortorder)
        {
            // inverse sort order if prev message require
            if ("prev" == direction)
                sortorder = "ascending" == sortorder ? "descending" : "ascending";

            var filter = new MailFilter
            {
                PrimaryFolder = folder.GetValueOrDefault(MailFolder.Ids.inbox),
                Unread = unread,
                Attachments = attachments.GetValueOrDefault(false),
                PeriodFrom = period_from.GetValueOrDefault(0),
                PeriodTo = period_to.GetValueOrDefault(0),
                Important = important.GetValueOrDefault(false),
                FindAddress = find_address,
                MailboxId = mailbox_id,
                CustomLabels = new ItemList<int>(tags),
                SearchFilter = search,
                PageSize = page_size.GetValueOrDefault(25),
                SortOrder = sortorder
            };

            return MailBoxManager.GetNextMessageId(TenantId, Username, id, filter);
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

            MailBoxManager.DeleteMessageAttachments(TenantId, Username, messageid, new List<int> { attachmentid });

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
                    MailBoxManager.SetMessagesReadFlags(TenantId, Username, ids, true);
                    break;

                case "unread":
                    MailBoxManager.SetMessagesReadFlags(TenantId, Username, ids, false);
                    break;

                case "important":
                    MailBoxManager.SetMessagesImportanceFlags(TenantId, Username, true, ids);
                    break;

                case "normal":
                    MailBoxManager.SetMessagesImportanceFlags(TenantId, Username, false, ids);
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

            MailBoxManager.RestoreMessages(TenantId, Username, ids);

            return ids;
        }

        /// <summary>
        ///    Moves the messages to the specified folder
        /// </summary>
        /// <param name="ids">List of mesasges ids.</param>
        /// <param name="folder">Folder ID - integer. 1 - inbox, 2 - sent, 3 - drafts, 4 - trash, 5 - spam.</param>
        /// <returns>List of moved messages ids.</returns>
        /// <short>Move message to folder</short> 
        /// <category>Messages</category>
        [Update(@"messages/move")]
        public IEnumerable<int> MoveMessages(List<int> ids, int folder)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailBoxManager.SetMessagesFolder(TenantId, Username, folder, ids);
            return ids;
        }

        /// <summary>
        ///    Sends the message with the ID specified in the request
        /// </summary>
        /// <param name="id">Massage id which will be sended.</param>
        /// <param name="attachments">List of attachments represented as MailAttachment object.</param>
        /// <param name="to">List of "to" emails. Format: Name&lt;name@domain&gt;. </param>
        /// <param name="bcc">List of "bcc" emails. Format: Name&lt;name@domain&gt;. </param>
        /// <param name="cc">List of "cc" emails. Format: Name&lt;name@domain&gt;. </param>
        /// <param name="mimeMessageId">Mime message id which will be sended.</param>
        /// <param name="mimeReplyToId">Mime message id to which the reply answer.</param>
        /// <param name="from">From email. Format: Name&lt;name@domain&gt;.</param>
        /// <param name="body">Message body as html string.</param>
        /// <param name="importance">Importanse fla. Values: true - important, false - not important.</param>
        /// <param name="tags">List of tags id added to message</param>
        /// <param name="streamId">Stream id. Needed for correct attachment saving.</param>
        /// <param name="subject">Sended message subject.</param>
        /// <param name="fileLinksShareMode">Share mode for attached file links</param>
        /// <returns>message id</returns>
        /// <short>Send message</short> 
        /// <category>Messages</category>
        [Update(@"messages/send")]
        public int SendMessage(int id,
            IEnumerable<MailAttachment> attachments,
            IEnumerable<string> to,
            IEnumerable<string> bcc,
            IEnumerable<string> cc,
            string mimeMessageId,
            string mimeReplyToId,
            string from,
            string body,
            bool importance,
            IEnumerable<int> tags,
            string streamId,
            string subject,
            FileShare fileLinksShareMode
            )
        {
            var item = new MailSendItem
                {
                    Attachments = new List<MailAttachment>(attachments),
                    Bcc = new List<string>(bcc),
                    Cc = new List<string>(cc),
                    MimeMessageId = string.IsNullOrEmpty(mimeMessageId) ? MailBoxManager.CreateMessageId() : mimeMessageId,
                    MimeReplyToId = mimeReplyToId,
                    From = from,
                    HtmlBody = body,
                    Important = importance,
                    Labels = new List<int>(tags),
                    StreamId = streamId,
                    Subject = subject,
                    To = new List<string>(to),
                    FileLinksShareMode = fileLinksShareMode
                };

            var accounts = MailBoxManager.GetAccountInfo(TenantId, Username).ToAddressData();
            var mailAddress = new MailAddress(item.From);
            var account = accounts.FirstOrDefault(a => a.Email.ToLower().Equals(mailAddress.Address));

            if (account == null)
                throw new ArgumentException("Mailbox not found");

            if(account.IsGroup)
                throw new InvalidOperationException("Sending emails from a group address is forbidden");

            if (id > 0)
            {
                var message = GetMessage(id, false, false, false);

                if (message.Folder != MailFolder.Ids.drafts)
                {
                    throw new InvalidOperationException("Sending emails is permitted only in the Drafts folder");
                }
            }

            return SendQueue.Send(TenantId, Username, item, id, account.MailboxId);
        }

        /// <summary>
        ///    Saves the message with the ID specified in the request
        /// </summary>
        /// <param name="id">Mwssage id which will be saved.</param>
        /// <param name="attachments">List of attachments represented as MailAttachment object</param>
        /// <param name="to">List of "to" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="bcc">List of "bcc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="cc">List of "cc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="mimeMessageId">Mime message id which will be saved.</param>
        /// <param name="mimeReplyToId">Message id to which the reply answer</param>
        /// <param name="from">From email. Format: Name&lt;name@domain&gt;</param>
        /// <param name="body">Message body as html string.</param>
        /// <param name="importance">Importanse fla. Values: true - important, false - not important.</param>
        /// <param name="tags">List of tags id added to message</param>
        /// <param name="streamId">Stream id. Needed for correct attachment saving.</param>
        /// <param name="subject">Saved message subject</param>
        /// <returns>Saved message id</returns>
        /// <short>SaveToDraft message</short> 
        /// <category>Messages</category>
        [Update(@"messages/save")]
        public MailMessageItem SaveMessage(int id,
            IEnumerable<MailAttachment> attachments,
            IEnumerable<string> to,
            IEnumerable<string> bcc,
            IEnumerable<string> cc,
            string mimeMessageId,
            string mimeReplyToId,
            string from,
            string body,
            bool importance,
            IEnumerable<int> tags,
            string streamId,
            string subject)
        {
            if (id < 1)
                id = 0;

            var item = new MailSendItem
            {
                Attachments = new List<MailAttachment>(attachments),
                Bcc = new List<string>(bcc),
                Cc = new List<string>(cc),
                MimeReplyToId = mimeReplyToId,
                MimeMessageId = string.IsNullOrEmpty(mimeMessageId) ? MailBoxManager.CreateMessageId() : mimeMessageId,
                From = from,
                HtmlBody = body,
                Important = importance,
                Labels = new List<int>(tags),
                StreamId = streamId,
                Subject = subject,
                To = new List<string>(to)
            };

            var accounts = MailBoxManager.GetAccountInfo(TenantId, Username).ToAddressData();
            var mailAddress = new MailAddress(item.From);
            var account = accounts.FirstOrDefault(a => a.Email.ToLower().Equals(mailAddress.Address));

            if (account == null)
                throw new ArgumentException("Mailbox not found");

            if (account.IsGroup)
                throw new InvalidOperationException("Saving emails from a group address is forbidden");

            if (id > 0)
            {
                var message = GetMessage(id, false, false, false);

                if (message.Folder != MailFolder.Ids.drafts)
                {
                    throw new InvalidOperationException("Saving emails is permitted only in the Drafts folder");
                }
            }

            return SendQueue.SaveToDraft(TenantId, Username, item, id, account.MailboxId);
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

            MailBoxManager.DeleteMessages(TenantId, Username, ids);
            return ids;
        }

        /// <summary>
        ///    Returns the message template. Message teplate - empty message JSON.
        /// </summary>
        /// <returns>Empty MailMessageItem</returns>
        /// <short>Get message template</short> 
        /// <category>Messages</category>
        [Read(@"messages/template")]
        public MailMessageItem GetMessageTemplate()
        {
            var sendTemplate = new MailMessageItem
            {
                Attachments = new List<MailAttachment>(),
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
                StreamId = MailBoxManager.CreateNewStreamId()
            };
            return sendTemplate;
        }

        /// <summary>
        ///    Returns the modification date for the messages.
        /// </summary>
        /// <returns>DateTime for Message modify date</returns>
        /// <short>Get message modify date</short>
        /// <category>Messages</category>
        [Read(@"messages/modify_date")]
        public ApiDateTime GetMessagesModifyDate()
        {
            return new ApiDateTime(MailBoxManager.GetMessagesModifyDate(TenantId, Username));
        }

        ///  <summary>
        ///     Attaches Teamlab document to the specified message
        ///  </summary>
        ///  <param name="id"> Message id for adding attachment</param>
        /// <param name="fileId">Teamlab document id.</param>
        /// <param name="version">Teamlab document version</param>
        /// <param name="shareLink">Teamlab document share link</param>
        /// <param name="streamId">Message stream id</param>
        /// <returns>Attached document as MailAttachment object</returns>
        /// <short>Attach Teamlab document</short>
        /// <category>Messages</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Create(@"messages/{id:[0-9]+}/document")]
        public MailAttachment AttachDocument(int id, string fileId, string version, string shareLink, string streamId)
        {
            try
            {
                var attachment = MailBoxManager.AttachFileFromDocuments(TenantId, Username, id, fileId, version, shareLink, streamId);
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
        public void ExportMessageToCrm(int id_message, IEnumerable<CrmContactEntity> crm_contact_ids)
        {
            try
            {
                if (id_message < 0)
                    throw new ArgumentException(@"Invalid message id", "id_message");
                if (crm_contact_ids == null)
                    throw new ArgumentException(@"Invalid contact ids list", "crm_contact_ids");

                var messageItem = MailBoxManager.GetMailInfo(TenantId, Username, id_message, true, true);
                messageItem.LinkedCrmEntityIds = crm_contact_ids.ToList();
                var crmDal = new ASC.Mail.Aggregator.Dal.CrmHistoryDal(TenantId, Username);
                crmDal.AddRelationshipEvents(messageItem);
            }
            catch (Exception ex)
            {
                MailBoxManager.CreateCrmOperationFailureAlert(TenantId, Username, id_message, MailBoxManager.AlertTypes.ExportFailure);
                Logger.Error(ex, "Issue with exort to crm message_id: {0}, crm_contacts_id {1}", new object[]{id_message, crm_contact_ids});
            }
        }

        private IEnumerable<MailMessageItem> GetFilteredMessages(MailFilter filter, int page, int pageSize, out long totalMessagesCount)
        {
            return MailBoxManager.GetMailsFiltered(TenantId, Username, filter, page, pageSize, out totalMessagesCount);
        }

        private void CorrectPageValue(MailFilter filter, long totalMessages)
        {
            var maxPage = (int)Math.Ceiling((double)totalMessages / filter.PageSize);
            if (filter.Page > maxPage) filter.Page = maxPage;
            if (filter.Page < 1) filter.Page = 1;
        }
    }
}
