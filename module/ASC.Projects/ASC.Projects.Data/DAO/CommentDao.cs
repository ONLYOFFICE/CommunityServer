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

using ASC.Common.Data;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using log4net;
using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Linq;

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
                "parent_id"
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

        public Comment GetLast(DomainObject<Int32> target)
        {
            using (var db = new DbManager(DatabaseId))
            {
                return db.ExecuteList(
                    Query(CommentsTable)
                        .Select(columns)
                        .Where("target_uniq_id", target.UniqID)
                        .Where("inactive", false)
                        .OrderBy("create_on", false)
                        .SetMaxResults(1))
                         .ConvertAll(ToComment)
                         .SingleOrDefault();
            }
        }

        public List<int> GetCommentsCount(List<ProjectEntity> targets)
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
                if (comment.ID == default(Guid)) comment.ID = Guid.NewGuid();

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
                        comment.ID,
                        comment.TargetUniqID,
                        comment.Content,
                        comment.Inactive,
                        comment.CreateBy.ToString(),
                        TenantUtil.DateTimeToUtc(comment.CreateOn),
                        comment.Parent.ToString());
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
                    ID = ToGuid(r[0]),
                    TargetUniqID = (string)r[1],
                    Content = (string)r[2],
                    Inactive = Convert.ToBoolean(r[3]),
                    CreateBy = ToGuid(r[4]),
                    CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[5])),
                    Parent = ToGuid(r[6]),
                };
        }
    }
}