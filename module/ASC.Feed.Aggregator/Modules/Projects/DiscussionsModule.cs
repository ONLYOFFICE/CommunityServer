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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Entities.Feed;
using ASC.Projects.Engine;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;

namespace ASC.Feed.Aggregator.Modules.Projects
{
    internal class DiscussionsModule : FeedModule
    {
        private const string item = "discussion";


        protected override string Table
        {
            get { return "projects_messages"; }
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


        public override string Name
        {
            get { return Constants.DiscussionsModule; }
        }

        public override string Product
        {
            get { return ModulesHelper.ProjectsProductName; }
        }

        public override Guid ProductID
        {
            get { return ModulesHelper.ProjectsProductID; }
        }

        public override IEnumerable<int> GetTenantsWithFeeds(DateTime fromTime)
        {
            var q1 = new SqlQuery("projects_messages")
                .Select("tenant_id")
                .Where(Exp.Gt("create_on", fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            var q2 = new SqlQuery("projects_comments")
                .Select("tenant_id")
                .Where("substring_index(target_uniq_id, '_', 1) = 'Message'")
                .Where(Exp.Gt("create_on", fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            using (var db = new DbManager(DbId))
            {
                return db.ExecuteList(q1)
                         .ConvertAll(r => Convert.ToInt32(r[0]))
                         .Union(db.ExecuteList(q2).ConvertAll(r => Convert.ToInt32(r[0])));
            }
        }

        public override bool VisibleFor(Feed feed, object data, Guid userId)
        {
            return base.VisibleFor(feed, data, userId) && ProjectSecurity.CanGoToFeed((Message)data, userId);
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var q1 = new SqlQuery("projects_messages d")
                    .Select(DiscussionColumns().Select(d => "d." + d).ToArray())
                    .Select(ProjectColumns().Select(p => "p." + p).ToArray())
                    .Select(CommentColumns().Select(c => "c." + c).ToArray())
                    .InnerJoin("projects_projects p", Exp.EqColumns("p.id", "d.project_id") & Exp.Eq("p.tenant_id", filter.Tenant))
                    .LeftOuterJoin("projects_comments c",
                        Exp.EqColumns("c.target_uniq_id", "concat('Message_', d.id)") &
                        Exp.Eq("c.tenant_id", filter.Tenant) & Exp.Eq("c.inactive", 0))
                    .Where("d.tenant_id", filter.Tenant)
                    .Where(Exp.Between("d.create_on", filter.Time.From, filter.Time.To));

            var q2 = new SqlQuery("projects_messages d")
                    .Select(DiscussionColumns().Select(d => "d." + d).ToArray())
                    .Select(ProjectColumns().Select(p => "p." + p).ToArray())
                    .Select(CommentColumns().Select(c => "c." + c).ToArray())
                    .InnerJoin("projects_projects p", Exp.EqColumns("p.id", "d.project_id") & Exp.Eq("p.tenant_id", filter.Tenant))
                    .LeftOuterJoin("projects_comments c",
                        Exp.EqColumns("c.target_uniq_id", "concat('Message_', d.id)") &
                        Exp.Eq("c.tenant_id", filter.Tenant) & Exp.Eq("c.inactive", 0))
                    .Where("d.tenant_id", filter.Tenant)
                    .Where(Exp.Between("c.create_on", filter.Time.From, filter.Time.To));

            using (var db = new DbManager(DbId))
            {
                var comments = db.ExecuteList(q1.Union(q2)).ConvertAll(ToComment);
                var groupedDiscussions = comments.GroupBy(c => c.Discussion.ID);

                return groupedDiscussions
                    .Select(d => new Tuple<Message, IEnumerable<ProjectComment>>(d.First().Discussion, d))
                    .Select(ToFeed);
            }
        }


        private static IEnumerable<string> DiscussionColumns()
        {
            return new[]
                {
                    "id",
                    "title",
                    "content",
                    "create_by",
                    "create_on",
                    "last_modified_by",
                    "last_modified_on" // 6
                };
        }

        private static IEnumerable<string> ProjectColumns()
        {
            return new[]
                {
                    "id", //7
                    "title",
                    "description",
                    "status",
                    "status_changed",
                    "responsible_id",
                    "private",
                    "create_by",
                    "create_on",
                    "last_modified_by",
                    "last_modified_on" // 17
                };
        }

        private static IEnumerable<string> CommentColumns()
        {
            return new[]
                {
                    "id", // 18
                    "content",
                    "create_by",
                    "create_on",
                    "parent_id",
                    "target_uniq_id" // 23
                };
        }

        private static ProjectComment ToComment(object[] r)
        {
            var p = new ProjectComment
                {
                    Discussion = new Message
                        {
                            ID = Convert.ToInt32(r[0]),
                            Title = Convert.ToString(r[1]),
                            Description = Convert.ToString(r[2]),
                            CreateBy = new Guid(Convert.ToString(r[3])),
                            CreateOn = Convert.ToDateTime(r[4]),
                            LastModifiedBy = ToGuid(r[5]),
                            LastModifiedOn = Convert.ToDateTime(r[6]),
                            Project = new Project
                                {
                                    ID = Convert.ToInt32(r[7]),
                                    Title = Convert.ToString(r[8]),
                                    Description = Convert.ToString(r[9]),
                                    Status = (ProjectStatus)Convert.ToInt32(10),
                                    StatusChangedOn = Convert.ToDateTime(r[11]),
                                    Responsible = new Guid(Convert.ToString(r[12])),
                                    Private = Convert.ToBoolean(r[13]),
                                    CreateBy = new Guid(Convert.ToString(r[14])),
                                    CreateOn = Convert.ToDateTime(r[15]),
                                    LastModifiedBy = ToGuid(r[16]),
                                    LastModifiedOn = Convert.ToDateTime(r[17]),
                                }
                        }
                };
            if (r[18] != null)
            {
                p.Comment = new Comment
                    {
                        OldGuidId = new Guid(Convert.ToString(r[18])),
                        Content = Convert.ToString(r[19]),
                        CreateBy = new Guid(Convert.ToString(r[20])),
                        CreateOn = Convert.ToDateTime(r[21]),
                        Parent = new Guid(Convert.ToString(r[22])),
                        TargetUniqID = Convert.ToString(r[23])
                    };
            }
            return p;
        }

        private Tuple<Feed, object> ToFeed(Tuple<Message, IEnumerable<ProjectComment>> d)
        {
            var discussion = d.Item1;

            var itemUrl = "/products/projects/messages.aspx?prjID=" + discussion.Project.ID + "&id=" + discussion.ID;
            var projectUrl = "/products/projects/tasks.aspx?prjID=" + discussion.Project.ID;
            var commentApiUrl = "/api/2.0/project/message/" + discussion.ID + "/comment.json";

            var comments = d.Item2.Where(c => c.Comment != null).OrderBy(c => c.Comment.CreateOn).ToList();
            var feedDate = comments.Any() ? comments.First().Comment.CreateOn : discussion.CreateOn;
            var feedAuthor = comments.Any() ? comments.Last().Comment.CreateBy : discussion.CreateBy;

            var feed = new Feed(discussion.CreateBy, discussion.CreateOn, true)
                {
                    Item = item,
                    ItemId = discussion.ID.ToString(CultureInfo.InvariantCulture),
                    ItemUrl = CommonLinkUtility.ToAbsolute(itemUrl),
                    ModifiedBy = feedAuthor,
                    ModifiedDate = feedDate,
                    Product = Product,
                    Module = Name,
                    Action = comments.Any() ? FeedAction.Commented : FeedAction.Created,
                    Title = discussion.Title,
                    Description = HtmlUtility.GetFull(discussion.Description),
                    ExtraLocation = discussion.Project.Title,
                    ExtraLocationUrl = CommonLinkUtility.ToAbsolute(projectUrl),
                    HasPreview = discussion.Description.Contains("class=\"asccut\""),
                    CanComment = true,
                    CommentApiUrl = CommonLinkUtility.ToAbsolute(commentApiUrl),
                    Comments = comments.Select(ToFeedComment),
                    GroupId = string.Format("{0}_{1}", item, discussion.ID)
                };
            feed.Keywords = string.Format("{0} {1} {2}",
                                          discussion.Title,
                                          Helper.GetText(discussion.Description),
                                          string.Join(" ", feed.Comments.Select(x => x.Description)));

            return new Tuple<Feed, object>(feed, discussion);
        }

        private static FeedComment ToFeedComment(ProjectComment comment)
        {
            return new FeedComment(comment.Comment.CreateBy)
                {
                    Id = comment.Comment.OldGuidId.ToString(),
                    Description = HtmlUtility.GetFull(comment.Comment.Content),
                    Date = comment.Comment.CreateOn
                };
        }
    }
}