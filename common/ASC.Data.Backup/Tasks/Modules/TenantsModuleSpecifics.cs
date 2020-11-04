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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using ASC.Core.Tenants;
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class TenantsModuleSpecifics : ModuleSpecificsBase
    {
        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("tenants_quota", "tenant"),
                new TableInfo("tenants_tariff", "tenant", "id"),
                new TableInfo("tenants_tenants", "id", "id")
                    {
                        DateColumns = new Dictionary<string, bool> {{"creationdatetime", false}, {"statuschanged", false}, {"version_changed", false}}
                    },
                new TableInfo("tenants_quotarow", "tenant") {InsertMethod = InsertMethod.Replace},
                new TableInfo("tenants_partners", "tenant_id"),
                new TableInfo("core_user", "tenant", "id", IdType.Guid)
                    {
                        DateColumns = new Dictionary<string, bool> {{"workfromdate", false}, {"terminateddate", false}, {"last_modified", false}}
                    },
                new TableInfo("core_group", "tenant", "id", IdType.Guid),
                new TableInfo("tenants_iprestrictions", "tenant", "id", IdType.Autoincrement)
            };

        private readonly RelationInfo[] _tableRelations = new[]
            {
                new RelationInfo("tenants_tenants", "id", "tenants_quota", "tenant"),
                new RelationInfo("tenants_tenants", "id", "tenants_tariff", "tenant"),
                new RelationInfo("tenants_tenants", "id", "tenants_tariff", "tariff", x => Convert.ToInt32(x["tariff"]) > 0),
                new RelationInfo("tenants_tenants", "id", "tenants_partners", "tenant_id"),
                new RelationInfo("core_user", "id", "tenants_tenants", "owner_id", null, null, RelationImportance.Low)
            };

        public override string ConnectionStringName
        {
            get { return "core"; }
        }

        public override ModuleName ModuleName
        {
            get { return ModuleName.Tenants; }
        }

        public override IEnumerable<TableInfo> Tables
        {
            get { return _tables; }
        }

        public override IEnumerable<RelationInfo> TableRelations
        {
            get { return _tableRelations; }
        }

        protected override bool TryPrepareRow(bool dump, DbConnection connection, ColumnMapper columnMapper, TableInfo table, DataRowInfo row, out Dictionary<string, object> preparedRow)
        {
            if (table.Name == "tenants_tenants" && string.IsNullOrEmpty(Convert.ToString(row["payment_id"])))
            {
                var oldTenantID = Convert.ToInt32(row["id"]);
                columnMapper.SetMapping("tenants_tenants", "payment_id", row["payment_id"], Core.CoreContext.Configuration.GetKey(oldTenantID));
            }
            return base.TryPrepareRow(dump, connection, columnMapper, table, row, out preparedRow);
        }

        protected override bool TryPrepareValue(DbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, ref object value)
        {
            //we insert tenant as suspended so it can't be accessed before restore operation is finished
            if (table.Name.Equals("tenants_tenants", StringComparison.InvariantCultureIgnoreCase) && 
                columnName.Equals("status", StringComparison.InvariantCultureIgnoreCase))
            {
                value = (int)TenantStatus.Restoring;
                return true;
            }

            if (table.Name.Equals("tenants_tenants", StringComparison.InvariantCultureIgnoreCase) &&
                columnName.Equals("last_modified", StringComparison.InvariantCultureIgnoreCase))
            {
                value = DateTime.UtcNow;
                return true;
            }

            if (table.Name.Equals("tenants_quotarow", StringComparison.InvariantCultureIgnoreCase) &&
                columnName.Equals("last_modified", StringComparison.InvariantCultureIgnoreCase))
            {
                value = DateTime.UtcNow;
                return true;
            }

            if ((table.Name == "core_user" || table.Name == "core_group") && columnName == "last_modified")
            {
                value = DateTime.UtcNow.AddMinutes(2);
                return true;
            }

            return base.TryPrepareValue(connection, columnMapper, table, columnName, ref value);
        }
    }
}
