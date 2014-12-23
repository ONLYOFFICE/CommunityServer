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
using System.Linq;
using System.Threading;
using ASC.Api.Attributes;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Collection;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.Filter;
using ASC.Specific;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns filtered conversations, if were changes since last check date
        /// </summary>
        /// <param optional="true" name="folder">Folder ID - integer. 1 - inbox, 2 - sent, 3 - drafts, 4 - trash, 5 - spam.</param>
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
        /// <param optional="true" name="last_check_date">Last messages request date</param>
        /// <param name="sortorder">Sort order by date. String parameter: "ascending" - ascended, "descending" - descended.</param> 
        /// <param optional="true" name="from_date">Date from wich conversations search performed</param>
        /// <param optional="true" name="from_message">Message from wich conversations search performed</param>
        /// <param name="prev_flag"></param>
        /// <returns>List of filtered chains</returns>
        /// <short>Gets filtered conversations</short>
        /// <category>Conversations</category>
        [Read(@"conversations")]
        public IEnumerable<MailMessageItem> GetFilteredConversations(int? folder,
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
            ApiDateTime last_check_date,
            string sortorder,
            ApiDateTime from_date,
            int? from_message,
            bool? prev_flag
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

                var compare_res = api_date.CompareTo(last_check_date);

                switch (compare_res)
                {
                    case 0:
                        return null;
                    case -1:
                        return new List<MailMessageItem>();
                }
            }

            bool has_more;
            var conversations = MailBoxManager.GetConversations(
                TenantId,
                Username,
                filter,
                from_date,
                from_message.GetValueOrDefault(0),
                prev_flag,
                out has_more);
            if (has_more)
                _context.SetTotalCount(filter.PageSize + 1);
            else
                _context.SetTotalCount(conversations.Count);
            return conversations;
        }

        /// <summary>
        /// Get list of messages linked into one chain (conversation)
        /// </summary>
        /// <param name="id">ID of any message in the chain</param>
        /// <param name="load_all_content">Load content of all messages</param>
        /// <returns>List messages linked in one chain</returns>
        /// <category>Conversations</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Read(@"conversation/{id:[0-9]+}")]
        public IEnumerable<MailMessageItem> GetConversation(int id, bool load_all_content)
        {
            if (id <= 0)
                throw new ArgumentException("id must be positive integer", "id");

            return MailBoxManager.GetConversationMessages(TenantId, Username, id, load_all_content);
        }

        /// <summary>
        /// Get previous or next conversation id.
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
        /// <returns>Head message id of previous or next conversation.</returns>
        /// <category>Conversations</category>
        [Read(@"conversation/{id:[0-9]+}/{direction:(next|prev)}")]
        public long GetPrevNextConversationId( int id,
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

            return MailBoxManager.GetNextConversationId(TenantId, Username, id, filter);
        }

        /// <summary>
        ///    Moves conversation specified in ids to the folder.
        /// </summary>
        /// <param name="ids">List of mesasges ids from conversations.</param>
        /// <param name="folder">Folder ID - integer. 1 - inbox, 2 - sent, 3 - drafts, 4 - trash, 5 - spam.</param>
        /// <returns>List of mesasges ids from conversations.</returns>
        /// <short>Move conversations to folder</short>
        /// <category>Conversations</category>
        [Update(@"conversations/move")]
        public IEnumerable<int> MoveConversations(IEnumerable<int> ids, int folder)
        {
            //Todo: remove useless transformations
            var conversations = ids as int[] ?? ids.ToArray();
            var ids_list = new List<int>(conversations);
            MailBoxManager.SetConversationsFolder(TenantId, Username, folder, ids_list);

            if(folder == MailFolder.Ids.spam)
                MailBoxManager.SendConversationsToSpamTrainer(TenantId, Username, ids_list, true);

            return conversations;
        }

        /// <summary>
        ///    Restores all the conversations previously moved to specific folders to their original folders.
        /// </summary>
        /// <param name="ids">List of conversation ids for restore.</param>
        /// <returns>List of restored conversations ids</returns>
        /// <short>Restore conversations to original folders</short>
        /// <category>Conversations</category>
        [Update(@"conversations/restore")]
        public IEnumerable<int> RestoreConversations(IEnumerable<int> ids)
        {
            //Todo: remove useless transformations
            var conversations = ids as int[] ?? ids.ToArray();
            var ids_list = new List<int>(conversations);
            MailBoxManager.RestoreConversations(TenantId, Username, ids_list);
            MailBoxManager.SendConversationsToSpamTrainer(TenantId, Username, ids_list, false);
            return conversations;
        }

        /// <summary>
        ///    Removes conversations from folders
        /// </summary>
        /// <param name="ids">List of conversation ids for remove.</param>
        /// <returns>List of removed conversation ids</returns>
        /// <short>Remove conversations</short>
        /// <category>Conversations</category>
        [Update(@"conversations/remove")]
        public IEnumerable<int> RemoveConversations(IEnumerable<int> ids)
        {
            //Todo: remove useless transformations
            var conversations = ids as int[] ?? ids.ToArray();
            var ids_list = new List<int>(conversations);
            MailBoxManager.DeleteConversations(TenantId, Username, ids_list);
            return conversations;
        }

        /// <summary>
        ///    Sets the status for the conversations specified by ids.
        /// </summary>
        /// <param name="ids">List of conversation ids for status changing.</param>
        /// <param name="status">String parameter specifies status for changing. Values: "read", "unread", "important" and "normal"</param>
        /// <returns>List of status changed conversations.</returns>
        /// <short>Set conversations status</short>
        /// <category>Conversations</category>
        [Update(@"conversations/mark")]
        public IEnumerable<int> MarkConversations(IEnumerable<int> ids, string status)
        {
            //Todo: remove useless transformations
            var conversations = ids as int[] ?? ids.ToArray();
            var ids_list = new List<int>(conversations);

            switch (status)
            {
                case "read":
                    MailBoxManager.SetConversationsReadFlags(TenantId, Username, ids_list, true);
                    break;

                case "unread":
                    MailBoxManager.SetConversationsReadFlags(TenantId, Username, ids_list, false);
                    break;

                case "important":
                    MailBoxManager.SetConversationsImportanceFlags(TenantId, Username, true, ids_list);
                    break;

                case "normal":
                    MailBoxManager.SetConversationsImportanceFlags(TenantId, Username, false, ids_list);
                    break;
            }
            return conversations;
        }

        /// <summary>
        ///    Add the specified tag to conversations.
        /// </summary>
        /// <param name="tag_id">Tag id for adding.</param>
        /// <param name="messages">List of conversation ids for tag adding.</param>
        /// <returns>Added tag_id</returns>
        /// <short>Add tag to conversations</short> 
        /// <category>Conversations</category>
        ///<exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"conversations/tag/{tag_id}/set")]
        public int SetConversationsTag(int tag_id, IEnumerable<int> messages)
        {
            var messages_ids = messages as IList<int> ?? messages.ToList();
            if (!messages_ids.Any())
                throw new ArgumentException("Message ids are empty", "messages_ids");

            MailBoxManager.SetConversationsTag(TenantId, Username, tag_id, messages_ids);
            return tag_id;
        }

        /// <summary>
        ///    Removes the specified tag from conversations.
        /// </summary>
        /// <param name="tag_id">Tag id to removing.</param>
        /// <param name="messages">List of conversation ids for tag removing.</param>
        /// <returns>Removed tag_id</returns>
        /// <short>Remove tag from conversations</short> 
        /// <category>Conversations</category>
        ///<exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"conversations/tag/{tag_id}/unset")]
        public int UnsetConversationsTag(int tag_id, IEnumerable<int> messages)
        {
            var messages_ids = messages as IList<int> ?? messages.ToList();
            if (!messages_ids.Any())
                throw new ArgumentException();

            MailBoxManager.UnsetConversationsTag(TenantId, Username, tag_id, messages_ids);
            return tag_id;
        }

        /// <summary>
        /// Marks conversation as CRM linked. All new mail will be added to CRM history.
        /// </summary>
        /// <param name="id_message">Id of any messages from the chain</param>
        /// <param name="crm_contact_ids">List of CrmContactEntity. List item format: {entity_id: 0, entity_type: 0}.
        /// Entity types: 1 - Contact, 2 - Case, 3 - Opportunity.
        /// </param>
        /// <returns>none</returns>
        /// <category>Conversations</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"conversations/crm/link")]
        public void LinkConversationToCrm(int id_message, IEnumerable<CrmContactEntity> crm_contact_ids)
        {
            try
            {
                if (id_message < 0)
                    throw new ArgumentException("Invalid message id", "id_message");
                if (crm_contact_ids == null)
                    throw new ArgumentException("Invalid contact ids list", "crm_contact_ids");

                MailBoxManager.LinkChainToCrm(id_message, TenantId, Username, crm_contact_ids.ToList());
            }
            catch (Exception ex)
            {
                MailBoxManager.CreateCrmOperationFailureAlert(TenantId, Username, id_message, MailBoxManager.AlertTypes.LinkFailure);
                Logger.Error(ex, "Issue with link to crm message_id: {0}, crm_contacts_id {1}", new object[] { id_message, crm_contact_ids });
            }
        }

        /// <summary>
        /// Marks conversation as CRM linked. All new mail will be added to CRM history.
        /// </summary>
        /// <param name="id_message">Id of any messages from the chain</param>
        /// <param name="crm_contact_ids">List of CrmContactEntity. List item format: {entity_id: 0, entity_type: 0}.
        /// Entity types: 1 - Contact, 2 - Case, 3 - Opportunity.
        /// </param>
        /// <returns>none</returns>
        /// <category>Conversations</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"conversations/crm/mark")]
        public void MarkConversationAsCrmLinked(int id_message, IEnumerable<CrmContactEntity> crm_contact_ids)
        {
            try
            {
                if (id_message < 0)
                    throw new ArgumentException("Invalid message id", "id_message");
                if (crm_contact_ids == null)
                    throw new ArgumentException("Invalid contact ids list", "crm_contact_ids");

                MailBoxManager.MarkChainAsCrmLinked(id_message, TenantId, Username, crm_contact_ids.ToList());
            }
            catch (Exception ex)
            {
                MailBoxManager.CreateCrmOperationFailureAlert(TenantId, Username, id_message, MailBoxManager.AlertTypes.LinkFailure);
                Logger.Error(ex, "Issue with link to crm message_id: {0}, crm_contacts_id {1}", new object[] { id_message, crm_contact_ids });
            }
        }

        /// <summary>
        /// Method tears conversation link with crm.
        /// </summary>
        /// <param name="id_message">Id of any messages from the chain</param>
        /// <param name="crm_contact_ids">List of CrmContactEntity. List item format: {entity_id: 0, entity_type: 0}.
        /// Entity types: 1 - Contact, 2 - Case, 3 - Opportunity.
        /// </param>
        /// <returns>none</returns>
        /// <category>Conversations</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"conversations/crm/unmark")]
        public void UnmarkConversationAsCrmLinked(int id_message, IEnumerable<CrmContactEntity> crm_contact_ids)
        {
            if (id_message < 0)
                throw new ArgumentException("Invalid message id", "id_message");
            if(crm_contact_ids == null)
                throw new ArgumentException("Invalid contact ids list", "crm_contact_ids");

            MailBoxManager.UnmarkChainAsCrmLinked(id_message, TenantId, Username, crm_contact_ids);
        }

        /// <summary>
        /// Method checks is chain crm linked by message_id.
        /// </summary>
        /// <param name="message_id">Id of any messages from the chain</param>
        /// <returns> Bool: true or false.</returns>
        /// <category>Conversations</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Read(@"conversations/link/crm/status")]
        public bool IsConversationLinkedWithCrm(int message_id)
        {
            if(message_id < 0)
                throw new ArgumentException("Invalid message id", "message_id");

            return MailBoxManager.GetLinkedCrmEntitiesId(message_id, TenantId, Username).Count > 0;
        }


    }
}
