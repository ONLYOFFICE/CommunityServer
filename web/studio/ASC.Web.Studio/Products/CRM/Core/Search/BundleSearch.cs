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


using System.Collections.Generic;
using System.Linq;
using ASC.CRM.Core;
using ASC.ElasticSearch;

namespace ASC.Web.CRM.Core.Search
{
    public class BundleSearch
    {
        public static bool Support(EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Person:
                case EntityType.Contact:
                case EntityType.Company:
                    return FactoryIndexer<ContactsWrapper>.Support &&
                           FactoryIndexer<InfoWrapper>.Support &&
                           FactoryIndexer<FieldsWrapper>.Support &&
                           FactoryIndexer<EventsWrapper>.Support;
                case EntityType.Opportunity:
                    return FactoryIndexer<DealsWrapper>.Support &&
                           FactoryIndexer<FieldsWrapper>.Support &&
                           FactoryIndexer<EventsWrapper>.Support;
                case EntityType.RelationshipEvent:
                    return FactoryIndexer<EventsWrapper>.Support;
                case EntityType.Task:
                    return FactoryIndexer<TasksWrapper>.Support;
                case EntityType.Case:
                    return FactoryIndexer<CasesWrapper>.Support &&
                           FactoryIndexer<FieldsWrapper>.Support &&
                           FactoryIndexer<EventsWrapper>.Support;
                case EntityType.Invoice:
                    return FactoryIndexer<InvoicesWrapper>.Support;
            }

            return false;
        }

        public static bool TrySelectCase(string text, out List<int> result)
        {
            var success = false;
            result = new List<int>();

            List<int> casesId;
            if (FactoryIndexer<CasesWrapper>.TrySelectIds(s => s.MatchAll(text), out casesId))
            {
                result.AddRange(casesId);
                success = true;
            }

            IReadOnlyCollection<FieldsWrapper> casesCustom;
            if (FactoryIndexer<FieldsWrapper>.TrySelect(s => s.MatchAll(text).Where(r => r.EntityType, 7), out casesCustom))
            {
                result.AddRange(casesCustom.Select(r => r.EntityId).ToList());
                success = true;
            }

            IReadOnlyCollection<EventsWrapper> events;
            if (!FactoryIndexer<EventsWrapper>.TrySelect(s => s.MatchAll(text).Where(r => r.EntityType, 7).Gt(r => r.EntityId, 0), out events))
            {
                result.AddRange(events.Select(r => r.EntityId).ToList());
                success = true;
            }

            return success;
        }

        public static bool TrySelectContact(string text, out List<int> result)
        {
            var success = false;
            result = new List<int>();

            List<int> contactsId;
            if (FactoryIndexer<ContactsWrapper>.TrySelectIds(s => s.MatchAll(text), out contactsId))
            {
                result.AddRange(contactsId);
                success = true;
            }

            IReadOnlyCollection<InfoWrapper> infos;
            if (FactoryIndexer<InfoWrapper>.TrySelect(s => s.MatchAll(text), out infos))
            {
                result.AddRange(infos.Select(r => r.ContactId).ToList());
                success = true;
            }

            IReadOnlyCollection<FieldsWrapper> personCustom;
            if (FactoryIndexer<FieldsWrapper>.TrySelect(s => s.MatchAll(text).In(r => r.EntityType, new[] {0, 4, 5}), out personCustom))
            {
                result.AddRange(personCustom.Select(r => r.EntityId).ToList());
                success = true;
            }

            IReadOnlyCollection<EventsWrapper> events;
            if (FactoryIndexer<EventsWrapper>.TrySelect(s => s.MatchAll(text).Gt(r => r.ContactId, 0), out events))
            {
                result.AddRange(events.Select(r => r.ContactId).ToList());
                success = true;
            }

            return success;
        }

        public static bool TrySelectOpportunity(string text, out List<int> result)
        {
            var success = false;
            result = new List<int>();

            List<int> dealsId;
            if (FactoryIndexer<DealsWrapper>.TrySelectIds(s => s.MatchAll(text), out dealsId))
            {
                result.AddRange(dealsId);
                success = true;
            }

            IReadOnlyCollection<FieldsWrapper> casesCustom;
            if (FactoryIndexer<FieldsWrapper>.TrySelect(s => s.MatchAll(text).Where(r => r.EntityType, 1), out casesCustom))
            {
                result.AddRange(casesCustom.Select(r => r.EntityId).ToList());
                success = true;
            }

            IReadOnlyCollection<EventsWrapper> events;
            if (!FactoryIndexer<EventsWrapper>.TrySelect(s => s.MatchAll(text).Where(r => r.EntityType, 1).Gt(r => r.EntityId, 0), out events))
            {
                result.AddRange(events.Select(r => r.EntityId).ToList());
                success = true;
            }

            return success;
        }
    }
}