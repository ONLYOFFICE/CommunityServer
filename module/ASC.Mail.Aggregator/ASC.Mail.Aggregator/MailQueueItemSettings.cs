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
using System.Configuration;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Aggregator
{
    public static class MailQueueItemSettings
    {
        static MailQueueItemSettings()
        {
            using (var db = new DbManager(MailBoxManager.ConnectionStringName))
            {
                var imapFlags = db.ExecuteList(new SqlQuery(Dal.DbSchema.ImapFlags.name)
                                                    .Select(Dal.DbSchema.ImapFlags.Columns.folder_id,
                                                            Dal.DbSchema.ImapFlags.Columns.name,
                                                            Dal.DbSchema.ImapFlags.Columns.skip))
                                   .ConvertAll(r => new
                                       {
                                           folder_id = Convert.ToInt32(r[0]),
                                           name = (string) r[1],
                                           skip = Convert.ToBoolean(r[2])
                                       });

                SkipImapFlags = imapFlags.FindAll(i => i.skip).ConvertAll(i => i.name).ToArray();

                ImapFlags = new Dictionary<string, int>();
                imapFlags.FindAll(i => !i.skip).ForEach(i => { ImapFlags[i.name] = i.folder_id; });

                SpecialDomainFolders = new Dictionary<string, Dictionary<string, ImapExtensions.MailboxInfo>>();
                db.ExecuteList(new SqlQuery(ImapSpecialMailbox.name)
                                   .Select(ImapSpecialMailbox.Columns.server,
                                           ImapSpecialMailbox.Columns.name,
                                           ImapSpecialMailbox.Columns.folder_id,
                                           ImapSpecialMailbox.Columns.skip))
                  .ForEach(r =>
                      {
                          var server = ((string) r[0]).ToLower();
                          var name = ((string)r[1]).ToLower();
                          var mb = new ImapExtensions.MailboxInfo
                              {
                                  folder_id = Convert.ToInt32(r[2]),
                                  skip = Convert.ToBoolean(r[3])
                              };
                          if (SpecialDomainFolders.Keys.Contains(server))
                              SpecialDomainFolders[server][name] = mb;
                          else
                              SpecialDomainFolders[server] = new Dictionary<string, ImapExtensions.MailboxInfo> { { name, mb } };
                      });

                PopUnorderedDomains = db.ExecuteList(new SqlQuery(PopUnorderedDomain.name)
                                                         .Select(PopUnorderedDomain.Columns.server))
                                        .ConvertAll(r => (string) r[0])
                                        .ToArray();
            }

            DefaultFolders = GetDefaultFolders();
        }

        private static Dictionary<string, int> GetDefaultFolders()
        {
            var list = new Dictionary<string, int>
                {
                    {"sent", 2},
                    {"drafts", 3},
                    {"trash", 4},
                    {"spam", 5},
                    {"junk", 5}
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

        public static Dictionary<string, int> ImapFlags { get; private set; }
        public static string[] SkipImapFlags { get; private set; }
        public static string[] PopUnorderedDomains { get; private set; }
        public static Dictionary<string, Dictionary<string, ImapExtensions.MailboxInfo>> SpecialDomainFolders { get; private set; }
        public static Dictionary<string, int> DefaultFolders { get; private set; }
    }
}
