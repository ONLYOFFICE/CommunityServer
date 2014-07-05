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
using System.Globalization;
using System.Web;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Studio.Utility;
using System.Linq;

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
            return base.VisibleFor(feed, data, userId) && ProjectSecurity.CanGoToFeed((Milestone)data, userId);
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
                    LastModifiedBy = new Guid(Convert.ToString(r[10])),
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
                            LastModifiedBy = new Guid(Convert.ToString(r[21])),
                            LastModifiedOn = Convert.ToDateTime(r[22])
                        }
                };
        }

        private Feed ToFeed(Milestone milestone)
        {
            var itemUrl = "/products/projects/milestones.aspx#project=" + milestone.Project.ID;
            var projectUrl = "/products/projects/tasks.aspx?prjID=" + milestone.Project.ID;
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
                    AdditionalInfo = Helper.GetUser(milestone.Responsible).DisplayName,
                    AdditionalInfo2 = milestone.DeadLine.ToString("MM.dd.yyyy"),
                    Keywords = string.Format("{0} {1}", milestone.Title, milestone.Description),
                    HasPreview = false,
                    CanComment = false,
                    GroupId = GetGroupId(item, milestone.CreateBy, milestone.Project.ID.ToString(CultureInfo.InvariantCulture))
                };
        }
    }
}