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

#region Usings

using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Globalization;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ActiveUp.Net.Mail;
using System.IO;
using ASC.Mail.Aggregator.Exceptions;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Common.Extension;

#endregion

namespace ASC.Mail.Aggregator.CollectionService
{
    public delegate void GetStoredMessagesUidlMd5Delegate(MailQueueItem account, Dictionary<int, string> uidl_list, Dictionary<int, string> md5_list);

    public delegate DateTime GetFolderModifyDateDelegate(MailQueueItem account, int folder_id);

    public delegate void RetrieveNewMessageDelegate(MailQueueItem account, Message message, int folder_id, string uidl, string md5_hash, bool has_parse_error, bool unread, int[] tags_ids);

    public delegate void DoneDelegate(MailQueueItem item, bool quota_error);

    public delegate void AuthSucceedDelegate(MailBox account);

    public delegate void AuthFailedDelegate(MailBox account, string response_line);

    public delegate void UpdateUidlDelegate(MailBox account, int message_id, string new_uidl);

    public delegate int[] GetTagsIdsDelegate(string[] tags, int tenant, string user);

    public delegate void SetTagsForMessages(int tenant, string user, int[] tag_ids, int[] message_ids);

    public delegate void UpdateAccountTimeCheckedDelegate(int id_mailbox, Int64 utc_ticks_time);

    public partial class MailQueueItem : IDisposable
    {
        private readonly ILogger _log;
        private const int CHECKED_TIME_INTERVAL = 3 * 60 * 1000; // min * sec_in_mins * miliseconds_in_sec
        private DateTime _lastTimeItemChecked;

        public MailQueueItem(MailBox account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Account = account;
            Priority = 0;
            _lastTimeItemChecked = DateTime.UtcNow;
            _log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "MailQueueItem");
        }

        private bool IsUidlSupported { get; set; }

        public MailBox Account { get; private set; }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private DateTime LastRetrieve { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private int Priority { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        public event GetStoredMessagesUidlMd5Delegate GetStoredMessagesUidlMd5;

        private void InvokeGetStoredMessagesUIDL_MD5(Dictionary<int, string> uidl_list, Dictionary<int, string> md5_list)
        {
            var handler = GetStoredMessagesUidlMd5;
            if (handler != null)
            {
                handler(this, uidl_list, md5_list);
            }
        }

        public event RetrieveNewMessageDelegate OnRetrieveNewMessage;

        private void InvokeOnRetrieve(Message message, int folder_id, string uidl, string md5_hash, bool has_parse_error, bool unread = true, int[] tags_ids = null)
        {
            if (OnRetrieveNewMessage != null)
                OnRetrieveNewMessage(this, message, folder_id, uidl, md5_hash, has_parse_error, unread, tags_ids);
        }

        public event UpdateAccountTimeCheckedDelegate OnTimeCheckedUpdate;

        private void UpdateTimeCheckedIfNeeded()
        {
            if ((DateTime.UtcNow - _lastTimeItemChecked).Milliseconds > CHECKED_TIME_INTERVAL)
            {
                if (OnTimeCheckedUpdate != null)
                {
                    _lastTimeItemChecked = DateTime.UtcNow;
                    OnTimeCheckedUpdate(Account.MailBoxId, _lastTimeItemChecked.Ticks);
                    _log.Debug(String.Format("Checked time was updated for mailbox: {0}", Account.MailBoxId));
                }
            }
        }

        public event DoneDelegate OnDone;

        private void InvokeOnDone(bool quota_error)
        {
            if (OnDone != null)
                OnDone(this, quota_error);
        }

        public event AuthSucceedDelegate OnAuthSucceed;
        public event AuthFailedDelegate OnAuthFailed;

        public event GetTagsIdsDelegate OnGetOrCreateTags;

        private int[] InvokeOnGetOrCreateTags(string[] tags_names)
        {
            return null != OnGetOrCreateTags && null != tags_names && tags_names.Any() ? OnGetOrCreateTags(tags_names, Account.TenantId, Account.UserId) : null;
        }

        public event SetTagsForMessages OnUpdateMessagesTags;
        
        private void InvokeOnUpdateMessagesTags(int tenant, string user, int[] tag_ids, int[] message_ids)
        {
            if (OnUpdateMessagesTags != null)
                OnUpdateMessagesTags(tenant, user, tag_ids, message_ids);
        }

        // ReSharper disable UnusedMethodReturnValue.Global
        public bool Retrieve(int max_messages_per_session, WaitHandle stop_event)
        // ReSharper restore UnusedMethodReturnValue.Global
        {
            long log_record_id = AggregatorLogger.Instance.MailBoxProccessingStarts(Account.MailBoxId, Thread.CurrentThread.ManagedThreadId);
            bool result = false;
            int processed_messages_count;
            try
            {
                result = Account.Imap ? RetrieveImap(max_messages_per_session, stop_event, out processed_messages_count) :
                                         RetrievePop(max_messages_per_session, stop_event, out processed_messages_count);
            }
            catch (Exception)
            {
                //Its records proccessing_end_time without proccessing_messages_count. That action needed for correct metrics work.
                AggregatorLogger.Instance.MailBoxProccessingEnds(log_record_id, null);
                throw;
            }

            AggregatorLogger.Instance.MailBoxProccessingEnds(log_record_id, processed_messages_count);

            return result;
        }


        private bool RetrievePop(int max_messages_per_session, WaitHandle stop_event, out int processed_messages_count)
        {
            var pop = MailClientBuilder.Pop();
            try
            {
                pop.Authenticated += OnAuthenticated;

                _log.Debug("Connecting to {0}", Account.EMail);

                try
                {
                    pop.Authorize(new MailServerSettings
                        {
                            AccountName = Account.Account,
                            AccountPass = Account.Password,
                            AuthenticationType = Account.AuthenticationTypeIn,
                            EncryptionType = Account.IncomingEncryptionType,
                            Port = Account.Port,
                            Url = Account.Server
                        }, Account.AuthorizeTimeoutInMilliseconds, _log);
                }
                catch (TargetInvocationException ex_target)
                {
                    throw ex_target.InnerException;
                }

                UpdateTimeCheckedIfNeeded();

                _log.Debug("UpdateStats()");

                pop.UpdateStats();

                _log.Debug("GetCAPA()");
                
                GetCAPA(pop);

                _log.Info("Account: MessagesCount={0}, TotalSize={1}, UIDL={2}, LoginDelay={3}",
                    pop.MessageCount, pop.TotalSize, IsUidlSupported, Account.ServerLoginDelay);

                if (ProcessMessagesPop(pop, max_messages_per_session, stop_event, out processed_messages_count))
                { // If all messages are proccessed
                    Account.MessagesCount = pop.MessageCount;
                    Account.Size = pop.TotalSize;
                    _log.Info("Account '{0}' has been processed.", Account.EMail);
                }

                LastRetrieve = DateTime.UtcNow;

                return true;
            }
            catch (SocketException s_ex)
            {
                if (s_ex.SocketErrorCode == SocketError.HostNotFound)
                    if (OnAuthFailed != null) OnAuthFailed(Account, "Host unknown");

                _log.Warn("Retrieve() Pop3: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                        Account.Server, Account.Port, Account.Account, s_ex.Message);

                throw;
            }
            catch (Pop3Exception e)
            {
                if (e.Command.StartsWith("USER") || e.Command.StartsWith("PASS"))
                {
                    if (OnAuthFailed != null) OnAuthFailed(Account, e.Response);

                    _log.Warn("Retrieve() Pop3: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                        Account.Server, Account.Port, Account.Account, e.Message);
                }
                else
                {
                    _log.Error("Retrieve() Pop3: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                        Account.Server, Account.Port, Account.Account, e.ToString());
                }

                throw;
            }
            finally
            {
                try
                {
                    if (pop.IsConnected)
                    {
                        pop.Disconnect();
                    }
                }
                catch(Exception ex)
                {
                    _log.Warn("Retrieve() Pop3->Disconnect: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                        Account.Server, Account.Port, Account.Account, ex.Message);
                }
            }
        }

        void OnAuthenticated(object sender, AuthenticatedEventArgs e)
        {
            if (OnAuthSucceed != null) OnAuthSucceed(Account);
        }

        private void GetCAPA(Pop3Client client)
        {
            try
            {
                var capa_params = client.GetServerCapabilities();

                if (capa_params.Length <= 0) return;
                var index = Array.IndexOf(capa_params, "LOGIN-DELAY");
                if (index > -1)
                {
                    int delay;
                    if (int.TryParse(capa_params[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out delay))
                        Account.ServerLoginDelay = delay;
                }

                IsUidlSupported = Array.IndexOf(capa_params, "UIDL") > -1;
            }
            catch
            { // CAPA NOT SUPPORTED 
                try
                { // CHECK UIDL SUPPORT
                    // ReSharper disable UnusedVariable
                    var uidls = client.GetUniqueIds();
                    // ReSharper restore UnusedVariable
                    IsUidlSupported = true;
                }
                catch { // UIDL NOT SUPPORTED 
                    IsUidlSupported = false;
                }
            }
        }

        // Returns: True if all messages are proccessed. False if at least one new message is not processed.
        private bool ProcessMessagesPop(Pop3Client client, int max_messages_per_session, WaitHandle stop_event, out int processed_messages_count)
        {
            UpdateTimeCheckedIfNeeded();
            processed_messages_count = max_messages_per_session;
            var bad_messages_exist = false;
            Dictionary<int, string> new_messages;
            var stored_uidl_list = new Dictionary<int, string>();
            var stored_md5_list = new Dictionary<int, string>();

            InvokeGetStoredMessagesUIDL_MD5(stored_uidl_list, stored_md5_list);

            if (!IsUidlSupported)
            {
                _log.Info("UIDL is not supported! Account '{0}' has been skiped.", Account.EMail);
                return true;
            }

            var email_ids = client.GetUniqueIds();

            new_messages =
                email_ids
                .Where(id => !stored_uidl_list.Values.Contains(id.UniqueId))
                .OrderBy(id => id.Index)
                .ToDictionary(id => id.Index, id => id.UniqueId );


            var quota_error_flag = false;

            if (client.IsConnected)
            {
                if (new_messages.Count == 0)
                    _log.Debug("New messages not found.\r\n");
                else
                {
                    _log.Debug("Found {0} new messages.\r\n", new_messages.Count);

                    if (new_messages.Count > 1)
                    {
                        _log.Debug("Calculating order");

                        try
                        {
                            var first_header = client.RetrieveHeaderObject(new_messages.First().Key);
                            var last_header = client.RetrieveHeaderObject(new_messages.Last().Key);

                            if (first_header.Date < last_header.Date)
                            {
                                _log.Debug("Account '{0}' order is DESC", Account.EMail.Address);
                                new_messages = new_messages
                                    .OrderByDescending(item => item.Key) // This is to ensure that the newest message would be handled primarily.
                                    .ToDictionary(id => id.Key, id => id.Value);
                            }
                            else
                                _log.Debug("Account '{0}' order is ASC", Account.EMail.Address);
                        }
                        catch (Exception)
                        {
                            _log.Warn("Calculating order skipped! Account '{0}' order is ASC", Account.EMail.Address);
                        }
                    }

                    var skip_on_date = Account.BeginDate != MailBoxManager.MIN_BEGIN_DATE;

                    var skip_break_on_date = MailQueueItemSettings.PopUnorderedDomains.Contains(Account.Server.ToLowerInvariant());

                    foreach (var new_message in new_messages)
                    {
                        var has_parse_error = false;

                        try
                        {
                            if (stop_event.WaitOne(0))
                            {
                                break;
                            }

                            if (max_messages_per_session == 0)
                            {
                                _log.Debug("Limit of max messages per session is exceeded!");
                                break;
                            }

                            if (!client.IsConnected)
                            {
                                _log.Warn("POP3-server is disconnected. Skip another messages.");
                                bad_messages_exist = true;
                                break;
                            }

                            _log.Debug("Processing new message\tUID: {0}\t{1}\t",
                                       new_message.Key,
                                       (IsUidlSupported ? "UIDL: " : "MD5: ") + new_message.Value);

                            var message = client.RetrieveMessageObject(new_message.Key);

                            if (message.HasParseError)
                            {
                                _log.Error("ActiveUp: message parsed with some errors.");
                                has_parse_error = true;
                            }


                            UpdateTimeCheckedIfNeeded();

                            if (message.Date < Account.BeginDate && skip_on_date)
                            {
                                if (!skip_break_on_date)
                                {
                                    _log.Info("Skip other messages older then {0}.", Account.BeginDate);
                                    break;
                                }
                                _log.Debug("Skip message (Date = {0}) on BeginDate = {1}", message.Date,
                                           Account.BeginDate);
                                continue;
                            }

                            var header_md5 = string.Empty;

                            if (IsUidlSupported)
                            {
                                var unique_identifier = string.Format("{0}|{1}|{2}|{3}",
                                                                      message.From.Email,
                                                                      message.Subject,
                                                                      message.DateString,
                                                                      message.MessageId);

                                header_md5 = unique_identifier.GetMd5();

                                if (!message.To.Exists(email =>
                                                       email.Email
                                                            .ToLowerInvariant()
                                                            .Equals(message.From.Email
                                                                           .ToLowerInvariant())))
                                {

                                    var found_message_id = stored_md5_list
                                        .Where(el => el.Value == header_md5)
                                        .Select(el => el.Key)
                                        .FirstOrDefault();

                                    if (found_message_id > 0)
                                    {
                                        InvokeOnUpdateUidl(found_message_id, new_message.Value);
                                        continue; // Skip saving founded message
                                    }
                                }
                            }

                            InvokeOnRetrieve(message,
                                             MailFolder.Ids.inbox,
                                             IsUidlSupported ? new_message.Value : "",
                                             IsUidlSupported ? header_md5 : new_message.Value, has_parse_error);
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
                                max_messages_per_session = 0; //It needed for stop messsages proccessing.
                                bad_messages_exist = true;
                                if (e is IOException)
                                    _log.Error(common_error);
                                else
                                    _log.Info(common_error);
                                break;
                            }

                            if (e is TenantQuotaException)
                            {
                                _log.Info("Tenant {0} quota exception: {1}", Account.TenantId, e.Message);
                                quota_error_flag = true;
                            }
                            else
                            {
                                bad_messages_exist = true;
                                _log.Error(common_error);
                            }
                        }

                        UpdateTimeCheckedIfNeeded();
                        max_messages_per_session--;
                    }
                }
            }
            else
            {
                _log.Debug("POP3 server is disconnected.");
                bad_messages_exist = true;
            }

            InvokeOnDone(quota_error_flag);

            processed_messages_count -= max_messages_per_session;
            return !bad_messages_exist && max_messages_per_session > 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == typeof(MailQueueItem) && Equals((MailQueueItem)obj);
        }

        private bool Equals(MailQueueItem other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || Equals(other.Account, Account);
        }

        public override int GetHashCode()
        {
            return (Account != null ? Account.GetHashCode() : 0);
        }

        public void Dispose()
        {

        }
    }
}