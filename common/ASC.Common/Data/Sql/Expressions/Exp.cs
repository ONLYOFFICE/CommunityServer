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