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
using ASC.Common.Data;
using ASC.Common.Data.Sql;

namespace ASC.Web.UserControls.Wiki.Data
{
    class BaseDao : IDisposable
    {
        private const string TenantColumn = "tenant";

        protected readonly DbManager db;
        protected readonly int tenant;


        public BaseDao(string dbid, int tenant)
        {
            this.db = new DbManager(dbid);
            this.tenant = tenant;
        }


        public void Dispose()
        {
            db.Dispose();
        }

        
        protected SqlQuery Query(string table)
        {
            return new SqlQuery(table).Where(GetTenantColumnName(table), tenant);
        }

        protected SqlInsert Insert(string table)
        {
            return new SqlInsert(table, true).InColumnValue(GetTenantColumnName(table), tenant);
        }

        protected SqlUpdate Update(string table)
        {
            return new SqlUpdate(table).Where(GetTenantColumnName(table), tenant);
        }

        protected SqlDelete Delete(string table)
        {
            return new SqlDelete(table).Where(GetTenantColumnName(table), tenant);
        }

        private string GetTenantColumnName(string table)
        {
            var pos = table.LastIndexOf(' ');
            return (0 < pos ? table.Substring(pos).Trim() + '.' : string.Empty) + TenantColumn;
        }
    }
}