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
using System.Text;
using ASC.Bookmarking.Common;
using ASC.Bookmarking.Common.Util;
using ASC.Bookmarking.Pojo;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.ElasticSearch;
using ASC.Web.Community.Search;

namespace ASC.Bookmarking.Dao
{
    public class BookmarkingHibernateDao : BookmarkingSessionObject<BookmarkingHibernateDao>
    {
        private static IDbManager DbManager
        {
            get { return ASC.Common.Data.DbManager.FromHttpContext(BookmarkingBusinessConstants.BookmarkingDbID); }
        }

        private static int Tenant
        {
            get { return TenantProvider.CurrentTenantID; }
        }

        public long ItemsCount { get; set; }

        #region Get Bookmarks

        internal IList<Bookmark> GetAllBookmarks(int firstResult, int maxResults)
        {
            try
            {
                var count = DbManager.ExecuteScalar<long>(
                        new SqlQuery()
                            .SelectCount()
                            .From("bookmarking_bookmark")
                            .Where("Tenant", Tenant));

                SetBookmarksCount(count);

                var bookmarks = DbManager.ExecuteList(
                        new SqlQuery()
                            .Select("id", "url", "date", "name", "description", "usercreatorid")
                            .From("bookmarking_bookmark")
                            .Where("tenant", Tenant)
                            .SetFirstResult(firstResult)
                            .SetMaxResults(maxResults))
                            .ConvertAll(Converters.ToBookmark);
                return bookmarks;
            }
            catch
            {
                return new List<Bookmark>();
            }
        }

        internal IList<Bookmark> GetAllBookmarks()
        {
            try
            {
                var bookmarks = DbManager.ExecuteList(
                        new SqlQuery()
                            .Select("id", "url", "date", "name", "description", "usercreatorid")
                            .From("bookmarking_bookmark")
                            .Where("tenant", Tenant))
                            .ConvertAll(Converters.ToBookmark);
                return bookmarks;
            }
            catch
            {
                return new List<Bookmark>();
            }
        }

        internal Bookmark GetBookmarkByUrl(string url)
        {
            try
            {
                var bookmarks = DbManager.ExecuteList(
                        new SqlQuery()
                            .Select("id", "url", "date", "name", "description", "usercreatorid")
                            .From("bookmarking_bookmark")
                            .Where("url", url)
                            .Where("tenant", Tenant))
                            .ConvertAll(Converters.ToBookmark);

                if (bookmarks.Count > 0)
                {
                    ItemsCount = bookmarks.Count;
                    return bookmarks[0];
                }
            }
            catch { }
            return null;
        }

        internal long GetBookmarksCountCreatedByUser(Guid userID)
        {
            try
            {
                var count = DbManager.ExecuteScalar<long>(
                    new SqlQuery()
                        .SelectCount()
                        .From("bookmarking_bookmark")
                        .Where("UserCreatorID", userID)
                        .Where("Tenant", Tenant));

                SetBookmarksCount(count);
                return count;
            }
            catch
            {
                return 0;
            }
        }

        internal IList<Bookmark> GetBookmarksCreatedByUser(Guid userID, int firstResult, int maxResults)
        {
            try
            {
                var bookmarks = DbManager.ExecuteList(
                    new SqlQuery()
                        .Select("id", "url", "date", "name", "description", "usercreatorid")
                        .From("bookmarking_bookmark")
                        .Where("UserCreatorID", userID)
                        .Where("Tenant", Tenant)
                        .SetFirstResult(firstResult)
                        .SetMaxResults(maxResults)
                        .OrderBy("Date", false))
                    .ConvertAll(Converters.ToBookmark);

                return bookmarks;
            }
            catch
            {
                return new List<Bookmark>();
            }
        }

        internal Bookmark GetBookmarkByID(long id)
        {
            try
            {
                var bookmarks = DbManager.ExecuteList(
                        new SqlQuery()
                            .Select("id", "url", "date", "name", "description", "usercreatorid")
                            .From("bookmarking_bookmark")
                            .Where("ID", id)
                            .Where("Tenant", Tenant))
                            .ConvertAll(Converters.ToBookmark);
                if (bookmarks.Count == 1)
                {
                    return bookmarks[0];
                }
            }
            catch { }
            return null;
        }

        internal List<Bookmark> GetBookmarksByIDs(List<int> ids)
        {
            return DbManager.ExecuteList(
                new SqlQuery()
                    .Select("id", "url", "date", "name", "description", "usercreatorid")
                    .From("bookmarking_bookmark")
                    .Where(Exp.In("ID", ids.ToArray()))
                    .Where("Tenant", Tenant))
                .ConvertAll(Converters.ToBookmark);
        }

        #endregion

        #region CurrentUser

        internal UserInfo GetCurrentUser()
        {
            UserInfo userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            return userInfo;
        }

        internal Guid GetCurrentUserId()
        {
            return GetCurrentUser().ID;
        }

        #endregion

        #region Tags

        internal IList<Tag> GetAllTags(string startSymbols, int limit)
        {
            var q = new SqlQuery("bookmarking_tag")
                .Select("TagID, Name")
                .Where("Tenant", Tenant);

            if (!string.IsNullOrEmpty(startSymbols))
            {
                q.Where(Exp.Like("Name", startSymbols.ToLower(), SqlLike.StartWith));
            }
            if (0 < limit && limit < int.MaxValue)
            {
                q.SetMaxResults(limit);
            }

            return DbManager.ExecuteList(q).ConvertAll(Converters.ToTag);
        }

        internal IList<Tag> GetBookmarkTags(Bookmark b)
        {
            var tags = DbManager.ExecuteList(
                    new SqlQuery()
                        .Select("t.TagID, t.Name")
                        .From("bookmarking_tag t", "bookmarking_bookmarktag b")
                        .Where(Exp.Eq("b.BookmarkID", b.ID) & Exp.EqColumns("b.TagID", "t.TagID"))
                        .Where("t.Tenant", Tenant)
                        .Where("b.Tenant", Tenant))
                        .ConvertAll(Converters.ToTag);
            return tags;
        }

        internal IList<Tag> GetUserBookmarkTags(UserBookmark ub)
        {
            var tags = DbManager.ExecuteList(
                new SqlQuery()
                    .Select("t.TagID, t.Name")
                    .From("bookmarking_tag t", "bookmarking_userbookmarktag u")
                    .Where(Exp.Eq("u.UserBookmarkID", ub.UserBookmarkID) & Exp.EqColumns("u.TagID", "t.TagID"))
                    .Where("t.Tenant", Tenant)
                    .Where("u.Tenant", Tenant))
                .ConvertAll(Converters.ToTag);
            return tags;
        }

        #endregion

        #region Update Bookmark

        internal void UpdateBookmark(UserBookmark userBookmark, IList<Tag> tags)
        {
            var tx = DbManager.BeginTransaction();
            try
            {
                var date = DateTime.UtcNow;
                userBookmark.UserBookmarkID =
                    DbManager.ExecuteScalar<long>(
                        new SqlInsert("bookmarking_userbookmark", true)
                            .InColumns("UserBookmarkID", "UserID", "DateAdded", "Name", "Description", "BookmarkID", "Raiting", "Tenant")
                            .Values(userBookmark.UserBookmarkID, GetCurrentUserId(), date, userBookmark.Name, userBookmark.Description, userBookmark.BookmarkID, 1, Tenant)
                            .Identity(0, 0L, true));

                DbManager.ExecuteNonQuery(
                            new SqlDelete("bookmarking_userbookmarktag")
                            .Where(Exp.Eq("UserBookmarkID", userBookmark.UserBookmarkID)
                                    & Exp.Eq("Tenant", Tenant)));

                foreach (var tag in tags)
                {
                    tag.TagID = DbManager.ExecuteScalar<long>(
                            new SqlInsert("bookmarking_tag", true)
                            .InColumns("TagID", "Name", "Tenant")
                            .Values(tag.TagID, tag.Name, Tenant)
                            .Identity(0, 0L, true));

                    var ubt = new UserBookmarkTag { UserBookmarkID = userBookmark.UserBookmarkID, TagID = tag.TagID };

                    ubt.UserBookmarkTagID = DbManager.ExecuteScalar<long>(
                            new SqlInsert("bookmarking_userbookmarktag", true)
                            .InColumns("UserBookmarkID", "TagID", "Tenant")
                            .Values(ubt.UserBookmarkID, tag.TagID, Tenant)
                            .Identity(0, 0L, true));
                }
                tx.Commit();
            }
            catch (Exception)
            {
                tx.Rollback();
            }
        }

        internal void UpdateBookmark(Bookmark bookmark, IList<Tag> tags)
        {
            UserBookmark userBookmark = null;
            var tx = DbManager.BeginTransaction();
            try
            {
                var date = Core.Tenants.TenantUtil.DateTimeToUtc(bookmark.Date);

                bookmark.ID = DbManager.ExecuteScalar<long>(
                            new SqlInsert("bookmarking_bookmark", true)
                            .InColumns("ID", "URL", "Date", "Name", "Description", "UserCreatorID", "Tenant")
                            .Values(bookmark.ID, bookmark.URL, date, bookmark.Name, bookmark.Description, bookmark.UserCreatorID, Tenant)
                            .Identity(0, 0L, true));

                DbManager.ExecuteNonQuery(
                            new SqlDelete("bookmarking_bookmarktag")
                            .Where(Exp.Eq("BookmarkID", bookmark.ID)
                                    & Exp.Eq("Tenant", Tenant)));

                userBookmark = GetCurrentUserBookmark(bookmark);
                long userBookmarkId = 0;
                if (userBookmark != null)
                {
                    userBookmarkId = userBookmark.UserBookmarkID;
                }

                var nowDate = DateTime.UtcNow;

                userBookmarkId = DbManager.ExecuteScalar<long>(
                            new SqlInsert("bookmarking_userbookmark", true)
                            .InColumns("UserBookmarkID", "UserID", "DateAdded", "Name", "Description", "BookmarkID", "Raiting", "Tenant")
                            .Values(userBookmarkId, GetCurrentUserId(), nowDate, bookmark.Name, bookmark.Description, bookmark.ID, 1, Tenant)
                            .Identity(0, 0L, true));

                userBookmark = new UserBookmark
                {
                    UserBookmarkID = userBookmarkId,
                    BookmarkID = bookmark.ID,
                    UserID = GetCurrentUserId(),
                    DateAdded = nowDate,
                    Name = bookmark.Name,
                    Description = bookmark.Description,
                    Raiting = 1
                };

                DbManager.ExecuteNonQuery(
                            new SqlDelete("bookmarking_userbookmarktag")
                            .Where(Exp.Eq("UserBookmarkID", userBookmarkId)
                                    & Exp.Eq("Tenant", Tenant)));

                if (bookmark.Tags == null)
                {
                    bookmark.Tags = new List<Tag>();
                }
                foreach (var tag in tags)
                {
                    tag.TagID = DbManager.ExecuteScalar<long>(
                            new SqlInsert("bookmarking_tag", true)
                            .InColumns("TagID", "Name", "Tenant")
                            .Values(tag.TagID, tag.Name, Tenant)
                            .Identity(0, 0L, true));

                    new BookmarkTag
                        {
                            BookmarkID = bookmark.ID,
                            TagID = tag.TagID,
                            BookmarkTagID = DbManager.ExecuteScalar<long>(
                                new SqlInsert("bookmarking_bookmarktag", true)
                                    .InColumns("BookmarkID", "TagID", "Tenant")
                                    .Values(bookmark.ID, tag.TagID, Tenant)
                                    .Identity(0, 0L, true))
                        };

                    

                    var ubt = new UserBookmarkTag { UserBookmarkID = userBookmarkId, TagID = tag.TagID };

                    ubt.UserBookmarkTagID = DbManager.ExecuteScalar<long>(
                            new SqlInsert("bookmarking_userbookmarktag", true)
                            .InColumns("UserBookmarkID", "TagID", "Tenant")
                            .Values(ubt.UserBookmarkID, tag.TagID, Tenant)
                            .Identity(0, 0L, true));

                    if (bookmark.Tags.All(r => r.TagID == tag.TagID))
                    {
                        bookmark.Tags.Add(tag);
                    }
                }

                tx.Commit();
            }
            catch (Exception)
            {
                tx.Rollback();
            }

            if (userBookmark != null)
            {
                FactoryIndexer<BookmarksUserWrapper>.IndexAsync(BookmarksUserWrapper.Create(userBookmark, bookmark));
            }
        }

        #endregion

        internal Bookmark RemoveBookmarkFromFavourite(long bookmarkID, Guid? userID = null)
        {
            var tx = DbManager.BeginTransaction();
            try
            {
                var userBookmarkID = DbManager.ExecuteScalar<long>(
                                        new SqlQuery()
                                        .Select("UserBookmarkID")
                                        .From("bookmarking_userbookmark")
                                        .Where("BookmarkID", bookmarkID)
                                        .Where("Tenant", Tenant)
                                        .Where("UserID", userID ?? GetCurrentUserId()));

                var raiting = GetUserBookmarksCount(bookmarkID);

                DbManager.ExecuteNonQuery(
                            new SqlDelete("bookmarking_userbookmark")
                            .Where("UserBookmarkID", userBookmarkID)
                            .Where("Tenant", Tenant));

                DbManager.ExecuteNonQuery(
                            new SqlDelete("bookmarking_userbookmarktag")
                            .Where("UserBookmarkID", userBookmarkID)
                            .Where("Tenant", Tenant));

                if (raiting <= 1)
                {
                    DbManager.ExecuteNonQuery(
                            new SqlDelete("bookmarking_bookmarktag")
                            .Where("BookmarkID", bookmarkID)
                            .Where("Tenant", Tenant));

                    DbManager.ExecuteNonQuery(
                            new SqlDelete("bookmarking_comment")
                            .Where("BookmarkID", bookmarkID)
                            .Where("Tenant", Tenant));

                    DbManager.ExecuteNonQuery(
                            new SqlDelete("bookmarking_bookmark")
                            .Where("ID", bookmarkID)
                            .Where("Tenant", Tenant));

                    DbManager.ExecuteNonQuery(
                        new SqlDelete("bookmarking_tag")
                        .Where("tenant", Tenant)
                        .Where(!Exp.Exists(new SqlQuery("bookmarking_bookmarktag b").Select("b.tagid").Where("b.Tenant", Tenant).Where(Exp.EqColumns("b.TagID", "bookmarking_tag.TagID"))))
                        );

                    tx.Commit();
                    return null;
                }

                var bookmark = GetBookmarkByID(bookmarkID);
                tx.Commit();
                return bookmark;
            }
            catch
            {
                tx.Rollback();
            }
            return null;
        }

        #region Sorting (currently in use)
        internal IList<Bookmark> GetMostRecentBookmarks(int firstResult, int maxResults)
        {
            try
            {
                SetAllBookmarksCount();

                var bookmarks = DbManager.ExecuteList(
                                    new SqlQuery()
                                    .Select("id", "url", "date", "name", "description", "usercreatorid")
                                    .From("bookmarking_bookmark")
                                    .Where("Tenant", Tenant)
                                    .OrderBy("Date", false)
                                    .SetFirstResult(firstResult)
                                    .SetMaxResults(maxResults))
                                    .ConvertAll(Converters.ToBookmark);
                return bookmarks;
            }
            catch
            {
                return null;
            }
        }

        internal IList<Bookmark> GetMostRecentBookmarksWithRaiting(int firstResult, int maxResults)
        {
            try
            {
                var bookmarks = DbManager.ExecuteList(
                    new SqlQuery()
                        .Select("b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID", "count(b.ID)")
                        .From("bookmarking_bookmark as b", "bookmarking_userbookmark as ub")
                        .Where(Exp.EqColumns("b.ID", "ub.BookmarkID"))
                        .Where(Exp.Eq("b.Tenant", Tenant) & Exp.Eq("ub.Tenant", Tenant))
                        .GroupBy("b.ID")
                        .OrderBy("b.Date", false)
                        .SetFirstResult(firstResult)
                        .SetMaxResults(maxResults))
                    .ConvertAll(Converters.ToBookmarkWithRaiting);
                return bookmarks;
            }
            catch
            {
                return null;
            }
        }

        internal IList<Bookmark> GetBookmarksSortedByRaiting(DateTime timeInterval, int firstResult, int maxResults)
        {
            try
            {
                var count = DbManager.ExecuteScalar<long>(
                    new SqlQuery()
                        .SelectCount("distinct ID")
                        .From("bookmarking_bookmark")
                        .Where(Exp.Gt("Date", timeInterval))
                        .Where("Tenant", Tenant)
                    );
                SetBookmarksCount(count);

                var bookmarks = DbManager.Connection.CreateCommand(
                    @"select b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID, count(b.ID) as r
from bookmarking_bookmark as b, bookmarking_userbookmark as ub
where b.ID = ub.BookmarkID and b.Tenant = @Tenant and ub.Tenant = @Tenant and b.Date > @date
group By b.ID order By r desc  limit @first, @max")
                    .AddParameter("date", timeInterval)
                    .AddParameter("first", firstResult)
                    .AddParameter("max", maxResults)
                    .AddParameter("tenant", Tenant)
                    .ExecuteList()
                    .ConvertAll(Converters.ToBookmark);
                return bookmarks;
            }
            catch
            {
                return null;
            }
        }

        internal IList<Bookmark> GetFavouriteBookmarksSortedByRaiting(int firstResult, int maxResults)
        {
            try
            {
                var userID = GetCurrentUserId();

                var count = DbManager.ExecuteScalar<long>(
                    new SqlQuery()
                        .SelectCount("distinct b.ID")
                        .From("bookmarking_bookmark as b")
                        .From("bookmarking_userbookmark as u")
                        .Where(Exp.EqColumns("b.ID", "u.BookmarkID"))
                        .Where(Exp.Eq("u.UserID", userID))
                        .Where(Exp.Eq("b.Tenant", Tenant))
                        .Where(Exp.Eq("u.Tenant", Tenant)));

                SetBookmarksCount(count);

                var bookmarks = DbManager.Connection.CreateCommand(@"select b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID , count(b.ID)
from bookmarking_bookmark as b
inner join bookmarking_userbookmark as ub on ub.Tenant = @Tenant and ub.BookmarkID = b.ID
inner join bookmarking_userbookmark as allub on allub.Tenant = @Tenant and allub.BookmarkID = b.ID and allub.UserID = @userID
Where b.Tenant = @Tenant 
Group By b.ID Order By count(ub.BookmarkID) DESC, Date DESC limit @first, @max")
                                        .AddParameter("Tenant", Tenant)
                                        .AddParameter("userID", userID)
                                        .AddParameter("first", firstResult)
                                        .AddParameter("max", maxResults)
                                        .ExecuteList()
                                        .ConvertAll(Converters.ToBookmark);

                return bookmarks;
            }
            catch
            {
                return null;
            }
        }

        internal IList<Bookmark> GetFavouriteBookmarksSortedByDate(int firstResult, int maxResults)
        {
            try
            {
                var userID = GetCurrentUserId();
                var count = DbManager.ExecuteScalar<long>(
                    new SqlQuery()
                        .SelectCount("b.ID")
                        .From("bookmarking_bookmark b", "bookmarking_userbookmark u")
                        .Where(Exp.EqColumns("b.ID", "u.BookmarkID"))
                        .Where("u.UserID", userID)
                        .Where("b.Tenant", Tenant)
                        .Where("u.Tenant", Tenant));

                SetBookmarksCount(count);

                var bookmarks = DbManager.ExecuteList(
                    new SqlQuery()
                        .Select("b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID")
                        .From("bookmarking_bookmark b", "bookmarking_userbookmark u")
                        .Where(Exp.EqColumns("b.ID", "u.BookmarkID"))
                        .Where("u.UserID", userID)
                        .Where("b.Tenant", Tenant)
                        .Where("u.Tenant", Tenant)
                        .OrderBy("u.DateAdded", false)
                        .SetFirstResult(firstResult)
                        .SetMaxResults(maxResults))
                        .ConvertAll(Converters.ToBookmark);

                return bookmarks;
            }
            catch
            {
                return null;
            }
        }

        internal IList<Bookmark> GetMostPopularBookmarks(List<long> ids, int firstResult, int maxResults)
        {
            try
            {
                var tenant = Tenant;

                var count = DbManager.ExecuteScalar<long>(
                    new SqlQuery()
                        .SelectCount("distinct b.ID")
                                    .From("bookmarking_bookmark as b", "bookmarking_userbookmark as ub")
                                    .Where(Exp.In("b.ID", ids))
                                    .Where(Exp.EqColumns("b.ID", "ub.BookmarkID"))
                                    .Where(Exp.Eq("b.Tenant", tenant) & Exp.Eq("ub.Tenant", tenant)));

                SetBookmarksCount(count);

                var bookmarks = DbManager.ExecuteList(
                                    new SqlQuery()
                                    .Select("b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID", "count(b.ID)")
                                    .From("bookmarking_bookmark as b", "bookmarking_userbookmark as ub")
                                    .Where(Exp.In("b.ID", ids))
                                    .Where(Exp.EqColumns("b.ID", "ub.BookmarkID"))
                                    .Where(Exp.Eq("b.Tenant", tenant) & Exp.Eq("ub.Tenant", tenant))
                                    .GroupBy("b.ID")
                                    .OrderBy("count(ub.BookmarkID)", false)
                                    .OrderBy("Date", false)
                                    .SetFirstResult(firstResult)
                                    .SetMaxResults(maxResults))
                                    .ConvertAll(Converters.ToBookmarkWithRaiting);

                return bookmarks;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Tags

        internal IList<Tag> GetFavouriteTagsSortByPopularity(int limit)
        {
            try
            {
                if (limit < 1)
                {
                    limit = 1;
                }
                var tags = DbManager.Connection.CreateCommand(@"select t.TagID, t.Name, count(t.TagID) as TagsCount
from bookmarking_userbookmarktag as ubt, bookmarking_tag as t, bookmarking_userbookmark as ub
where ubt.Tenant = @Tenant and t.TagID = ubt.TagID and t.Tenant = @Tenant and ub.UserBookmarkID = ubt.UserBookmarkID and ub.UserID = @userId
group by t.TagID order by TagsCount desc limit @l")
                                        .AddParameter("Tenant", Tenant)
                                        .AddParameter("userId", GetCurrentUserId())
                                        .AddParameter("l", limit)
                                        .ExecuteList()
                                        .ConvertAll(Converters.ToTagWithRaiting);
                return tags;
            }
            catch
            {
                return null;
            }
        }


        internal IList<Bookmark> GetMostPopularBookmarksByTag(Tag t, int limit)
        {
            return SearchMostPopularBookmarksByTag(t.Name, 0, limit);
        }

        internal IList<Tag> GetRelatedTagsSortedByName(long bookmarkID, int tagsCount)
        {
            var tags = DbManager.Connection.CreateCommand(
                @"select distinct allbtt.TagID, t.Name, (select count(*) from bookmarking_bookmarktag as a where a.TagID = allbtt.TagID) as Raiting
from bookmarking_bookmark as b
inner join bookmarking_bookmarktag as bt on b.ID = bt.BookmarkID
inner join bookmarking_bookmarktag as btt on bt.TagID = btt.TagID and btt.BookmarkID != b.ID
inner join bookmarking_bookmarktag as allbtt on btt.BookmarkID = allbtt.BookmarkID and allbtt.TagID != bt.TagID
inner join bookmarking_tag as t on allbtt.TagID = t.TagID 
where b.ID = @bookmarkID
group by TagID order by t.Name asc limit @l")
                .AddParameter("bookmarkID", bookmarkID)
                .AddParameter("l", tagsCount)
                .ExecuteList()
                .ConvertAll(Converters.ToTagWithRaiting);
            return tags;
        }

        #endregion

        #region Pagination

        private void SetAllBookmarksCount()
        {
            var count = DbManager.ExecuteScalar<long>(
                    new SqlQuery()
                        .SelectCount()
                        .From("bookmarking_bookmark")
                        .Where("Tenant", Tenant));

            SetBookmarksCount(count);
        }

        private long SetBookmarksCount(long count)
        {
            ItemsCount = count;
            BookmarkingHibernateDao.UpdateCurrentInstanse(this);
            return ItemsCount;
        }

        #endregion

        #region Comments

        internal long GetCommentsCount(long bookmarkID)
        {
            var count = DbManager.ExecuteScalar<long>(
                                new SqlQuery()
                                .SelectCount("ID")
                                .From("bookmarking_comment")
                                .Where(Exp.Eq("BookmarkID", bookmarkID) & Exp.Eq("Inactive", 0) & Exp.Eq("Tenant", Tenant)));
            return count;
        }

        internal Bookmark GetCommentBookmark(Comment c)
        {
            var result = DbManager.ExecuteList(
                new SqlQuery()
                    .Select("b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID")
                    .From("bookmarking_bookmark b", "bookmarking_comment c")
                    .Where(Exp.Eq("c.ID", c.ID) & Exp.Eq("c.BookmarkID", c.BookmarkID)
                           & Exp.EqColumns("c.BookmarkID", "b.ID")
                           & Exp.Eq("c.Tenant", Tenant)
                           & Exp.Eq("b.Tenant", Tenant)))
                .ConvertAll(Converters.ToBookmark);

            return result.Count == 1 ? result[0] : null;
        }

        internal IList<Comment> GetBookmarkComments(Bookmark b)
        {
            var comments = DbManager.ExecuteList(
                                new SqlQuery()
                                .Select("ID, UserID, Content, Datetime, Parent, BookmarkID, Inactive")
                                .From("bookmarking_comment")
                                .Where(Exp.Eq("BookmarkID", b.ID) & Exp.Eq("Tenant", Tenant)))
                                .ConvertAll(Converters.ToComment);

            return comments;
        }

        internal Comment GetCommentById(Guid commentID)
        {
            var comments = DbManager.ExecuteList(
                new SqlQuery()
                    .Select("ID, UserID, Content, Datetime, Parent, BookmarkID, Inactive")
                    .From("bookmarking_comment")
                    .Where(Exp.Eq("ID", commentID) & Exp.Eq("Tenant", Tenant)))
                .ConvertAll(Converters.ToComment);

            return comments.Count == 1 ? comments[0] : null;
        }

        internal Comment UpdateComment(Guid commentID, string text)
        {
            var tx = DbManager.BeginTransaction();
            try
            {
                var c = GetCommentById(commentID);
                c.Content = text;

                DbManager.ExecuteNonQuery(
                    new SqlUpdate("bookmarking_comment")
                        .Set("Content", c.Content)
                        .Where(Exp.Eq("ID", commentID) & Exp.Eq("Tenant", Tenant)));
                tx.Commit();
                return c;
            }
            catch
            {
                tx.Rollback();
                return null;
            }
        }

        internal Comment RemoveComment(Guid commentID)
        {
            var tx = DbManager.BeginTransaction();
            try
            {
                var c = GetCommentById(commentID);
                c.Inactive = true;

                DbManager.ExecuteNonQuery(
                            new SqlUpdate("bookmarking_comment")
                            .Set("Inactive", 1)
                            .Where(Exp.Eq("ID", commentID) & Exp.Eq("Tenant", Tenant)));

                tx.Commit();
                return c;
            }
            catch
            {
                tx.Rollback();
                return null;
            }
        }

        internal Comment AddComment(Comment comment)
        {
            try
            {
                var date = Core.Tenants.TenantUtil.DateTimeToUtc(comment.Datetime);
                DbManager.ExecuteNonQuery(
                    new SqlInsert("bookmarking_comment", true)
                        .InColumns("ID", "UserID", "Content", "Datetime", "Parent", "BookmarkID", "Inactive", "Tenant")
                        .Values(comment.ID, comment.UserID, comment.Content, date, comment.Parent, comment.BookmarkID, comment.Inactive, Tenant));

                return comment;
            }
            catch
            {
                return null;
            }
        }

        public IList<Comment> GetChildComments(Comment c)
        {
            var children = DbManager.ExecuteList(
                            new SqlQuery()
                            .Select("ID, UserID, Content, Datetime, Parent, BookmarkID, Inactive")
                            .From("bookmarking_comment")
                            .Where(Exp.Eq("Parent", c.ID) & Exp.Eq("Tenant", Tenant)))
                            .ConvertAll(Converters.ToComment);

            return children;
        }
        #endregion

        #region Search

        internal IList<Bookmark> SearchBookmarks(IList<string> searchStringList, int firstResult, int maxResults)
        {
            return SearchBookmarks(searchStringList, firstResult, maxResults, false);
        }

        internal IList<Bookmark> SearchBookmarks(IEnumerable<string> searchStringList, int firstResult, int maxResults, bool skipCountQuery)
        {
            try
            {
                if (searchStringList == null) return new List<Bookmark>();

                searchStringList = searchStringList
                    .Select(w => (w ?? string.Empty).Trim().Trim('/', '\\').ToLower())
                    .Where(w => w.Length >= 3);

                if (!searchStringList.Any()) return new List<Bookmark>();

                IReadOnlyCollection<BookmarksUserWrapper> bookmarks;
                if (FactoryIndexer<BookmarksUserWrapper>.TrySelect(r => r.MatchAll(string.Join(" ", searchStringList.ToArray())), out bookmarks))
                {
                    return GetBookmarksByIDs(bookmarks.Select(r => r.BookmarkID).ToList());
                }

                var sb = new StringBuilder();
                sb.Append(@" from bookmarking_bookmark b, bookmarking_tag t, bookmarking_bookmarktag g
where b.Tenant = @Tenant and g.BookmarkID = b.ID and t.TagID = g.TagID ");

                var i = 0;
                foreach (var w in searchStringList)
                {
                    sb.AppendFormat(@" and ( lower(b.url) like @w{0} or lower(b.name) like @w{0} or lower(b.description) like @w{0} or lower(t.name) like @w{0} ) ", i);
                    i++;
                }
                sb.Append(@" order by b.Date desc ");

                var query = DbManager.Connection.CreateCommand("select distinct b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID" + sb + " limit @FirstResult, @MaxResults")
                                .AddParameter("Tenant", Tenant)
                                .AddParameter("FirstResult", firstResult)
                                .AddParameter("MaxResults", maxResults);

                var countQuery = DbManager.Connection.CreateCommand("select count(distinct b.ID)" + sb)
                                .AddParameter("Tenant", Tenant);

                i = 0;
                foreach (var w in searchStringList)
                {
                    query.AddParameter("w" + i, "%" + w + "%");
                    countQuery.AddParameter("w" + i, "%" + w + "%");
                    i++;
                }

                if (!skipCountQuery)
                {
                    SetBookmarksCount(countQuery.ExecuteScalar<long>());
                }

                var bookmakrs = query.ExecuteList().ConvertAll(Converters.ToBookmark);
                return bookmakrs;
            }
            catch
            {
                return null;
            }
        }

        internal IList<Bookmark> SearchBookmarksByTag(string searchString, int firstResult, int maxResults)
        {
            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(searchString))
            {
                return GetAllBookmarks(firstResult, maxResults);
            }

            const string selectClause = @"select distinct b.ID, b.URL, b.Date, ub.Name, ub.Description, b.UserCreatorID";

            const string selectCountClause = @"select count(distinct b.ID)";

            sb.Append(@" from bookmarking_userbookmark as ub, bookmarking_bookmark as b, bookmarking_userbookmarktag as ubt, bookmarking_tag as t
where b.ID = ub.BookmarkID and ubt.UserBookmarkID = ub.UserBookmarkID and t.TagID = ubt.TagID and t.Name like @tagName and ub.Tenant = @Tenant
order by b.Date desc ");

            const string limitString = @"limit @first,@max";

            try
            {
                var count = DbManager.Connection.CreateCommand(selectCountClause + sb)
                                        .AddParameter("tagName", searchString)
                                        .AddParameter("Tenant", Tenant)
                                        .ExecuteScalar<long>();

                SetBookmarksCount(count);



                var bookmarks = DbManager.Connection.CreateCommand(selectClause + sb + limitString)
                                        .AddParameter("tagName", searchString)
                                        .AddParameter("Tenant", Tenant)
                                        .AddParameter("first", firstResult)
                                        .AddParameter("max", maxResults)
                                        .ExecuteList()
                                        .ConvertAll(Converters.ToBookmark);
                return bookmarks;
            }
            catch
            {
                return null;
            }
        }

        internal IList<Bookmark> SearchMostPopularBookmarksByTag(string tagName, int firstResult, int maxResults)
        {
            try
            {
                var bookmarks = DbManager.Connection.CreateCommand(@"select  b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID , t.TagID, count(b.ID) as CountIds
from bookmarking_userbookmark as ub, bookmarking_userbookmarktag as ubt, bookmarking_tag as t, bookmarking_bookmark as b
where b.ID = ub.BookmarkID and ub.Tenant = @Tenant and ub.UserBookmarkID = ubt.UserBookmarkID and ubt.TagID = t.TagID and t.Name = @tagName 
group by b.ID, t.TagID order by CountIds desc limit @FirstResult, @MaxResults")
                        .AddParameter("tagName", tagName)
                        .AddParameter("Tenant", Tenant)
                        .AddParameter("FirstResult", firstResult)
                        .AddParameter("MaxResults", maxResults)
                        .ExecuteList()
                        .ConvertAll(Converters.ToBookmarkWithRaiting);

                return bookmarks;
            }
            catch
            {
                return null;
            }
        }

        internal IList<Bookmark> SearchMostPopularBookmarksByTag(IList<Tag> tags, int firstResult, int maxResults)
        {
            try
            {
                var tagsString = new StringBuilder();

                for (int i = 0; i < tags.Count; i++)
                {
                    tagsString.AppendFormat("@tagName{0}", i);
                    if (i < tags.Count - 1)
                    {
                        tagsString.Append(", ");
                    }
                }

                var command = DbManager.Connection.CreateCommand(string.Format(@"select distinct b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID , count(b.ID) as CountIds
from bookmarking_userbookmark as ub, bookmarking_userbookmarktag as ubt, bookmarking_tag as t, bookmarking_bookmark as b
where b.ID = ub.BookmarkID and ub.Tenant = @Tenant and ub.UserBookmarkID = ubt.UserBookmarkID and ubt.TagID = t.TagID and t.Name in ({0}) 
group by  b.ID, t.TagID order by CountIds desc limit @FirstResult, @MaxResults", tagsString))
                    .AddParameter("Tenant", Tenant)
                    .AddParameter("FirstResult", firstResult)
                    .AddParameter("MaxResults", maxResults);
                int j = 0;
                foreach (var t in tags)
                {
                    command.AddParameter(string.Format("tagName{0}", j++), t.Name);
                }

                var bookmarks = command.ExecuteList().ConvertAll(Converters.ToBookmarkWithRaiting);
                SetAllBookmarkTags(bookmarks);
                return bookmarks;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region User Bookmarks

        internal IList<UserBookmark> GetUserBookmarks(Bookmark b)
        {
            var userBookmarks = DbManager.ExecuteList(
                                    new SqlQuery()
                                    .Select("UserBookmarkID, UserID, DateAdded, Name, Description, BookmarkID, Raiting")
                                    .From("bookmarking_userbookmark")
                                    .Where(Exp.Eq("BookmarkID", b.ID) & Exp.Eq("Tenant", Tenant))
                                    )
                                    .ConvertAll(Converters.ToUserBookmark);
            return userBookmarks;
        }

        internal long GetUserBookmarksCount(Bookmark b)
        {
            return GetUserBookmarksCount(b.ID);
        }

        private long GetUserBookmarksCount(long bookmarkID)
        {
            var count = DbManager.ExecuteScalar<long>(
                new SqlQuery()
                    .SelectCount("UserBookmarkID")
                    .From("bookmarking_userbookmark")
                    .Where(Exp.Eq("BookmarkID", bookmarkID) & Exp.Eq("Tenant", Tenant)));
            return count;
        }

        internal UserBookmark GetCurrentUserBookmark(Bookmark b)
        {
            var userBookmark = DbManager.ExecuteList(
                                            new SqlQuery()
                                            .Select("UserBookmarkID, UserID, DateAdded, Name, Description, BookmarkID, Raiting")
                                            .From("bookmarking_userbookmark")
                                            .Where(Exp.Eq("UserID", GetCurrentUserId())
                                                   & Exp.Eq("BookmarkID", b.ID)
                                                   & Exp.Eq("Tenant", Tenant)))
                                            .ConvertAll(Converters.ToUserBookmark);
            return userBookmark.Count == 1 ? userBookmark[0] : null;
        }

        internal Bookmark GetBookmarkWithUserBookmarks(string url)
        {
            var bookmarks = DbManager.ExecuteList(
                new SqlQuery()
                    .Select("b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID",
                            "u.UserBookmarkID, u.UserID, u.DateAdded, u.Name, u.Description, u.BookmarkID, u.Raiting",
                            "c.ID, c.UserID, c.Content, c.Datetime, c.Parent, c.BookmarkID, c.Inactive")
                    .From("bookmarking_bookmark b")
                    .LeftOuterJoin("bookmarking_userbookmark u", Exp.EqColumns("b.ID", "u.BookmarkID"))
                    .LeftOuterJoin("bookmarking_comment c", Exp.EqColumns("b.ID", "c.BookmarkID") & Exp.Eq("c.tenant", Tenant))
                    .Where(Exp.Eq("b.URL", url) & Exp.Eq("b.Tenant", Tenant)))
                .ConvertAll(Converters.ToBookmarkWithUserBookmarks);


            var result = Converters.ToBookmarkWithUserBookmarks(bookmarks);
            SetBookmarkTags(result);
            return result;
        }

        internal IList<Bookmark> GetFullBookmarksInfo(IList<long> bookmarkIds)
        {
            var ids = bookmarkIds as List<long>;
            var bookmarks = DbManager.ExecuteList(
                new SqlQuery()
                    .Select("b.ID, b.URL, b.Date, b.Name, b.Description, b.UserCreatorID",
                            "u.UserBookmarkID, u.UserID, u.DateAdded, u.Name, u.Description, u.BookmarkID, u.Raiting",
                            "c.ID, c.UserID, c.Content, c.Datetime, c.Parent, c.BookmarkID, c.Inactive")
                    .From("bookmarking_bookmark b")
                    .LeftOuterJoin("bookmarking_userbookmark u", Exp.EqColumns("b.ID", "u.BookmarkID"))
                    .LeftOuterJoin("bookmarking_comment c", Exp.EqColumns("b.ID", "c.BookmarkID") & Exp.Eq("c.tenant", Tenant))
                    .Where(Exp.In("b.ID", ids.ToArray()) & Exp.Eq("b.Tenant", Tenant)))
                .ConvertAll(Converters.ToBookmarkWithUserBookmarks);

            var result = Converters.ToBookmarkWithUserBookmarks(bookmarks, bookmarkIds);

            SetBookmarkTags(result);
            return result;
        }

        internal void SetBookmarkTags(Bookmark b)
        {
            if (b == null)
            {
                return;
            }
            var bookmarks = new List<Bookmark> { b };
            SetBookmarkTags(bookmarks);
        }

        internal void SetBookmarkTags(IList<Bookmark> bookmarks)
        {
            if (bookmarks == null || bookmarks.Count == 0)
            {
                return;
            }

            var ids = (from b in bookmarks
                       select b.ID).Distinct<long>().ToList<long>();

            var tags = DbManager.ExecuteList(
                new SqlQuery()
                .Select("ub.BookmarkID", "ubt.TagID", "t.Name")
                .From("bookmarking_userbookmark as ub")
                .LeftOuterJoin("bookmarking_userbookmarktag as ubt", Exp.EqColumns("ub.UserBookmarkID", "ubt.UserBookmarkID"))
                .LeftOuterJoin("bookmarking_tag as t", Exp.EqColumns("ubt.TagID", "t.TagID"))
                .Where(Exp.In("ub.BookmarkID", ids) & Exp.Eq("ub.Tenant", Tenant) & Exp.Eq("ub.UserID", GetCurrentUserId())))
                .ConvertAll(Converters.ToTagWithBookmarkID);

            tags.RemoveAll(x => x.TagID == 0);
            Converters.SetBookmarksTags(bookmarks, tags);

            ids = (from b in bookmarks
                   where b.Tags.Count == 0
                   select b.ID
                   ).Distinct<long>().ToList<long>();
            if (ids.Count != 0)
            {
                tags = DbManager.ExecuteList(
                                                new SqlQuery()
                                                .Select("bt.BookmarkID", "bt.TagID", "t.Name")
                                                .From("bookmarking_bookmarktag as bt")
                                                .LeftOuterJoin("bookmarking_tag as t", Exp.EqColumns("bt.TagID", "t.TagID"))
                                                .Where(Exp.In("bt.BookmarkID", ids) & Exp.Eq("bt.Tenant", Tenant)))
                                                .ConvertAll(Converters.ToTagWithBookmarkID);
                tags.RemoveAll(x => x.TagID == 0);

                var bookmarksToSet = (from b in bookmarks
                                      where b.Tags.Count == 0
                                      select b
                                      ).ToList<Bookmark>();

                Converters.SetBookmarksTags(bookmarksToSet, tags);
            }
        }

        internal void SetAllBookmarkTags(IList<Bookmark> bookmarks)
        {
            var ids = (from b in bookmarks
                       select b.ID).Distinct<long>().ToList<long>();

            var tags = DbManager.ExecuteList(
                                            new SqlQuery()
                                            .Select("ub.BookmarkID", "ubt.TagID", "t.Name")
                                            .From("bookmarking_userbookmark as ub")
                                            .LeftOuterJoin("bookmarking_userbookmarktag as ubt", Exp.EqColumns("ub.UserBookmarkID", "ubt.UserBookmarkID"))
                                            .LeftOuterJoin("bookmarking_tag as t", Exp.EqColumns("ubt.TagID", "t.TagID"))
                                            .Where(Exp.In("ub.BookmarkID", ids) & Exp.Eq("ub.Tenant", Tenant) & Exp.Eq("ub.UserID", GetCurrentUserId())))
                                            .ConvertAll(Converters.ToTagWithBookmarkID);


            tags.AddRange(DbManager.ExecuteList(
                                            new SqlQuery()
                                            .Select("bt.BookmarkID", "bt.TagID", "t.Name")
                                            .From("bookmarking_bookmarktag as bt")
                                            .LeftOuterJoin("bookmarking_tag as t", Exp.EqColumns("bt.TagID", "t.TagID"))
                                            .Where(Exp.In("bt.BookmarkID", ids) & Exp.Eq("bt.Tenant", Tenant)))
                                            .ConvertAll(Converters.ToTagWithBookmarkID));
            Converters.SetBookmarksTags(bookmarks, tags);
        }

        #endregion

        internal IList<Bookmark> GetBookmarksUpdates(DateTime from, DateTime to)
        {
            var query = new SqlQuery("bookmarking_bookmark")
                .SelectAll()
                .Where(Exp.Eq("Tenant", Tenant) & Exp.Between("Date", from, to));
            return DbManager.ExecuteList(query).Select(Converters.ToBookmark).ToList();
        }

        internal IList<Comment> GetCommentsUpdates(DateTime from, DateTime to)
        {
            var query = new SqlQuery("bookmarking_comment")
                .SelectAll()
                .Where(Exp.Eq("Tenant", Tenant) & Exp.Between("Datetime", from, to));
            return DbManager.ExecuteList(query).Select(Converters.ToComment).ToList();
        }
    }
}