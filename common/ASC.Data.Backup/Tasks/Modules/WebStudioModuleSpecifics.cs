/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, RelationInfo relation, ref object value)
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
