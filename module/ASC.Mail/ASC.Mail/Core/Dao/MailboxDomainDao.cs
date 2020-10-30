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
    public class MailboxDomainDao : BaseDao, IMailboxDomainDao
    {
        protected static ITable table = new MailTableFactory().Create<MailboxDomainTable>();

        public MailboxDomainDao(IDbManager dbManager) 
            : base(table, dbManager, -1)
        {
        }

        public MailboxDomain GetDomain(string domainName)
        {
            var query = Query()
                .Where(MailboxDomainTable.Columns.DomainName, domainName);

            return Db.ExecuteList(query)
                .ConvertAll(ToMailboxDomain)
                .FirstOrDefault();
        }

        public int SaveDomain(MailboxDomain domain)
        {
            var query = new SqlInsert(MailboxDomainTable.TABLE_NAME, true)
                .InColumnValue(MailboxDomainTable.Columns.Id, domain.Id)
                .InColumnValue(MailboxDomainTable.Columns.ProviderId, domain.ProviderId)
                .InColumnValue(MailboxDomainTable.Columns.DomainName, domain.Name)
                .Identity(0, 0, true);

            var id = Db.ExecuteScalar<int>(query);

            return id;
        }

        protected MailboxDomain ToMailboxDomain(object[] r)
        {
            var d = new MailboxDomain
            {
                Id = Convert.ToInt32(r[0]),
                ProviderId = Convert.ToInt32(r[1]),
                Name = Convert.ToString(r[2])
            };

            return d;
        }
    }
}