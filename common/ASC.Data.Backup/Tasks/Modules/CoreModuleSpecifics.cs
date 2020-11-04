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
using System.Linq;
using ASC.Core.Billing;
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class CoreModuleSpecifics : ModuleSpecificsBase
    {
        private static readonly Guid ProjectsSourceID = new Guid("6045B68C-2C2E-42db-9E53-C272E814C4AD");
        private static readonly Guid BookmarksSourceID = new Guid("28B10049-DD20-4f54-B986-873BC14CCFC7");
        private static readonly Guid ForumsSourceID = new Guid("853B6EB9-73EE-438d-9B09-8FFEEDF36234");
        private static readonly Guid NewsSourceID = new Guid("6504977C-75AF-4691-9099-084D3DDEEA04");
        private static readonly Guid BlogsSourceID = new Guid("6A598C74-91AE-437d-A5F4-AD339BD11BB2");

        private const string ForumsNewPostInTopicActionID = "new post in topic";
        private const string ForumsNewPostInThreadActionID = "new post in thread";
        private const string NewsNewCommentActionID = "new feed comment";
        private const string BlogsNewCommentActionID = "new comment";

        private const string CrmCompanyAclObjectStart = "ASC.CRM.Core.Entities.Company|";
        private const string CrmPersonAclObjectStart = "ASC.CRM.Core.Entities.Person|";
        private const string CrmDealAclObjectStart = "ASC.CRM.Core.Entities.Deal|";
        private const string CrmCasesAclObjectStart = "ASC.CRM.Core.Entities.Cases|";
        private const string CrmRelationshipEventAclObjectStart = "ASC.CRM.Core.Entities.RelationshipEvent|";
        private const string CalendarCalendarAclObjectStart = "ASC.Api.Calendar.BusinessObjects.Calendar|";
        private const string CalendarEventAclObjectStart = "ASC.Api.Calendar.BusinessObjects.Event|";

        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("core_acl", "tenant") {InsertMethod = InsertMethod.Ignore},
                new TableInfo("core_subscription", "tenant"),
                new TableInfo("core_subscriptionmethod", "tenant"),
                new TableInfo("core_userphoto", "tenant") {UserIDColumns = new[] {"userid"}},
                new TableInfo("core_usersecurity", "tenant") {UserIDColumns = new[] {"userid"}},
                new TableInfo("core_usergroup", "tenant") {UserIDColumns = new[] {"userid"}},
                new TableInfo("feed_aggregate", "tenant")
                    {
                        InsertMethod = InsertMethod.None,
                        DateColumns = new Dictionary<string, bool> {{"created_date", false}, {"aggregated_date", false}}
                    },
                new TableInfo("feed_readed", "tenant_id")
                    {
                        InsertMethod = InsertMethod.None,
                        DateColumns = new Dictionary<string, bool> {{"timestamp", false}}
                    },
                new TableInfo("feed_users") {InsertMethod = InsertMethod.None},
                new TableInfo("backup_backup", "tenant_id", "id", IdType.Guid),
                new TableInfo("backup_schedule", "tenant_id"),
                new TableInfo("core_settings", "tenant") 
            };

        private readonly RelationInfo[] _tableRelations = new[]
            {
                new RelationInfo("core_user", "id", "core_acl", "subject", typeof(TenantsModuleSpecifics)),

                new RelationInfo("core_group", "id", "core_acl", "subject", typeof(TenantsModuleSpecifics)),

                new RelationInfo("core_user", "id", "core_subscription", "recipient", typeof(TenantsModuleSpecifics)),

                new RelationInfo("core_group", "id", "core_subscription", "recipient", typeof(TenantsModuleSpecifics)),

                new RelationInfo("core_user", "id", "core_subscriptionmethod", "recipient", typeof(TenantsModuleSpecifics)),

                new RelationInfo("core_group", "id", "core_subscriptionmethod", "recipient", typeof(TenantsModuleSpecifics)),

                new RelationInfo("core_group", "id", "core_usergroup", "groupid", typeof(TenantsModuleSpecifics),
                                 x => !Helpers.IsEmptyOrSystemGroup(Convert.ToString(x["groupid"]))),

                new RelationInfo("crm_contact", "id", "core_acl", "object", typeof(CrmModuleSpecifics),
                                 x => Convert.ToString(x["object"]).StartsWith(CrmCompanyAclObjectStart) || Convert.ToString(x["object"]).StartsWith(CrmPersonAclObjectStart)),

                new RelationInfo("crm_deal", "id", "core_acl", "object", typeof(CrmModuleSpecifics),
                                 x => Convert.ToString(x["object"]).StartsWith(CrmDealAclObjectStart)),

                new RelationInfo("crm_case", "id", "core_acl", "object", typeof(CrmModuleSpecifics),
                                 x => Convert.ToString(x["object"]).StartsWith(CrmCasesAclObjectStart)),

                new RelationInfo("crm_relationship_event", "id", "core_acl", "object", typeof(CrmModuleSpecifics2),
                                 x => Convert.ToString(x["object"]).StartsWith(CrmRelationshipEventAclObjectStart)),

                new RelationInfo("calendar_calendars", "id", "core_acl", "object", typeof(CalendarModuleSpecifics),
                                 x => Convert.ToString(x["object"]).StartsWith(CalendarCalendarAclObjectStart)),

                new RelationInfo("calendar_events", "id", "core_acl", "object", typeof(CalendarModuleSpecifics),
                                 x => Convert.ToString(x["object"]).StartsWith(CalendarEventAclObjectStart)),

                new RelationInfo("projects_projects", "id", "core_subscription", "object", typeof(ProjectsModuleSpecifics),
                                 x => ValidateSource(ProjectsSourceID, x)),

                new RelationInfo("projects_tasks", "id", "core_subscription", "object", typeof(ProjectsModuleSpecifics),
                                 x => ValidateSource(ProjectsSourceID, x) && Convert.ToString(x["object"]).StartsWith("Task_")),

                new RelationInfo("projects_messages", "id", "core_subscription", "object", typeof(ProjectsModuleSpecifics),
                                 x => ValidateSource(ProjectsSourceID, x) && Convert.ToString(x["object"]).StartsWith("Message_")),

                new RelationInfo("projects_milestones", "id", "core_subscription", "object", typeof(ProjectsModuleSpecifics),
                                 x => ValidateSource(ProjectsSourceID, x) && Convert.ToString(x["object"]).StartsWith("Milestone_")),

                new RelationInfo("bookmarking_bookmark", "ID", "core_subscription", "object", typeof(CommunityModuleSpecifics),
                                 x => ValidateSource(BookmarksSourceID, x) && !string.IsNullOrEmpty(Convert.ToString(x["object"]))),

                new RelationInfo("forum_topic", "id", "core_subscription", "object", typeof(CommunityModuleSpecifics),
                                 x => ValidateSource(ForumsSourceID, x) && Convert.ToString(x["action"]) == ForumsNewPostInTopicActionID && !string.IsNullOrEmpty(Convert.ToString(x["object"]))),

                new RelationInfo("forum_thread", "id", "core_subscription", "object", typeof(CommunityModuleSpecifics),
                                 x => ValidateSource(ForumsSourceID, x) && Convert.ToString(x["action"]) == ForumsNewPostInThreadActionID && !string.IsNullOrEmpty(Convert.ToString(x["object"]))),

                new RelationInfo("events_feed", "id", "core_subscription", "object", typeof(CommunityModuleSpecifics),
                                 x => ValidateSource(NewsSourceID, x) && Convert.ToString(x["action"]) == NewsNewCommentActionID && !string.IsNullOrEmpty(Convert.ToString(x["object"]))),

                new RelationInfo("blogs_posts", "id", "core_subscription", "object", typeof(CommunityModuleSpecifics),
                                 x => ValidateSource(BlogsSourceID, x) && Convert.ToString(x["action"]) == BlogsNewCommentActionID),

                new RelationInfo("core_user", "id", "feed_users", "user_id", typeof(CoreModuleSpecifics)),

                new RelationInfo("files_folder", "id", "backup_backup", "storage_base_path", typeof(FilesModuleSpecifics),
                                 x => IsDocumentsStorageType(Convert.ToString(x["storage_type"]))),

                new RelationInfo("files_file", "id", "backup_backup", "storage_path", typeof(FilesModuleSpecifics),
                                 x => IsDocumentsStorageType(Convert.ToString(x["storage_type"]))),

                new RelationInfo("files_folder", "id", "backup_schedule", "storage_base_path", typeof(FilesModuleSpecifics),
                                 x => IsDocumentsStorageType(Convert.ToString(x["storage_type"]))),
            };

        public override ModuleName ModuleName
        {
            get { return ModuleName.Core; }
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
            
            if (table.Name == "feed_users")
                return "inner join core_user t1 on t1.id = t.user_id where t1.tenant = " + tenantId;

            if (table.Name == "core_settings")
                return string.Format("where t.{0} = {1} and id not in ('{2}')", table.TenantColumn, tenantId, LicenseReader.CustomerIdKey);

            return base.GetSelectCommandConditionText(tenantId, table);
        }

        protected override bool TryPrepareValue(DbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, ref object value)
        {
            if (table.Name == "core_usergroup" && columnName == "last_modified")
            {
                value = DateTime.UtcNow;
                return true;
            }
            return base.TryPrepareValue(connection, columnMapper, table, columnName, ref value);
        }

        protected override bool TryPrepareRow(bool dump, DbConnection connection, ColumnMapper columnMapper, TableInfo table, DataRowInfo row, out Dictionary<string, object> preparedRow)
        {
            if (table.Name == "core_acl")
            {
                if (int.Parse((string)row["tenant"]) == -1)
                {
                    preparedRow = null;
                    return false;
                }
            }
            return base.TryPrepareRow(dump, connection, columnMapper, table, row, out preparedRow);
        }

        protected override bool TryPrepareValue(DbConnection connection, ColumnMapper columnMapper, RelationInfo relation, ref object value)
        {
            if (relation.ChildTable == "core_acl" && relation.ChildColumn == "object")
            {
                var valParts = Convert.ToString(value).Split('|');

                var entityId = columnMapper.GetMapping(relation.ParentTable, relation.ParentColumn, valParts[1]);
                if (entityId == null)
                    return false;

                value = string.Format("{0}|{1}", valParts[0], entityId);
                return true;
            }
            return base.TryPrepareValue(connection, columnMapper, relation, ref value);
        }

        protected override bool TryPrepareValue(bool dump, DbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, IEnumerable<RelationInfo> relations, ref object value)
        {
            var relationList = relations.ToList();

            if (relationList.All(x => x.ChildTable == "core_subscription" && x.ChildColumn == "object" && x.ParentTable.StartsWith("projects_")))
            {
                var valParts = Convert.ToString(value).Split('_');

                var projectId = columnMapper.GetMapping("projects_projects", "id", valParts[2]);
                if (projectId == null)
                    return false;

                var firstRelation = relationList.First(x => x.ParentTable != "projects_projects");
                var entityId = columnMapper.GetMapping(firstRelation.ParentTable, firstRelation.ParentColumn, valParts[1]);
                if (entityId == null)
                    return false;

                value = string.Format("{0}_{1}_{2}", valParts[0], entityId, projectId);
                return true;
            }

            if (relationList.All(x => x.ChildTable == "core_subscription" && x.ChildColumn == "recipient")
                || relationList.All(x => x.ChildTable == "core_subscriptionmethod" && x.ChildColumn == "recipient")
                || relationList.All(x => x.ChildTable == "core_acl" && x.ChildColumn == "subject"))
            {
                var strVal = Convert.ToString(value);
                if (Helpers.IsEmptyOrSystemUser(strVal) || Helpers.IsEmptyOrSystemGroup(strVal))
                    return true;

                foreach (var relation in relationList)
                {
                    var mapping = columnMapper.GetMapping(relation.ParentTable, relation.ParentColumn, value);
                    if (mapping != null)
                    {
                        value = mapping;
                        return true;
                    }
                }
                return false;
            }

            return base.TryPrepareValue(dump, connection, columnMapper, table, columnName, relationList, ref value);
        }

        private static bool ValidateSource(Guid expectedValue, DataRowInfo row)
        {
            var source = Convert.ToString(row["source"]);
            try
            {
                return expectedValue == new Guid(source);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsDocumentsStorageType(string strStorageType)
        {
            var storageType = int.Parse(strStorageType);
            return storageType == 0 || storageType == 1;
        }
    }
}
