/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Data;

namespace ASC.Common.Data.Sql.Dialects
{
    class MySQLDialect : ISqlDialect
    {
        public string IdentityQuery
        {
            get { return "last_insert_id()"; }
        }

        public string Autoincrement
        {
            get { return "auto_increment"; }
        }

        public virtual string InsertIgnore
        {
            get { return "insert ignore"; }
        }

        public bool SupportMultiTableUpdate
        {
            get { return true; }
        }

        public bool SeparateCreateIndex
        {
            get { return false; }
        }

        public string DbTypeToString(DbType type, int size, int precision)
        {
            switch (type)
            {
                case DbType.Guid:
                    return "char(38)";

                case DbType.AnsiString:
                case DbType.String:
                    if (size <= 8192) return string.Format("VARCHAR({0})", size);
                    else if (8192 < size && size <= UInt16.MaxValue) return "TEXT";
                    else if (UInt16.MaxValue < size && size <= ((int)Math.Pow(2, 24) - 1)) return "MEDIUMTEXT";
                    else return "LONGTEXT";

                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return string.Format("CHAR({0})", size);

                case DbType.Xml:
                    return "MEDIUMTEXT";

                case DbType.Binary:
                case DbType.Object:
                    if (size <= 8192) return string.Format("BINARY({0})", size);
                    else if (8192 < size && size <= UInt16.MaxValue) return "BLOB";
                    else if (UInt16.MaxValue < size && size <= ((int)Math.Pow(2, 24) - 1)) return "MEDIUMBLOB";
                    else return "LONGBLOB";

                case DbType.Boolean:
                case DbType.Byte:
                    return "TINYINY";
                case DbType.SByte:
                    return "TINYINY UNSIGNED";

                case DbType.Int16:
                    return "SMALLINT";
                case DbType.UInt16:
                    return "SMALLINT UNSIGNED";

                case DbType.Int32:
                    return "INT";
                case DbType.UInt32:
                    return "INT UNSIGNED";

                case DbType.Int64:
                    return "BIGINT";
                case DbType.UInt64:
                    return "BIGINT UNSIGNED";

                case DbType.Date:
                    return "DATE";
                case DbType.DateTime:
                case DbType.DateTime2:
                    return "DATETIME";
                case DbType.Time:
                    return "TIME";

                case DbType.Decimal:
                    return string.Format("DECIMAL({0},{1})", size, precision);
                case DbType.Double:
                    return "DOUBLE";
                case DbType.Single:
                    return "FLOAT";

                default:
                    throw new ArgumentOutOfRangeException(type.ToString());
            }
        }

        public IsolationLevel GetSupportedIsolationLevel(IsolationLevel il)
        {
            return il;
        }

        public string UseIndex(string index)
        {
            return string.Format("use index ({0})", index);
        }
    }
}