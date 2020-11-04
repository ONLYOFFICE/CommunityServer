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


using System.Collections;
using System.Diagnostics;

namespace ASC.Common.Data.Sql.Expressions
{
    [DebuggerTypeProxy(typeof(SqlDebugView))]
    public abstract class Exp : ISqlInstruction
    {
        public static readonly Exp Empty;

        public static Exp True
        {
            get { return new EqColumnsExp("1", "1"); }
        }

        public static Exp False
        {
            get { return new EqColumnsExp("1", "0"); }
        }


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

        public static Exp Sum(Exp exp)
        {
            return new SumExp(exp);
        }

        public static Exp If(Exp exp, object trueValue, object falseValue)
        {
            return new IfExp(exp, trueValue, falseValue);
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