/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

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
    }
}