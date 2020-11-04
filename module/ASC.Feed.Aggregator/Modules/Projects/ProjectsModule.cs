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
using System.Globalization;
using System.Web;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Studio.Utility;
using System.Linq;
using ASC.Core.Users;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Feed.Aggregator.Modules.Projects
{
    internal class ProjectsModule : FeedModule
    {
        private const string item = "project";

        public override string Name
        {
            get { return Constants.ProjectsModule; }
        }

        public override string Product
        {
            get { return ModulesHelper.ProjectsProductName; }
        }

        public override Guid ProductID
        {
            get { return ModulesHelper.ProjectsProductID; }
        }

        protected override string Table
        {
            get { return "projects_projects"; }
        }

        protected override string LastUpdatedColumn
        {
            get { return "create_on"; }
        }

        protected override string TenantColumn
        {
            get { return "tenant_id"; }
        }

        protected override string DbId
        {
            get { return Constants.ProjectsDbId; }
        }

        public override bool VisibleFor(Feed feed, object data, Guid userId)
        {
            using (var scope = DIHelper.Resolve())
            {
                return base.VisibleFor(feed, data, userId) && scope.Resolve<ProjectSecurity>().CanGoToFeed((Project)data, userId);
            }
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var query = new SqlQuery("projects_projects p")
                .Select("p.id", "p.title", "p.description", "p.status", "p.status_changed", "p.responsible_id")
                .Select("p.private", "p.create_by", "p.create_on", "p.last_modified_by", "p.last_modified_on")
                .Where("tenant_id", filter.Tenant)
                .Where(Exp.Between("create_on", filter.Time.From, filter.Time.To));

            using (var db = new DbManager(DbId))
            {
                var projects = db.ExecuteList(query).ConvertAll(ToProject);
                return projects.Select(p => new Tuple<Feed, object>(ToFeed(p), p));
            }
        }

        private static Project ToProject(object[] r)
        {
            return new Project
                {
                    ID = Convert.ToInt32(r[0]),
                    Title = Convert.ToString(r[1]),
                    Description = Convert.ToString(r[2]),
                    Status = (ProjectStatus)Convert.ToInt32(3),
                    StatusChangedOn = Convert.ToDateTime(r[4]),
                    Responsible = new Guid(Convert.ToString(r[5])),
                    Private = Convert.ToBoolean(r[6]),
                    CreateBy = new Guid(Convert.ToString(r[7])),
                    CreateOn = Convert.ToDateTime(r[8]),
                    LastModifiedBy = ToGuid(r[9]),
                    LastModifiedOn = Convert.ToDateTime(r[10])
                };
        }

        private Feed ToFeed(Project project)
        {
            var itemUrl = "/Products/Projects/Tasks.aspx?prjID=" + project.ID;
            return new Feed(project.CreateBy, project.CreateOn)
                {
                    Item = item,
                    ItemId = project.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    Product = Product,
                    Module = Name,
                    Title = project.Title,
                    Description = Helper.GetHtmlDescription(HttpUtility.HtmlEncode(project.Description)),
                    AdditionalInfo = Helper.GetUser(project.Responsible).DisplayUserName(),
                    Keywords = string.Format("{0} {1}", project.Title, project.Description),
                    HasPreview = false,
                    CanComment = false,
                    GroupId = string.Format("{0}_{1}", item, project.ID)
                };
        }
    }
}