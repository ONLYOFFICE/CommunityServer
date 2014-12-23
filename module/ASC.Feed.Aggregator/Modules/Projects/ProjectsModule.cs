/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
            return base.VisibleFor(feed, data, userId) && ProjectSecurity.CanGoToFeed((Project)data, userId);
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
                    LastModifiedBy = new Guid(Convert.ToString(r[9])),
                    LastModifiedOn = Convert.ToDateTime(r[10])
                };
        }

        private Feed ToFeed(Project project)
        {
            var itemUrl = "/products/projects/tasks.aspx?prjID=" + project.ID;
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