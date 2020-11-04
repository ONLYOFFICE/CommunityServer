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
using System.Data;

namespace ASC.Common.Data.Sql
{
    public class SQLiteDialect : ISqlDialect
    {
        public string IdentityQuery
        {
            get { return "last_insert_rowid()"; }
        }

        public string Autoincrement
        {
            get { return "autoincrement"; }
        }

        public virtual string InsertIgnore
        {
            get { return "insert or ignore"; }
        }

        public bool SupportMultiTableUpdate
        {
            get { return false; }
        }

        public bool SeparateCreateIndex
        {
            get { return true; }
        }


        public string DbTypeToString(DbType type, int size, int precision)
        {
            switch (type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Xml:
                case DbType.Guid:
                    return "TEXT";

                case DbType.Binary:
                case DbType.Object:
                    return "BLOB";

                case DbType.Boolean:
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.VarNumeric:
                    return "NUMERIC";

                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                case DbType.Time:
                    return "DATETIME";

                case DbType.Byte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.SByte:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return "INTEGER";

                case DbType.Double:
                case DbType.Single:
                    return "REAL";
            }
            throw new ArgumentOutOfRangeException(type.ToString());
        }

        public IsolationLevel GetSupportedIsolationLevel(IsolationLevel il)
        {
            if (il <= IsolationLevel.ReadCommitted) return IsolationLevel.ReadCommitted;
            return IsolationLevel.Serializable;
        }

        public string UseIndex(string index)
        {
            return string.Empty;
        }
    }
}