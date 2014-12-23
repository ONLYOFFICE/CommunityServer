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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ASC.Mail.Aggregator.Common.Authorization;
using ASC.Mail.Aggregator.Common.Logging;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator.Common.Extension
{
    public static class ImapExtensions
    {
        static readonly Regex SysfolderRegex = new Regex("\\*\\sLIST\\s\\(([^\\)]*)\\)\\s\"([^\"]*)\"\\s([\\s\\S]+)");
        static readonly Regex ImapFlagRegex = new Regex("\\\\([^\\\\\\s]+)");
        static readonly Regex FolderNameDecodeHelper = new Regex("(&[^-]+-)");
        static readonly Regex ListNewlineRegex = new Regex("\"\\s\\{([\\d]+)\\}\r\n");

        public static string GetImapMailboxes(this Imap4Client client)
        {
            return client.Command("LIST \"\" \"*\"");
        }

        // gets mailboxes, messages from wich we should get
        public static IEnumerable<ImapMailboxInfo> GetImapMailboxes(this Imap4Client client, string server, Dictionary<string, Dictionary<string, MailboxInfo>> special_domain_folders, string[] skip_imap_flags, Dictionary<string, int> imap_flags)
        {
            // get all mailboxes
            var response = client.GetImapMailboxes();

            var mailboxes = ParseImapMailboxes(response, server, special_domain_folders, skip_imap_flags, imap_flags);

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

        public static IEnumerable<ImapMailboxInfo> ParseImapMailboxes(string imap_response, string server = "",
                                                                      Dictionary
                                                                          <string, Dictionary<string, MailboxInfo>>
                                                                          special_domain_folders = null,
                                                                      string[] skip_imap_flags = null,
                                                                      Dictionary<string, int> imap_flags = null)
        {
            var mailboxes = new List<ImapMailboxInfo>();

            var domain_special_folders = new Dictionary<string, MailboxInfo>();
            if (special_domain_folders != null && special_domain_folders.Keys.Contains(server))
                domain_special_folders = special_domain_folders[server];


            var response = ListNewlineRegex.Replace(imap_response, m => "\" ");

            var t = Regex.Split(response, "\r\n");

            for (var i = 0; i < t.Length - 1; i++)
            {
                var m = SysfolderRegex.Match(t[i]);

                if (!m.Success)
                    continue;

                var name = m.Groups[3].Value;
                var separator = m.Groups[2].Value;
                var flags = m.Groups[1].Value.ToLower();

                if (!string.IsNullOrEmpty(name))
                {
                    if (name[0] == '\"' && name[name.Length - 1] == '\"')
                    {
                        name = name.Substring(1, name.Length - 2);
                    }

                    name = name.Replace("\\\"", "\"").Replace("\\\\", "\\");
                }

                var new_mailbox = new ImapMailboxInfo
                    {
                        folder_id = MailFolder.Ids.inbox,
                        name = name,
                        tags = new string[] {}
                    };

                if (new_mailbox.name.ToLower() != "inbox")
                {
                    var utf8_name = FolderNameDecodeHelper.Replace(new_mailbox.name, DecodeUtf7);
                    if (domain_special_folders.ContainsKey(utf8_name.ToLower()))
                    {
                        var info = domain_special_folders[utf8_name.ToLower()];
                        if (info.skip)
                            continue;

                        new_mailbox.folder_id = info.folder_id;
                    }
                    else
                    {
                        var look_for_parent = false;

                        var flags_match = ImapFlagRegex.Matches(flags);
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
                                    skip_imap_flags != null && skip_imap_flags.Contains(
                                        @group.ToString(CultureInfo.InvariantCulture).ToLowerInvariant())))
                                continue;

                            if (imap_flags != null)
                            {
                                var flag = imap_flags.FirstOrDefault(f => matches.Contains(f.Key));
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

                            new_mailbox.tags = new[] {tag};
                        }
                    }
                }
                mailboxes.Add(new_mailbox);
            }

            return mailboxes;
        }

        public static void AuthenticateImapGoogleOAuth2(this Imap4Client imap, MailBox account, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            var auth = new GoogleOAuth2Authorization(log);
            var granted_access = auth.RequestAccessToken(account.RefreshToken);
            if (granted_access == null)
                throw new DotNetOpenAuth.Messaging.ProtocolException("Access denied");
            log.Info("IMAP SSL connecting to {0}", account.EMail);
            imap.ConnectSsl(account.Server, account.Port);

            log.Info("IMAP connecting OK {0}", account.EMail);

            log.Info("IMAP logging to {0} via OAuth 2.0", account.EMail);
            imap.LoginOAuth2(account.Account, granted_access.AccessToken);
            log.Info("IMAP logged to {0} via OAuth 2.0", account.EMail);
        }

        public static void AuthenticateImap(this Imap4Client imap, MailBox account, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            if (account.RefreshToken != null)
            {
                var service_type = (AuthorizationServiceType)account.ServiceType;

                switch (service_type)
                {
                    case AuthorizationServiceType.Google:
                        imap.AuthenticateImapGoogleOAuth2(account, log);
                        return;
                }
            }

            imap.Authorize(new MailServerSettings
            {
                AccountName = account.Account,
                AccountPass = account.Password,
                AuthenticationType = account.AuthenticationTypeIn,
                EncryptionType = account.IncomingEncryptionType,
                Port = account.Port,
                Url = account.Server
            }, account.AuthorizeTimeoutInMilliseconds, log);

            log.Info("IMAP logged in to {0}", account.EMail);
        }


        public class ImapMailboxInfo
        {
            public int folder_id;
            public string name;
            public string[] tags;
        }

        public struct MailboxInfo
        {
            public int folder_id;
            public bool skip;
        }

        private static string DecodeUtf7(Match m)
        {
            var src = m.ToString().Replace('&', '+').Replace(',', '/');
            var bytes = Encoding.ASCII.GetBytes(src);
            return Encoding.UTF7.GetString(bytes);
        }
    }
}
