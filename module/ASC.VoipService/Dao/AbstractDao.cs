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


namespace ASC.VoipService.Dao
{
    public class AbstractDao
    {
        private readonly string dbid = "default";

        protected AbstractDao(int tenantID)
        {
            TenantID = tenantID;
        }

        protected DbManager GetDb()
        {
            return new DbManager(dbid);
        }

        protected int TenantID
        {
            get;
            private set;
        }

        protected SqlQuery Query(string table)
        {
            return new SqlQuery(table).Where(GetTenantColumnName(table), TenantID);
        }

        protected SqlDelete Delete(string table)
        {
            return new SqlDelete(table).Where(GetTenantColumnName(table), TenantID);
        }

        protected SqlInsert Insert(string table)
        {
            return new SqlInsert(table, true).InColumns(GetTenantColumnName(table)).Values(TenantID);
        }

        protected SqlUpdate Update(string table)
        {
            return new SqlUpdate(table).Where(GetTenantColumnName(table), TenantID);
        }

        protected string GetTenantColumnName(string table)
        {
            const string tenant = "tenant_id";
            if (!table.Contains(" ")) return tenant;
            return table.Substring(table.IndexOf(" ", StringComparison.Ordinal)).Trim() + "." + tenant;
        }


        protected static Guid ToGuid(object guid)
        {
            var str = guid as string;
            return !string.IsNullOrEmpty(str) ? new Guid(str) : Guid.Empty;
        }
    }
}
