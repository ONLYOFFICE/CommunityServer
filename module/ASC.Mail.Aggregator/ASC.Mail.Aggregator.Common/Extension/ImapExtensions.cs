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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ActiveUp.Net.Mail;
using ASC.Mail.Aggregator.Common.Authorization;
using ASC.Mail.Aggregator.Common.Logging;
using DotNetOpenAuth.Messaging;

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
        public static IEnumerable<ImapMailboxInfo> GetImapMailboxes(this Imap4Client client, string server, 
                                                Dictionary<string, int> defaultFolsers,
                                                Dictionary<string, Dictionary<string, MailboxInfo>> specialDomainFolders, 
                                                string[] skipImapFlags, Dictionary<string, int> imapFlags)
        {
            // get all mailboxes
            var response = client.GetImapMailboxes();

            var mailboxes = ParseImapMailboxes(response, server, defaultFolsers, specialDomainFolders, skipImapFlags, imapFlags);

            return mailboxes;
        }

        private static int GetParentFolderIndex(List<ImapMailboxInfo> mailboxes, ImapMailboxInfo newMailbox, string separator)
        {
            var potentialParentIndexes = new List<int>();
            for (var ind = 0; ind < mailboxes.Count; ++ind)
            {
                if (newMailbox.name.StartsWith(mailboxes[ind].name + separator))
                {
                    potentialParentIndexes.Add(ind);
                }
            }

            var parentIndex = -1;
            if (potentialParentIndexes.Count > 0)
            {
                parentIndex = potentialParentIndexes.OrderByDescending(index => mailboxes[index].name.Length).First();
            }
            return parentIndex;
        }

        public static IEnumerable<ImapMailboxInfo> ParseImapMailboxes(string imapResponse, string server = "",
                                                                Dictionary<string, int> defaultFolders = null,
                                                                Dictionary <string, Dictionary<string, MailboxInfo>> specialDomainFolders = null,
                                                                string[] skipImapFlags = null,
                                                                Dictionary<string, int> imapFlags = null)
        {
            var mailboxes = new List<ImapMailboxInfo>();

            var domainSpecialFolders = new Dictionary<string, MailboxInfo>();
            if (specialDomainFolders != null && specialDomainFolders.Keys.Contains(server))
                domainSpecialFolders = specialDomainFolders[server];


            var response = ListNewlineRegex.Replace(imapResponse, m => "\" ");

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

                var newMailbox = new ImapMailboxInfo
                    {
                        folder_id = MailFolder.Ids.inbox,
                        name = name,
                        tags = new string[] {}
                    };

                if (newMailbox.name.ToLower() != "inbox")
                {
                    var utf8Name = FolderNameDecodeHelper.Replace(newMailbox.name, DecodeUtf7);
                    var defaultMapping = false;

                    if (defaultFolders != null && defaultFolders.ContainsKey(utf8Name.ToLower()))
                    {
                        newMailbox.folder_id = defaultFolders[utf8Name.ToLower()];
                        defaultMapping = true;
                    }

                    if (domainSpecialFolders.ContainsKey(utf8Name.ToLower()))
                    {
                        var info = domainSpecialFolders[utf8Name.ToLower()];
                        if (info.skip)
                            continue;

                        newMailbox.folder_id = info.folder_id;
                    }
                    else
                    {
                        var lookForParent = false;

                        var flagsMatch = ImapFlagRegex.Matches(flags);
                        if (flagsMatch.Count > 0)
                        {
                            var matches = new List<string>();
                            for (var j = 0; j < flagsMatch.Count; j++)
                            {
                                matches.Add(flagsMatch[j].Groups[1].Value);
                            }

                            if (
                                matches.Any(
                                    @group =>
                                    skipImapFlags != null && skipImapFlags.Contains(
                                        @group.ToString(CultureInfo.InvariantCulture).ToLowerInvariant())))
                                continue;

                            if (imapFlags != null)
                            {
                                var flag = imapFlags.FirstOrDefault(f => matches.Contains(f.Key));
                                if (null != flag.Key)
                                {
                                    newMailbox.folder_id = flag.Value;
                                    // special case for inbox - gmail l10n issue
                                    if (MailFolder.Ids.inbox == flag.Value && newMailbox.name.ToLower() != "inbox")
                                        newMailbox.name = "inbox";
                                }
                                else
                                {
                                    lookForParent = true;
                                }
                            }
                        }
                        else
                        {
                            lookForParent = true;
                        }

                        if (lookForParent && !defaultMapping)
                        {
                            // if mailbox is potentialy child - add tag. Tags looks like Tag1/Tag2/Tag3
                            const string tag_for_store_separator = "/";
                            var tag = utf8Name.Replace(separator, tag_for_store_separator);

                            var parentIndex = GetParentFolderIndex(mailboxes, newMailbox, separator);

                            if (parentIndex >= 0)
                            {
                                var parent = mailboxes[parentIndex];
                                newMailbox.folder_id = parent.folder_id;

                                // if system mailbox - removes first tag
                                // if not system mailbox child - removes same count of tags as in parent
                                if (!parent.tags.Any())
                                    tag = tag.Substring(tag.IndexOf(tag_for_store_separator, StringComparison.Ordinal));
                            }

                            newMailbox.tags = new[] {tag};
                        }
                    }
                }
                mailboxes.Add(newMailbox);
            }

            return mailboxes;
        }

        public static void AuthenticateImapGoogleOAuth2(this Imap4Client imap, MailBox account, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            var auth = new GoogleOAuth2Authorization(log);
            var grantedAccess = auth.RequestAccessToken(account.RefreshToken);
            if (grantedAccess == null)
                throw new ProtocolException("Access denied");
            log.Info("IMAP SSL connecting to {0}", account.EMail);
            imap.ConnectSsl(account.Server, account.Port);

            log.Info("IMAP connecting OK {0}", account.EMail);

            log.Info("IMAP logging to {0} via OAuth 2.0", account.EMail);
            imap.LoginOAuth2(account.Account, grantedAccess.AccessToken);
            log.Info("IMAP logged to {0} via OAuth 2.0", account.EMail);
        }

        public static void AuthenticateImap(this Imap4Client imap, MailBox account, ILogger log = null)
        {
            if (log == null)
                log = new NullLogger();

            if (account.RefreshToken != null)
            {
                var serviceType = (AuthorizationServiceType)account.ServiceType;

                switch (serviceType)
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
