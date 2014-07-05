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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ASC.Mail.Aggregator.Common;
using ActiveUp.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using ASC.Mail.Aggregator.Exceptions;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Authorization;

namespace ASC.Mail.Aggregator.CollectionService
{
    public partial class MailQueueItem
    {
        private void AuthenticateImapPlain(Imap4Client imap)
        {
            switch (Account.IncomingEncryptionType)
            {
                case EncryptionType.StartTLS:
                    _log.Info("IMAP StartTLS connecting to {0}", Account.EMail);
                    imap.ConnectTLS(Account.Server, Account.Port);
                    break;
                case EncryptionType.SSL:
                    _log.Info("IMAP SSL connecting to {0}", Account.EMail);
                    imap.ConnectSsl(Account.Server, Account.Port);
                    break;
                case EncryptionType.None:
                    _log.Info("IMAP connecting to {0}", Account.EMail);
                    imap.Connect(Account.Server, Account.Port);
                    break;
            }

            _log.Info("IMAP connecting OK {0}", Account.EMail);

            _log.Info("IMAP logging in to {0}", Account.EMail);

            if (Account.AuthenticationTypeIn == SaslMechanism.Login)
            {
                imap.Login(Account.Account, Account.Password, "");
            }
            else
            {
                imap.Authenticate(Account.Account, Account.Password, Account.AuthenticationTypeIn);
            }

            _log.Info("IMAP logged in to {0}", Account.EMail);
        }

        private void AuthenticateImapGoogleOAuth2(Imap4Client imap)
        {
            var auth = new GoogleOAuth2Authorization(_log);
            var granted_access = auth.RequestAccessToken(Account.RefreshToken);
            if (granted_access == null) 
                throw new DotNetOpenAuth.Messaging.ProtocolException("Access denied");
            _log.Info("IMAP SSL connecting to {0}", Account.EMail);
            imap.ConnectSsl(Account.Server, Account.Port);

            _log.Info("IMAP connecting OK {0}", Account.EMail);

            _log.Info("IMAP logging to {0} via OAuth 2.0", Account.EMail);
            imap.LoginOAuth2(Account.Account, granted_access.AccessToken);
            _log.Info("IMAP logged to {0} via OAuth 2.0", Account.EMail);
        }

        private void AuthenticateImap(Imap4Client imap)
        {
            if (Account.RefreshToken != null)
            {
                var service_type = (AuthorizationServiceType)Account.ServiceType;

                switch (service_type)
                {
                    case AuthorizationServiceType.Google:
                        AuthenticateImapGoogleOAuth2(imap);
                        break;
                    default:
                        AuthenticateImapPlain(imap);
                        break;
                }
            }
            else
            {
                AuthenticateImapPlain(imap);
            }
        }

        private bool RetrieveImap(int max_messages_per_session, WaitHandle stop_event, out int proccessed_messages_count)
        {
            proccessed_messages_count = max_messages_per_session;
            var imap = MailClientBuilder.Imap();
            try
            {
                imap.Authenticated += OnAuthenticated;

                AuthenticateImap(imap);
                UpdateTimeCheckedIfNeeded();

                // reverse folders and order them to download tagged incoming messages first
                // gmail returns tagged letters in mailboxes & duplicate them in inbox
                // to retrieve tags - first we need to download files from "sub" mailboxes
                var mailboxes = GetImapMailboxes(imap, Account.Server).Reverse().OrderBy(m => m.folder_id);

                foreach (var mailbox in mailboxes) {
                    _log.Info("Select imap folder: {0}", mailbox.name);

                    var mb_obj = imap.SelectMailbox(mailbox.name);

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
                catch { }
            }
        }

        static readonly Regex SysfolderRegex = new Regex("\\*\\sLIST\\s\\(([^\\)]*)\\)\\s\"([^\"]*)\"\\s\"?([^\"]+)\"?");
        static readonly Regex ImapFlagRegex = new Regex("\\\\([^\\\\\\s]+)");
        static readonly Regex FolderNameDecodeHelper = new Regex("(&[^-]+-)");

        public struct ImapMailboxInfo
        {
            public int folder_id;
            public string name;
            public string[] tags;
        }

        private static string DecodeUtf7(Match m)
        {
            var src = m.ToString().Replace('&', '+').Replace(',', '/');
            var bytes = Encoding.ASCII.GetBytes(src);
            return Encoding.UTF7.GetString(bytes);
        }

        // gets mailboxes, messages from wich we should get
        public static IEnumerable<ImapMailboxInfo> GetImapMailboxes(Imap4Client client, string server)
        {
            var mailboxes = new List<ImapMailboxInfo>();

            var special_domain_folders = new Dictionary<string, MailQueueItemSettings.MailboxInfo>();
            if (MailQueueItemSettings.SpecialDomainFolders.Keys.Contains(server))
                special_domain_folders = MailQueueItemSettings.SpecialDomainFolders[server];

            // get all mailboxes
            var response = client.Command("LIST \"\" \"*\"");
            var t = Regex.Split(response, "\r\n");
            for (var i = 0; i < t.Length - 2; i++)
            {
                var m = SysfolderRegex.Match(t[i]);
                if (!m.Success)
                    continue;

                var new_mailbox = new ImapMailboxInfo
                    {
                        folder_id = MailFolder.Ids.inbox,
                        name = m.Groups[3].Value,
                        tags = new string[]{}
                    };
                var separator = m.Groups[2].Value;
                if (new_mailbox.name.ToLower() != "inbox")
                {
                    var utf8_name = FolderNameDecodeHelper.Replace(new_mailbox.name, DecodeUtf7);
                    if (special_domain_folders.ContainsKey(utf8_name.ToLower()))
                    {
                        var info = special_domain_folders[utf8_name.ToLower()];
                        if (info.skip)
                            continue;

                        new_mailbox.folder_id = info.folder_id;
                    }
                    else
                    {
                        var look_for_parent = false;

                        var flags_match = ImapFlagRegex.Matches(m.Groups[1].Value.ToLower());
                        if (flags_match.Count > 0)
                        {
                            var matches = new List<string>();
                            for (var j = 0; j < flags_match.Count; j++)
                            {
                                matches.Add(flags_match[j].Groups[1].Value);
                            }

                            if (
                                matches.Any(
                                    @group =>
                                    MailQueueItemSettings.SkipImapFlags.Contains(
                                        @group.ToString(CultureInfo.InvariantCulture).ToLowerInvariant())))
                                continue;

                            var flag = MailQueueItemSettings.ImapFlags.FirstOrDefault(f => matches.Contains(f.Key));
                            if (null != flag.Key)
                            {
                                new_mailbox.folder_id = flag.Value;
                                // special case for inbox - gmail l10n issue
                                if (MailFolder.Ids.inbox == flag.Value && new_mailbox.name.ToLower() != "inbox")
                                    new_mailbox.name = "inbox";
                            }
                            else
                            {
                                look_for_parent = true;
                            }
                        }
                        else
                        {
                            look_for_parent = true;
                        }

                        if (look_for_parent)
                        {
                            // if mailbox is potentialy child - add tag. Tags looks like Tag1/Tag2/Tag3
                            const string tag_for_store_separator = "/";
                            var tag = utf8_name.Replace(separator, tag_for_store_separator);

                            var parent_index = GetParentFolderIndex(mailboxes, new_mailbox, separator);

                            if (parent_index >= 0)
                            {
                                var parent = mailboxes[parent_index];
                                new_mailbox.folder_id = parent.folder_id;

                                // if system mailbox - removes first tag
                                // if not system mailbox child - removes same count of tags as in parent
                                if (!parent.tags.Any())
                                    tag = tag.Substring(tag.IndexOf(tag_for_store_separator, StringComparison.Ordinal));
                            }

                            new_mailbox.tags = new[] { tag };
                        }
                    }
                }
                mailboxes.Add(new_mailbox);
            }

            return mailboxes;
        }

        private static int GetParentFolderIndex(List<ImapMailboxInfo> mailboxes, ImapMailboxInfo new_mailbox, string separator)
        {
            var potential_parent_indexes = new List<int>();
            for (int ind = 0; ind < mailboxes.Count; ++ind)
            {
                if (new_mailbox.name.StartsWith(mailboxes[ind].name + separator))
                {
                    potential_parent_indexes.Add(ind);
                }
            }

            var parent_index = -1;
            if (potential_parent_indexes.Count > 0)
            {
                parent_index = potential_parent_indexes.OrderByDescending(index => mailboxes[index].name.Length).First();
            }
            return parent_index;
        }

        // Returns: maxMessagesPerSession minus count of downloaded messages or zero if limit excided
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
                        _log.Debug("Limit of max messages per session is exceeded!");
                        update_folder_uid_flag = false;
                        break;
                    }

                    // flags should be retrieved before message fetch - because mail server
                    // could add seen flag right after message was retrieved by us
                    var flags = mb.Fetch.UidFlags(new_message.Key);

                    Message message = null;

                    try
                    {
                        //Peek method didn't set \Seen flag on mail
                        message = mb.Fetch.UidMessageObjectPeek(new_message.Key);
                    }
                    catch (Exception ex)
                    {
                        if (ex is ParsingException || ex is IndexOutOfRangeException)
                        {
                            _log.Error("ActiveUp Parse error: trying to save message with 'has_parse_error' flag. Exception:\r\n {0}", ex.ToString());
                            message = GetImapMessageAfterParseError(mb, new_message.Key);
                            has_parse_error = true;
                        }
                        else
                            throw;
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
                catch (IOException io_ex)
                {
                    if (io_ex.Message.StartsWith("Unable to write data to the transport connection") || 
                        io_ex.Message.StartsWith("Unable to read data from the transport connection"))
                    {
                        _log.Error("ProcessMessages() Account='{0}': {1}",
                             Account.EMail.Address, io_ex.ToString());
                        message_ids_for_tag_update.Remove(last_founded_message_id);
                        update_folder_uid_flag = false;
                        max_messages_per_session = 0; // stop checking other mailboxes

                        break;
                    }
                }
                catch (MailBoxOutException ex)
                {
                    _log.Info("ProcessMessages() Account='{0}': {1}",
                              Account.EMail.Address, ex.Message);
                    message_ids_for_tag_update.Remove(last_founded_message_id);
                    update_folder_uid_flag = false;
                    max_messages_per_session = 0; // stop checking other mailboxes

                    break;
                }
                catch (TenantQuotaException qex)
                {
                    _log.Info("Tenant {0} quota exception: {1}", Account.TenantId, qex.Message);
                    message_ids_for_tag_update.Remove(last_founded_message_id);
                    quota_error_flag = true;
                    update_folder_uid_flag = false;
                }
                catch (Exception e)
                {
                    _log.Error("ProcessMessages() Account='{0}', MessageId={1} Exception:\r\n{2}\r\n",
                               Account.EMail.Address, new_message, e.ToString());
                    update_folder_uid_flag = false;
                    message_ids_for_tag_update.Remove(last_founded_message_id);
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

        private Message GetImapMessageAfterParseError(Mailbox mb, int uid)
        {
            var header = GetImapHeader(mb, uid);

            var message = new Message(header);

            var eml_message_string = mb.Fetch.UidMessageString(uid);

            message.AddAttachmentFromString("original_message.eml", eml_message_string);

            return message;
        }

        private static readonly Regex RegxHeader = new Regex(@"[\s\S]+?((?=\r?\n\r?\n)|\Z)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RegxUnfold = new Regex(@"\r?\n(?=[ \t])", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RegxHeaderLines = new Regex(@"(?<=((\r?\n)|\n)|\A)\S+:(.|(\r?\n[\t ]))+(?=((\r?\n)\S)|\Z)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private Header GetImapHeader(Mailbox mb, int uid)
        {
            var header_bytes = mb.Fetch.UidHeader(uid);

            var header = new Header {OriginalData = header_bytes};

            var hdr = Encoding.GetEncoding("iso-8859-1")
                              .GetString(header.OriginalData, 0, header.OriginalData.Length);

            hdr = RegxHeader.Match(hdr).Value;
            hdr = RegxUnfold.Replace(hdr, "");

            var m = RegxHeaderLines.Match(hdr);
            while (m.Success)
            {
                try
                {
                    var name = FormatFieldName(m.Value.Substring(0, m.Value.IndexOf(':')));
                    var value =
                        Codec.RFC2047Decode(m.Value.Substring(m.Value.IndexOf(":", StringComparison.Ordinal) + 1))
                             .Trim('\r', '\n')
                             .TrimStart(' ');
                    if (name.Equals("received")) header.Trace.Add(Parser.ParseTrace(m.Value.Trim(' ')));
                    else if (name.Equals("to")) header.To = Parser.ParseAddresses(value);
                    else if (name.Equals("cc")) header.Cc = Parser.ParseAddresses(value);
                    else if (name.Equals("bcc")) header.Bcc = Parser.ParseAddresses(value);
                    else if (name.Equals("reply-to")) header.ReplyTo = Parser.ParseAddress(value);
                    else if (name.Equals("from")) header.From = Parser.ParseAddress(value);
                    else if (name.Equals("sender")) header.Sender = Parser.ParseAddress(value);
                    else if (name.Equals("content-type")) header.ContentType = Parser.GetContentType(m.Value);
                    else if (name.Equals("content-disposition"))
                        header.ContentDisposition = Parser.GetContentDisposition(m.Value);

                    header.HeaderFields.Add(name, value);
                    header.HeaderFieldNames.Add(name, m.Value.Substring(0, m.Value.IndexOf(':')));
                }
                catch (Exception parser_header_line_ex)
                {
                    _log.Debug("Parser header field error: {0} ", parser_header_line_ex.ToString());
                }

                m = m.NextMatch();
            }

            return header;
        }

        static string FormatFieldName(string field_name)
        {
            return field_name.ToLower().Trim();
        }
    }
}
