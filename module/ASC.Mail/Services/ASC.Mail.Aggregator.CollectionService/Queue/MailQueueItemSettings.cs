/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
                if (!string.IsNullOrEmpty(ConfigurationManagerExtension.AppSettings["mail.folders-mapping"]))
                    // "sent:2|"drafts:3|trash:4|spam:5|junk:5"
                {
                    list = ConfigurationManagerExtension.AppSettings["mail.folders-mapping"]
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
