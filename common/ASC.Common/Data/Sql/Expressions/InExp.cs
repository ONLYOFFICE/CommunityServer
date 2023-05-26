/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASC.Common.Data.Sql.Expressions
{
    public class InExp<T> : Exp
    {
        private readonly string column;
        private readonly SqlQuery subQuery;
        private readonly IEnumerable<T> values;

        public InExp(string column, IEnumerable<T> values)
        {
            this.column = column;
            this.values = values;
        }

        public InExp(string column, SqlQuery subQuery)
        {
            this.column = column;
            this.subQuery = subQuery;
        }

        public override string ToString(ISqlDialect dialect)
        {
            if (values != null && values.Count() < 2)
            {
                var exp = values.Count() == 0 ? Exp.False : Exp.Eq(column, values.ElementAt(0));
                return (Not ? !exp : exp).ToString(dialect);
            }

            var sql = new StringBuilder(column);
            if (Not) sql.Append(" not");
            sql.Append(" in (");

            if (values != null)
            {
                sql.Append(string.Join(",", Enumerable.Repeat("?", values.Count()).ToArray()));
            }
            if (subQuery != null)
            {
                sql.Append(subQuery.ToString(dialect));
            }
            return sql.Append(")").ToString();
        }

        public override IEnumerable<object> GetParameters()
        {
            if (values != null) return values.Cast<object>().ToList();
            if (subQuery != null) return subQuery.GetParameters();
            return new object[0];
        }
    }
}