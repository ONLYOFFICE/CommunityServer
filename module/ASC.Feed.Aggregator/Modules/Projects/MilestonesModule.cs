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
    internal class MilestonesModule : FeedModule
    {
        private const string item = "milestone";

        public override string Name
        {
            get { return Constants.MilestonesModule; }
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
            get { return "projects_milestones"; }
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
                return base.VisibleFor(feed, data, userId) && scope.Resolve<ProjectSecurity>().CanGoToFeed((Milestone)data, userId);
            }
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var q = new SqlQuery("projects_milestones" + " m")
                .Select("m.id", "m.title", "m.description", "m.deadline", "m.responsible_id", "m.status",
                        "m.status_changed")
                .Select("m.is_key", "m.create_by", "m.create_on", "m.last_modified_by", "m.last_modified_on")
                .Where("m.tenant_id", filter.Tenant)
                .Where(Exp.Between("m.create_on", filter.Time.From, filter.Time.To))
                .InnerJoin("projects_projects" + " p",
                           Exp.EqColumns("m.project_id", "p.id") & Exp.Eq("m.tenant_id", filter.Tenant))
                .Select("p.id", "p.title", "p.description", "p.status", "p.status_changed", "p.responsible_id")
                .Select("p.private", "p.create_by", "p.create_on", "p.last_modified_by", "p.last_modified_on");

            using (var db = new DbManager(DbId))
            {
                var milestones = db.ExecuteList(q).ConvertAll(ToMilestone);
                return milestones.Select(m => new Tuple<Feed, object>(ToFeed(m), m));
            }
        }

        private static Milestone ToMilestone(object[] r)
        {
            return new Milestone
                {
                    ID = Convert.ToInt32(r[0]),
                    Title = Convert.ToString(r[1]),
                    Description = Convert.ToString(r[2]),
                    DeadLine = Convert.ToDateTime(r[3]),
                    Responsible = new Guid(Convert.ToString(r[4])),
                    Status = (MilestoneStatus)Convert.ToInt32(r[5]),
                    StatusChangedOn = Convert.ToDateTime(r[6]),
                    IsKey = Convert.ToBoolean(r[7]),
                    CreateBy = new Guid(Convert.ToString(r[8])),
                    CreateOn = Convert.ToDateTime(r[9]),
                    LastModifiedBy = ToGuid(r[10]),
                    LastModifiedOn = Convert.ToDateTime(r[11]),
                    Project = new Project
                        {
                            ID = Convert.ToInt32(r[12]),
                            Title = Convert.ToString(r[13]),
                            Description = Convert.ToString(r[14]),
                            Status = (ProjectStatus)Convert.ToInt32(15),
                            StatusChangedOn = Convert.ToDateTime(r[16]),
                            Responsible = new Guid(Convert.ToString(r[17])),
                            Private = Convert.ToBoolean(r[18]),
                            CreateBy = new Guid(Convert.ToString(r[19])),
                            CreateOn = Convert.ToDateTime(r[20]),
                            LastModifiedBy = ToGuid(r[21]),
                            LastModifiedOn = Convert.ToDateTime(r[22])
                        }
                };
        }

        private Feed ToFeed(Milestone milestone)
        {
            var itemUrl = "/Products/Projects/Milestones.aspx#project=" + milestone.Project.ID;
            var projectUrl = "/Products/Projects/Tasks.aspx?prjID=" + milestone.Project.ID;
            return new Feed(milestone.CreateBy, milestone.CreateOn)
                {
                    Item = item,
                    ItemId = milestone.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    Product = Product,
                    Module = Name,
                    Title = milestone.Title,
                    Description = Helper.GetHtmlDescription(HttpUtility.HtmlEncode(milestone.Description)),
                    ExtraLocation = milestone.Project.Title,
                    ExtraLocationUrl = CommonLinkUtility.ToAbsolute(projectUrl),
                    AdditionalInfo = Helper.GetUser(milestone.Responsible).DisplayUserName(),
                    AdditionalInfo2 = milestone.DeadLine.ToString("MM.dd.yyyy"),
                    Keywords = string.Format("{0} {1}", milestone.Title, milestone.Description),
                    HasPreview = false,
                    CanComment = false,
                    GroupId = GetGroupId(item, milestone.CreateBy, milestone.Project.ID.ToString(CultureInfo.InvariantCulture))
                };
        }
    }
}