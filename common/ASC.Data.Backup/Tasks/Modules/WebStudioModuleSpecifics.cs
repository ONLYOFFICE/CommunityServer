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
using System.Text.RegularExpressions;
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class WebStudioModuleSpecifics : ModuleSpecificsBase
    {
        private static readonly Guid CrmSettingsId = new Guid("fdf39b9a-ec96-4eb7-aeab-63f2c608eada");

        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("webstudio_fckuploads", "TenantID") {InsertMethod = InsertMethod.None},
                new TableInfo("webstudio_settings", "TenantID") {UserIDColumns = new[] {"UserID"}},
                new TableInfo("webstudio_uservisit", "tenantid") {InsertMethod = InsertMethod.None}
            };

        private readonly RelationInfo[] _relations = new[]
            {
                new RelationInfo("crm_organisation_logo", "id", "webstudio_settings", "Data", typeof(CrmInvoiceModuleSpecifics),
                                 x => CrmSettingsId == new Guid(Convert.ToString(x["ID"])))
            };

        public override ModuleName ModuleName
        {
            get { return ModuleName.WebStudio; }
        }

        public override IEnumerable<TableInfo> Tables
        {
            get { return _tables; }
        }

        public override IEnumerable<RelationInfo> TableRelations
        {
            get { return _relations; }
        }

        protected override bool TryPrepareValue(DbConnection connection, ColumnMapper columnMapper, RelationInfo relation, ref object value)
        {
            if (relation.ParentTable == "crm_organisation_logo")
            {
                bool success = true;
                value = Regex.Replace(
                    Convert.ToString(value),
                    @"(?<=""CompanyLogoID"":)\d+",
                    match =>
                        {
                            if (Convert.ToInt32(match.Value) == 0)
                            {
                                success = true;
                                return match.Value;
                            }
                            var mappedMessageId = Convert.ToString(columnMapper.GetMapping(relation.ParentTable, relation.ParentColumn, match.Value));
                            success = !string.IsNullOrEmpty(mappedMessageId);
                            return mappedMessageId;
                        });
                return success;
            }
            return base.TryPrepareValue(connection, columnMapper, relation, ref value);
        }
    }
}
