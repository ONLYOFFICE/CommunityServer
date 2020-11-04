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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Core;

namespace ASC.Projects.Data
{
    public abstract class BaseDao
    {
        protected static readonly string CommentsTable = "projects_comments";
        protected static readonly string FollowingProjectTable = "projects_following_project_participant";
        protected static readonly string MessagesTable = "projects_messages";
        protected static readonly string MilestonesTable = "projects_milestones";
        protected static readonly string ProjectsTable = "projects_projects";
        protected static readonly string ParticipantTable = "projects_project_participant";
        protected static readonly string ProjectTagTable = "projects_project_tag";
        protected static readonly string ReportTemplateTable = "projects_report_template";
        protected static readonly string ReportTable = "projects_reports";
        protected static readonly string SubtasksTable = "projects_subtasks";
        protected static readonly string TagsTable = "projects_tags";
        protected static readonly string TasksTable = "projects_tasks";
        protected static readonly string TasksResponsibleTable = "projects_tasks_responsible";
        protected static readonly string TemplatesTable = "projects_templates";
        protected static readonly string TimeTrackingTable = "projects_time_tracking";
        protected static readonly string TasksLinksTable = "projects_tasks_links";
        protected static readonly string TasksOrderTable = "projects_tasks_order";
        protected static readonly string StatusTable = "projects_status";

        protected int Tenant { get; private set; }

        protected Guid CurrentUserID { get; private set; }

        public IDbManager Db { get; set; }

        protected BaseDao(int tenant)
        {
            Tenant = tenant;
            CurrentUserID = SecurityContext.CurrentAccount.ID;
        }


        protected SqlQuery Query(string table)
        {
            return new SqlQuery(table).Where("tenant_id", Tenant);
        }

        protected SqlInsert Insert(string table, bool replace = true)
        {
            return new SqlInsert(table, replace).InColumnValue("tenant_id", Tenant);
        }

        protected SqlUpdate Update(string table)
        {
            return new SqlUpdate(table).Where("tenant_id", Tenant);
        }

        protected SqlDelete Delete(string table)
        {
            return new SqlDelete(table).Where("tenant_id", Tenant);
        }

        protected static Guid ToGuid(object guid)
        {
            try
            {
                var str = guid as string;
                return !string.IsNullOrEmpty(str) ? new Guid(str) : Guid.Empty;
            }
            catch (Exception)
            {
                return Guid.Empty;
            }

        }
    }
}
