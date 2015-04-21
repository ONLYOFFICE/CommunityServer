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


using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ASC.Common.Data.Sql
{
    public abstract class SqlCreate : ISqlInstruction
    {
        protected string Name
        {
            get;
            private set;
        }


        protected SqlCreate(string name)
        {
            Name = name;
        }


        public abstract string ToString(ISqlDialect dialect);

        public object[] GetParameters()
        {
            return new object[0];
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }


        public class Table : SqlCreate
        {
            private bool ifNotExists;
            private List<string> primaryKey = new List<string>();
            private List<Column> columns = new List<Column>();
            private List<Index> indexes = new List<Index>();


            public Table(string name)
                : base(name)
            {

            }

            public Table(string name, bool ifNotExists)
                : this(name)
            {
                IfNotExists(ifNotExists);
            }

            public Table IfNotExists(bool ifNotExists)
            {
                this.ifNotExists = ifNotExists;
                return this;
            }

            public Table AddColumn(Column column)
            {
                this.columns.Add(column);
                return this;
            }

            public Table AddColumn(string name, DbType type)
            {
                AddColumn(name, type, 0, false);
                return this;
            }

            public Table AddColumn(string name, DbType type, int size)
            {
                AddColumn(name, type, size, false);
                return this;
            }

            public Table AddColumn(string name, DbType type, bool notNull)
            {
                AddColumn(name, type, 0, notNull);
                return this;
            }

            public Table AddColumn(string name, DbType type, int size, bool notNull)
            {
                AddColumn(new Column(name, type, size).NotNull(notNull));
                return this;
            }

            public Table PrimaryKey(params string[] columns)
            {
                primaryKey.AddRange(columns);
                return this;
            }

            public Table AddIndex(Index index)
            {
                index.IfNotExists(ifNotExists);
                this.indexes.Add(index);
                return this;
            }

            public Table AddIndex(string name, params string[] columns)
            {
                AddIndex(new Index(name, Name, columns));
                return this;
            }

            public override string ToString(ISqlDialect dialect)
            {
                var sql = new StringBuilder("create table ");
                if (ifNotExists) sql.Append("if not exists ");
                sql.AppendFormat("{0} (", Name);

                foreach (var c in columns)
                {
                    sql.AppendFormat("{0}, ", c.ToString(dialect));
                }
                if (0 < primaryKey.Count)
                {
                    sql.Append("primary key (");
                    foreach (var c in primaryKey)
                    {
                        sql.AppendFormat("{0}, ", c);
                    }
                    sql.Replace(", ", "), ", sql.Length - 2, 2);
                }
                if (dialect.SeparateCreateIndex)
                {
                    sql.Replace(", ", ");", sql.Length - 2, 2);
                    sql.AppendLine();
                    indexes.ForEach(i => sql.AppendLine(i.ToString(dialect)));
                }
                else
                {
                    foreach (var i in indexes)
                    {
                        sql.AppendFormat("index {0}{1} (", i.unique ? "unique " : string.Empty, i.Name);
                        foreach (var c in i.columns)
                        {
                            sql.AppendFormat("{0}, ", c);
                        }
                        sql.Replace(", ", "), ", sql.Length - 2, 2);
                    }
                    sql.Replace(", ", ");", sql.Length - 2, 2);
                }

                return sql.ToString();
            }
        }

        public class Column : SqlCreate
        {
            private DbType type;
            private int size;
            private int precision;
            private bool notNull;
            private bool primaryKey;
            private bool autoinc;
            private object defaultValue;


            public Column(string name, DbType type)
                : this(name, type, 0, 0)
            {
                this.type = type;
            }

            public Column(string name, DbType type, int size)
                : this(name, type, size, 0)
            {
                this.type = type;
            }

            public Column(string name, DbType type, int size, int precision)
                : base(name)
            {
                this.type = type;
                this.size = size;
                this.precision = precision;
            }


            public Column NotNull(bool notNull)
            {
                this.notNull = notNull;
                return this;
            }

            public Column Autoincrement(bool autoincrement)
            {
                this.autoinc = autoincrement;
                return this;
            }

            public Column PrimaryKey(bool primaryKey)
            {
                this.primaryKey = primaryKey;
                return this;
            }

            public Column Default(object value)
            {
                this.defaultValue = value;
                return this;
            }

            public override string ToString(ISqlDialect dialect)
            {
                var sql = new StringBuilder().AppendFormat("{0} {1} {2} ", Name, dialect.DbTypeToString(type, size, precision), notNull ? "not null" : "null");
                if (defaultValue != null) sql.AppendFormat("default {0} ", defaultValue);
                if (primaryKey) sql.Append("primary key ");
                if (autoinc) sql.Append(dialect.Autoincrement);

                return sql.ToString().Trim();
            }
        }


        public class Index : SqlCreate
        {
            private string table;
            internal string[] columns;
            internal bool unique;
            private bool ifNotExists;


            public Index(string name, string table, params string[] columns)
                : base(name)
            {
                this.table = table;
                this.columns = columns ?? new string[0];
            }

            public Index Unique(bool unique)
            {
                this.unique = unique;
                return this;
            }

            public Index IfNotExists(bool ifNotExists)
            {
                this.ifNotExists = ifNotExists;
                return this;
            }

            public override string ToString(ISqlDialect dialect)
            {
                var sql = new StringBuilder("create ");
                if (unique) sql.Append("unique ");
                sql.Append("index ");
                if (ifNotExists) sql.Append("if not exists ");
                sql.AppendFormat("{0} on {1} (", Name, table);

                foreach (var c in columns)
                {
                    sql.AppendFormat("{0}, ", c);
                }

                return sql.Replace(", ", ");", sql.Length - 2, 2).ToString();
            }
        }
    }
}
