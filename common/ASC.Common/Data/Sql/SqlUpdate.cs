/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
            sql.AppendFormat("update {0} set ", table.ToString(dialect));
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
    }
}