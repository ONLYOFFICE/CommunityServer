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