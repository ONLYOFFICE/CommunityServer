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