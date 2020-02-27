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


using System.Linq;
using System.Text;

namespace ASC.Common.Data.Sql.Expressions
{
    public class InExp : Exp
    {
        private readonly string column;
        private readonly SqlQuery subQuery;
        private readonly object[] values;

        public InExp(string column, object[] values)
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

        public override object[] GetParameters()
        {
            if (values != null) return values;
            if (subQuery != null) return subQuery.GetParameters();
            return new object[0];
        }
    }
}