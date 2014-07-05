/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class MailModuleSpecifics : ModuleSpecificsBase
    {
        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("mail_alerts", "tenant", "id") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_attachment", "tenant", "id"),
                new TableInfo("mail_chain", "tenant") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_contacts", "tenant", "id") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_folder", "tenant") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_mail", "tenant", "id")
                    {
                        UserIDColumns = new[] {"id_user"},
                        DateColumns = new Dictionary<string, bool> {{"date_received", false}, {"date_sent", false}, {"chain_date", false}}
                    },
                new TableInfo("mail_mailbox", "tenant", "id")
                    {
                        UserIDColumns = new[] {"id_user"},
                        DateColumns = new Dictionary<string, bool> {{"begin_date", false}}
                    },
                new TableInfo("mail_tag", "tenant", "id") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_tag_addresses", "tenant"),
                new TableInfo("mail_tag_mail", "tenant") {UserIDColumns = new[] {"id_user"}},
                new TableInfo("mail_chain_x_crm_entity", "id_tenant")
            };

        private readonly RelationInfo[] _tableRelations = new[]
            {
                new RelationInfo("mail_mail", "id", "mail_attachment", "id_mail"),
                new RelationInfo("mail_mailbox", "id", "mail_chain", "id_mailbox"),
                new RelationInfo("mail_tag", "id", "mail_chain", "tags"),
                new RelationInfo("crm_tag", "id", "mail_chain", "tags", typeof(CrmModuleSpecifics)),
                new RelationInfo("mail_mailbox", "id", "mail_mail", "id_mailbox"),
                new RelationInfo("crm_tag", "id", "mail_tag", "crm_id", typeof(CrmModuleSpecifics)),
                new RelationInfo("mail_tag", "id", "mail_tag_addresses", "id_tag", x => Convert.ToInt32(x["id_tag"]) > 0),
                new RelationInfo("crm_tag", "id", "mail_tag_addresses", "id_tag", typeof(CrmModuleSpecifics), x => Convert.ToInt32(x["id_tag"]) < 0),
                new RelationInfo("mail_mail", "id", "mail_tag_mail", "id_mail"),
                new RelationInfo("mail_tag", "id", "mail_tag_mail", "id_tag", x => Convert.ToInt32(x["id_tag"]) > 0),
                new RelationInfo("crm_tag", "id", "mail_tag_mail", "id_tag", typeof(CrmModuleSpecifics), x => Convert.ToInt32(x["id_tag"]) < 0),
                new RelationInfo("mail_mailbox", "id", "mail_chain_x_crm_entity", "id_mailbox"),
                new RelationInfo("crm_contact", "id", "mail_chain_x_crm_entity", "entity_id", typeof(CrmModuleSpecifics), x => Convert.ToInt32(x["entity_type"]) == 1),
                new RelationInfo("crm_case", "id", "mail_chain_x_crm_entity", "entity_id", typeof(CrmModuleSpecifics), x => Convert.ToInt32(x["entity_type"]) == 2),
                new RelationInfo("crm_deal", "id", "mail_chain_x_crm_entity", "entity_id", typeof(CrmModuleSpecifics), x => Convert.ToInt32(x["entity_type"]) == 3),
            };

        public override ModuleName ModuleName
        {
            get { return ModuleName.Mail; }
        }

        public override IEnumerable<TableInfo> Tables
        {
            get { return _tables; }
        }

        public override IEnumerable<RelationInfo> TableRelations
        {
            get { return _tableRelations; }
        }

        protected override string GetSelectCommandConditionText(int tenantId, TableInfo table)
        {
            //optimization: 1) do not include "deleted" rows, 2) backup mail only for the last 30 days
            switch (table.Name)
            {
                case "mail_mailbox":
                    return string.Format("where t.tenant = {0} and t.is_removed <> 1", tenantId);

                //condition on chain_date because of Bug 18855 - transfer mail only for the last 30 days
                case "mail_mail":
                    return string.Format("inner join mail_mailbox t1 on t1.id = t.id_mailbox " +
                                         "where t.tenant = {0} and t1.is_removed <> 1 and t.chain_date > '{1}'",
                                         tenantId,
                                         DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)).ToString("yyyy-MM-dd HH:mm:ss"));

                case "mail_attachment": case "mail_tag_mail":
                    return string.Format("inner join mail_mail as t1 on t1.id = t.id_mail " +
                                         "inner join mail_mailbox as t2 on t2.id = t1.id_mailbox " +
                                         "where t1.tenant = {0} and t2.is_removed <> 1 and t1.chain_date > '{1}'",
                                         tenantId,
                                         DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)).ToString("yyyy-MM-dd HH:mm:ss"));

                case "mail_chain":
                    return string.Format("inner join mail_mailbox t1 on t1.id = t.id_mailbox " +
                                         "where t.tenant = {0} and t1.is_removed <> 1",
                                         tenantId);

                default:
                    return base.GetSelectCommandConditionText(tenantId, table);
            }
        }

        protected override string GetDeleteCommandConditionText(int tenantId, TableInfo table)
        {
            //delete all rows regardless of whether there is is_removed = 1 or not
            return base.GetSelectCommandConditionText(tenantId, table);
        }

        public override bool TryAdjustFilePath(ColumnMapper columnMapper, ref string filePath)
        {
            //todo: hack: will be changed later
            filePath = Regex.Replace(filePath, @"^[-\w]+(?=/)", match => columnMapper.GetUserMapping(match.Value));
            return !filePath.StartsWith("/");
        }

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, IEnumerable<RelationInfo> relations, ref object value)
        {
            relations = relations.ToList();

            if (relations.All(x => x.ChildTable == "mail_chain" && x.ChildColumn == "tags"))
            {
                var mappedTags = new List<string>();

                foreach (var tag in value.ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)))
                {
                    object tagId;
                    if (tag > 0)
                    {
                        tagId = columnMapper.GetMapping("mail_tag", "id", tag);
                    }
                    else
                    {
                        tagId = columnMapper.GetMapping("crm_tag", "id", -tag);
                        if (tagId != null)
                        {
                            tagId = -Convert.ToInt32(tagId);
                        }
                    }
                    if (tagId != null)
                    {
                        mappedTags.Add(tagId.ToString());
                    }
                }

                value = string.Join(",", mappedTags.ToArray());
                return true;
            }

            return base.TryPrepareValue(connection, columnMapper, table, columnName, relations, ref value);
        }

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, RelationInfo relation, ref object value)
        {
            if (relation.ParentTable == "crm_tag" && relation.ChildColumn == "id_tag"
                && (relation.ChildTable == "mail_tag_mail" || relation.ChildTable == "mail_tag_addresses"))
            {
                var crmTagId = columnMapper.GetMapping(relation.ParentTable, relation.ParentColumn, -Convert.ToInt32(value));
                if (crmTagId == null)
                    return false;

                value = -Convert.ToInt32(crmTagId);
                return true;
            }

            return base.TryPrepareValue(connection, columnMapper, relation, ref value);
        }
    }
}
