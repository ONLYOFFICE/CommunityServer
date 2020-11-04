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


#region Usings

using System;
using System.Data;
using ASC.Common.Data;
using ASC.Common.Data.Sql;

#endregion

namespace ASC.Blogs.Core.Data
{
    public class DbDao
    {
        private readonly DbManager db;


        protected DbDao(DbManager db, int tenant)
        {
            if (db == null) throw new ArgumentNullException("db");

            this.db = db;
            Tenant = tenant;
        }

        public int Tenant { get; private set; }

        public DbManager Db { get { return db; } }

        public IDbConnection OpenConnection()
        {
            return db.Connection;
        }

        protected SqlQuery Query(string table)
        {
            return new SqlQuery(table).Where(GetTenantColumnName(table), Tenant);
        }

        protected SqlUpdate Update(string table)
        {
            return new SqlUpdate(table).Where(GetTenantColumnName(table), Tenant);
        }

        protected SqlDelete Delete(string table)
        {
            return new SqlDelete(table).Where(GetTenantColumnName(table), Tenant);
        }

        protected SqlInsert Insert(string table)
        {
            return new SqlInsert(table, true).InColumns(TenantColumnName).Values(Tenant);
        }

        protected string TenantColumnName { get { return "Tenant"; } }
        
        protected string GetTenantColumnName(string table)
        {
            return String.Format("{0}.{1}", table, TenantColumnName);
        }
    }
}
