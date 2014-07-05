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

using System.Collections;
using System.Diagnostics;

namespace ASC.Common.Data.Sql.Expressions
{
    [DebuggerTypeProxy(typeof(SqlDebugView))]
    public abstract class Exp : ISqlInstruction
    {
        public static readonly Exp Empty;

        public static readonly Exp True = new EqColumnsExp("1", "1");

        public static readonly Exp False = new EqColumnsExp("1", "0");


        protected bool Not { get; private set; }


        public abstract string ToString(ISqlDialect dialect);

        public virtual object[] GetParameters()
        {
            return new object[0];
        }


        public static Exp Eq(string column, object value)
        {
            return new EqExp(column, value);
        }

        public static Exp EqColumns(string column1, string column2)
        {
            return new EqColumnsExp(column1, column2);
        }

        public static Exp EqColumns(string column1, SqlQuery query)
        {
            return new EqColumnsExp(column1, query);
        }

        public static Exp And(Exp exp1, Exp exp2)
        {
            return Junction(exp1, exp2, true);
        }

        public static Exp Or(Exp exp1, Exp exp2)
        {
            return Junction(exp1, exp2, false);
        }

        private static Exp Junction(Exp exp1, Exp exp2, bool and)
        {
            if (exp1 == null && exp2 == null) return null;
            if (exp1 == null) return exp2;
            if (exp2 == null) return exp1;
            return new JunctionExp(exp1, exp2, and);
        }

        public static Exp Like(string column, string value)
        {
            return Like(column, value, SqlLike.AnyWhere);
        }

        public static Exp Like(string column, string value, SqlLike like)
        {
            return new LikeExp(column, value, like);
        }

        public static Exp In(string column, ICollection values)
        {
            return new InExp(column, new ArrayList(values).ToArray());
        }

        public static Exp In(string column, object[] values)
        {
            return new InExp(column, values);
        }

        public static Exp In(string column, SqlQuery subQuery)
        {
            return new InExp(column, subQuery);
        }

        public static Exp Between(string column, object minValue, object maxValue)
        {
            return new BetweenExp(column, minValue, maxValue);
        }

        public static Exp Lt(string column, object value)
        {
            return new LGExp(column, value, false);
        }

        public static Exp Le(string column, object value)
        {
            return new LGExp(column, value, true);
        }

        public static Exp Gt(string column, object value)
        {
            return !Le(column, value);
        }

        public static Exp Ge(string column, object value)
        {
            return !Lt(column, value);
        }

        public static Exp Sql(string sql)
        {
            return new SqlExp(sql);
        }

        public static Exp Exists(SqlQuery query)
        {
            return new ExistsExp(query);
        }

        public static Exp operator &(Exp exp1, Exp exp2)
        {
            return And(exp1, exp2);
        }

        public static Exp operator |(Exp exp1, Exp exp2)
        {
            return Or(exp1, exp2);
        }

        public static Exp operator !(Exp exp)
        {
            exp.Not = !exp.Not;
            return exp;
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }
}