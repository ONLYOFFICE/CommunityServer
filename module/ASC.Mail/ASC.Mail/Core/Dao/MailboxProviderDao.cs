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