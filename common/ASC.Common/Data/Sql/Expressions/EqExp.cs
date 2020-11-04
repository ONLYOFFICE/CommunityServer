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


namespace ASC.Common.Data.Sql.Expressions
{
    public class EqExp : Exp
    {
        private readonly string column;
        private readonly object value;

        public EqExp(string column, object value)
        {
            this.column = column;
            this.value = value;
        }

        public override string ToString(ISqlDialect dialect)
        {
            return string.Format("{0} {1}",
                                 column,
                                 value != null ? (Not ? "<> ?" : "= ?") : (Not ? "is not null" : "is null"));
        }

        public override object[] GetParameters()
        {
            return value == null ? new object[0] : new[] {value};
        }
    }
}