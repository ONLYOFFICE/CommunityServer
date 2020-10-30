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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema;
using ASC.Mail.Core.DbSchema.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;

namespace ASC.Mail.Core.Dao
{
    public class TagAddressDao: BaseDao, ITagAddressDao
    {
        protected static ITable table = new MailTableFactory().Create<TagAddressTable>();

        protected string CurrentUserId { get; private set; }

        public TagAddressDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public List<int> GetTagIds(string email)
        {
            var query = new SqlQuery(TagAddressTable.TABLE_NAME)
                .Distinct()
                .Select(TagAddressTable.Columns.TagId)
                .Where(TagAddressTable.Columns.Address, email)
                .Where(Exp.In(TagAddressTable.Columns.TagId,
                    new SqlQuery(TagTable.TABLE_NAME)
                        .Select(TagTable.Columns.Id)
                        .Where(TagTable.Columns.Tenant, Tenant)
                        .Where(TagTable.Columns.User, CurrentUserId)
                    )
                );

            var tagIds = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToInt32(r[0]));

            return tagIds;
        }

        public List<string> GetTagAddresses(int tagId)
        {
            var query = new SqlQuery(TagAddressTable.TABLE_NAME)
                .Select(TagAddressTable.Columns.Address)
                .Where(TagAddressTable.Columns.TagId, tagId)
                .Where(TagAddressTable.Columns.Tenant, Tenant);

            var list = Db.ExecuteList(query)
                .ConvertAll(r => Convert.ToString(r[0]));

            return list;
        }

        public int Save(int tagId, string email)
        {
            var query = new SqlInsert(TagAddressTable.TABLE_NAME, true)
                .InColumnValue(TagAddressTable.Columns.TagId, tagId)
                .InColumnValue(TagAddressTable.Columns.Address, email)
                .InColumnValue(TagAddressTable.Columns.Tenant, Tenant);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int Delete(int tagId, string email = null)
        {
            var query = new SqlDelete(TagAddressTable.TABLE_NAME)
                .Where(TagAddressTable.Columns.TagId, tagId)
                .Where(TagAddressTable.Columns.Tenant, Tenant);

            if (!string.IsNullOrEmpty(email))
            {
                query.Where(TagAddressTable.Columns.Address, email);
            }

            var result = Db.ExecuteNonQuery(query);
            return result;
        }

        public int DeleteAll()
        {
            var query = new SqlDelete(TagAddressTable.TABLE_NAME)
                .Where(TagAddressTable.Columns.Tenant, Tenant);

            var result = Db.ExecuteNonQuery(query);
            return result;
        }
    }
}