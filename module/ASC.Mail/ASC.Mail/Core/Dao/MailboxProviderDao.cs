/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao
{
    public class MailboxProviderDao : BaseDao, IMailboxProviderDao
    {
        protected static ITable table = new MailTableFactory().Create<MailboxProviderTable>();

        public MailboxProviderDao(IDbManager dbManager) 
            : base(table, dbManager, -1)
        {
        }

        public MailboxProvider GetProvider(int id)
        {
            var query = Query()
               .Where(MailboxProviderTable.Columns.Id, id);

            return Db.ExecuteList(query)
                .ConvertAll(ToMailboxProvider)
                .FirstOrDefault();
        }

        public MailboxProvider GetProvider(string providerName)
        {
            var query = Query()
                .Where(MailboxProviderTable.Columns.ProviderName, providerName);

            return Db.ExecuteList(query)
                .ConvertAll(ToMailboxProvider)
                .FirstOrDefault();
        }

        public int SaveProvider(MailboxProvider mailboxProvider)
        {
            var query = new SqlInsert(MailboxProviderTable.TABLE_NAME, true)
                .InColumnValue(MailboxProviderTable.Columns.Id, mailboxProvider.Id)
                .InColumnValue(MailboxProviderTable.Columns.ProviderName, mailboxProvider.Name)
                .InColumnValue(MailboxProviderTable.Columns.DisplayName, mailboxProvider.DisplayName)
                .InColumnValue(MailboxProviderTable.Columns.DisplayShortName,
                    mailboxProvider.DisplayShortName)
                .InColumnValue(MailboxProviderTable.Columns.Documentation,
                    mailboxProvider.Url)
                .Identity(0, 0, true);

            var idProvider = Db.ExecuteScalar<int>(query);

            return idProvider;
        }

        protected MailboxProvider ToMailboxProvider(object[] r)
        {
            var p = new MailboxProvider
            {
                Id = Convert.ToInt32(r[0]),
                Name = Convert.ToString(r[1]),
                DisplayName = Convert.ToString(r[2]),
                DisplayShortName = Convert.ToString(r[3]),
                Url = Convert.ToString(r[4])
            };

            return p;
        }
    }
}