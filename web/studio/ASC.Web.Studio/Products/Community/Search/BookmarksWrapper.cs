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
using ASC.Bookmarking.Pojo;
using ASC.Core;
using ASC.ElasticSearch;

namespace ASC.Web.Community.Search
{
    public sealed class BookmarksUserWrapper : Wrapper
    {
        [ColumnId("UserBookmarkID")]
        public override int Id { get; set; }

        [ColumnTenantId("tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("LastModified")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnMeta("BookmarkID", 1)]
        public int BookmarkID { get; set; }

        [Column("Name", 2)]
        public string Name { get; set; }

        [Column("Description", 3)]
        public string Description { get; set; }

        protected override string Table { get { return "bookmarking_userbookmark"; } }

        [Join(JoinTypeEnum.Sub, "BookmarkID:ID")]
        public List<BookmarksWrapper> BookmarksWrapper { get; set; }

        [Join(JoinTypeEnum.Sub, "UserBookmarkID:UserBookmarkID")]
        public List<UserbookMarkTagWrapper> UserbookMarkTagWrapper { get; set; }

        public static BookmarksUserWrapper Create(UserBookmark userBookmark, Bookmark bookmark)
        {
            return new BookmarksUserWrapper
            {
                Id = (int) userBookmark.UserBookmarkID,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                Name = userBookmark.Name,
                Description = userBookmark.Description,
                BookmarkID = (int)bookmark.ID,
                BookmarksWrapper = new List<BookmarksWrapper>
                {
                    new BookmarksWrapper
                    {
                        Url = bookmark.URL
                    }
                },
                UserbookMarkTagWrapper = bookmark.Tags.Select(r => new UserbookMarkTagWrapper
                {
                    BookmarksTagWrapper = new BookmarksTagWrapper
                    {
                        Name = r.Name
                    }
                }).ToList()
            };
        }
    }

    public sealed class BookmarksWrapper : Wrapper
    {
        [Column("URL", 1)]
        public string Url { get; set; }

        [ColumnId("ID")]
        public override int Id { get; set; }

        [ColumnTenantId("Tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("Date")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table { get { return "bookmarking_bookmark"; } }
    }

    public sealed class UserbookMarkTagWrapper : Wrapper
    {
        [Join(JoinTypeEnum.Inner, "TagID:TagID")]
        public BookmarksTagWrapper BookmarksTagWrapper { get; set; }

        [ColumnId("")]
        public override int Id { get; set; }

        [ColumnTenantId("")]
        public override int TenantId { get; set; }

        [ColumnLastModified("")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table { get { return "bookmarking_userbookmarktag"; } }
    }

    public sealed class BookmarksTagWrapper : Wrapper
    {
        [Column("Name", 1)]
        public string Name { get; set; }

        [ColumnId("")]
        public override int Id { get; set; }

        [ColumnTenantId("")]
        public override int TenantId { get; set; }

        [ColumnLastModified("")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table { get { return "bookmarking_tag"; } }
    }
}