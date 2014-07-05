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
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using System.Collections.Generic;

namespace ASC.Mail.Aggregator.CollectionService
{
    // Looks like this class is only  for logging requests from MailQueueItem to MailBoxManager
    public class MailItemManager
    {
        private readonly MailBoxManager _mailBoxManager;
        private readonly ILogger _log;
        private readonly List<MessageHandlerBase> _messageHandlers;

        public MailItemManager(MailBoxManager mail_box_manager, List<MessageHandlerBase> message_handlers)
        {
            if (mail_box_manager == null) throw new ArgumentNullException("mail_box_manager");
            _mailBoxManager = mail_box_manager;
            _messageHandlers = message_handlers;
            _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "MailItemManager");
        }

        public void GetStoredMessagesUIDL_MD5(MailQueueItem account, Dictionary<int, string> uidl_list, Dictionary<int, string> md5_list)
        {
            _mailBoxManager.GetStoredMessagesUIDL_MD5(account.Account.MailBoxId, uidl_list, md5_list);
        }

        public DateTime GetFolderModifyDate(MailQueueItem account, int folder_id)
        {
            return _mailBoxManager.GetMailboxFolderModifyDate(account.Account.MailBoxId, folder_id);
        }

        public void OnRetrieveNewMessage(MailQueueItem account,
            ActiveUp.Net.Mail.Message message,
            int folder_id,
            string uidl,
            string md5_hash,
            bool has_parse_error,
            bool unread,
            int[] tags_ids)
        {
            MailMessageItem message_item;
            if (_mailBoxManager.MailReceive(account.Account, message, folder_id, uidl, md5_hash, has_parse_error, unread, tags_ids, out message_item) < 1)
                throw new Exception("MailReceive() returned id < 1;");

            if (message_item == null) return;

            foreach (var handler in _messageHandlers)
            {
                try
                {
                    handler.HandleRetrievedMessage(account.Account, message, message_item, folder_id, uidl, md5_hash, unread, tags_ids);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "MailItemManager::OnRetrieveNewMessage");
                }
            }
        }

        public void OnUpdateUidl(MailBox account, int message_id, string new_uidl)
        {
            _mailBoxManager.UpdateMessageUidl(account.TenantId, account.UserId, message_id, new_uidl);
        }

        private void SetDelayExpires(MailBox mailbox) {
            var expires = DateTime.UtcNow + TimeSpan.FromSeconds(mailbox.ServerLoginDelay);
            _mailBoxManager.SetEmailLoginDelayExpires(mailbox.EMail.ToString(), expires);
        }

        public void OnAuthSucceed(MailBox mailbox)
        {
            SetDelayExpires(mailbox);
            _mailBoxManager.SetAuthError(mailbox, false);
        }

        public void OnAuthFailed(MailBox mailbox, string response_line)
        {
            SetDelayExpires(mailbox);
            _mailBoxManager.SetAuthError(mailbox, true);
        }

        public void OnDone(MailQueueItem item, bool quota_error)
        {
            _mailBoxManager.SetMailboxQuotaError(item.Account, quota_error);
        }

        public int[] OnGetOrCreateTags(string[] tags_names, int tenant, string user)
        {
            return _mailBoxManager.GetOrCreateTags(tenant, user, tags_names);
        }

        public void OnUpdateMessagesTags(int tenant, string user_id, int[] tag_ids, int[] message_ids)
        {
            foreach (var tag_id in tag_ids)
                _mailBoxManager.SetMessagesTag(tenant, user_id, tag_id, message_ids);
        }

        public void OnCheckedTimeUpdate(int id_mailbox, Int64 utc_ticks_time)
        {
            _mailBoxManager.LockMailbox(id_mailbox, utc_ticks_time);
        }
    }
}