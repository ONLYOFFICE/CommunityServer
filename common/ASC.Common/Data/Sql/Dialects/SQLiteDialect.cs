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