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


using System.Data;
using System.Text;

namespace ASC.Common.Data.Sql
{
    public class SqlDialect : ISqlDialect
    {
        public static readonly ISqlDialect Default = new SqlDialect();


        public virtual string IdentityQuery
        {
            get { return "@@identity"; }
        }

        public virtual string Autoincrement
        {
            get { return "AUTOINCREMENT"; }
        }

        public virtual string InsertIgnore
        {
            get { return "insert ignore"; }
        }


        public virtual bool SupportMultiTableUpdate
        {
            get { return true; }
        }

        public virtual bool SeparateCreateIndex
        {
            get { return true; }
        }


        public virtual string DbTypeToString(DbType type, int size, int precision)
        {
            var s = new StringBuilder(type.ToString().ToLower());
            if (0 < size)
            {
                s.AppendFormat(0 < precision ? "({0}, {1})" : "({0})", size, precision);
            }
            return s.ToString();
        }

        public virtual IsolationLevel GetSupportedIsolationLevel(IsolationLevel il)
        {
            return il;
        }

        public string UseIndex(string index)
        {
            return string.Empty;
        }
    }
}