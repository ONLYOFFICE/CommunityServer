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


using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ASC.Data.Backup.Tasks.Data
{
    internal enum InsertMethod
    {
        None,
        Insert,
        Replace,
        Ignore
    }

    internal enum IdType
    {
        Autoincrement,
        Guid,
        Integer
    }

    [DebuggerDisplay("{Name}")]
    internal class TableInfo
    {
        public string Name { get; private set; }
        public string IdColumn { get; private set; }
        public IdType IdType { get; private set; }
        public string TenantColumn { get; private set; }
        public string[] Columns { get; set; }
        public string[] UserIDColumns { get; set; }
        public Dictionary<string, bool> DateColumns { get; set; }
        public InsertMethod InsertMethod { get; set; }

        public TableInfo(string name, string tenantColumn = null, string idColumn = null, IdType idType = IdType.Autoincrement)
        {
            Name = name;
            IdColumn = idColumn;
            IdType = idType;
            TenantColumn = tenantColumn;
            UserIDColumns = new string[0];
            DateColumns = new Dictionary<string, bool>();
            InsertMethod = InsertMethod.Insert;
        }

        public bool HasIdColumn()
        {
            return !string.IsNullOrEmpty(IdColumn);
        }

        public bool HasDateColumns()
        {
            return DateColumns.Any();
        }

        public bool HasTenantColumn()
        {
            return !string.IsNullOrEmpty(TenantColumn);
        }

        public override string ToString()
        {
            return string.Format("{0} {1} [{2} ({3}), {4}]", InsertMethod, Name, IdColumn, IdType, TenantColumn);
        }
    }
}
