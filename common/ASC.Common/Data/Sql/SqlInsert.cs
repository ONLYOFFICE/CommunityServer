/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ASC.Common.Data.Sql
{
    [DebuggerTypeProxy(typeof(SqlDebugView))]
    public class SqlInsert : ISqlInstruction
    {
        private readonly List<string> columns = new List<string>();
        private readonly string table;
        private readonly List<object> values = new List<object>();
        private int identityPosition = -1;
        private object nullValue;
        private SqlQuery query;
        private bool replaceExists;
        private bool ignoreExists;
        private bool returnIdentity;


        public SqlInsert(string table)
            : this(table, false)
        {
        }

        public SqlInsert(string table, bool replaceExists)
        {
            this.table = table;
            ReplaceExists(replaceExists);
        }


        public string ToString(ISqlDialect dialect)
        {
            var sql = new StringBuilder();

            if (ignoreExists)
            {
                sql.Append(dialect.InsertIgnore);
            }
            else
            {
                sql.Append(replaceExists ? "replace" : "insert");
            }
            sql.AppendFormat(" into {0}", table);
            bool identityInsert = IsIdentityInsert();
            if (0 < columns.Count)
            {
                sql.Append("(");
                for (int i = 0; i < columns.Count; i++)
                {
                    if (identityInsert && identityPosition == i) continue;
                    sql.AppendFormat("{0},", columns[i]);
                }
                sql.Remove(sql.Length - 1, 1).Append(")");
            }
            if (query != null)
            {
                sql.AppendFormat(" {0}", query.ToString(dialect));
                return sql.ToString();
            }
            sql.Append(" values (");
            for (int i = 0; i < values.Count; i++)
            {
                if (identityInsert && identityPosition == i)
                {
                    continue;
                }
                sql.Append("?");
                if (i + 1 == values.Count)
                {
                    sql.Append(")");
                }
                else if (0 < columns.Count && (i + 1) % columns.Count == 0)
                {
                    sql.Append("),(");
                }
                else
                {
                    sql.Append(",");
                }
            }

            if (returnIdentity)
            {
                sql.AppendFormat("; select {0}", identityInsert ? dialect.IdentityQuery : "?");
            }
            return sql.ToString();
        }

        public object[] GetParameters()
        {
            if (query != null)
            {
                return query.GetParameters();
            }
            var copy = new List<object>(values);
            if (IsIdentityInsert())
            {
                copy.RemoveAt(identityPosition);
            }
            else if (returnIdentity)
            {
                copy.Add(copy[identityPosition]);
            }
            return copy.ToArray();
        }


        public SqlInsert InColumns(params string[] columns)
        {
            this.columns.AddRange(columns);
            return this;
        }

        public SqlInsert Values(params object[] values)
        {
            this.values.AddRange(values);
            return this;
        }

        public SqlInsert Values(SqlQuery query)
        {
            this.query = query;
            return this;
        }

        public SqlInsert InColumnValue(string column, object value)
        {
            return InColumns(column).Values(value);
        }

        public SqlInsert ReplaceExists(bool replaceExists)
        {
            this.replaceExists = replaceExists;
            return this;
        }

        public SqlInsert IgnoreExists(bool ignoreExists)
        {
            this.ignoreExists = ignoreExists;
            return this;
        }

        public SqlInsert Identity<TIdentity>(int position, TIdentity nullValue)
        {
            return Identity(position, nullValue, false);
        }

        public SqlInsert Identity<TIdentity>(int position, TIdentity nullValue, bool returnIdentity)
        {
            identityPosition = position;
            this.nullValue = nullValue;
            this.returnIdentity = returnIdentity;
            return this;
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }

        private bool IsIdentityInsert()
        {
            if (identityPosition < 0) return false;
            if (values[identityPosition] != null && nullValue != null &&
                values[identityPosition].GetType() != nullValue.GetType())
            {
                throw new InvalidCastException(string.Format("Identity null value must be {0} type.",
                                                             values[identityPosition].GetType()));
            }
            return Equals(values[identityPosition], nullValue);
        }
    }
}