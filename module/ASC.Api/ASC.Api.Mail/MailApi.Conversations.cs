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
using System.Web;

using ASC.Api.Attributes;
using ASC.Mail;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Specific;
// ReSharper disable InconsistentNaming

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Returns the conversations with the parameters specified in the request.
        /// </summary>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="folder">Folder type: 1 - inbox, 2 - sent, 3 - drafts, 4 - trash, 5 - spam</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="unread">Conversation status: unread (true), read (false), or all (null) messages</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="attachments">Defines if a conversation has attachments or not: with attachments (true), without attachments (false), or all (null) messages</param>
        /// <param type="System.Nullable{System.Int64}, System" method="url" optional="true" name="period_from">Start search period date</param>
        /// <param type="System.Nullable{System.Int64}, System" method="url" optional="true" name="period_to">End search period date</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="important">Specifies if only important conversations will be displayed or not</param>
        /// <param type="System.String, System" method="url" optional="true" name="from_address">Mail address from which the conversation messages were received</param>
        /// <param type="System.String, System" method="url" optional="true" name="to_address">Mail address to which the conversation messages were sent</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="mailbox_id">Recipient mailbox ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" method="url" optional="true" name="tags">IDs of tags linked to the target conversation</param>
        /// <param type="System.String, System" method="url" optional="true" name="search">Text to search in the conversation bodies and subjects</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="page_size">Number of conversations on the page</param>
        /// <param type="System.String, System" method="url" name="sortorder">Sort order by date: "ascending" or "descending"</param> 
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" optional="true" name="from_date">Start date of conversation search</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="from_message">Message from which the conversations are searched</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="with_calendar">Specifies if the conversations have a calendar or not</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="user_folder_id">User folder ID</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" name="prev_flag">Direction sort of the conversation list: true for previous, false for next</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailMessageData, ASC.Mail">List of filtered conversations</returns>
        /// <short>Get filtered conversations</short>
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversations</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"conversations")]
        public IEnumerable<MailMessageData> GetFilteredConversations(int? folder,
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
            ApiDateTime from_date,
            int? from_message,
            bool? prev_flag,
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
                PageSize = page_size.GetValueOrDefault(25),
                Sort = Defines.ORDER_BY_DATE_CHAIN,
                SortOrder = sortorder,
                WithCalendar = with_calendar,
                UserFolderId = user_folder_id,
                FromDate = from_date,
                FromMessage = from_message.GetValueOrDefault(0),
                PrevFlag = prev_flag.GetValueOrDefault(false)
            };

            bool hasMore;

            var conversations = MailEngineFactory.ChainEngine.GetConversations(filter, out hasMore);

            if (hasMore)
            {
                _context.SetTotalCount(page_size.GetValueOrDefault(25) + 1);
            }
            else
            {
                _context.SetTotalCount(conversations.Count);
            }

            return conversations;
        }

        /// <summary>
        /// Returns a list of messages linked in one chain (conversation).
        /// </summary>
        /// <short>Get a conversation</short>
        /// <param type="System.Int32, System" method="url" name="id">ID of any message from the conversation</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" name="loadAll">Loads the content of all messages</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="markRead">Specifies if a conversation will be marked as read or not</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="needSanitize">Specifies if HTML needs to be prepared for the FCK editor or not</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailMessageData, ASC.Mail">List of messages linked in one chain</returns>
        /// <category>Conversations</category>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        /// <path>api/2.0/mail/conversation/{id}</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"conversation/{id:[0-9]+}")]
        public IEnumerable<MailMessageData> GetConversation(int id, bool? loadAll, bool? markRead, bool? needSanitize)
        {
            if (id <= 0)
                throw new ArgumentException(@"id must be positive integer", "id");
#if DEBUG
            var watch = new Stopwatch();
            watch.Start();
#endif
            var list = MailEngineFactory.ChainEngine.GetConversationMessages(TenantId, Username, id,
                loadAll.GetValueOrDefault(false),
                Defines.NeedProxyHttp,
                needSanitize.GetValueOrDefault(false),
                markRead.GetValueOrDefault(false));

            if (markRead.GetValueOrDefault(false))
            {
                ActionEngine.SendUserActivity(list.Select(x=>x.Id).ToList(), MailUserAction.SetAsRead);
            }
#if DEBUG
            watch.Stop();
            Logger.DebugFormat("Mail->GetConversation(id={0})->Elapsed {1}ms (NeedProxyHttp={2}, NeedSanitizer={3})", id,
                watch.Elapsed.TotalMilliseconds, Defines.NeedProxyHttp, needSanitize.GetValueOrDefault(false));
#endif
            var item = list.FirstOrDefault(m => m.Id == id);

            if (item == null || item.Folder != FolderType.UserFolder)
                return list;

            var userFolder = GetUserFolderByMailId((uint)item.Id);

            if (userFolder != null)
            {
                list.ForEach(m => m.UserFolderId = userFolder.Id);
            }

            return list;
        }

        /// <summary>
        /// Returns the previous or next conversation ID filtered with the parameters specified in the request.
        /// </summary>
        /// <short>Get the previous or next conversation ID</short>
        /// <param type="System.Int32, System" method="url" name="id">Head message ID of the current conversation</param>
        /// <param type="System.String, System" method="url" name="direction">Defines if the previous or next conversation is needed: "prev" for previous, "next" for next</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="folder">Folder type: 1 - inbox, 2 - sent, 5 - spam</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="unread">Conversation status: unread (true), read (false), or all (null) conversations</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="attachments">Defines if a conversation has attachments or not: with attachments (true), without attachments (false) or all (null) messages</param>
        /// <param type="System.Nullable{System.Int64}, System" method="url" optional="true" name="period_from">Start search period date</param>
        /// <param type="System.Nullable{System.Int64}, System" method="url" optional="true" name="period_to">End search period date</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="important">Specifies if only important conversations will be displayed or not</param>
        /// <param type="System.String, System" method="url" optional="true" name="from_address">Mail address from which the conversation messages were received</param>
        /// <param type="System.String, System" method="url" optional="true" name="to_address">Mail address to which the conversation messages were sent</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="mailbox_id">Recipient mailbox ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" method="url" optional="true" name="tags">IDs of tags linked to the target conversation</param>
        /// <param type="System.String, System" method="url" optional="true" name="search">Text to search in the conversation bodies and subjects</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="page_size">Number of conversations on the page</param>
        /// <param type="System.String, System" method="url" name="sortorder">Sort order by date: "ascending" or "descending"</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="with_calendar">Specifies if the conversations have a calendar or not</param> 
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="user_folder_id">User folder ID</param>
        /// <returns>Head message ID of the previous or next conversation</returns>
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversation/{id}/{direction}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"conversation/{id:[0-9]+}/{direction:(next|prev)}")]
        public long GetPrevNextConversationId(int id,
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
                PageSize = page_size.GetValueOrDefault(25),
                Sort = Defines.ORDER_BY_DATE_CHAIN,
                SortOrder = sortorder,
                WithCalendar = with_calendar,
                UserFolderId = user_folder_id
            };

            return MailEngineFactory.ChainEngine.GetNextConversationId(id, filter);
        }

        /// <summary>
        /// Moves conversations with the IDs specified in the request to the selected folder.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="ids">List of message IDs from the conversations</param>
        /// <param type="System.Int32, System" name="folder">Folder type: 1 - inbox, 2 - sent, 3 - drafts, 4 - trash, 5 - spam</param>
        /// <param type="System.Nullable{System.UInt32}, System" optional="true" name="userFolderId">User folder ID</param>
        /// <returns>List of message IDs from the conversations</returns>
        /// <short>Move conversations</short>
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversations/move</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"conversations/move")]
        public IEnumerable<int> MoveConversations(List<int> ids, int folder, uint? userFolderId = null)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            var toFolder = (FolderType)folder;

            if (!MailFolder.IsIdOk(toFolder))
                throw new ArgumentException(@"Invalid folder id", "folder");

            List<int> idsToImapSync = MailEngineFactory.ChainEngine.SetConversationsFolder(ids, toFolder, userFolderId);

            ActionEngine.SendUserActivity(idsToImapSync, MailUserAction.MoveTo, folder, userFolderId);

            if (toFolder != FolderType.Spam)
                return ids;


            //TODO: Try to move message (IMAP only) to spam folder on original server (need new separated operation)

            var scheme = HttpContext.Current == null ? Uri.UriSchemeHttp : HttpContext.Current.Request.GetUrlRewriter().Scheme;
            MailEngineFactory.SpamEngine.SendConversationsToSpamTrainer(TenantId, Username, ids, true, scheme);

            return ids;
        }

        /// <summary>
        /// Restores the conversations with the IDs specified in the request to their original folders.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="ids">List of conversation IDs</param>
        /// <param type="System.Boolean, System" optional="true" name="learnSpamTrainer">Specifies whether to send messages to the spam training or not</param>
        /// <returns>List of restored conversation IDs</returns>
        /// <short>Restore conversations</short>
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversations/restore</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"conversations/restore")]
        public IEnumerable<int> RestoreConversations(List<int> ids, bool learnSpamTrainer = false)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            List<int> idsToImapSync=MailEngineFactory.ChainEngine.RestoreConversations(TenantId, Username, ids);

            ActionEngine.SendUserActivity(idsToImapSync, MailUserAction.Restore);

            if (learnSpamTrainer)
            {
                var scheme = HttpContext.Current == null ? Uri.UriSchemeHttp : HttpContext.Current.Request.GetUrlRewriter().Scheme;
                MailEngineFactory.SpamEngine.SendConversationsToSpamTrainer(TenantId, Username, ids, false, scheme);
            }

            return ids;
        }

        /// <summary>
        /// Removes conversations with the IDs specified in the request from the folders.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="ids">List of conversation IDs</param>
        /// <returns>List of removed conversation IDs</returns>
        /// <short>Remove conversations</short>
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversations/remove</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"conversations/remove")]
        public IEnumerable<int> RemoveConversations(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            List<int> idsToImapSync = MailEngineFactory.ChainEngine.DeleteConversations(TenantId, Username, ids);

            ActionEngine.SendUserActivity(idsToImapSync, MailUserAction.SetAsDeleted);

            return ids;
        }

        /// <summary>
        /// Sets a status to the conversations with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="ids">List of conversation IDs</param>
        /// <param type="System.String, System" name="status">New status ("read", "unread", "important", or "normal")</param>
        /// <returns>List of conversations with the changed status</returns>
        /// <short>Set a conversation status</short>
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversations/mark</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"conversations/mark")]
        public IEnumerable<int> MarkConversations(List<int> ids, string status)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailUserAction mailUserAction = MailUserAction.Nothing;
            List<int> idsToImapSync = new List<int>();

            switch (status)
            {
                case "read":
                    idsToImapSync=MailEngineFactory.MessageEngine.SetUnread(ids, false, true);
                    mailUserAction = MailUserAction.SetAsRead;
                    break;

                case "unread":
                    idsToImapSync = MailEngineFactory.MessageEngine.SetUnread(ids, true, true);
                    mailUserAction = MailUserAction.SetAsUnread;
                    break;

                case "important":
                    idsToImapSync = MailEngineFactory.ChainEngine.SetConversationsImportanceFlags(TenantId, Username, true, ids);
                    mailUserAction = MailUserAction.SetAsImportant;
                    break;

                case "normal":
                    idsToImapSync = MailEngineFactory.ChainEngine.SetConversationsImportanceFlags(TenantId, Username, false, ids);
                    mailUserAction = MailUserAction.SetAsNotImpotant;
                    break;
            }

            ActionEngine.SendUserActivity(idsToImapSync, mailUserAction);

            return ids;
        }

        /// <summary>
        /// Adds a tag specified in the request to the selected conversations.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="tag_id">Tag ID</param>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="messages">List of conversation IDs</param>
        /// <returns>Tag ID</returns>
        /// <short>Add a tag to the conversations</short> 
        /// <category>Conversations</category>
        ///<exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        ///<path>api/2.0/mail/conversations/tag/{tag_id}/set</path>
        ///<httpMethod>PUT</httpMethod>
        [Update(@"conversations/tag/{tag_id}/set")]
        public int SetConversationsTag(int tag_id, List<int> messages)
        {
            if (!messages.Any())
                throw new ArgumentException(@"Message ids are empty", "messages");

            MailEngineFactory.TagEngine.SetConversationsTag(messages, tag_id);

            return tag_id;
        }

        /// <summary>
        /// Removes a tag specified in the request from the conversations.
        /// </summary>
        /// <param type="System.Int32, System"  method="url" name="tag_id">Tag ID</param>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="messages">List of conversation IDs</param>
        /// <returns>Tag ID</returns>
        /// <short>Remove a tag from the conversations</short> 
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversations/tag/{tag_id}/unset</path>
        /// <httpMethod>PUT</httpMethod>
        ///<exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Update(@"conversations/tag/{tag_id}/unset")]
        public int UnsetConversationsTag(int tag_id, List<int> messages)
        {
            if (!messages.Any())
                throw new ArgumentException(@"Message ids are empty", "messages");

            MailEngineFactory.TagEngine.UnsetConversationsTag(messages, tag_id);

            return tag_id;
        }

        /// <summary>
        /// Links a conversation to the CRM entities. All the new mails will be added to the CRM history.
        /// </summary>
        /// <short>Link a conversation to CRM</short>
        /// <param type="System.Int32, System" name="id_message">ID of any message from the conversation</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Mail.Data.Contracts.CrmContactData}, System.Collections.Generic" name="crm_contact_ids">List of CRM entities in the following format: {entity_id: 0, entity_type: 0}.
        /// Entity types: 1 - Contact, 2 - Case, 3 - Opportunity
        /// </param>
        /// <returns></returns>
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversations/crm/link</path>
        /// <httpMethod>PUT</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Update(@"conversations/crm/link")]
        public void LinkConversationToCrm(int id_message, IEnumerable<CrmContactData> crm_contact_ids)
        {
            if (id_message < 0)
                throw new ArgumentException(@"Invalid message id", "id_message");
            if (crm_contact_ids == null)
                throw new ArgumentException(@"Invalid contact ids list", "crm_contact_ids");

            var scheme = HttpContext.Current == null
                ? Uri.UriSchemeHttp
                : HttpContext.Current.Request.GetUrlRewriter().Scheme;

            MailEngineFactory.CrmLinkEngine.LinkChainToCrm(id_message, crm_contact_ids.ToList(), scheme);
        }

        /// <summary>
        /// Marks a conversation as CRM linked. All the new mails will be added to the CRM history.
        /// </summary>
        /// <short>Mark a conversation as CRM linked</short>
        /// <param type="System.Int32, System" name="id_message">ID of any messages from the conversation</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Mail.Data.Contracts.CrmContactData}, System.Collections.Generic" name="crm_contact_ids">List of CRM entities in the following format: {entity_id: 0, entity_type: 0}.
        /// Entity types: 1 - Contact, 2 - Case, 3 - Opportunity
        /// </param>
        /// <returns></returns>
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversations/crm/mark</path>
        /// <httpMethod>PUT</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Update(@"conversations/crm/mark")]
        public void MarkConversationAsCrmLinked(int id_message, IEnumerable<CrmContactData> crm_contact_ids)
        {
            if (id_message < 0)
                throw new ArgumentException(@"Invalid message id", "id_message");
            if (crm_contact_ids == null)
                throw new ArgumentException(@"Invalid contact ids list", "crm_contact_ids");

            MailEngineFactory.CrmLinkEngine.MarkChainAsCrmLinked(id_message, crm_contact_ids.ToList());
        }

        /// <summary>
        /// Unmarks a conversation as CRM linked.
        /// </summary>
        /// <short>Unmark a conversation as CRM linked</short>
        /// <param type="System.Int32, System" name="id_message">ID of any messages from the conversation</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Mail.Data.Contracts.CrmContactData}, System.Collections.Generic" name="crm_contact_ids">List of CRM entities in the following format: {entity_id: 0, entity_type: 0}.
        /// Entity types: 1 - Contact, 2 - Case, 3 - Opportunity
        /// </param>
        /// <returns></returns>
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversations/crm/unmark</path>
        /// <httpMethod>PUT</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Update(@"conversations/crm/unmark")]
        public void UnmarkConversationAsCrmLinked(int id_message, IEnumerable<CrmContactData> crm_contact_ids)
        {
            if (id_message < 0)
                throw new ArgumentException(@"Invalid message id", "id_message");
            if (crm_contact_ids == null)
                throw new ArgumentException(@"Invalid contact ids list", "crm_contact_ids");

            MailEngineFactory.CrmLinkEngine.UnmarkChainAsCrmLinked(id_message, crm_contact_ids);
        }

        /// <summary>
        /// Checks if a conversation is CRM linked or not by message ID.
        /// </summary>
        /// <short>Check a conversation CRM status</short>
        /// <param type="System.Int32, System" name="message_id">ID of any messages from the conversation</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailCrmStatus, ASC.Mail">Conversation CRM status</returns>
        /// <category>Conversations</category>
        /// <path>api/2.0/mail/conversations/link/crm/status</path>
        /// <httpMethod>GET</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Read(@"conversations/link/crm/status")]
        public MailCrmStatus IsConversationLinkedWithCrm(int message_id)
        {
            if (message_id < 0)
                throw new ArgumentException(@"Invalid message id", "message_id");

            var entities = GetLinkedCrmEntitiesInfo(message_id);

            var result = new MailCrmStatus(message_id, entities.Any());

            return result;
        }
    }
}
