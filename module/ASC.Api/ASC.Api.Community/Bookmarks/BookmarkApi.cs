/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

using ASC.Api.Attributes;
using ASC.Api.Bookmarks;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Bookmarking.Business;
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Pojo;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility.HtmlUtility;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Common.Util;

namespace ASC.Api.Community
{
    public partial class CommunityApi
    {
        private BookmarkingService _bookmarkingDao;

        private BookmarkingService BookmarkingDao
        {
            get { return _bookmarkingDao ?? (_bookmarkingDao = BookmarkingService.GetCurrentInstanse()); }
        }

        ///<summary>
        ///Returns a list of all the portal bookmarks with the bookmark titles, dates of creation and update, bookmark texts and authors.
        ///</summary>
        ///<short>
        ///Get bookmarks
        ///</short>
        ///<category>Bookmarks</category>
        ///<returns>List of all bookmarks</returns>
        [Read("bookmark")]
        public IEnumerable<BookmarkWrapper> GetBookmarks()
        {
            var bookmarks = BookmarkingDao.GetAllBookmarks((int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return bookmarks.Select(x => new BookmarkWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of all the bookmarks for the current user with the bookmark titles, dates of creation and update, bookmark texts and author.
        ///</summary>
        ///<short>
        ///Get my bookmarks
        ///</short>
        ///<category>Bookmarks</category>
        ///<returns>List of bookmarks</returns>
        [Read("bookmark/@self")]
        public IEnumerable<BookmarkWrapper> GetMyBookmarks()
        {
            var bookmarks = BookmarkingDao.GetBookmarksCreatedByUser(SecurityContext.CurrentAccount.ID, (int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return bookmarks.Select(x => new BookmarkWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of bookmarks matching the search query specified in the request with the bookmark titles, dates of creation and update, bookmark descriptions and author.
        ///</summary>
        ///<short>
        ///Search bookmarks
        ///</short>
        ///<category>Bookmarks</category>
        /// <param name="query">Search query</param>
        ///<returns>List of bookmarks</returns>
        [Read("bookmark/@search/{query}")]
        public IEnumerable<BookmarkWrapper> SearchBookmarks(string query)
        {
            var bookmarks = BookmarkingDao.SearchBookmarks(new List<string> { query }, (int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return bookmarks.Select(x => new BookmarkWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of favorite bookmarks for the current user with the bookmark titles, dates of creation and update, bookmark texts and author.
        ///</summary>
        ///<short>
        ///Get my favorite bookmarks
        ///</short>
        ///<category>Bookmarks</category>
        ///<returns>List of bookmarks</returns>
        [Read("bookmark/@favs")]
        public IEnumerable<BookmarkWrapper> GetFavsBookmarks()
        {
            var bookmarks = BookmarkingDao.GetFavouriteBookmarksSortedByDate((int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return bookmarks.Select(x => new BookmarkWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of all the bookmark tags with a number specifying the tag usage.
        ///</summary>
        ///<short>
        ///Get all tags
        ///</short>
        ///<category>Bookmarks</category>
        ///<returns>List of tags</returns>
        [Read("bookmark/tag")]
        public IEnumerable<TagWrapper> GetBookmarksTags()
        {
            var bookmarks = BookmarkingDao.GetAllTags();
            return bookmarks.Select(x => new TagWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of all the bookmarks marked by the tag specified in the request with the bookmark titles, dates of creation and update, bookmark texts and authors.
        ///</summary>
        ///<short>
        ///Get a bookmark by a tag
        ///</short>
        ///<category>Bookmarks</category>
        ///<param name="tag">Tag name</param>
        ///<returns>List of bookmarks</returns>
        [Read("bookmark/bytag")]
        public IEnumerable<BookmarkWrapper> GetBookmarksByTag(string tag)
        {
            var bookmarks = BookmarkingDao.SearchBookmarksByTag(tag, (int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return bookmarks.Select(x => new BookmarkWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of recently added bookmarks with the bookmark titles, dates of creation and update, bookmark texts and authors.
        ///</summary>
        ///<short>
        ///Get recent bookmarks
        ///</short>
        ///<category>Bookmarks</category>
        ///<returns>List of bookmarks</returns>
        [Read("bookmark/top/recent")]
        public IEnumerable<BookmarkWrapper> GetRecentBookmarks()
        {
            var bookmarks = BookmarkingDao.GetMostRecentBookmarks((int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return bookmarks.Select(x => new BookmarkWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of the bookmarks most popular for the current date with the bookmark titles, dates of creation and update, bookmark texts and authors.
        ///</summary>
        ///<short>
        ///Get top of day bookmarks
        ///</short>
        ///<category>Bookmarks</category>
        ///<returns>List of bookmarks</returns>
        [Read("bookmark/top/day")]
        public IEnumerable<BookmarkWrapper> GetTopDayBookmarks()
        {
            var bookmarks = BookmarkingDao.GetTopOfTheDay((int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return bookmarks.Select(x => new BookmarkWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of the bookmarks most popular for the current month with the bookmark titles, dates of creation and update, bookmark texts and authors.
        ///</summary>
        ///<short>
        ///Get top of month bookmarks
        ///</short>
        ///<category>Bookmarks</category>
        ///<returns>List of bookmarks</returns>
        [Read("bookmark/top/month")]
        public IEnumerable<BookmarkWrapper> GetTopMonthBookmarks()
        {
            var bookmarks = BookmarkingDao.GetTopOfTheMonth((int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return bookmarks.Select(x => new BookmarkWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of the bookmarks most popular for the current week with the bookmark titles, dates of creation and update, bookmark texts and authors.
        ///</summary>
        ///<short>
        ///Get top of week bookmarks
        ///</short>
        ///<category>Bookmarks</category>
        ///<returns>List of bookmarks</returns>
        [Read("bookmark/top/week")]
        public IEnumerable<BookmarkWrapper> GetTopWeekBookmarks()
        {
            var bookmarks = BookmarkingDao.GetTopOfTheWeek((int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return bookmarks.Select(x => new BookmarkWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns a list of the bookmarks most popular for the current year with the bookmark titles, dates of creation and update, bookmark texts and authors.
        ///</summary>
        ///<short>
        ///Get top of year bookmarks
        ///</short>
        ///<category>Bookmarks</category>
        ///<returns>List of bookmarks</returns>
        [Read("bookmark/top/year")]
        public IEnumerable<BookmarkWrapper> GetTopYearBookmarks()
        {
            var bookmarks = BookmarkingDao.GetTopOfTheYear((int)_context.StartIndex, (int)_context.Count);
            _context.SetDataPaginated();
            return bookmarks.Select(x => new BookmarkWrapper(x)).ToSmartList();
        }

        ///<summary>
        /// Returns a list of all the comments on the bookmark with the ID specified in the request.
        ///</summary>
        ///<short>
        /// Get bookmark comments
        ///</short>
        ///<category>Bookmarks</category>
        ///<param name="id">Bookmark ID</param>
        ///<returns>List of bookmark comments</returns>
        [Read("bookmark/{id}/comment")]
        public IEnumerable<BookmarkCommentWrapper> GetBookmarkComments(long id)
        {
            var comments = BookmarkingDao.GetBookmarkComments(BookmarkingDao.GetBookmarkByID(id));
            return comments.Select(x => new BookmarkCommentWrapper(x)).ToSmartList();
        }

        ///<summary>
        /// Adds a comment to the bookmark with the ID specified in the request. The parent bookmark ID can be also specified if needed.
        ///</summary>
        ///<short>
        /// Add a bookmark comment by bookmark ID
        ///</short>
        ///<param name="id">Bookmark ID</param>
        ///<param name="content">Comment text</param>
        ///<param name="parentId">Parent comment ID</param>
        ///<returns>List of bookmark comments</returns>
        /// <example>
        /// <![CDATA[
        /// Sending data in application/json:
        /// 
        /// {
        ///     content:"My comment",
        ///     parentId:"9924256A-739C-462b-AF15-E652A3B1B6EB"
        /// }
        /// 
        /// Sending data in application/x-www-form-urlencoded
        /// content="My comment"&parentId="9924256A-739C-462b-AF15-E652A3B1B6EB"
        /// ]]>
        /// </example>
        /// <remarks>
        /// Send parentId=00000000-0000-0000-0000-000000000000 or doesn't send it at all if you want your comment to be on the root level.
        /// </remarks>
        /// <category>Bookmarks</category>
        [Create("bookmark/{id}/comment")]
        public BookmarkCommentWrapper AddBookmarkComment(long id, string content, Guid parentId)
        {
            var bookmark = BookmarkingDao.GetBookmarkByID(id);
            if (bookmark == null) throw new ItemNotFoundException("bookmark not found");

            var comment = new Comment
            {
                ID = Guid.NewGuid(),
                BookmarkID = id,
                Content = content,
                Datetime = DateTime.UtcNow,
                UserID = SecurityContext.CurrentAccount.ID,
                Parent = parentId.ToString()
            };
            BookmarkingDao.AddComment(comment);
            return new BookmarkCommentWrapper(comment);
        }

        ///<summary>
        /// Returns the detailed information on the bookmark with the ID specified in the request.
        ///</summary>
        ///<short>
        /// Get a bookmark by ID
        ///</short>
        ///<param name="id">Bookmark ID</param>
        ///<returns>Bookmark information</returns>
        ///<category>Bookmarks</category>
        [Read("bookmark/{id}")]
        public BookmarkWrapper GetBookmarkById(long id)
        {
            return new BookmarkWrapper(BookmarkingDao.GetBookmarkByID(id));
        }

        ///<summary>
        /// Adds a bookmark with the title, description and tags specified in the request.
        ///</summary>
        ///<short>
        /// Add a bookmark
        ///</short>
        ///<param name="url">Absolute URL to the bookmark page</param>
        ///<param name="title">Bookmark title</param>
        ///<param name="description">Bookmark description</param>
        ///<param name="tags">Bookmark tags separated by semicolon</param>
        ///<returns>Newly added bookmark</returns>
        /// <example>
        /// <![CDATA[
        /// Sending data in application/json:
        /// 
        /// {
        ///     url:"https://www.teamlab.com",
        ///     title: "TeamLab",
        ///     description: "best site i've ever seen",
        ///     tags: "project management, collaboration"
        /// }
        /// 
        /// Sending data in application/x-www-form-urlencoded
        /// url="https://www.teamlab.com"&title="TeamLab"&description="best site i've ever seen"&tags="project management, collaboration"
        /// ]]>
        /// </example>
        /// <category>Bookmarks</category>
        [Create("bookmark")]
        public BookmarkWrapper AddBookmark(string url, string title, string description, string tags)
        {
            try
            {
                var uri = new Uri(url, UriKind.Absolute);
            }
            catch (Exception)
            {
                throw new ArgumentException("invalid absolute url", "url");
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("title can't be empty", "title");
            }

            var bookmark = new Bookmark(url, TenantUtil.DateTimeNow(), title, description) { UserCreatorID = SecurityContext.CurrentAccount.ID };
            BookmarkingDao.AddBookmark(bookmark, !string.IsNullOrEmpty(tags) ? tags.Split(',').Select(x => new Tag { Name = x }).ToList() : new List<Tag>());
            return new BookmarkWrapper(bookmark);
        }


        /// <summary>
        /// Returns a comment preview with the content specified in the request.
        /// </summary>
        /// <short>Get a comment preview</short>
        /// <section>Comments</section>
        /// <param name="commentid">Comment ID</param>
        /// <param name="htmltext">Comment content</param>
        /// <returns>Comment information</returns>
        /// <category>Bookmarks</category>
        [Create("bookmark/comment/preview")]
        public CommentInfo GetBookmarkCommentPreview(string commentid, string htmltext)
        {
            var comment = new Comment
            {
                Datetime = TenantUtil.DateTimeNow(),
                UserID = SecurityContext.CurrentAccount.ID
            };

            if (!String.IsNullOrEmpty(commentid))
            {
                comment = BookmarkingServiceHelper.GetCurrentInstanse().GetCommentById(commentid);

                comment.Parent = string.Empty;
            }
            comment.Content = htmltext;

            var ci = BookmarkingConverter.ConvertComment(comment, new List<Comment>());
            ci.IsEditPermissions = false;
            ci.IsResponsePermissions = false;

            //var isRoot = string.IsNullOrEmpty(comment.Parent) || comment.Parent.Equals(Guid.Empty.ToString(), StringComparison.CurrentCultureIgnoreCase);

            return ci;
        }


        /// <summary>
        ///Removes a comment with the ID specified in the request.
        /// </summary>
        /// <short>Remove a comment</short>
        /// <section>Comments</section>
        /// <param name="commentid">Comment ID</param>
        /// <returns>Comment ID</returns>
        /// <category>Bookmarks</category>
        [Delete("bookmark/comment/{commentid}")]
        public string RemoveBookmarkComment(string commentid)
        {
            var comment = BookmarkingServiceHelper.GetCurrentInstanse().GetCommentById(commentid);
            if (comment != null && BookmarkingPermissionsCheck.PermissionCheckEditComment(comment))
            {
                BookmarkingServiceHelper.GetCurrentInstanse().RemoveComment(commentid);
                return commentid;
            }
            return null;
        }

        ///<summary>
        /// Adds a comment to the entity with the ID specified in the request. The parent bookmark ID can be also specified if needed.
        ///</summary>
        ///<short>
        /// Add a bookmark comment by entity ID
        ///</short>
        ///<param name="parentcommentid">Parent comment ID</param>
        ///<param name="entityid">Entity ID</param>
        ///<param name="content">Comment text</param>
        ///<returns>List of bookmark comments</returns>
        /// <category>Bookmarks</category>
        [Create("bookmark/comment")]
        public CommentInfo AddBookmarkComment(string parentcommentid, long entityid, string content)
        {
            var comment = new Comment
            {
                Content = content,
                Datetime = TenantUtil.DateTimeNow(),
                UserID = SecurityContext.CurrentAccount.ID
            };

            var parentID = Guid.Empty;
            try
            {
                if (!string.IsNullOrEmpty(parentcommentid))
                {
                    parentID = new Guid(parentcommentid);
                }
            }
            catch
            {
                parentID = Guid.Empty;
            }
            comment.Parent = parentID.ToString();
            comment.BookmarkID = entityid;
            comment.ID = Guid.NewGuid();

            BookmarkingServiceHelper.GetCurrentInstanse().AddComment(comment);

            return BookmarkingConverter.ConvertComment(comment, new List<Comment>());
        }

        /// <summary>
        /// Updates the selected bookmark comment with the content specified in the request.
        /// </summary>
        /// <short>Update a bookmark comment</short>
        /// <param name="commentid">Comment ID</param>
        /// <param name="content">New comment text</param>
        /// <returns>Updated bookmark</returns>
        /// <category>Bookmarks</category>
        [Update("bookmark/comment/{commentid}")]
        public string UpdateBookmarkComment(string commentid, string content)
        {
            var comment = BookmarkingServiceHelper.GetCurrentInstanse().GetCommentById(commentid);
            if (comment == null || !BookmarkingPermissionsCheck.PermissionCheckEditComment(comment)) throw new ArgumentException();

            BookmarkingServiceHelper.GetCurrentInstanse().UpdateComment(commentid, content);
            return HtmlUtility.GetFull(content);
        }

        /// <summary>
        /// Removes a bookmark from favorites. If after removing user bookmark, its rating is 0, the bookmark will be removed completely.
        /// </summary>
        /// <short>Remove a bookmark from favorites</short>
        /// <param name="id">Bookmark ID</param>
        /// <returns>Bookmark</returns>
        /// <category>Bookmarks</category>
        [Delete("bookmark/@favs/{id}")]
        public BookmarkWrapper RemoveBookmarkFromFavourite(long id)
        {
            var bookmark = BookmarkingServiceHelper.GetCurrentInstanse().RemoveBookmarkFromFavourite(id);

            return bookmark == null ? null : new BookmarkWrapper(bookmark);
        }

        /// <summary>
        /// Removes a bookmark with the ID specified in the request.
        /// </summary>
        /// <short>Remove a bookmark</short>
        /// <param name="id">Bookmark ID</param>
        /// <category>Bookmarks</category>
        [Delete("bookmark/{id}")]
        public void RemoveBookmark(long id)
        {
            BookmarkingServiceHelper.GetCurrentInstanse().RemoveBookmark(id);
        }
    }
}