/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
    enum AgregateType
    {
        count,
        min,
        max,
        avg,
        sum
    }

    class SelectAgregate : ISqlInstruction
    {
        private readonly AgregateType agregateType;
        private readonly string column;

        public SelectAgregate(AgregateType agregateType)
            : this(agregateType, null)
        {
        }

        public SelectAgregate(AgregateType agregateType, string column)
        {
            this.agregateType = agregateType;
            this.column = column;
        }

        public string ToString(ISqlDialect dialect)
        {
            return string.Format("{0}({1})", agregateType, column == null ? "*" : column);
        }

        public object[] GetParameters()
        {
            return new object[0];
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }
}