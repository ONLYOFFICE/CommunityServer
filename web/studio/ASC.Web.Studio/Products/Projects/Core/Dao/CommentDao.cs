/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using log4net;
using Microsoft.Security.Application;

namespace ASC.Projects.Data.DAO
{
    internal class CommentDao : BaseDao, ICommentDao
    {
        private readonly string[] columns = new[]
            {
                "id",
                "target_uniq_id",
                "content",
                "inactive",
                "create_by",
                "create_on",
                "parent_id",
                "comment_id"
            };


        public CommentDao(string dbId, int tenantID)
            : base(dbId, tenantID)
        {
        }


        public List<Comment> GetAll(DomainObject<int> target)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db
                    .ExecuteList(
                        Query("projects_comments")
                            .Select(columns)
                            .Where("target_uniq_id", target.UniqID))
                    .ConvertAll(ToComment)
                    .OrderBy(c => c.CreateOn)
                    .ToList();
            }
        }

        public Comment GetById(Guid id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(Query(CommentsTable).Select(columns).Where("id", id.ToString()))
                         .ConvertAll(ToComment)
                         .SingleOrDefault();
            }
        }

        public List<int> Count(List<ProjectEntity> targets)
        {
            using (var db = new DbManager(DatabaseId))
            {
                var pairs = db.ExecuteList(
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
        }

        public List<Comment> GetComments(Exp where)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(
                    Query(CommentsTable)
                        .Select(columns)
                        .Where(where)
                        .Where("inactive", false)
                        .OrderBy("create_on", false))
                    .ConvertAll(ToComment);
            }
        }

        public int Count(DomainObject<Int32> target)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteScalar<int>(
                    Query(CommentsTable)
                        .SelectCount()
                        .Where("target_uniq_id", target.UniqID)
                        .Where("inactive", false));
            }
        }


        public Comment Save(Comment comment)
        {
            using (var db = new DbManager(DatabaseId))
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
                        LogManager.GetLogger(GetType()).Error(err);
                    }
                }

                var insert = Insert(CommentsTable)
                    .InColumns(columns)
                    .Values(
                        comment.OldGuidId,
                        comment.TargetUniqID,
                        comment.Content,
                        comment.Inactive,
                        comment.CreateBy.ToString(),
                        TenantUtil.DateTimeToUtc(comment.CreateOn),
                        comment.Parent.ToString(),
                        comment.ID
                        );
                db.ExecuteNonQuery(insert);
                return comment;
            }
        }

        public void Delete(Guid id)
        {
            using (var db = new DbManager(DatabaseId))
            {
                db.ExecuteNonQuery(Delete(CommentsTable).Where("id", id.ToString()));
            }
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