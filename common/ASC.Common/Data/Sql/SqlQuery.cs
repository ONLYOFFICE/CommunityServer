/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Common.Data.Sql
{
    [DebuggerTypeProxy(typeof(SqlDebugView))]
    public class SqlQuery : ISqlInstruction
    {
        private readonly List<ISqlInstruction> columns = new List<ISqlInstruction>();
        private readonly ArrayList groups = new ArrayList();
        private readonly List<JoinInfo> joins = new List<JoinInfo>();
        private readonly List<ISqlInstruction> tables = new List<ISqlInstruction>();
        private readonly List<UnionInfo> unions = new List<UnionInfo>();
        private bool distinct;
        private int firstResult;
        private Exp having;
        private int maxResults;
        protected IDictionary<object, bool> orders = new Dictionary<object, bool>();
        private bool selectAll;
        protected Exp where;
        private bool forupdate;
        private string index;


        public SqlQuery()
        {
        }

        public SqlQuery(string table)
        {
            From(table);
        }

        #region ISqlInstruction Members

        public string ToString(ISqlDialect dialect)
        {
            var sql = new StringBuilder();

            sql.Append("select ");
            if (distinct) sql.Append("distinct ");
            if (selectAll)
            {
                sql.Append("*");
            }
            else
            {
                columns.ForEach(column => sql.AppendFormat("{0}, ", column.ToString(dialect)));
                sql.Remove(sql.Length - 2, 2);
            }

            if (0 < tables.Count)
            {
                sql.Append(" from ");
                tables.ForEach(table => sql.AppendFormat("{0}, ", table.ToString(dialect)));
                sql.Remove(sql.Length - 2, 2);
            }

            if (!string.IsNullOrEmpty(index))
            {
                sql.AppendFormat(" {0}", dialect.UseIndex(index));
            }

            if (0 < joins.Count)
            {
                foreach (JoinInfo join in joins)
                {
                    if (join.JoinType == SqlJoin.Inner) sql.Append(" inner join ");
                    if (join.JoinType == SqlJoin.LeftOuter) sql.Append(" left outer join ");
                    if (join.JoinType == SqlJoin.RightOuter) sql.Append(" right outer join ");
                    sql.AppendFormat("{0} on {1}", join.With.ToString(dialect), join.On.ToString(dialect));
                }
            }

            if (where != Exp.Empty) sql.AppendFormat(" where {0}", where.ToString(dialect));

            if (0 < groups.Count)
            {
                sql.Append(" group by ");
                foreach (object group in groups)
                {
                    sql.AppendFormat("{0}, ", group.ToString());
                }
                sql.Remove(sql.Length - 2, 2);
            }

            if (having != Exp.Empty) sql.AppendFormat(" having {0}", having.ToString(dialect));

            if (0 < orders.Count)
            {
                sql.Append(" order by ");
                foreach (var order in orders)
                {
                    sql.Append(order.Key.ToString());
                    sql.AppendFormat(" {0}, ", order.Value ? "asc" : "desc");
                }
                sql.Remove(sql.Length - 2, 2);
            }

            if (0 < maxResults)
            {
                sql.AppendFormat(" limit {0}", maxResults);
                if (0 < firstResult) sql.AppendFormat(" offset {0}", firstResult);
            }

            if (forupdate)
            {
                sql.Append(" for update");
            }

            unions.ForEach(u => sql.AppendFormat(" union {0}{1}", u.UnionAll ? "all " : string.Empty, u.Query.ToString(dialect)));
            return sql.ToString();
        }

        public object[] GetParameters()
        {
            var parameters = new List<object>();
            columns.ForEach(column => parameters.AddRange(column.GetParameters()));
            tables.ForEach(table => parameters.AddRange(table.GetParameters()));
            foreach (JoinInfo join in joins)
            {
                parameters.AddRange(join.With.GetParameters());
                parameters.AddRange(join.On.GetParameters());
            }
            if (where != Exp.Empty) parameters.AddRange(where.GetParameters());
            if (having != Exp.Empty) parameters.AddRange(having.GetParameters());
            unions.ForEach(u => parameters.AddRange(u.Query.GetParameters()));
            return parameters.ToArray();
        }

        #endregion


        public SqlQuery From(params string[] tables)
        {
            Array.ForEach(tables, table => this.tables.Add((SqlIdentifier)table));
            return this;
        }

        public SqlQuery From(SqlQuery subQuery)
        {
            return From(subQuery, null);
        }

        public SqlQuery From(SqlQuery subQuery, string alias)
        {
            tables.Add(new AsExp(subQuery, alias));
            return this;
        }

        public SqlQuery UseIndex(string index)
        {
            this.index = index;
            return this;
        }

        private SqlQuery Join(SqlJoin join, ISqlInstruction instruction, Exp on)
        {
            joins.Add(new JoinInfo(join, instruction, on));
            return this;
        }

        public SqlQuery InnerJoin(string table, Exp on)
        {
            return Join(SqlJoin.Inner, new SqlIdentifier(table), on);
        }

        public SqlQuery LeftOuterJoin(string table, Exp on)
        {
            return Join(SqlJoin.LeftOuter, new SqlIdentifier(table), on);
        }

        public SqlQuery RightOuterJoin(string table, Exp on)
        {
            return Join(SqlJoin.RightOuter, new SqlIdentifier(table), on);
        }

        public SqlQuery InnerJoin(SqlQuery subQuery, Exp on)
        {
            return InnerJoin(subQuery, string.Empty, on);
        }

        public SqlQuery LeftOuterJoin(SqlQuery subQuery, Exp on)
        {
            return LeftOuterJoin(subQuery, string.Empty, on);
        }

        public SqlQuery RightOuterJoin(SqlQuery subQuery, Exp on)
        {
            return RightOuterJoin(subQuery, string.Empty, on);
        }

        public SqlQuery InnerJoin(SqlQuery subQuery, string alias, Exp on)
        {
            return Join(SqlJoin.Inner, new AsExp(subQuery, alias), on);
        }

        public SqlQuery LeftOuterJoin(SqlQuery subQuery, string alias, Exp on)
        {
            return Join(SqlJoin.LeftOuter, new AsExp(subQuery, alias), on);
        }

        public SqlQuery RightOuterJoin(SqlQuery subQuery, string alias, Exp on)
        {
            return Join(SqlJoin.RightOuter, new AsExp(subQuery, alias), on);
        }

        public SqlQuery Select(params string[] columns)
        {
            Array.ForEach(columns, column => this.columns.Add((SqlIdentifier)column));
            return this;
        }

        public SqlQuery Select(SqlQuery subQuery, string alias = null)
        {
            columns.Add(new AsExp(subQuery, alias));
            return this;
        }

        public SqlQuery Select(ISqlInstruction instraction)
        {
            columns.Add(instraction);
            return this;
        }

        public SqlQuery SelectAll()
        {
            selectAll = true;
            return this;
        }

        public SqlQuery SelectCount()
        {
            selectAll = false;
            columns.Add(new SelectAgregate(AgregateType.count));
            return this;
        }

        public SqlQuery SelectCount(string column)
        {
            selectAll = false;
            columns.Add(new SelectAgregate(AgregateType.count, column));
            return this;
        }

        public SqlQuery SelectMin(string column)
        {
            selectAll = false;
            columns.Add(new SelectAgregate(AgregateType.min, column));
            return this;
        }

        public SqlQuery SelectMax(string column)
        {
            selectAll = false;
            columns.Add(new SelectAgregate(AgregateType.max, column));
            return this;
        }

        public SqlQuery SelectAvg(string column)
        {
            selectAll = false;
            columns.Add(new SelectAgregate(AgregateType.avg, column));
            return this;
        }

        public SqlQuery SelectSum(string column)
        {
            selectAll = false;
            columns.Add(new SelectAgregate(AgregateType.sum, column));
            return this;
        }

        public SqlQuery Distinct()
        {
            distinct = true;
            return this;
        }

        public SqlQuery Where(Exp exp)
        {
            where = where & exp;
            return this;
        }

        public SqlQuery Where(string sql)
        {
            return Where(new SqlExp(sql));
        }

        public SqlQuery Where(string column, object value)
        {
            return Where(new EqExp(column, value));
        }

        public SqlQuery OrderBy(string column, bool asc)
        {
            orders.Add(column, asc);
            return this;
        }

        public SqlQuery OrderBy(int positin, bool asc)
        {
            orders.Add(positin, asc);
            return this;
        }

        public SqlQuery GroupBy(params string[] columns)
        {
            groups.AddRange(columns);
            return this;
        }

        public SqlQuery GroupBy(params int[] positins)
        {
            groups.AddRange(positins);
            return this;
        }

        public SqlQuery Having(Exp having)
        {
            this.having = having;
            return this;
        }

        public SqlQuery SetFirstResult(int firstResult)
        {
            this.firstResult = firstResult;
            return this;
        }

        public SqlQuery SetMaxResults(int maxResults)
        {
            this.maxResults = maxResults;
            return this;
        }

        public SqlQuery Union(params SqlQuery[] queries)
        {
            foreach (var q in queries)
            {
                unions.Add(new UnionInfo(q, false));
            }
            return this;
        }

        public SqlQuery UnionAll(params SqlQuery[] queries)
        {
            foreach (var q in queries)
            {
                unions.Add(new UnionInfo(q, true));
            }
            return this;
        }

        public SqlQuery ForUpdate(bool forupdate)
        {
            this.forupdate = forupdate;
            return this;
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }


        private class JoinInfo
        {
            public readonly SqlJoin JoinType;
            public readonly Exp On;
            public readonly ISqlInstruction With;

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

        private class UnionInfo
        {
            public readonly SqlQuery Query;
            public readonly bool UnionAll;

            public UnionInfo(SqlQuery query, bool unionAll)
            {
                Query = query;
                UnionAll = unionAll;
            }
        }
    }
}