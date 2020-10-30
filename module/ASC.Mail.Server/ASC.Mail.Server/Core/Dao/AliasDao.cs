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
    public class AliasDao : BaseDao, IAliasDao
    {
        protected static ITable table = new MailServerTableFactory().Create<AliasTable>();

        public AliasDao(IDbManager dbManager) 
            : base(table, dbManager)
        {
        }

        public int Save(Alias alias)
        {
            var query = new SqlInsert(AliasTable.TABLE_NAME, true)
                .InColumnValue(AliasTable.Columns.ADDRESS, alias.Address)
                .InColumnValue(AliasTable.Columns.GOTO, alias.GoTo)
                .InColumnValue(AliasTable.Columns.DOMAIN, alias.Domain)
                .InColumnValue(AliasTable.Columns.CREATED, alias.Created)
                .InColumnValue(AliasTable.Columns.MODIFIED, alias.Modified)
                .InColumnValue(AliasTable.Columns.ACTIVE, alias.IsActive)
                .InColumnValue(AliasTable.Columns.IS_GROUP, alias.IsGroup);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int Remove(string address)
        {
            var query = new SqlDelete(AliasTable.TABLE_NAME)
                .Where(AliasTable.Columns.ADDRESS, address);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int RemoveByDomain(string domain)
        {
            var query = new SqlDelete(AliasTable.TABLE_NAME)
                .Where(AliasTable.Columns.DOMAIN, domain);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }
    }
}