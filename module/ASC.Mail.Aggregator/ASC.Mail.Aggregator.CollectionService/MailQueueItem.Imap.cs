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
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using ASC.Mail.Aggregator.Common;
using ActiveUp.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using ASC.Mail.Aggregator.Exceptions;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Extension;

namespace ASC.Mail.Aggregator.CollectionService
{
    public partial class MailQueueItem
    {
        private bool RetrieveImap(int max_messages_per_session, WaitHandle stop_event, out int proccessed_messages_count)
        {
            proccessed_messages_count = max_messages_per_session;
            var imap = MailClientBuilder.Imap();
            try
            {
                imap.Authenticated += OnAuthenticated;

                try
                {
                    imap.AuthenticateImap(Account, _log);
                }
                catch (TargetInvocationException ex_target)
                {
                    throw ex_target.InnerException;
                }

                UpdateTimeCheckedIfNeeded();

                // reverse folders and order them to download tagged incoming messages first
                // gmail returns tagged letters in mailboxes & duplicate them in inbox
                // to retrieve tags - first we need to download files from "sub" mailboxes
                var mailboxes =
                    imap.GetImapMailboxes(Account.Server, MailQueueItemSettings.SpecialDomainFolders,
                                          MailQueueItemSettings.SkipImapFlags, MailQueueItemSettings.ImapFlags)
                        .Reverse()
                        .OrderBy(m => m.folder_id);

                if(!mailboxes.Any())
                    _log.Error("There was no folder parsed! MailboxId={0}", Account.MailBoxId);

                foreach (var mailbox in mailboxes) {
                    _log.Info("Select imap folder: {0}", mailbox.name);

                    Mailbox mb_obj;

                    try
                    {
                        mb_obj = imap.SelectMailbox(mailbox.name);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("imap.SelectMailbox(\"{0}\") in MailboxId={1} failed with exception:\r\n{2}",
                                   mailbox.name, Account.MailBoxId, ex.ToString());
                        continue;
                    }

                    int last_folder_uid;
                    Account.ImapFolders.TryGetValue(mailbox.name, out last_folder_uid);

                    max_messages_per_session = ProcessMessages(mb_obj,
                        mailbox.folder_id,
                        last_folder_uid,
                        max_messages_per_session,
                        stop_event,
                        mailbox.tags);

                    if (0 == max_messages_per_session)
                        break;
                    UpdateTimeCheckedIfNeeded();
                }

                _log.Info("Account '{0}' has been processed.", Account.EMail);

                LastRetrieve = DateTime.UtcNow;

                proccessed_messages_count -= max_messages_per_session;
                return true;
            }
            catch (SocketException s_ex)
            {
                if (s_ex.SocketErrorCode == SocketError.HostNotFound)
                    if (OnAuthFailed != null) OnAuthFailed(Account, "Host unknown");

                _log.Warn("RetrieveImap Server: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                          Account.Server, Account.Port, Account.Account, s_ex.Message);

                throw;
            }
            catch (Imap4Exception e)
            {
                if (e.Message.StartsWith("Command \"LOGIN") || e.Message.StartsWith("PASS"))
                {
                    if (OnAuthFailed != null) OnAuthFailed(Account, "Password changed");

                    _log.Warn("RetrieveImap Server: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                              Account.Server, Account.Port, Account.Account, e.Message);
                }
                else
                {
                    _log.Error("RetrieveImap Server: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                               Account.Server, Account.Port, Account.Account, e.ToString());
                }

                throw;
            }
            catch (DotNetOpenAuth.Messaging.ProtocolException ex_oauth)
            {
                if (OnAuthFailed != null) OnAuthFailed(Account, "Access denied");

                _log.Warn("RetrieveImap Server: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                          Account.Server, Account.Port, Account.Account, ex_oauth.Message);

                throw;
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
                catch(Exception ex)
                {
                    _log.Warn("RetrieveImap() Imap4->Disconnect: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                        Account.Server, Account.Port, Account.Account, ex.Message);
                }
            }
        }
        
        // Returns: maxMessagesPerSession minus count of downloaded messages or zero if limit exceeded
        private int ProcessMessages( Mailbox mb,
            int folder_id,
            int last_uid,
            int max_messages_per_session,
            WaitHandle stop_event,
            string[] tags_names)
        {
            UpdateTimeCheckedIfNeeded();
            int[] uids_collection;

            try
            {
                uids_collection = mb.UidSearch("UID " + (0 != last_uid ? last_uid : 1) + ":*")
                    .Where(uid => uid != last_uid)
                    .ToArray();

                if (!uids_collection.Any()) throw new Exception("Empty folder");
            }
            catch (Exception)
            {
                _log.Info("New messages not found.");
                return max_messages_per_session;
            }

            var stored_uid_list = new Dictionary<int, string>();
            var stored_md5_list = new Dictionary<int, string>();

            InvokeGetStoredMessagesUIDL_MD5(stored_uid_list, stored_md5_list);

            var tags_hash = tags_names.Aggregate(string.Empty, (current, name) => current + name).GetMD5Hash();

            var new_messages = uids_collection
                .Select(
                    item_uid =>
                    new
                        {
                            uid = item_uid,
                            uidl = tags_names.Any()
                                ? string.Format("{0}-{1}-{2}-{3}", item_uid, folder_id, mb.UidValidity, tags_hash)
                                : string.Format("{0}-{1}-{2}", item_uid, folder_id, mb.UidValidity)
                        })
                .Where(msg => !stored_uid_list.Values.Contains(msg.uidl))
                .OrderByDescending(msg => msg.uid)
                .ToDictionary(msg => msg.uid, id => id.uidl);

            var quota_error_flag = false;
            var update_folder_uid_flag = new_messages.Any();

            int[] tags_ids = null;
            var message_ids_for_tag_update = new List<int>();
            var tags_retrieved = false;

            foreach (var new_message in new_messages)
            {
                var last_founded_message_id = -1;
                var has_parse_error = false;
                try
                {
                    if (stop_event.WaitOne(0))
                    {
                        _log.Debug("Stop event occure.");
                        break;
                    }

                    if (max_messages_per_session == 0)
                    {
                        _log.Info("Limit of max messages per session is exceeded!");
                        update_folder_uid_flag = false;
                        break;
                    }

                    // flags should be retrieved before message fetch - because mail server
                    // could add seen flag right after message was retrieved by us
                    var flags = mb.Fetch.UidFlags(new_message.Key);

                    if (!mb.SourceClient.IsConnected)
                    {
                        _log.Warn("Imap4-server is disconnected. Skip another messages.");
                        break;
                    }

                    _log.Debug("Processing new message\tUID: {0}\tUIDL: {1}\t",
                               new_message.Key, new_message.Value);

                    //Peek method didn't set \Seen flag on mail
                    var message = mb.Fetch.UidMessageObjectPeek(new_message.Key);

                    if (message.HasParseError)
                    {
                        _log.Error("ActiveUp: message parsed with some errors.");
                        has_parse_error = true;
                    }

                    UpdateTimeCheckedIfNeeded();

                    if (message.Date < Account.BeginDate)
                    {
                        _log.Debug("Skip message (Date = {0}) on BeginDate = {1}", message.Date, Account.BeginDate);
                        break;
                    }

                    var unique_identifier = string.Format("{0}|{1}|{2}|{3}",
                                                          message.From.Email,
                                                          message.Subject,
                                                          message.DateString,
                                                          message.MessageId);

                    var header_md5 = unique_identifier.GetMd5();

                    //Get tags ids for folder before message proccessing only once
                    if (!tags_retrieved)
                    {
                        tags_ids = tags_names.Any() ? InvokeOnGetOrCreateTags(tags_names) : null;
                        tags_retrieved = true;
                    }

                    if (folder_id == MailFolder.Ids.inbox || !message.To.Exists(email =>
                                                                email.Email.ToLowerInvariant()
                                                               .Equals(message.From.Email.ToLowerInvariant())
                                                            )
                       )
                    {
                        var found_message_id = stored_md5_list
                            .Where(el => el.Value == header_md5)
                            .Select(el => el.Key)
                            .FirstOrDefault();

                        if (found_message_id > 0)
                        {
                            InvokeOnUpdateUidl(found_message_id, new_message.Value);
                            last_founded_message_id = found_message_id;
                            message_ids_for_tag_update.Add(found_message_id); //Todo: Remove id if exception happened
                            continue; // Skip saving founded message
                        }
                    }

                    var unread = null == flags["seen"];
                    InvokeOnRetrieve(message, folder_id, new_message.Value, header_md5, has_parse_error, unread, tags_ids);
                }
                catch (Exception e)
                {
                    var common_error =
                        string.Format(
                            "ProcessMessages() Tenant={0} User='{1}' Account='{2}', MailboxId={3}, MessageIndex={4}, UIDL='{5}' Exception:\r\n{6}\r\n",
                            Account.TenantId, Account.UserId, Account.EMail.Address, Account.MailBoxId,
                            new_message.Key, new_message.Value, e.ToString());


                    if (e is IOException || e is MailBoxOutException)
                    {
                        if (e is IOException)
                            _log.Error(common_error);
                        else
                            _log.Info(common_error);
                        message_ids_for_tag_update.Remove(last_founded_message_id);
                        update_folder_uid_flag = false;
                        max_messages_per_session = 0; // stop checking other mailboxes
                        break;
                    }

                    if (e is TenantQuotaException)
                    {
                        _log.Info("Tenant {0} quota exception: {1}", Account.TenantId, e.Message);
                        message_ids_for_tag_update.Remove(last_founded_message_id);
                        quota_error_flag = true;
                        update_folder_uid_flag = false;
                    }
                    else
                    {
                        _log.Error(common_error);
                        update_folder_uid_flag = false;
                        message_ids_for_tag_update.Remove(last_founded_message_id);
                    }
                }

                UpdateTimeCheckedIfNeeded();
                max_messages_per_session--;
            }

            if (tags_ids != null && tags_ids.Length > 0 && message_ids_for_tag_update.Count > 0)
            {
                InvokeOnUpdateMessagesTags(Account.TenantId, Account.UserId, tags_ids, message_ids_for_tag_update.ToArray());
            }

            if (update_folder_uid_flag)
            {
                var max_uid = new_messages.Keys.Max();

                if(Account.ImapFolders.Keys.Contains(mb.Name))
                    Account.ImapFolders[mb.Name] = max_uid;
                else
                    Account.ImapFolders.Add(mb.Name, max_uid);

                Account.ImapFolderChanged = true;
            }

            InvokeOnDone(quota_error_flag);

            return max_messages_per_session;
        }

        public event UpdateUidlDelegate OnUpdateUidl;

        private void InvokeOnUpdateUidl(int message_id, string new_uidl)
        {
            if (OnUpdateUidl != null)
                OnUpdateUidl(Account, message_id, new_uidl);
        }

    }
}
