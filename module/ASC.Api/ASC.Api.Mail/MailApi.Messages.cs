/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Mail.DAO;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.Filter;
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
                Period_from = period_from.GetValueOrDefault(0),
                Period_to = period_to.GetValueOrDefault(0),
                Important = important.GetValueOrDefault(false),
                FindAddress = find_address,
                MailboxId = mailbox_id,
                CustomLabels = new ItemList<int>(tags),
                SearchFilter = search,
                PageSize = page_size.GetValueOrDefault(25),
                SortOrder = sortorder
            };

            MailBoxManager.UpdateUserActivity(TenantId, Username);

            if (null != last_check_date)
            {
                var date_time = MailBoxManager.GetFolderModifyDate(TenantId, Username, filter.PrimaryFolder);
                var api_date = new ApiDateTime(date_time);

                var compare_rez = api_date.CompareTo(last_check_date);

                if (compare_rez == 0) // equals
                    return null;
                if (compare_rez == -1) // less
                    return new List<MailMessageItem>();
            }

            long total_messages;
            var messages = GetFilteredMessages(filter, filter.Page, filter.PageSize, out total_messages);
            CorrectPageValue(filter, total_messages);
            _context.SetTotalCount(total_messages);
            return messages;
        }
        /// <summary>
        ///    Returns the detailed information about message with the ID specified in the request
        /// </summary>
        /// <param name="id">Message ID</param>
        /// <param optional="true" name="unblocked">Unblock suspicious content or not</param>
        /// <param optional="true" name="is_need_to_sanitize_html">Flag specifies is needed to prepare html for FCKeditor</param>
        /// <returns>MailMessageItem</returns>
        /// <short>Get message</short>
        /// <category>Messages</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="ItemNotFoundException">Exception happens when message with specified id wasn't founded.</exception>
        [Read(@"messages/{id:[0-9]+}")]
        public MailMessageItem GetMessage(int id, bool? unblocked, bool? is_need_to_sanitize_html)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid message id", "id");

            var unblocked_flag = unblocked.GetValueOrDefault(false);
            var is_need_to_sanitize_html_f = is_need_to_sanitize_html.GetValueOrDefault(false);

            var item = MailBoxManager.GetMailInfo(TenantId, Username, id, unblocked_flag, true);
            if (item == null)
                throw new ItemNotFoundException(String.Format("Message with {0} wasn't founded.", id));

            if (is_need_to_sanitize_html_f)
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
                Period_from = period_from.GetValueOrDefault(0),
                Period_to = period_to.GetValueOrDefault(0),
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
                throw new ArgumentException("Invalid message id. Message id must be positive integer", "messageid");

            if (attachmentid <= 0)
                throw new ArgumentException("Invalid attachment id. Attachment id must be positive integer", "attachmentid");

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
        public IEnumerable<int> MarkMessages(IEnumerable<int> ids, string status)
        {
            //todo: remove useless conversions
            var mark_messages = ids as int[] ?? ids.ToArray();
            var ids_list = new List<int>(mark_messages);

            switch (status)
            {
                case "read":
                    MailBoxManager.SetMessagesReadFlags(TenantId, Username, ids_list, true);
                    break;

                case "unread":
                    MailBoxManager.SetMessagesReadFlags(TenantId, Username, ids_list, false);
                    break;

                case "important":
                    MailBoxManager.SetMessagesImportanceFlags(TenantId, Username, true, ids_list);
                    break;

                case "normal":
                    MailBoxManager.SetMessagesImportanceFlags(TenantId, Username, false, ids_list);
                    break;
            }
            return mark_messages;
        }

        /// <summary>
        ///    Restores the messages to their original folders
        /// </summary>
        /// <returns>IEnumerable</returns>
        /// <short>Restore message to folders</short> 
        /// <category>Messages</category>
        [Update(@"messages/restore")]
        public IEnumerable<int> RestoreMessages(IEnumerable<int> ids)
        {
            //todo: remove useless conversions
            var restore_messages = ids as int[] ?? ids.ToArray();
            var ids_list = new List<int>(restore_messages);
            MailBoxManager.RestoreMessages(TenantId, Username, ids_list);
            return restore_messages;
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
        public IEnumerable<int> MoveMessages(IEnumerable<int> ids, int folder)
        {
            //todo: remove useless conversions
            var messages_ids = ids as int[] ?? ids.ToArray();
            var ids_list = new List<int>(messages_ids);
            MailBoxManager.SetMessagesFolder(TenantId, Username, folder, ids_list);
            return messages_ids;
        }

        /// <summary>
        ///    Sends the message with the ID specified in the request
        /// </summary>
        /// <param name="id">Mwssage id which will be sended.</param>
        /// <param name="attachments">List of attachments represented as MailAttachment object</param>
        /// <param name="to">List of "to" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="bcc">List of "bcc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="cc">List of "cc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="replyToId">Message id to which the reply answer</param>
        /// <param name="from">From email. Format: Name&lt;name@domain&gt;</param>
        /// <param name="body">Message body as html string.</param>
        /// <param name="importance">Importanse fla. Values: true - important, false - not important.</param>
        /// <param name="tags">List of tags id added to message</param>
        /// <param name="streamId">Stream id. Needed for correct attachment saving.</param>
        /// <param name="subject">Sended message subject</param>
        /// <returns>message id</returns>
        /// <short>Send message</short> 
        /// <category>Messages</category>
        [Update(@"messages/send")]
        public int SendMessages(int id,
            IEnumerable<MailAttachment> attachments,
            IEnumerable<string> to,
            IEnumerable<string> bcc,
            IEnumerable<string> cc,
            int replyToId,
            string from,
            string body,
            bool importance,
            IEnumerable<int> tags,
            string streamId,
            string subject)
        {
            var item = new MailSendItem
                {
                    Attachments = new List<MailAttachment>(attachments),
                    Bcc = new List<string>(bcc),
                    Cc = new List<string>(cc),
                    ReplyToId = replyToId,
                    From = from,
                    HtmlBody = body,
                    Important = importance,
                    Labels = new List<int>(tags),
                    StreamId = streamId,
                    Subject = subject,
                    To = new List<string>(to)
                };
            return SendQueue.Send(TenantId, Username, item, id);
        }

        /// <summary>
        ///    Saves the message with the ID specified in the request
        /// </summary>
        /// <param name="id">Mwssage id which will be saved.</param>
        /// <param name="attachments">List of attachments represented as MailAttachment object</param>
        /// <param name="to">List of "to" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="bcc">List of "bcc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="cc">List of "cc" emails. Format: Name&lt;name@domain&gt; </param>
        /// <param name="replyToId">Message id to which the reply answer</param>
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
        public MailMessageItem SaveMessages(int id,
            IEnumerable<MailAttachment> attachments,
            IEnumerable<string> to,
            IEnumerable<string> bcc,
            IEnumerable<string> cc,
            int replyToId,
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
                ReplyToId = replyToId,
                From = from,
                HtmlBody = body,
                Important = importance,
                Labels = new List<int>(tags),
                StreamId = streamId,
                Subject = subject,
                To = new List<string>(to)
            };
            return SendQueue.SaveToDraft(TenantId, Username, item, id);
        }

        /// <summary>
        ///    Removes the selected messages
        /// </summary>
        /// <param name="ids">List of messages ids for remove.</param>
        /// <returns>List of removed messages ids</returns>
        /// <short>Remove messages</short> 
        /// <category>Messages</category>
        [Update(@"messages/remove")]
        public IEnumerable<int> RemoveMessages(IEnumerable<int> ids)
        {
            //todo: remove useless conversions
            var messages_ids = ids as int[] ?? ids.ToArray();
            var ids_list = new List<int>(messages_ids);
            MailBoxManager.DeleteMessages(TenantId, Username, ids_list);
            return messages_ids;
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
            var send_template = new MailMessageItem
            {
                Attachments = new List<MailAttachment>(),
                Bcc = "",
                Cc = "",
                Subject = "",
                From = "",
                HtmlBody = "",
                Important = false,
                ReplyTo = "",
                To = "",
                StreamId = MailBoxManager.CreateNewStreamId()
            };
            return send_template;
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
                //todo: rewrite to typed exceptions
            catch (AttachmentsException e)
            {
                string error_message;

                switch (e.ErrorType)
                {
                    case AttachmentsException.Types.BAD_PARAMS:
                        error_message = MailApiResource.AttachmentsBadInputParamsError;
                        break;
                    case AttachmentsException.Types.EMPTY_FILE:
                        error_message = MailApiResource.AttachmentsEmptyFileNotSupportedError;
                        break;
                    case AttachmentsException.Types.MESSAGE_NOT_FOUND:
                        error_message = MailApiResource.AttachmentsMessageNotFoundError;
                        break;
                    case AttachmentsException.Types.TOTAL_SIZE_EXCEEDED:
                        error_message = MailApiResource.AttachmentsTotalLimitError;
                        break;
                    case AttachmentsException.Types.DOCUMENT_NOT_FOUND:
                        error_message = MailApiResource.AttachmentsDocumentNotFoundError;
                        break;
                    case AttachmentsException.Types.DOCUMENT_ACCESS_DENIED:
                        error_message = MailApiResource.AttachmentsDocumentAccessDeniedError;
                        break;
                    default:
                        error_message = MailApiResource.AttachmentsUnknownError;
                        break;
                }
                throw new Exception(error_message);
            }
            catch (Exception)
            {
                throw new Exception(MailApiResource.AttachmentsUnknownError);
            }
        }

        /// <summary>
        ///    Sets the is_from_crm status to true for the selected messages. Method needed for hide Add to CRM Contact link in From field.
        /// </summary>
        /// <param name="emails">Emails which messages must be marked as from crm.</param>
        /// <param name="userIds">Teamlab users id in list.</param>
        /// <returns>List of updated emails</returns>
        /// <short>Set message crm status</short>
        /// <category>Messages</category>
        [Create(@"messages/update_crm")]
        public IEnumerable<string> UpdateCrmMessages(IEnumerable<string> emails, IEnumerable<string> userIds)
        {
            MailBoxManager.UpdateCrmMessages(TenantId, emails, userIds);
            return emails;
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
                    throw new ArgumentException("Invalid message id", "id_message");
                if (crm_contact_ids == null)
                    throw new ArgumentException("Invalid contact ids list", "crm_contact_ids");

                var message_item = MailBoxManager.GetMailInfo(TenantId, Username, id_message, true, true);
                message_item.LinkedCrmEntityIds = crm_contact_ids.ToList();
                var crm_dal = new ASC.Mail.Aggregator.Dal.CrmHistoryDal(MailBoxManager, TenantId, Username);
                crm_dal.AddRelationshipEvents(message_item);
            }
            catch (Exception ex)
            {
                MailBoxManager.CreateCrmOperationFailureAlert(TenantId, Username, id_message, MailBoxManager.AlertTypes.ExportFailure);
                Logger.Error(ex, "Issue with exort to crm message_id: {0}, crm_contacts_id {1}", new object[]{id_message, crm_contact_ids});
            }
        }

        private List<MailMessageItem> GetFilteredMessages(MailFilter filter, int page, int page_size, out long total_messages_count)
        {
            return MailBoxManager.GetMailsFiltered(TenantId, Username, filter, page, page_size, out total_messages_count);
        }

        private void CorrectPageValue(MailFilter filter, long total_messages)
        {
            var max_page = (int)Math.Ceiling((double)total_messages / filter.PageSize);
            if (filter.Page > max_page) filter.Page = max_page;
            if (filter.Page < 1) filter.Page = 1;
        }
    }
}
