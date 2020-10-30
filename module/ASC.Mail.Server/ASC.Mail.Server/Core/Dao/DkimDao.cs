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


using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Server.Core.Dao.Interfaces;
using ASC.Mail.Server.Core.DbSchema;
using ASC.Mail.Server.Core.DbSchema.Interfaces;
using ASC.Mail.Server.Core.DbSchema.Tables;
using ASC.Mail.Server.Core.Entities;

namespace ASC.Mail.Server.Core.Dao
{
    public class DkimDao : BaseDao, IDkimDao
    {
        protected static ITable table = new MailServerTableFactory().Create<DkimTable>();

        public DkimDao(IDbManager dbManager) 
            : base(table, dbManager)
        {
        }

        public int Save(Dkim dkim)
        {
            var query = new SqlInsert(DkimTable.TABLE_NAME, true)
                .InColumnValue(DkimTable.Columns.ID, dkim.Id)
                .InColumnValue(DkimTable.Columns.DOMAIN_NAME, dkim.DomainName)
                .InColumnValue(DkimTable.Columns.SELECTOR, dkim.Selector)
                .InColumnValue(DkimTable.Columns.PRIVATE_KEY, dkim.PrivateKey)
                .InColumnValue(DkimTable.Columns.PUBLIC_KEY, dkim.PublicKey);

            var id = Db.ExecuteScalar<int>(query);

            return id;
        }

        public int Remove(string domain)
        {
            var query = new SqlDelete(DkimTable.TABLE_NAME)
                .Where(DkimTable.Columns.DOMAIN_NAME, domain);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }
    }
}