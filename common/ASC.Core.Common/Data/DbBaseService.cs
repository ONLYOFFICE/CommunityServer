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
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using ASC.Security.Cryptography;

namespace ASC.Core.Data
{
    public abstract class DbBaseService
    {
        private readonly string dbid;

        protected string TenantColumn
        {
            get;
            private set;
        }

        protected DbBaseService(ConnectionStringSettings connectionString, string tenantColumn)
        {
            dbid = connectionString.Name;
            TenantColumn = tenantColumn;
        }

        protected T ExecScalar<T>(ISqlInstruction sql)
        {
            return Execute(db => db.ExecuteScalar<T>(sql));
        }

        protected int ExecNonQuery(ISqlInstruction sql)
        {
            return Execute(db => db.ExecuteNonQuery(sql));
        }

        protected List<object[]> ExecList(ISqlInstruction sql)
        {
            return Execute(db => db.ExecuteList(sql));
        }

        protected void ExecBatch(params ISqlInstruction[] batch)
        {
            Execute(db => db.ExecuteBatch(batch));
        }

        protected void ExecBatch(IEnumerable<ISqlInstruction> batch)
        {
            Execute(db => db.ExecuteBatch(batch));
        }

        protected IDbManager GetDb()
        {
            return DbManager.FromHttpContext(dbid);
        }

        protected SqlQuery Query(string table, int tenant)
        {
            return new SqlQuery(table).Where(GetTenantColumnName(table), tenant);
        }

        protected SqlInsert Insert(string table, int tenant)
        {
            return new SqlInsert(table, true).InColumnValue(GetTenantColumnName(table), tenant);
        }

        protected SqlUpdate Update(string table, int tenant)
        {
            return new SqlUpdate(table).Where(GetTenantColumnName(table), tenant);
        }

        protected SqlDelete Delete(string table, int tenant)
        {
            return new SqlDelete(table).Where(GetTenantColumnName(table), tenant);
        }

        private string GetTenantColumnName(string table)
        {
            var pos = table.LastIndexOf(' ');
            return (0 < pos ? table.Substring(pos).Trim() + '.' : string.Empty) + TenantColumn;
        }

        private T Execute<T>(Func<IDbManager, T> action)
        {
            using (var db = GetDb())
            {
                return action(db);
            }
        }

        protected static string GetPasswordHash(Guid userId, string password)
        {
            return Hasher.Base64Hash(password + userId + Encoding.UTF8.GetString(MachinePseudoKeys.GetMachineConstant()), HashAlg.SHA512);
        }
    }
}