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


#region usings

using System.Diagnostics;
using System.Text;
using ASC.Common.Data.Sql.Expressions;

#endregion

namespace ASC.Common.Data.Sql
{
    [DebuggerTypeProxy(typeof(SqlDebugView))]
    public class SqlDelete : ISqlInstruction
    {
        private readonly string table;
        private Exp where = Exp.Empty;

        public SqlDelete(string table)
        {
            this.table = table;
        }

        #region ISqlInstruction Members

        public string ToString(ISqlDialect dialect)
        {
            var sql = new StringBuilder();
            sql.AppendFormat("delete from {0}", table);
            if (where != Exp.Empty) sql.AppendFormat(" where {0}", where.ToString(dialect));
            return sql.ToString();
        }

        public object[] GetParameters()
        {
            return where != Exp.Empty ? where.GetParameters() : new object[0];
        }

        #endregion

        public SqlDelete Where(Exp where)
        {
            this.where = this.where & where;
            return this;
        }

        public SqlDelete Where(string column, object value)
        {
            return Where(Exp.Eq(column, value));
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }
}