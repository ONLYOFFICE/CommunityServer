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
using ASC.Blogs.Core.Domain;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;

namespace ASC.Blogs.Core.Data
{
    public class DbPostDao : DbDao, IPostDao
    {
        public DbPostDao(DbManager db, int tenant) : base(db, tenant) { }

        #region IPostDao

        private Exp GetWhere(Guid? id, long? blogId, Guid? userId, string tag)
        {
            var where = Exp.Empty;
            if (id.HasValue) where &= Exp.Eq("id", id.Value.ToString());
            if (blogId.HasValue) where &= Exp.Eq("blog_id", blogId.Value);
            if (userId.HasValue) where &= Exp.Eq("created_by", userId.Value.ToString());
            if (tag != null) where &= Exp.In("id", Query("blogs_tags").Select("post_id").Where("name", tag));
            return where;
        }

        public int GetCount(Guid? id, long? blogId, Guid? userId)
        {
            return GetCount(id, blogId, userId, null);
        }

        public int GetCount(Guid? id, long? blogId, Guid? userId, string tag)
        {
            var where = GetWhere(id, blogId, userId, tag);
            var query = Query("blogs_posts").SelectCount().Where(where);
            return Db.ExecuteScalar<int>(query);
        }

        public List<Post> Select(Guid? id, long? blogId, Guid? userId, string tag, bool withContent, bool asc, int? from, int? count, bool fillTags, bool withCommentsCount)
        {
            var where = GetWhere(id, blogId, userId, tag);
            var query = Query("blogs_posts")
                .Select("id", "title", "created_by", "created_when", "blog_id", "post_id")
                .Where(where);

            if (withContent) query.Select("content");
            if (count.HasValue) query.SetMaxResults(count.Value);
            if (from.HasValue) query.SetFirstResult(from.Value);

            query.OrderBy("created_when", asc);

            var posts = Db.ExecuteList<Post>(query, r => RowMappers.ToPost(r, withContent));

            if (posts.Count > 0)
            {
                var pids = posts.ConvertAll(p => p.ID.ToString());
                var postQ = Query("blogs_posts").Select("id").Where(where).OrderBy("created_when", asc);
                if (count.HasValue) postQ.SetMaxResults(count.Value);
                if (from.HasValue) postQ.SetFirstResult(from.Value);
                var postsFilter = Exp.In("post_id", pids);

                if (fillTags)
                {
                    var tagQuery = Query("blogs_tags").Where(postsFilter).Select("name", "post_id");
                    var tlist = Db.ExecuteList<Tag>(tagQuery, RowMappers.ToTag);
                    foreach (var post in posts)
                    {
                        post.TagList = tlist.FindAll(t => t.PostId == post.ID);
                    }
                }
            }
            return posts;
        }

        public List<Post> Select(Guid? id, long? blogId, Guid? userId, bool withContent, bool asc, int? from, int? count, bool fillTags, bool withCommentsCount)
        {
            return Select(id, blogId, userId, null, withContent, true, null, null, fillTags, withCommentsCount);
        }

        public List<Post> Select(Guid? id, long? blogId, Guid? userId, bool withContent, bool fillTags, bool withCommentsCount)
        {
            return Select(id, blogId, userId, withContent, true, null, null, fillTags, withCommentsCount);
        }


        public List<int> SearchPostsByWord(string word)
        {
            var q1 = Query("blogs_posts")
                .Select("post_id")
                .Where(Exp.Like("lower(title)", word, SqlLike.StartWith) | Exp.Like("lower(title)", " " + word) | Exp.Like("lower(content)", word, SqlLike.StartWith) | Exp.Like("lower(content)", " " + word));

            var q2 = new SqlQuery("blogs_tags t")
                .InnerJoin("blogs_posts p", Exp.EqColumns("t.Tenant", "p.Tenant") & Exp.EqColumns("t.post_id", "p.id"))
                .Select("p.post_id")
                .Where("t.Tenant", Tenant)
                .Where(Exp.Like("lower(name)", word, SqlLike.StartWith) | Exp.Like("lower(name)", " " + word));

            return Db
                    .ExecuteList(q1.Union(q2))
                    .ConvertAll(row => Convert.ToInt32(row[0]));
        }

        public List<Tag> SelectTags(Guid? postId)
        {
            var query = Query("blogs_tags").Select("name", "post_id");
            if (postId.HasValue) query.Where("post_id", postId.Value.ToString());
            return Db.ExecuteList<Tag>(query, RowMappers.ToTag);
        }

        public List<string> GetTags(string like, int limit)
        {
            var query = Query("blogs_tags").Select("name").Distinct();
            query.Where(Exp.Like("name", like, SqlLike.StartWith)).SetMaxResults(limit);
            return Db.ExecuteList<string>(query, RowMappers.ToString);
        }

        public List<Post> GetPosts(List<int> ids, bool withContent, bool withTags)
        {
            var postIds = ids.ConvertAll(id => id.ToString(CultureInfo.InvariantCulture));
            var select = Query("blogs_posts")
                .Select("id", "title", "created_by", "created_when", "blog_id", "post_id")
                .Where(Exp.In("post_id", postIds));

            if (withContent) select.Select("content");

            var posts = Db.ExecuteList(select, r => RowMappers.ToPost(r, withContent));

            if (posts.Count > 0)
            {
                if (withTags)
                {
                    var tagQuery = Query("blogs_tags").Where(Exp.In("post_id", postIds)).Select("name", "post_id");
                    var tlist = Db.ExecuteList(tagQuery, RowMappers.ToTag);
                    foreach (var post in posts)
                    {
                        post.TagList = tlist.FindAll(t => t.PostId == post.ID);
                    }
                }
            }

            return posts;
        }

        public List<Post> GetPosts(List<Guid> ids, bool withContent, bool withTags)
        {
            var postIds = ids.ConvertAll(id => id.ToString());
            var select = Query("blogs_posts")
                .Select("id", "title", "created_by", "created_when", "blog_id", "post_id")
                .Where(Exp.In("id", postIds));

            if (withContent) select.Select("content");

            var posts = Db.ExecuteList<Post>(select, r => RowMappers.ToPost(r, withContent));

            if (posts.Count > 0)
            {
                if (withTags)
                {
                    var tagQuery = Query("blogs_tags").Where(Exp.In("post_id", postIds)).Select("name", "post_id");
                    var tlist = Db.ExecuteList<Tag>(tagQuery, RowMappers.ToTag);
                    foreach (var post in posts)
                    {
                        post.TagList = tlist.FindAll(t => t.PostId == post.ID);
                    }
                }
            }

            return posts;
        }

        public void SavePost(Post post)
        {
            var isInsert = false;
            if (post.ID == Guid.Empty)
            {
                post.ID = Guid.NewGuid();
                isInsert = true;
            }

            using (var tx = Db.BeginTransaction())
            {
                var tagsForSave = new List<Tag>(post.TagList);
                var tagsForDelete = new List<Tag>();
                if (!isInsert)
                {
                    var savedTags = SelectTags(post.ID);
                    tagsForDelete.AddRange(savedTags);
                    tagsForDelete.RemoveAll(_1 => tagsForSave.Exists(_2 => String.Equals(_1.Content, _2.Content, StringComparison.CurrentCultureIgnoreCase)));
                    tagsForSave.RemoveAll(_1 => savedTags.Exists(_2 => String.Equals(_1.Content, _2.Content, StringComparison.CurrentCultureIgnoreCase)));
                }

                if (tagsForDelete.Count > 0)
                {
                    var deleteq = Delete("blogs_tags")
                        .Where("post_id", post.ID.ToString())
                        .Where(Exp.In("name", tagsForDelete.ConvertAll(_ => (object)_.Content)));

                    Db.ExecuteNonQuery(deleteq);
                }

                if (tagsForSave.Count > 0)
                {
                    foreach (var tag in tagsForSave)
                    {
                        var insertq = Insert("blogs_tags")
                            .InColumns("name", "post_id")
                            .Values(tag.Content, post.ID.ToString());
                        Db.ExecuteNonQuery(insertq);
                    }
                }

                var queryi = Insert("blogs_posts")
                    .InColumnValue("post_id", post.AutoIncrementID)
                    .InColumnValue("id", post.ID.ToString())
                    .InColumnValue("title", post.Title)
                    .InColumnValue("content", post.Content)
                    .InColumnValue("created_by", post.UserID.ToString())
                    .InColumnValue("created_when", TenantUtil.DateTimeToUtc(post.Datetime))
                    .InColumnValue("blog_id", post.BlogId)
                    .Identity(1, 0, true);

                post.AutoIncrementID = Db.ExecuteScalar<int>(queryi);

                tx.Commit();
            }
        }

        public void DeletePost(Guid postId)
        {
            using (var tx = Db.BeginTransaction())
            {
                Db.ExecuteNonQuery(Delete("blogs_posts").Where("id", postId.ToString()));
                Db.ExecuteNonQuery(Delete("blogs_comments").Where("post_id", postId.ToString()));
                Db.ExecuteNonQuery(Delete("blogs_tags").Where("post_id", postId.ToString()));
                Db.ExecuteNonQuery(Delete("blogs_reviewposts").Where("post_id", postId.ToString()));

                tx.Commit();
            }
        }

        public List<Comment> GetComments(Guid postId)
        {
            var query = Query("blogs_comments")
                .Select("id", "post_id", "parent_id", "content", "created_by", "created_when", "inactive")
                .Where("post_id", postId.ToString())
                .OrderBy("created_when", true);

            return Db.ExecuteList<Comment>(query, RowMappers.ToComment);
        }

        public Comment GetCommentById(Guid commentId)
        {
            var select = Query("blogs_comments")
                .Select("id", "post_id", "parent_id", "content", "created_by", "created_when", "inactive")
                .Where("id", commentId.ToString());

            var list = Db.ExecuteList<Comment>(select, RowMappers.ToComment);
            return list.Count > 0 ? list[0] : null;
        }

        public void SaveComment(Comment comment)
        {
            var isInsert = (comment.ID == Guid.Empty);
            if (isInsert) comment.ID = Guid.NewGuid();

            var query = Insert("blogs_comments")
                .InColumns("id", "post_id", "parent_id", "content", "created_by", "created_when", "inactive")
                .Values(comment.ID, comment.PostId, comment.ParentId, comment.Content, comment.UserID.ToString(), TenantUtil.DateTimeToUtc(comment.Datetime), comment.Inactive ? 1 : 0);

            using (var tx = Db.BeginTransaction())
            {
                Db.ExecuteNonQuery(query);

                if (isInsert)
                {
                    var update = Update("blogs_posts")
                        .Set("LastCommentId", comment.ID.ToString())
                        .Where("id", comment.PostId.ToString());

                    Db.ExecuteNonQuery(update);
                }

                tx.Commit();
            }
        }

        public List<TagStat> GetTagStat(int? top)
        {
            var query = Query("blogs_tags")
                        .Select("name", "count(*) as cnt")
                        .OrderBy("cnt", false)
                        .GroupBy("name");

            if (top.HasValue) query.SetMaxResults(top.Value);

            var result = Db.ExecuteList(query, (r => new TagStat { Name = (string)r[0], Count = Convert.ToInt32(r[1]) }));

            return result;
        }

        public List<int> GetCommentsCount(List<Guid> postsIds)
        {
            var select = Query("blogs_comments")
                .Select("post_id", "count(*)")
                .Where(Exp.In("post_id", postsIds.ConvertAll(id => id.ToString())))
                .Where(Exp.Like("inactive", "0"))
                .GroupBy("post_id");

            var list = Db.ExecuteList(select);

            var result = new List<int>(postsIds.Count);
            for (var i = 0; i < postsIds.Count; i++)
            {
                var finded = list.Find(row => new Guid(row[0].ToString()) == postsIds[i]);
                result.Add(finded == null ? 0 : Convert.ToInt32(finded[1]));
            }
            return result;
        }

        public void SavePostReview(Guid userId, Guid postId, DateTime datetime)
        {
            var insert = Insert("blogs_reviewposts")
                .InColumns("post_id", "reviewed_by", "timestamp")
                .Values(postId.ToString(), userId.ToString(), TenantUtil.DateTimeToUtc(datetime));
            Db.ExecuteNonQuery(insert);
        }

        #endregion
    }
}