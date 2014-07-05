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
using System.Linq;
using System.Text.RegularExpressions;
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class CrmModuleSpecifics : ModuleSpecificsBase
    {
        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("crm_case", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modifed_by"},
                        DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modifed_on", false}}
                    },
                new TableInfo("crm_contact", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modifed_by"},
                        DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modifed_on", false}}
                    },
                new TableInfo("crm_contact_info", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"last_modifed_by"},
                        DateColumns = new Dictionary<string, bool> {{"last_modifed_on", false}}
                    },
                new TableInfo("crm_deal", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modifed_by", "responsible_id"},
                        DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modifed_on", false}, {"expected_close_date", true}, {"actual_close_date", true}}
                    },
                new TableInfo("crm_deal_milestone", "tenant_id", "id"),
                new TableInfo("crm_field_description", "tenant_id", "id"),
                new TableInfo("crm_list_item", "tenant_id", "id"),
                new TableInfo("crm_projects", "tenant_id"),
                new TableInfo("crm_tag", "tenant_id", "id"),
                new TableInfo("crm_task", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modifed_by", "responsible_id"},
                        DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modifed_on", false}, {"deadline", true}}
                    },
                new TableInfo("crm_task_template", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modifed_by", "responsible_id"},
                        DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modifed_on", false}}
                    },
                new TableInfo("crm_task_template_container", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modifed_by"},
                        DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modifed_on", false}}
                    },
                new TableInfo("crm_task_template_task", "tenant_id"),
                new TableInfo("crm_currency_rate", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modified_by"},
                        DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modified_on", false}}
                    }
            };

        private readonly RelationInfo[] _tableRelations = new[]
            {
                new RelationInfo("crm_contact", "id", "crm_contact", "company_id"),
                new RelationInfo("crm_list_item", "id", "crm_contact", "status_id"),
                new RelationInfo("crm_list_item", "id", "crm_contact", "contact_type_id"),  
                new RelationInfo("crm_contact", "id", "crm_contact_info", "contact_id"),
                new RelationInfo("crm_deal_milestone", "id", "crm_deal", "deal_milestone_id"),
                new RelationInfo("crm_contact", "id", "crm_deal", "contact_id"), 
                new RelationInfo("projects_projects", "id", "crm_projects", "project_id", typeof(ProjectsModuleSpecifics)),
                new RelationInfo("crm_contact", "id", "crm_projects", "contact_id"),
                new RelationInfo("crm_contact", "id", "crm_task", "entity_id", x => ResolveRelation(x, 0, 4, 5)),
                new RelationInfo("crm_deal", "id", "crm_task", "entity_id", x => ResolveRelation(x, 1)),
                new RelationInfo("crm_task", "id", "crm_task", "entity_id", x => ResolveRelation(x, 3)),
                new RelationInfo("crm_case", "id", "crm_task", "entity_id", x => ResolveRelation(x, 7)),
                new RelationInfo("crm_contact", "id", "crm_task", "contact_id"),
                new RelationInfo("crm_list_item", "id", "crm_task", "category_id"),
                new RelationInfo("crm_list_item", "id", "crm_task_template", "category_id"),
                new RelationInfo("crm_task_template_container", "id", "crm_task_template", "container_id"),
                new RelationInfo("crm_task", "id", "crm_task_template_task", "task_id"),
                new RelationInfo("crm_task_template", "id", "crm_task_template_task", "task_template_id")
            };

        public override ModuleName ModuleName
        {
            get { return ModuleName.Crm; }
        }

        public override IEnumerable<TableInfo> Tables
        {
            get { return _tables; }
        }

        public override IEnumerable<RelationInfo> TableRelations
        {
            get { return _tableRelations; }
        }

        public override bool TryAdjustFilePath(ColumnMapper columnMapper, ref string filePath)
        {
            var pathMatch = Regex.Match(filePath, @"^photos/\d+/\d+/\d+/contact_(?'contactId'\d+)(?'sizeExtension'_\d+_\d+\.\w+)$", RegexOptions.Compiled);
            if (pathMatch.Success)
            {
                var contactId = columnMapper.GetMapping("crm_contact", "id", pathMatch.Groups["contactId"].Value);
                if (contactId == null)
                    return false;

                var s = contactId.ToString().PadLeft(6, '0');
                filePath = string.Format("photos/{0}/{1}/{2}/contact_{3}{4}", s.Substring(0, 2), s.Substring(2, 2), s.Substring(4), contactId, pathMatch.Groups["sizeExtension"].Value);
                return true;
            }

            return false;
        }

        private static bool ResolveRelation(DataRowInfo row, params int[] matchingTypes)
        {
            var entityType = Convert.ToInt32(row["entity_type"]);
            return matchingTypes.Contains(entityType);
        }
    }

    //todo: hack: in future there be no modules only tables!!!
    internal class CrmModuleSpecifics2 : ModuleSpecificsBase
    {
        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("crm_field_value", "tenant_id") {UserIDColumns = new[] {"last_modifed_by"}},
                new TableInfo("crm_entity_contact"),
                new TableInfo("crm_entity_tag"),
                new TableInfo("crm_relationship_event", "tenant_id", "id") {UserIDColumns = new[] {"create_by", "last_modifed_by"}},
            };

        private readonly RelationInfo[] _tableRelations = new[]
            {
                new RelationInfo("crm_contact", "id", "crm_field_value", "entity_id", x => ResolveRelation(x, 0, 4, 5)),
                new RelationInfo("crm_deal", "id", "crm_field_value", "entity_id", x => ResolveRelation(x, 1)),
                new RelationInfo("crm_task", "id", "crm_field_value", "entity_id", x => ResolveRelation(x, 3)),
                new RelationInfo("crm_case", "id", "crm_field_value", "entity_id", x => ResolveRelation(x, 7)),
                new RelationInfo("crm_field_description", "id", "crm_field_value", "field_id"),
                new RelationInfo("crm_contact", "id", "crm_entity_contact", "entity_id", x => ResolveRelation(x, 0, 4, 5)),
                new RelationInfo("crm_deal", "id", "crm_entity_contact", "entity_id", x => ResolveRelation(x, 1)),
                new RelationInfo("crm_task", "id", "crm_entity_contact", "entity_id", x => ResolveRelation(x, 3)),
                new RelationInfo("crm_case", "id", "crm_entity_contact", "entity_id", x => ResolveRelation(x, 7)),
                new RelationInfo("crm_contact", "id", "crm_entity_contact", "contact_id"),
                new RelationInfo("crm_contact", "id", "crm_entity_tag", "entity_id", x => ResolveRelation(x, 0, 4, 5)),
                new RelationInfo("crm_deal", "id", "crm_entity_tag", "entity_id", x => ResolveRelation(x, 1)),
                new RelationInfo("crm_task", "id", "crm_entity_tag", "entity_id", x => ResolveRelation(x, 3)),
                new RelationInfo("crm_case", "id", "crm_entity_tag", "entity_id", x => ResolveRelation(x, 7)),
                new RelationInfo("crm_tag", "id", "crm_entity_tag", "tag_id"),
                new RelationInfo("crm_contact", "id", "crm_relationship_event", "entity_id", typeof(CrmModuleSpecifics), x => ResolveRelation(x, 0, 4, 5)),
                new RelationInfo("crm_deal", "id", "crm_relationship_event", "entity_id", typeof(CrmModuleSpecifics), x => ResolveRelation(x, 1)),
                new RelationInfo("crm_relationship_event", "id", "crm_relationship_event", "entity_id", typeof(CrmModuleSpecifics), x => ResolveRelation(x, 2)),
                new RelationInfo("crm_task", "id", "crm_relationship_event", "entity_id", typeof(CrmModuleSpecifics), x => ResolveRelation(x, 3)),
                new RelationInfo("crm_case", "id", "crm_relationship_event", "entity_id", typeof(CrmModuleSpecifics), x => ResolveRelation(x, 7)),
                new RelationInfo("crm_contact", "id", "crm_relationship_event", "contact_id", typeof(CrmModuleSpecifics)),
                new RelationInfo("crm_list_item", "id", "crm_relationship_event", "category_id", typeof(CrmModuleSpecifics)),
                new RelationInfo("crm_relationship_event", "id", "crm_field_value", "entity_id", typeof(CrmModuleSpecifics), x => ResolveRelation(x, 2)),
                new RelationInfo("crm_relationship_event", "id", "crm_entity_tag", "entity_id", typeof(CrmModuleSpecifics), x => ResolveRelation(x, 2)),
                new RelationInfo("crm_relationship_event", "id", "crm_entity_contact", "entity_id", typeof(CrmModuleSpecifics), x => ResolveRelation(x, 2)),
                new RelationInfo("mail_mail", "id", "crm_relationship_event", "content", typeof(MailModuleSpecifics), x => Convert.ToInt32(x["category_id"]) == -3), 
            };

        public override string ConnectionStringName
        {
            get { return "crm"; }
        }

        public override ModuleName ModuleName
        {
            get { return ModuleName.Crm2; }
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
            if (table.Name == "crm_entity_contact")
                return "inner join crm_contact as t1 on t1.id = t.contact_id where t1.tenant_id = " + tenantId;

            if (table.Name == "crm_entity_tag")
                return "inner join crm_tag as t1 on t1.id = t.tag_id where t1.tenant_id = " + tenantId;

            return base.GetSelectCommandConditionText(tenantId, table);
        }

        public override bool TryAdjustFilePath(ColumnMapper columnMapper, ref string filePath)
        {
            var match = Regex.Match(filePath, @"(?<=folder_\d+/message_)\d+(?=\.html)"); //todo:
            if (match.Success)
            {
                var mappedMessageId = Convert.ToString(columnMapper.GetMapping("mail_mail", "id", match.Value));
                if (string.IsNullOrEmpty(mappedMessageId))
                    return false;
                filePath = string.Format("folder_{0}/message_{1}.html", (Convert.ToInt32(mappedMessageId)/1000 + 1)*1000, mappedMessageId);
                return true;
            }
            return base.TryAdjustFilePath(columnMapper, ref filePath);
        }

        protected override bool TryPrepareValue(System.Data.IDbConnection connection, ColumnMapper columnMapper, RelationInfo relation, ref object value)
        {
            if (relation.ChildTable == "crm_relationship_event" && relation.ChildColumn == "content")
            {
                bool success = true;
                value = Regex.Replace(
                    Convert.ToString(value),
                    @"(?<=""message_id"":|/products/crm/httphandlers/filehandler\.ashx\?action=mailmessage&message_id=)\d+",
                    match =>
                        {
                            var mappedMessageId = Convert.ToString(columnMapper.GetMapping(relation.ParentTable, relation.ParentColumn, match.Value));
                            success = !string.IsNullOrEmpty(mappedMessageId);
                            return mappedMessageId;
                        });

                return success;
            }
            return base.TryPrepareValue(connection, columnMapper, relation, ref value);
        }

        private static bool ResolveRelation(DataRowInfo row, params int[] matchingTypes)
        {
            var entityType = Convert.ToInt32(row["entity_type"]);
            return matchingTypes.Contains(entityType);
        }
    }

    internal class CrmInvoiceModuleSpecifics : ModuleSpecificsBase
    {
        public override ModuleName ModuleName
        {
            get { return ModuleName.CrmInvoice; }
        }

        public override string ConnectionStringName
        {
            get { return "crm"; }
        }

        public override IEnumerable<TableInfo> Tables
        {
            get
            {
                return new List<TableInfo>
                    {

                        new TableInfo("crm_invoice", "tenant_id", "id")
                            {
                                UserIDColumns = new[] {"create_by", "last_modified_by"},
                                DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modified_on", false}, {"due_date", false}, {"issue_date", false}}
                            },
                        new TableInfo("crm_invoice_item", "tenant_id", "id")
                            {
                                UserIDColumns = new[] {"create_by", "last_modified_by"},
                                DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modified_on", false}}
                            },
                        new TableInfo("crm_invoice_line", "tenant_id", "id"),
                        new TableInfo("crm_invoice_tax", "tenant_id", "id")
                            {
                                UserIDColumns = new[] {"create_by", "last_modified_by"},
                                DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modified_on", false}}
                            },
                        new TableInfo("crm_organisation_logo", "tenant_id", "id")
                            {
                                UserIDColumns = new[] {"create_by"},
                                DateColumns = new Dictionary<string, bool> {{"create_on", false}}
                            }
                    };
            }
        }

        public override IEnumerable<RelationInfo> TableRelations
        {
            get
            {
                return new List<RelationInfo>
                    {
                        new RelationInfo("crm_contact", "id", "crm_invoice", "contact_id"),
                        new RelationInfo("crm_contact", "id", "crm_invoice", "consignee_id"),
                        new RelationInfo("crm_contact", "id", "crm_invoice", "entity_id", x => ResolveRelation(x, 0, 4, 5)),
                        new RelationInfo("crm_deal", "id", "crm_invoice", "entity_id", x => ResolveRelation(x, 1)),
                        new RelationInfo("crm_task", "id", "crm_invoice", "entity_id", x => ResolveRelation(x, 3)),
                        new RelationInfo("crm_case", "id", "crm_invoice", "entity_id", x => ResolveRelation(x, 7)),
                        new RelationInfo("files_file", "id", "crm_invoice", "file_id", typeof(FilesModuleSpecifics)),
                        new RelationInfo("crm_invoice_tax", "id", "crm_invoice_item", "invoice_tax1_id"),
                        new RelationInfo("crm_invoice_tax", "id", "crm_invoice_item", "invoice_tax2_id"),
                        new RelationInfo("crm_invoice_tax", "id", "crm_invoice_line", "invoice_tax1_id"),
                        new RelationInfo("crm_invoice_tax", "id", "crm_invoice_line", "invoice_tax2_id"),
                        new RelationInfo("crm_invoice", "id", "crm_invoice_line", "invoice_id"),
                        new RelationInfo("crm_invoice_item", "id", "crm_invoice_line", "invoice_item_id"),
                    };
            }
        }

        private static bool ResolveRelation(DataRowInfo row, params int[] matchingTypes)
        {
            var entityType = Convert.ToInt32(row["entity_type"]);
            return matchingTypes.Contains(entityType);
        }
    }
}
