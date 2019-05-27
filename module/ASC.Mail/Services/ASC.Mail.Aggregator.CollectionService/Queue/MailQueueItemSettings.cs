/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Configuration;
using System.Linq;
using ASC.Mail.Core;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Aggregator.CollectionService.Queue
{
    public class MailQueueItemSettings
    {
        public Dictionary<string, int> ImapFlags { get; private set; }

        public string[] SkipImapFlags { get; private set; }

        public Dictionary<string, Dictionary<string, MailBoxData.MailboxInfo>> SpecialDomainFolders { get; private set; }

        public Dictionary<string, int> DefaultFolders { get; private set; }

        public MailQueueItemSettings()
        {
            using (var daoFactory = new DaoFactory())
            {
                var imapFlags = daoFactory.CreateImapFlagsDao().GetImapFlags();

                SkipImapFlags = imapFlags.FindAll(i => i.Skip).ConvertAll(i => i.Name).ToArray();

                ImapFlags = new Dictionary<string, int>();
                imapFlags.FindAll(i => !i.Skip).ForEach(i => { ImapFlags[i.Name] = i.FolderId; });

                var imapSpecialMailboxes = daoFactory.CreateImapSpecialMailboxDao().GetImapSpecialMailboxes();

                SpecialDomainFolders = new Dictionary<string, Dictionary<string, MailBoxData.MailboxInfo>>();
                imapSpecialMailboxes.ForEach(r =>
                {
                    var mb = new MailBoxData.MailboxInfo
                    {
                        folder_id = r.FolderId,
                        skip = r.Skip
                    };
                    if (SpecialDomainFolders.Keys.Contains(r.Server))
                        SpecialDomainFolders[r.Server][r.MailboxName] = mb;
                    else
                        SpecialDomainFolders[r.Server] = new Dictionary<string, MailBoxData.MailboxInfo>
                        {
                            {r.MailboxName, mb}
                        };
                });
            }

            DefaultFolders = GetDefaultFolders();
        }

        private static Dictionary<string, int> GetDefaultFolders()
        {
            var list = new Dictionary<string, int>
            {
                {"inbox", 1},
                {"sent", 2},
                {"sent items", 2},
                {"drafts", 3},
                {"trash", 4},
                {"spam", 5},
                {"junk", 5},
                {"bulk", 5}
            };

            try
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mail.folders-mapping"]))
                    // "sent:2|"drafts:3|trash:4|spam:5|junk:5"
                {
                    list = ConfigurationManager.AppSettings["mail.folders-mapping"]
                        .Split('|')
                        .Select(s => s.Split(':'))
                        .ToDictionary(s => s[0].ToLower(), s => Convert.ToInt32(s[1]));
                }
            }
            catch
            {
                //ignore
            }
            return list;
        }
    }
}
