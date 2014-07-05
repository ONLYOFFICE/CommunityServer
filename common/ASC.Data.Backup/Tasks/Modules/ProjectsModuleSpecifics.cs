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
using System.Text.RegularExpressions;
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class ProjectsModuleSpecifics : ModuleSpecificsBase
    {
        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("projects_comments", "tenant_id", "id", IdType.Guid) {UserIDColumns = new[] {"create_by"}},
                new TableInfo("projects_following_project_participant") {UserIDColumns = new[] {"participant_id"}},
                new TableInfo("projects_messages", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modified_by"},
                        DateColumns = new Dictionary<string, bool> {{"create_on", false}, {"last_modified_on", false}}
                    },
                new TableInfo("projects_milestones", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modified_by", "responsible_id"},
                        DateColumns = new Dictionary<string, bool> {{"deadline", true}, {"status_changed", false}, {"create_on", false}, {"last_modified_on", false}}
                    },
                new TableInfo("projects_projects", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modified_by", "responsible_id"},
                        DateColumns = new Dictionary<string, bool> {{"status_changed", false}, {"create_on", false}, {"last_modified_on", false}}
                    },
                new TableInfo("projects_project_participant", "tenant") {UserIDColumns = new[] {"participant_id"}},
                new TableInfo("projects_project_tag"),
                new TableInfo("projects_report_template", "tenant_id", "id") {UserIDColumns = new[] {"create_by"}},
                new TableInfo("projects_subtasks", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modified_by", "responsible_id"},
                        DateColumns = new Dictionary<string, bool> {{"status_changed", false}, {"create_on", false}, {"last_modified_on", false}}
                    },
                new TableInfo("projects_tags", "tenant_id", "id"),
                new TableInfo("projects_tasks", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "last_modified_by", "responsible_id"},
                        DateColumns = new Dictionary<string, bool> {{"status_changed", true}, {"deadline", true}, {"create_on", false}, {"last_modified_on", true}, {"start_date", true}}
                    },
                new TableInfo("projects_tasks_responsible", "tenant_id") {UserIDColumns = new[] {"responsible_id"}},
                new TableInfo("projects_templates", "tenant_id", "id") {UserIDColumns = new[] {"create_by", "last_modified_by"}},
                new TableInfo("projects_time_tracking", "tenant_id", "id")
                    {
                        UserIDColumns = new[] {"create_by", "person_id"},
                        DateColumns = new Dictionary<string, bool> {{"date", true}, {"create_on", false}, {"status_changed", true}}
                    },
                new TableInfo("projects_tasks_links", "tenant_id"),
                new TableInfo("projects_tasks_order", "tenant_id")
            };

        private readonly RelationInfo[] _tableRelations = new[]
            {
                new RelationInfo("projects_comments", "id", "projects_comments", "parent_id"), 
                new RelationInfo("projects_messages", "id", "projects_comments", "target_uniq_id", x => Convert.ToString(x["target_uniq_id"]).StartsWith("Message_", StringComparison.InvariantCultureIgnoreCase)),
                new RelationInfo("projects_tasks", "id", "projects_comments", "target_uniq_id", x => Convert.ToString(x["target_uniq_id"]).StartsWith("Task_", StringComparison.InvariantCultureIgnoreCase)),
                new RelationInfo("projects_milestones", "id", "projects_comments", "target_uniq_id", x => Convert.ToString(x["target_uniq_id"]).StartsWith("Milestone_", StringComparison.InvariantCultureIgnoreCase)),
                new RelationInfo("projects_projects", "id", "projects_following_project_participant", "project_id"),
                new RelationInfo("projects_projects", "id", "projects_messages", "project_id"),
                new RelationInfo("projects_projects", "id", "projects_milestones", "project_id"),
                new RelationInfo("projects_projects", "id", "projects_project_participant", "project_id"),
                new RelationInfo("projects_projects", "id", "projects_project_tag", "project_id"),
                new RelationInfo("projects_tags", "id", "projects_project_tag", "tag_id"),
                new RelationInfo("projects_tasks", "id", "projects_subtasks", "task_id"),
                new RelationInfo("projects_projects", "id", "projects_tasks", "project_id"),
                new RelationInfo("projects_milestones", "id", "projects_tasks", "milestone_id"),
                new RelationInfo("projects_tasks", "id", "projects_tasks_responsible", "task_id"),
                new RelationInfo("projects_projects", "id", "projects_time_tracking", "project_id"),
                new RelationInfo("projects_tasks", "id", "projects_time_tracking", "relative_task_id"),
                new RelationInfo("projects_tasks", "id", "projects_tasks_links", "task_id"),
                new RelationInfo("projects_tasks", "id", "projects_tasks_links", "parent_id"),
                new RelationInfo("projects_projects", "id", "projects_tasks_order", "project_id"),
                new RelationInfo("projects_tasks", "id", "projects_tasks_order", "task_order"),
                new RelationInfo("projects_milestones", "id", "projects_tasks_order", "task_order") 
            };

        public override ModuleName ModuleName
        {
            get { return ModuleName.Projects; }
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
            var match = Regex.Match(filePath, @"^thumbs/\d+/\d+/\d+/(?'fileId'\d+)\.jpg$");
            if (match.Success)
            {
                var fileId = columnMapper.GetMapping("files_file", "id", match.Groups["fileId"].Value);
                if (fileId == null)
                    return false;
                              
                var s = fileId.ToString().PadRight(6, '0');
                filePath = string.Format("thumbs/{0}/{1}/{2}/{3}.jpg", s.Substring(0, 2), s.Substring(2, 2), s.Substring(4), fileId);
                return true;
            }

            return false;
        }

        protected override string GetSelectCommandConditionText(int tenantId, TableInfo table)
        {
            if (table.Name == "projects_project_tag" || table.Name == "projects_following_project_participant")
                return "inner join projects_projects as t1 on t1.id = t.project_id where t1.tenant_id = " + tenantId;

            return base.GetSelectCommandConditionText(tenantId, table);
        }

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, RelationInfo relation, ref object value)
        {
            if (relation.ChildTable == "projects_comments" && relation.ChildColumn == "target_uniq_id")
            {
                var valParts = value.ToString().Split('_');

                var entityId = columnMapper.GetMapping(relation.ParentTable, relation.ParentColumn, valParts[1]);
                if (entityId == null)
                    return false;

                value = string.Format("{0}_{1}", valParts[0], entityId);
                return true;
            }

            return base.TryPrepareValue(connection, columnMapper, relation, ref value);
        }

        protected override bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, IEnumerable<RelationInfo> relations, ref object value)
        {
            if (table.Name == "projects_tasks_order" && columnName == "task_order")
            {
                value = Regex.Replace(
                    Convert.ToString(value),
                    @"(?<=""tasks"":\[(\d+,)*)\d+,?",
                    match =>
                        {
                            var mappedId = Convert.ToString(columnMapper.GetMapping("projects_tasks", "id", match.Value.TrimEnd(',')));
                            return !string.IsNullOrEmpty(mappedId) && match.Value.EndsWith(",") ? mappedId + "," : mappedId;
                        },
                    RegexOptions.Compiled);

                value = Regex.Replace(
                    Convert.ToString(value),
                    @"(?<=""milestones"":\[(\d+,)*)\d+,?",
                    match =>
                        {
                            var mappedId = Convert.ToString(columnMapper.GetMapping("projects_milestones", "id", match.Value.TrimEnd(',')));
                            return !string.IsNullOrEmpty(mappedId) && match.Value.EndsWith(",") ? mappedId + "," : mappedId;
                        },
                    RegexOptions.Compiled);

                return true;
            }
            return base.TryPrepareValue(connection, columnMapper, table, columnName, relations, ref value);
        }
    }
}
