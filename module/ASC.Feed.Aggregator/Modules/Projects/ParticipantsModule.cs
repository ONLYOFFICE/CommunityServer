/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Studio.Utility;
using System.Linq;
using ASC.Core.Users;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Feed.Aggregator.Modules.Projects
{
    internal class ParticipantsModule : FeedModule
    {
        private const string item = "participant";

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
            get { return "projects_project_participant"; }
        }

        protected override string LastUpdatedColumn
        {
            get { return "created"; }
        }

        protected override string TenantColumn
        {
            get { return "tenant"; }
        }

        protected override string DbId
        {
            get { return Constants.ProjectsDbId; }
        }

        public override bool VisibleFor(Feed feed, object data, Guid userId)
        {
            using (var scope = DIHelper.Resolve())
            {
                return base.VisibleFor(feed, data, userId) && scope.Resolve<ProjectSecurity>().CanGoToFeed((ParticipantFull)data, userId);
            }
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var filterTimeInterval = TimeSpan.FromTicks(filter.Time.To.Ticks - filter.Time.From.Ticks).TotalMinutes;

            var q = new SqlQuery("projects_project_participant" + " pp")
                .Select("pp.participant_id", "pp.created", "pp.updated")
                .InnerJoin("projects_projects" + " p",
                           Exp.EqColumns("p.id", "pp.project_id") & Exp.Eq("p.tenant_id", filter.Tenant))
                .Select("p.id", "p.title", "p.description", "p.status", "p.status_changed", "p.responsible_id")
                .Select("p.private", "p.create_by", "p.create_on", "p.last_modified_by", "p.last_modified_on")
                .Where("pp.removed", 0)
                .Where("pp.tenant", filter.Tenant)
                .Where(Exp.Between("pp.created", filter.Time.From, filter.Time.To) &
                       !Exp.Between("TIMESTAMPDIFF(MINUTE,p.create_on, pp.created)", -filterTimeInterval, filterTimeInterval));

            using (var db = new DbManager(DbId))
            {
                var participants = db.ExecuteList(q).ConvertAll(ToParticipant);
                return participants.Select(p => new Tuple<Feed, object>(ToFeed(p), p));
            }
        }

        private static ParticipantFull ToParticipant(object[] r)
        {
            var participantId = new Guid(Convert.ToString(r[0]));
            return new ParticipantFull(participantId)
                {
                    Created = Convert.ToDateTime(r[1]),
                    Updated = Convert.ToDateTime(r[2]),
                    Project = new Project
                        {
                            ID = Convert.ToInt32(r[3]),
                            Title = Convert.ToString(r[4]),
                            Description = Convert.ToString(r[5]),
                            Status = (ProjectStatus)Convert.ToInt32(6),
                            StatusChangedOn = Convert.ToDateTime(r[7]),
                            Responsible = new Guid(Convert.ToString(r[8])),
                            Private = Convert.ToBoolean(r[9]),
                            CreateBy = new Guid(Convert.ToString(r[10])),
                            CreateOn = Convert.ToDateTime(r[11]),
                            LastModifiedBy = ToGuid(r[12]),
                            LastModifiedOn = Convert.ToDateTime(r[13])
                        }
                };
        }

        private Feed ToFeed(ParticipantFull participant)
        {
            var projectUrl = "/products/projects/tasks.aspx?prjID=" + participant.Project.ID;
            var feed = new Feed(participant.ID, participant.Created)
                {
                    Item = item,
                    ItemId = string.Format("{0}_{1}", participant.ID, participant.Project.ID),
                    ItemUrl = CommonLinkUtility.ToAbsolute(projectUrl),
                    Product = Product,
                    Module = Name,
                    Title = participant.Project.Title,
                    ExtraLocation = participant.Project.Title,
                    ExtraLocationUrl = CommonLinkUtility.ToAbsolute(projectUrl),
                    HasPreview = false,
                    CanComment = false,
                    GroupId = GetGroupId(item, Guid.Empty, participant.Project.ID.ToString(CultureInfo.InvariantCulture)),
                    Keywords = CoreContext.UserManager.GetUsers(participant.ID).DisplayUserName(false)
                };
            return feed;
        }
    }
}