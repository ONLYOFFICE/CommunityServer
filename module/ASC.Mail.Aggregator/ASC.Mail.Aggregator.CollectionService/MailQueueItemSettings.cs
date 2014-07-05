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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Aggregator.CollectionService
{
    public static class MailQueueItemSettings
    {
        static MailQueueItemSettings()
        {
            using (var db = new DbManager(MailBoxManager.ConnectionStringName))
            {
                var imap_flags = db.ExecuteList(new SqlQuery(Dal.DbSchema.ImapFlags.table)
                                                    .Select(Dal.DbSchema.ImapFlags.Columns.folder_id,
                                                            Dal.DbSchema.ImapFlags.Columns.name,
                                                            Dal.DbSchema.ImapFlags.Columns.skip))
                                   .ConvertAll(r => new
                                       {
                                           folder_id = Convert.ToInt32(r[0]),
                                           name = (string) r[1],
                                           skip = Convert.ToBoolean(r[2])
                                       });

                SkipImapFlags = imap_flags.FindAll(i => i.skip).ConvertAll(i => i.name).ToArray();

                ImapFlags = new Dictionary<string, int>();
                imap_flags.FindAll(i => !i.skip).ForEach(i => { ImapFlags[i.name] = i.folder_id; });

                SpecialDomainFolders = new Dictionary<string, Dictionary<string, MailboxInfo>>();
                db.ExecuteList(new SqlQuery(ImapSpecialMailbox.table)
                                   .Select(ImapSpecialMailbox.Columns.server,
                                           ImapSpecialMailbox.Columns.name,
                                           ImapSpecialMailbox.Columns.folder_id,
                                           ImapSpecialMailbox.Columns.skip))
                  .ForEach(r =>
                      {
                          var server = ((string) r[0]).ToLower();
                          var name = ((string)r[1]).ToLower();
                          var mb = new MailboxInfo
                              {
                                  folder_id = Convert.ToInt32(r[2]),
                                  skip = Convert.ToBoolean(r[3])
                              };
                          if (SpecialDomainFolders.Keys.Contains(server))
                              SpecialDomainFolders[server][name] = mb;
                          else
                              SpecialDomainFolders[server] = new Dictionary<string, MailboxInfo> { { name, mb } };
                      });

                PopUnorderedDomains = db.ExecuteList(new SqlQuery(PopUnorderedDomain.table)
                                                         .Select(PopUnorderedDomain.Columns.server))
                                        .ConvertAll(r => (string) r[0])
                                        .ToArray();
            }
        }

        public static Dictionary<string, int> ImapFlags { get; private set; }
        public static string[] SkipImapFlags { get; private set; }
        public static string[] PopUnorderedDomains { get; private set; }

        public struct MailboxInfo
        {
            public int folder_id;
            public bool skip;
        }
        public static Dictionary<string, Dictionary<string, MailboxInfo>> SpecialDomainFolders { get; private set; }
    }
}
