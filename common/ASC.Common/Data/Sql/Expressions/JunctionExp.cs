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

namespace ASC.Common.Data.Sql.Expressions
{
    public class JunctionExp : Exp
    {
        private readonly bool and;
        private readonly Exp exp1;
        private readonly Exp exp2;

        public JunctionExp(Exp exp1, Exp exp2, bool and)
        {
            this.exp1 = exp1;
            this.exp2 = exp2;
            this.and = and;
        }

        public override string ToString(ISqlDialect dialect)
        {
            string format = exp1 is JunctionExp && ((JunctionExp) exp1).and != and ? "({0})" : "{0}";
            format += " {1} ";
            format += exp2 is JunctionExp && ((JunctionExp) exp2).and != and ? "({2})" : "{2}";
            return Not
                       ? string.Format(format, (!exp1).ToString(dialect), and ? "or" : "and",
                                       (!exp2).ToString(dialect))
                       : string.Format(format, exp1.ToString(dialect), and ? "and" : "or", exp2.ToString(dialect));
        }

        public override object[] GetParameters()
        {
            var parameters = new List<object>();
            parameters.AddRange(exp1.GetParameters());
            parameters.AddRange(exp2.GetParameters());
            return parameters.ToArray();
        }
    }
}