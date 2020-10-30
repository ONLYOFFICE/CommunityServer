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
    public sealed class FieldsWrapper : Wrapper
    {
        [ColumnLastModified("last_modifed_on")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnCondition("field_id", 1)]
        public int FieldId { get; set; }

        [ColumnCondition("entity_id", 2)]
        public int EntityId { get; set; }

        [ColumnCondition("entity_type", 3)]
        public int EntityType { get; set; }

        [Column("value", 4)]
        public string Value { get; set; }

        protected override string Table { get { return "crm_field_value"; } }

        public static implicit operator FieldsWrapper(ASC.CRM.Core.Entities.CustomField cf)
        {
            return new FieldsWrapper
            {
                Id = cf.ID,
                EntityId = cf.EntityID,
                EntityType = (int)cf.EntityType,
                Value = cf.Value,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
            };
        }
    }
}