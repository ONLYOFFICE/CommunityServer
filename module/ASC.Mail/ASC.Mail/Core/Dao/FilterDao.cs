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
    public class FilterDao : BaseDao, IFilterDao
    {
        protected static ITable table = new MailTableFactory().Create<FilterTable>();

        protected string CurrentUserId { get; private set; }

        public FilterDao(IDbManager dbManager, int tenant, string user) 
            : base(table, dbManager, tenant)
        {
            CurrentUserId = user;
        }

        public List<Filter> GetList()
        {
            var query = Query()
                .Where(FilterTable.Columns.Tenant, Tenant)
                .Where(FilterTable.Columns.User, CurrentUserId);

            return Db.ExecuteList(query)
                .ConvertAll(ToFilter);
        }

        public Filter Get(int id)
        {
            var query = Query()
                .Where(FilterTable.Columns.Tenant, Tenant)
                .Where(FilterTable.Columns.User, CurrentUserId)
                .Where(FilterTable.Columns.Id, id);

            return Db.ExecuteList(query)
                .ConvertAll(ToFilter)
                .SingleOrDefault();
        }

        public int Save(Filter filter)
        {
            var now = DateTime.UtcNow;

            var query = new SqlInsert(FilterTable.TABLE_NAME, true)
                .InColumnValue(FilterTable.Columns.Id, filter.Id)
                .InColumnValue(FilterTable.Columns.Tenant, filter.Tenant)
                .InColumnValue(FilterTable.Columns.User, filter.User)
                .InColumnValue(FilterTable.Columns.Enabled, filter.Enabled)
                .InColumnValue(FilterTable.Columns.Filter, filter.FilterData)
                .InColumnValue(FilterTable.Columns.Position, filter.Position)
                .InColumnValue(FilterTable.Columns.MofifiedOn, now)
                .Identity(0, 0, true);

            if (filter.Id == 0)
            {
                query.InColumnValue(FilterTable.Columns.CreatedOn, now);
            }

            return Db.ExecuteScalar<int>(query);
        }

        public int Delete(int id)
        {
            var query = new SqlDelete(FilterTable.TABLE_NAME)
                .Where(FilterTable.Columns.Tenant, Tenant)
                .Where(FilterTable.Columns.User, CurrentUserId)
                .Where(FilterTable.Columns.Id, id);

            return Db.ExecuteNonQuery(query);
        }

        protected Filter ToFilter(object[] r)
        {
            var f = new Filter
            {
                Id = Convert.ToInt32(r[0]),
                Tenant = Convert.ToInt32(r[1]),
                User = Convert.ToString(r[2]),
                Enabled = Convert.ToBoolean(r[3]),
                FilterData = Convert.ToString(r[4]),
                Position = Convert.ToInt32(r[5])
            };

            return f;
        }
    }
}