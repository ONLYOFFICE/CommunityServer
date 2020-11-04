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