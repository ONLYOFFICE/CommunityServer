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
    public class AsExp : Exp
    {
        private readonly string alias;
        private readonly SqlQuery subQuery;

        public AsExp(SqlQuery subQuery)
            : this(subQuery, null)
        {
        }

        public AsExp(SqlQuery subQuery, string alias)
        {
            this.subQuery = subQuery;
            this.alias = alias;
        }

        public override string ToString(ISqlDialect dialect)
        {
            return string.Format("({0}){1}", subQuery.ToString(dialect),
                                 string.IsNullOrEmpty(alias) ? string.Empty : " as " + alias);
        }

        public override object[] GetParameters()
        {
            return subQuery.GetParameters();
        }
    }
}