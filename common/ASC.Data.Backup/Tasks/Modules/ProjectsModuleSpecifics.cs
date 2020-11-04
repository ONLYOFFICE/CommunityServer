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


using ASC.Data.Backup.Tasks.Data;
using ASC.Data.Backup.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class ProjectsModuleSpecifics : ModuleSpecificsBase
    {
        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("projects_comments", "tenant_id", "comment_id", IdType.Autoincrement) {UserIDColumns = new[] {"create_by"}},
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

        public override bool TryAdjustFilePath(bool dump, ColumnMapper columnMapper, ref string filePath)
        {
            var match = Regex.Match(filePath, @"^thumbs/\d+/\d+/\d+/(?'fileId'\d+)\.jpg$");
            if (match.Success)
            {
                var fileId = columnMapper.GetMapping("files_file", "id", match.Groups["fileId"].Value);
                if (fileId == null)
                {
                    if(!dump) return false;

                    fileId = match.Groups["fileId"].Value;
                }

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

        protected override bool TryPrepareValue(DbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, ref object value)
        {

            if (table.Name == "projects_comments" && columnName == "content" ||
                table.Name == "projects_messages" && columnName == "content")
            {
                value = FCKEditorPathUtility.CorrectStoragePath(value as string, columnMapper.GetTenantMapping());
                return true;
            }
            return base.TryPrepareValue(connection, columnMapper, table, columnName, ref value);
        }

        protected override bool TryPrepareValue(DbConnection connection, ColumnMapper columnMapper, RelationInfo relation, ref object value)
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

        protected override bool TryPrepareValue(bool dump, DbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, IEnumerable<RelationInfo> relations, ref object value)
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

            return base.TryPrepareValue(dump, connection, columnMapper, table, columnName, relations, ref value);
        }
    }
}
