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
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using Microsoft.Security.Application;

namespace ASC.Projects.Data.DAO
{
    internal class CommentDao : BaseDao, ICommentDao
    {
        private static readonly string ColumnId = "id";
        private static readonly string ColumnTargetUniqId = "target_uniq_id";
        private static readonly string ColumnContent = "content";
        private static readonly string ColumnInactive = "inactive";
        private static readonly string ColumnCreateBy = "create_by";
        private static readonly string ColumnCreateOn = "create_on";
        private static readonly string ColumnParentId = "parent_id";
        private static readonly string ColumnCommentId = "comment_id";


        public CommentDao(int tenantID)
            : base(tenantID)
        {
        }


        public List<Comment> GetAll(DomainObject<int> target)
        {
            return Db
                .ExecuteList(
                    Query("projects_comments")
                        .Select(ColumnId,
                ColumnTargetUniqId,
                ColumnContent,
                ColumnInactive,
                ColumnCreateBy,
                ColumnCreateOn,
                ColumnParentId,
                ColumnCommentId)
                        .Where("target_uniq_id", target.UniqID))
                .ConvertAll(ToComment)
                .OrderBy(c => c.CreateOn)
                .ToList();
        }

        public Comment GetById(Guid id)
        {
            return Db.ExecuteList(Query(CommentsTable).Select(ColumnId,
                ColumnTargetUniqId,
                ColumnContent,
                ColumnInactive,
                ColumnCreateBy,
                ColumnCreateOn,
                ColumnParentId,
                ColumnCommentId).Where("id", id.ToString()))
                        .ConvertAll(ToComment)
                        .SingleOrDefault();
        }

        public List<int> Count(List<ProjectEntity> targets)
        {
            var pairs = Db.ExecuteList(
                Query(CommentsTable)
                    .Select("target_uniq_id", "count(*)")
                    .Where(Exp.In("target_uniq_id", targets.ConvertAll(target => target.UniqID)))
                    .Where("inactive", false)
                    .GroupBy(1)
                ).ConvertAll(r => new object[] { Convert.ToString(r[0]), Convert.ToInt32(r[1]) });

            return targets.ConvertAll(
                target =>
                {
                    var pair = pairs.Find(p => String.Equals(Convert.ToString(p[0]), target.UniqID));
                    return pair == null ? 0 : Convert.ToInt32(pair[1]);
                });
        }

        public List<Comment> GetComments(Exp where)
        {
            return Db.ExecuteList(
                Query(CommentsTable)
                    .Select(ColumnId,
                ColumnTargetUniqId,
                ColumnContent,
                ColumnInactive,
                ColumnCreateBy,
                ColumnCreateOn,
                ColumnParentId,
                ColumnCommentId)
                    .Where(where)
                    .Where("inactive", false)
                    .OrderBy("create_on", false))
                .ConvertAll(ToComment);
        }

        public int Count(DomainObject<Int32> target)
        {
            return Db.ExecuteScalar<int>(
                Query(CommentsTable)
                    .SelectCount()
                    .Where("target_uniq_id", target.UniqID)
                    .Where("inactive", false));
        }


        public Comment Save(Comment comment)
        {
            if (comment.OldGuidId == default(Guid)) comment.OldGuidId = Guid.NewGuid();

            if (!string.IsNullOrWhiteSpace(comment.Content) && comment.Content.Contains("<w:WordDocument>"))
            {
                try
                {
                    comment.Content = Sanitizer.GetSafeHtmlFragment(comment.Content);
                }
                catch (Exception err)
                {
                    LogManager.GetLogger("ASC").Error(err);
                }
            }

            var insert = Insert(CommentsTable)
                .InColumnValue(ColumnCommentId, comment.ID)
                .InColumnValue(ColumnId, comment.OldGuidId)
                .InColumnValue(ColumnTargetUniqId, comment.TargetUniqID)
                .InColumnValue(ColumnContent, comment.Content)
                .InColumnValue(ColumnInactive, comment.Inactive)
                .InColumnValue(ColumnCreateBy, comment.CreateBy.ToString())
                .InColumnValue(ColumnCreateOn, TenantUtil.DateTimeToUtc(comment.CreateOn))
                .InColumnValue(ColumnParentId, comment.Parent.ToString())
                .Identity(1, 0, true);

            comment.ID = Db.ExecuteScalar<int>(insert);
            return comment;
        }

        public void Delete(Guid id)
        {
            Db.ExecuteNonQuery(Delete(CommentsTable).Where("id", id.ToString()));
        }


        private static Comment ToComment(object[] r)
        {
            return new Comment
            {
                OldGuidId = ToGuid(r[0]),
                TargetUniqID = (string)r[1],
                Content = (string)r[2],
                Inactive = Convert.ToBoolean(r[3]),
                CreateBy = ToGuid(r[4]),
                CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[5])),
                Parent = ToGuid(r[6]),
                ID = Convert.ToInt32(r[7])
            };
        }
    }
}