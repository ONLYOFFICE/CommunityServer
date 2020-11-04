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
using ASC.Core;
using ASC.ElasticSearch;
using ASC.Forum;

namespace ASC.Web.Community.Search
{
    public sealed class TopicWrapper : Wrapper
    {
        [ColumnTenantId("tenantid")]
        public override int TenantId { get; set; }

        [ColumnLastModified("LastModified")]
        public override DateTime LastModifiedOn { get; set; }

        [Column("title", 1)]
        public string Title { get; set; }

        protected override string Table { get { return "forum_topic"; } }

        [Join(JoinTypeEnum.Sub, "id:topic_id")]
        public List<ForumTopicTagWrapper> ForumTopicTagWrapper { get; set; }

        public static implicit operator TopicWrapper(Topic topic)
        {
            return new TopicWrapper
            {
                Id = topic.ID,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                Title = topic.Title,
                ForumTopicTagWrapper = topic.Tags.Select(r => new ForumTopicTagWrapper
                {
                    ForumTagWrapper = new ForumTagWrapper
                    {
                        Name = r.Name
                    }
                }).ToList()
            };
        }
    }

    public sealed class ForumTopicTagWrapper : Wrapper
    {
        [Join(JoinTypeEnum.Inner, "tag_id:id")]
        public ForumTagWrapper ForumTagWrapper { get; set; }

        [ColumnId("")]
        public override int Id { get; set; }

        [ColumnTenantId("")]
        public override int TenantId { get; set; }

        [ColumnLastModified("")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table { get { return "forum_topic_tag"; } }
    }

    public sealed class ForumTagWrapper : Wrapper
    {
        [Column("name", 1)]
        public string Name { get; set; }

        [ColumnId("")]
        public override int Id { get; set; }

        [ColumnTenantId("")]
        public override int TenantId { get; set; }

        [ColumnLastModified("")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table { get { return "forum_tag"; } }
    }

}