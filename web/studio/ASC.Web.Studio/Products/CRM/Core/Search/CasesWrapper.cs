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
using ASC.Core;
using ASC.ElasticSearch;

namespace ASC.Web.CRM.Core.Search
{
    public sealed class CasesWrapper : Wrapper
    {
        [ColumnLastModified("last_modifed_on")]
        public override DateTime LastModifiedOn { get; set; }

        [Column("title", 1)]
        public string Title { get; set; }

        protected override string Table { get { return "crm_case"; } }

        public static implicit operator CasesWrapper(ASC.CRM.Core.Entities.Cases d)
        {
            return new CasesWrapper
            {
                Id = d.ID,
                Title = d.Title,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
            };
        }
    }
}