/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
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