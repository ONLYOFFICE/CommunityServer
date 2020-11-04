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


using System;

namespace ASC.Common.Data.Sql.Expressions
{
    [Flags]
    public enum SqlLike
    {
        StartWith = 1,
        EndWith = 2,
        AnyWhere = StartWith | EndWith,
    }

    public class LikeExp : Exp
    {
        private readonly string column;
        private readonly string str;

        public LikeExp(string column, string str, SqlLike like)
        {
            this.column = column;
            if (str != null)
            {
                if ((like & SqlLike.StartWith) == SqlLike.StartWith) str += "%";
                if ((like & SqlLike.EndWith) == SqlLike.EndWith) str = "%" + str;
            }
            this.str = str;
        }

        public override string ToString(ISqlDialect dialect)
        {
            return str != null
                       ? string.Format("{0} {1}like ?", column, Not ? "not " : string.Empty)
                       : string.Format("{0} is {1}null", column, Not ? "not " : string.Empty);
        }

        public override object[] GetParameters()
        {
            return str == null ? new object[0] : new object[] {str};
        }
    }
}