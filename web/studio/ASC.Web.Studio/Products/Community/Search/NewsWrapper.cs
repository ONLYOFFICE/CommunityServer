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
using ASC.Web.Community.News.Code;

namespace ASC.Web.Community.Search
{
    public sealed class NewsWrapper : Wrapper
    {
        [ColumnTenantId("tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("LastModified")]
        public override DateTime LastModifiedOn { get; set; }

        [Column("caption", 1)]
        public string Caption { get; set; }

        [Column("text", 2, charFilter: CharFilter.html | CharFilter.io)]
        public string Text { get; set; }

        protected override string Table { get { return "events_feed"; } }

        [Join(JoinTypeEnum.Sub, "tenant:tenant", "id:poll")]
        public List<PollVariantWrapper> PollVariantWrapper { get; set; }

        public static implicit operator NewsWrapper(Feed feed)
        {
            var result =  new NewsWrapper
            {
                Id = (int) feed.Id,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                Caption = feed.Caption,
                Text = feed.Text
            };

            var poll = feed as FeedPoll;
            if (poll != null)
            {
                result.PollVariantWrapper = poll.Variants.Select(r => new PollVariantWrapper {Name = r.Name}).ToList();
            }

            return result;
        }

        /*
         * }
            }
         * */
    }

    public sealed class PollVariantWrapper : Wrapper
    {
        [Column("name", 1)]
        public string Name { get; set; }

        [ColumnId("")]
        public override int Id { get; set; }

        [ColumnTenantId("")]
        public override int TenantId { get; set; }

        [ColumnLastModified("")]
        public override DateTime LastModifiedOn { get; set; }

        protected override string Table { get { return "events_pollvariant"; } }
    }
}