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
        /// <param name="sortorder">Sort order by date. String parameter: "ascending" - ascended, "descending" - descended.</param> 
        /// <param optional="true" name="from_date">Date from wich conversations search performed</param>
        /// <param optional="true" name="from_message">Message from wich conversations search performed</param>
        /// <param name="prev_flag"></param>
        /// <returns>List of filtered chains</returns>
        /// <short>Gets filtered conversations</short>
        /// <category>Conversations</category>
        [Read(@"conversations")]
        public IEnumerable<MailMessage> GetFilteredConversations(int? folder,
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

            bool hasMore;
            var conversations = MailBoxManager.GetConversations(
                TenantId,
                Username,
                filter,
                from_date,
                from_message.GetValueOrDefault(0),
                prev_flag,
                out hasMore);
            if (hasMore)
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
        /// <param optional="true" name="mark_read">Mark conversation as read</param>
        /// <returns>List messages linked in one chain</returns>
        /// <category>Conversations</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Read(@"conversation/{id:[0-9]+}")]
        public IEnumerable<MailMessage> GetConversation(int id, bool? load_all_content, bool? mark_read)
        {
            if (id <= 0)
                throw new ArgumentException(@"id must be positive integer", "id");

            return MailBoxManager.GetConversationMessages(TenantId, Username, id, load_all_content.GetValueOrDefault(false),
                                                          mark_read.GetValueOrDefault(false));
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
        public long GetPrevNextConversationId(int id,
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
        public IEnumerable<int> MoveConversations(List<int> ids, int folder)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailBoxManager.SetConversationsFolder(TenantId, Username, folder, ids);

            if(folder == MailFolder.Ids.spam)
                MailBoxManager.SendConversationsToSpamTrainer(TenantId, Username, ids, true);

            return ids;
        }

        /// <summary>
        ///    Restores all the conversations previously moved to specific folders to their original folders.
        /// </summary>
        /// <param name="ids">List of conversation ids for restore.</param>
        /// <param optional="true" name="learnSpamTrainer">send messages tp spam training</param>
        /// <returns>List of restored conversations ids</returns>
        /// <short>Restore conversations to original folders</short>
        /// <category>Conversations</category>
        [Update(@"conversations/restore")]
        public IEnumerable<int> RestoreConversations(List<int> ids, bool learnSpamTrainer = false)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailBoxManager.RestoreConversations(TenantId, Username, ids);

            if (learnSpamTrainer)
                MailBoxManager.SendConversationsToSpamTrainer(TenantId, Username, ids, false);

            return ids;
        }

        /// <summary>
        ///    Removes conversations from folders
        /// </summary>
        /// <param name="ids">List of conversation ids for remove.</param>
        /// <returns>List of removed conversation ids</returns>
        /// <short>Remove conversations</short>
        /// <category>Conversations</category>
        [Update(@"conversations/remove")]
        public IEnumerable<int> RemoveConversations(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailBoxManager.DeleteConversations(TenantId, Username, ids);
            return ids;
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
        public IEnumerable<int> MarkConversations(List<int> ids, string status)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");
            
            switch (status)
            {
                case "read":
                    MailBoxManager.SetConversationsReadFlags(TenantId, Username, ids, true);
                    break;

                case "unread":
                    MailBoxManager.SetConversationsReadFlags(TenantId, Username, ids, false);
                    break;

                case "important":
                    MailBoxManager.SetConversationsImportanceFlags(TenantId, Username, true, ids);
                    break;

                case "normal":
                    MailBoxManager.SetConversationsImportanceFlags(TenantId, Username, false, ids);
                    break;
            }
            return ids;
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
        public int SetConversationsTag(int tag_id, List<int> messages)
        {
            if (!messages.Any())
                throw new ArgumentException(@"Message ids are empty", "messages");

            MailBoxManager.SetConversationsTag(TenantId, Username, tag_id, messages);
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
        public int UnsetConversationsTag(int tag_id, List<int> messages)
        {
            if (!messages.Any())
                throw new ArgumentException(@"Message ids are empty", "messages");

            MailBoxManager.UnsetConversationsTag(TenantId, Username, tag_id, messages);
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
            if (id_message < 0)
                throw new ArgumentException(@"Invalid message id", "id_message");
            if (crm_contact_ids == null)
                throw new ArgumentException(@"Invalid contact ids list", "crm_contact_ids");

            MailBoxManager.LinkChainToCrm(id_message, TenantId, Username, crm_contact_ids.ToList());
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
            if (id_message < 0)
                throw new ArgumentException(@"Invalid message id", "id_message");
            if (crm_contact_ids == null)
                throw new ArgumentException(@"Invalid contact ids list", "crm_contact_ids");

            MailBoxManager.MarkChainAsCrmLinked(id_message, TenantId, Username, crm_contact_ids.ToList());
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
                throw new ArgumentException(@"Invalid message id", "id_message");
            if(crm_contact_ids == null)
                throw new ArgumentException(@"Invalid contact ids list", "crm_contact_ids");

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
                throw new ArgumentException(@"Invalid message id", "message_id");

            return MailBoxManager.GetLinkedCrmEntitiesId(message_id, TenantId, Username).Count > 0;
        }
    }
}
