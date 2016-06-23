/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Data;
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

        protected override bool TryPrepareRow(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, DataRowInfo row, out Dictionary<string, object> preparedRow)
        {
            if (table.Name == "tenants_tenants" && string.IsNullOrEmpty(Convert.ToString(row["payment_id"])))
            {
                var oldTenantID = Convert.ToInt32(row["id"]);
                columnMapper.SetMapping("tenants_tenants", "payment_id", row["payment_id"], Core.CoreContext.Configuration.GetKey(oldTenantID));
            }
            return base.TryPrepareRow(connection, columnMapper, table, row, out preparedRow);
        }

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, ref object value)
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
