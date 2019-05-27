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
using ASC.Blogs.Core.Domain;
using ASC.Core;
using ASC.ElasticSearch;

namespace ASC.Web.Community.Search
{
    public sealed class BlogsWrapper : Wrapper
    {
        [ColumnId("post_id")]
        public override int Id { get; set; }

        [ColumnTenantId("tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("LastModified")]
        public override DateTime LastModifiedOn { get; set; }

        [Column("title", 1)]
        public string Title { get; set; }

        [Column("content", 2, charFilter: CharFilter.html | CharFilter.io)]
        public string Content { get; set; }

        protected override string Table { get { return "blogs_posts"; } }

        [Join(JoinTypeEnum.Sub, "tenant:tenant", "id:post_id")]
        public List<TagsWrapper> TagsWrapper { get; set; }

        public static implicit operator BlogsWrapper(Post post)
        {
            return new BlogsWrapper
            {
                Id = post.AutoIncrementID,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                Title = post.Title,
                Content = post.Content,
                TagsWrapper = post.TagList.Select(r=> new TagsWrapper
                {
                    Title = r.Content
                }).ToList()
            };
        }
    }

    public sealed class TagsWrapper : Wrapper
    {
        [Column("name", 1)]
        public string Title { get; set; }

        [ColumnTenantId("")]
        public override int TenantId { get; set; }

        [ColumnId("")]
        public override int Id { get; set; }

        [ColumnLastModified("")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table { get { return "blogs_tags"; } }
    }
}