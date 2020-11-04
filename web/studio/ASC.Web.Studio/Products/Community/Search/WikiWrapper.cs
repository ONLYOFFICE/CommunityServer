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
using ASC.Core;
using ASC.ElasticSearch;
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Web.Community.Search
{
    public sealed class WikiWrapper : Wrapper
    {
        [ColumnTenantId("tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("modified_on")]
        public override DateTime LastModifiedOn { get; set; }

        [Column("pagename", 1)]
        public string Title { get; set; }

        protected override string Table { get { return "wiki_pages"; } }

        [Join(JoinTypeEnum.Sub, "tenant:tenant", "pagename:pagename", "version:version")]
        public List<WikiHistoryWrapper> WikiHistoryWrapper { get; set; }

        public static implicit operator WikiWrapper(Page page)
        {
            return new WikiWrapper
            {
                Id = page.ID,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                Title = page.PageName,
                WikiHistoryWrapper = new List<WikiHistoryWrapper>
                {
                    new WikiHistoryWrapper
                    {
                        Body = page.Body
                    }
                }
            };
        }
    }

    public sealed class WikiHistoryWrapper : Wrapper
    {
        [ColumnId("")]
        public override int Id { get; set; }

        [ColumnLastModified("")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnTenantId("")]
        public override int TenantId { get; set; }

        [Column("body", 1)]
        public string Body { get; set; }

        protected override string Table { get { return "wiki_pages_history"; } }
    }
}