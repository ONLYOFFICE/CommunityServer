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
