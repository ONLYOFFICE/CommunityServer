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
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;

namespace ASC.Web.CRM.Core.Search
{
    public sealed class EventsWrapper : Wrapper
    {
        [ColumnLastModified("last_modifed_on")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnCondition("contact_id", 1)]
        public int ContactId { get; set; }

        [ColumnCondition("entity_id", 2)]
        public int EntityId { get; set; }

        [ColumnCondition("entity_type", 2)]
        public int EntityType { get; set; }

        [Column("content", 3, charFilter: CharFilter.html)]
        public string Content { get; set; }

        protected override string Table { get { return "crm_relationship_event"; } }

        public static implicit operator EventsWrapper(RelationshipEvent relationshipEvent)
        {
            return new EventsWrapper
            {
                Id = relationshipEvent.ID,
                ContactId = relationshipEvent.ContactID,
                EntityId = relationshipEvent.EntityID,
                EntityType = (int)relationshipEvent.EntityType,
                Content = relationshipEvent.Content,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
            };
        }
    }
}