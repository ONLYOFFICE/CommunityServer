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


using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Common.Data.Sql
{
    [DebuggerTypeProxy(typeof(SqlDebugView))]
    public class SqlUpdate : ISqlInstruction
    {
        private readonly List<string> expressions = new List<string>();
        private readonly List<JoinInfo> joins = new List<JoinInfo>();
        private readonly Dictionary<string, object> sets = new Dictionary<string, object>();
        private readonly SqlIdentifier table;
        private Exp where = Exp.Empty;

        public SqlUpdate(string table)
        {
            this.table = (SqlIdentifier)table;
        }

        #region ISqlInstruction Members

        public string ToString(ISqlDialect dialect)
        {
            var sql = new StringBuilder();

            if (0 < joins.Count)
            {
                sql.AppendFormat("update {0}", table.ToString(dialect));

                foreach (var join in joins)
                {
                    if (@join.JoinType == SqlJoin.Inner) sql.Append(" inner join ");
                    if (@join.JoinType == SqlJoin.LeftOuter) sql.Append(" left outer join ");
                    if (@join.JoinType == SqlJoin.RightOuter) sql.Append(" right outer join ");
                    sql.AppendFormat("{0} on {1}", @join.With.ToString(dialect), @join.On.ToString(dialect));
                }

                sql.Append(" set ");
            }
            else
            {
                sql.AppendFormat("update {0} set ", table.ToString(dialect));
            }

            foreach (var set in sets)
            {
                sql.AppendFormat("{0} = {1}, ", set.Key, set.Value is ISqlInstruction ? ((ISqlInstruction)set.Value).ToString(dialect) : "?");
            }
            expressions.ForEach(expression => sql.AppendFormat("{0}, ", expression));
            sql.Remove(sql.Length - 2, 2);
            if (where != Exp.Empty) sql.AppendFormat(" where {0}", where.ToString(dialect));
            return sql.ToString();
        }

        public object[] GetParameters()
        {
            var parameters = new List<object>();
            foreach (var join in joins)
            {
                parameters.AddRange(@join.With.GetParameters());
                parameters.AddRange(@join.On.GetParameters());
            }

            foreach (object parameter in sets.Values)
            {
                if (parameter is ISqlInstruction)
                {
                    parameters.AddRange(((ISqlInstruction)parameter).GetParameters());
                }
                else
                {
                    parameters.Add(parameter);
                }
            }
            if (where != Exp.Empty) parameters.AddRange(where.GetParameters());
            return parameters.ToArray();
        }

        #endregion

        private SqlUpdate Join(SqlJoin join, ISqlInstruction instruction, Exp on)
        {
            joins.Add(new JoinInfo(join, instruction, on));
            return this;
        }

        public SqlUpdate InnerJoin(string tableName, Exp on)
        {
            return Join(SqlJoin.Inner, new SqlIdentifier(tableName), on);
        }

        public SqlUpdate LeftOuterJoin(string tableName, Exp on)
        {
            return Join(SqlJoin.LeftOuter, new SqlIdentifier(tableName), on);
        }

        public SqlUpdate RightOuterJoin(string tableName, Exp on)
        {
            return Join(SqlJoin.RightOuter, new SqlIdentifier(tableName), on);
        }

        public SqlUpdate Set(string expression)
        {
            expressions.Add(expression);
            return this;
        }

        public SqlUpdate Set(string column, object value)
        {
            sets[column] = value is SqlQuery ? new AsExp((SqlQuery)value) : value;
            return this;
        }

        public SqlUpdate Where(Exp where)
        {
            this.where = this.where & where;
            return this;
        }

        public SqlUpdate Where(string column, object value)
        {
            return Where(Exp.Eq(column, value));
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }

        private class JoinInfo
        {
            public SqlJoin JoinType { get; private set; }
            public Exp On { get; private set; }
            public ISqlInstruction With { get; private set; }

            public JoinInfo(SqlJoin joinType, ISqlInstruction with, Exp on)
            {
                With = with;
                JoinType = joinType;
                On = on;
            }
        }

        private enum SqlJoin
        {
            LeftOuter,
            RightOuter,
            Inner
        }
    }
}