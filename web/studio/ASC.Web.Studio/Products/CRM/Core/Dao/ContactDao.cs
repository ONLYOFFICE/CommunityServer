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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;
using ASC.Files.Core;
using ASC.Web.CRM.Core.Search;
using ASC.Web.CRM.Core.Enums;
using ASC.Web.Files.Api;
using OrderBy = ASC.CRM.Core.Entities.OrderBy;

namespace ASC.CRM.Core.Dao
{
    public class CachedContactDao : ContactDao
    {

        private readonly HttpRequestDictionary<Contact> _contactCache = new HttpRequestDictionary<Contact>("crm_contact");

        public CachedContactDao(int tenantID)
            : base(tenantID)
        {

        }

        public override Contact GetByID(int contactID)
        {
            return _contactCache.Get(contactID.ToString(CultureInfo.InvariantCulture), () => GetByIDBase(contactID));
        }

        private Contact GetByIDBase(int contactID)
        {
            return base.GetByID(contactID);
        }

        public override Contact DeleteContact(int contactID)
        {
            ResetCache(contactID);

            return base.DeleteContact(contactID);
        }

        public override void UpdateContact(Contact contact)
        {
            if (contact != null && contact.ID > 0)
                ResetCache(contact.ID);

            base.UpdateContact(contact);
        }


        public override int SaveContact(Contact contact)
        {
            if (contact != null)
            {
                ResetCache(contact.ID);
            }

            return base.SaveContact(contact);

        }

        private void ResetCache(int contactID)
        {
            _contactCache.Reset(contactID.ToString());
        }
    }




    public class ContactDao : AbstractDao
    {
        #region Constructor

        public ContactDao(int tenantID)
            : base(tenantID)
        {
        }

        #endregion

        #region Members

        private readonly String _displayNameSeparator = "!=!";


        #endregion

        public List<Contact> GetContactsByPrefix(String prefix, int searchType, int from, int count)
        {
            if (count == 0)
                throw new ArgumentException();

            var keywords = prefix.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var query = Query("crm_contact c").Select(GetContactColumnsTable("c"));

            switch (searchType)
            {
                case 0: // Company
                    query.Where(Exp.Eq("c.is_company", true));
                    break;
                case 1: // Persons
                    query.Where(Exp.Eq("c.is_company", false));
                    break;
                case 2: // PersonsWithoutCompany
                    query.Where(Exp.Eq("c.company_id", 0) & Exp.Eq("c.is_company", false));
                    break;
                case 3: // CompaniesAndPersonsWithoutCompany
                    query.Where(Exp.Eq("c.company_id", 0));
                    break;
            }

            foreach (var k in keywords)
            {
                query.Where(Exp.Like("c.display_name", k));
            }

            if (!CRMSecurity.IsAdmin)
            {
                var sharedContacts = Exp.Gt("c.is_shared", 0);

                var oldContacts = Exp.And(Exp.Eq("c.is_shared", null), !ExistAclRecord(false, "c"));

                var privateContacts = Exp.And(Exp.Eq("c.is_shared", 0), ExistAclRecord(true, "c"));

                query.Where(Exp.Or(sharedContacts, Exp.Or(oldContacts, privateContacts)));
            }

            query.OrderBy("c.display_name", true);

            if (0 < from && from < int.MaxValue) query.SetFirstResult(from);
            if (0 < count && count < int.MaxValue) query.SetMaxResults(count);

            return Db.ExecuteList(query).ConvertAll(ToContact);
        }

        private static Exp ExistAclRecord(bool forCurrentUser, string contactTebleAlias = "")
        {
            var alias = string.IsNullOrEmpty(contactTebleAlias) ? string.Empty : contactTebleAlias + ".";

            var query = new SqlQuery("core_acl a")
                .Select("a.object")
                .Where(Exp.EqColumns("a.tenant", alias + "tenant_id"))
                .Where(Exp.Eq("a.action", CRMSecurity._actionRead.ID));

            if (forCurrentUser)
                query.Where(Exp.Eq("a.subject", SecurityContext.CurrentAccount.ID));

            query.Where(
                Exp.Or(
                    Exp.EqColumns("a.object", string.Format("concat('ASC.CRM.Core.Entities.Company|', {0}id)", alias)),
                    Exp.EqColumns("a.object", string.Format("concat('ASC.CRM.Core.Entities.Person|', {0}id)", alias))
                    )
                );

            return Exp.Exists(query);
        }

        public int GetAllContactsCount()
        {
            return GetContactsCount(String.Empty, null, -1, -1, ContactListViewType.All, DateTime.MinValue, DateTime.MinValue);
        }

        public List<Contact> GetAllContacts()
        {
            return GetContacts(String.Empty, new List<string>(), -1, -1, ContactListViewType.All, DateTime.MinValue, DateTime.MinValue, 0, 0,
                new OrderBy(ContactSortedByType.DisplayName, true));
        }

        public int GetContactsCount(String searchText,
                                   IEnumerable<String> tags,
                                   int contactStage,
                                   int contactType,
                                   ContactListViewType contactListView,
                                   DateTime fromDate,
                                   DateTime toDate,
                                   Guid? responsibleid = null,
                                   bool? isShared = null)
        {

            var cacheKey = TenantID.ToString(CultureInfo.InvariantCulture) +
                           "contacts" +
                           SecurityContext.CurrentAccount.ID +
                           searchText +
                           contactStage +
                           contactType +
                           (int)contactListView +
                           responsibleid +
                           isShared;

            if (tags != null)
                cacheKey += String.Join("", tags.ToArray());

            if (fromDate != DateTime.MinValue)
                cacheKey += fromDate.ToString();

            if (toDate != DateTime.MinValue)
                cacheKey += toDate.ToString();

            var fromCache = _cache.Get<string>(cacheKey);

            if (fromCache != null) return Convert.ToInt32(fromCache);

            var withParams = HasSearchParams(searchText,
                                            tags,
                                            contactStage,
                                            contactType,
                                            contactListView,
                                            fromDate,
                                            toDate,
                                            responsibleid,
                                            isShared);
            int result;

                if (withParams)
                {
                    ICollection<int> excludedContactIDs;

                    switch (contactListView)
                    {
                        case ContactListViewType.Person:
                            excludedContactIDs = CRMSecurity.GetPrivateItems(typeof(Person))
                                                .Except(Db.ExecuteList(Query("crm_contact").Select("id")
                                                        .Where(Exp.In("is_shared", new[] { (int)ShareType.Read, (int)ShareType.ReadWrite }) & Exp.Eq("is_company", false)))
                                                  .Select(x => Convert.ToInt32(x[0]))).ToList();
                            break;
                        case ContactListViewType.Company:
                            excludedContactIDs = CRMSecurity.GetPrivateItems(typeof(Company))
                                               .Except(Db.ExecuteList(Query("crm_contact").Select("id")
                                                      .Where(Exp.In("is_shared", new[] { (int)ShareType.Read, (int)ShareType.ReadWrite }) & Exp.Eq("is_company", true)))
                                                      .Select(x => Convert.ToInt32(x[0]))).ToList();

                            break;
                        default:
                            excludedContactIDs = CRMSecurity.GetPrivateItems(typeof(Company)).Union(CRMSecurity.GetPrivateItems(typeof(Person)))
                                        .Except(Db.ExecuteList(Query("crm_contact").Select("id").Where(Exp.In("is_shared", new[] { (int)ShareType.Read, (int)ShareType.ReadWrite })))
                                              .Select(x => Convert.ToInt32(x[0]))).ToList();



                            break;
                    }

                    var whereConditional = WhereConditional(excludedContactIDs,
                                                            searchText,
                                                            tags,
                                                            contactStage,
                                                            contactType,
                                                            contactListView,
                                                            fromDate,
                                                            toDate,
                                                            responsibleid,
                                                            isShared);

                if (whereConditional != null)
                {
                    if (!isShared.HasValue)
                    {
                        var sqlQuery = Query("crm_contact").SelectCount().Where(whereConditional);
                        result = Db.ExecuteScalar<int>(sqlQuery);
                    }
                    else
                    {
                        var sqlQuery = Query("crm_contact").Select("id, is_company, is_shared").Where(whereConditional);
                        var sqlResultRows = Db.ExecuteList(sqlQuery);

                        var resultContactsNewScheme_Count = sqlResultRows.Where(row => row[2] != null).ToList().Count; //new scheme

                        var fakeContactsOldScheme = sqlResultRows
                            .Where(row => row[2] == null).ToList() // old scheme
                            .ConvertAll(row =>  Convert.ToBoolean(row[1]) == true ? new Company() {ID = Convert.ToInt32(row[0])} as Contact : new Person() {ID = Convert.ToInt32(row[0])} as Contact );
                        var resultFakeContactsOldScheme_Count = fakeContactsOldScheme.Where(fc => {
                            var accessSubjectToContact = CRMSecurity.GetAccessSubjectTo(fc);
                            if (isShared.Value == true)
                            {
                                return !accessSubjectToContact.Any();
                            }
                            else
                            {
                                return accessSubjectToContact.Any();
                            }
                        }).ToList().Count;

                        return resultContactsNewScheme_Count + resultFakeContactsOldScheme_Count;
                    }
                }
                else
                {
                    result = 0;
                }
            }
            else
            {
                var countWithoutPrivate = Db.ExecuteScalar<int>(Query("crm_contact").SelectCount());

                var privateCount = CRMSecurity.GetPrivateItemsCount(typeof(Person)) +
                                    CRMSecurity.GetPrivateItemsCount(typeof(Company)) -
                                    Db.ExecuteScalar<int>(Query("crm_contact").Where(Exp.In("is_shared", new[] { (int)ShareType.Read, (int)ShareType.ReadWrite })).SelectCount());

                if (privateCount < 0)
                    privateCount = 0;

                if (privateCount > countWithoutPrivate)
                {
                    _log.Error("Private contacts count more than all contacts");

                    privateCount = 0;
                }

                result = countWithoutPrivate - privateCount;
            }
            if (result > 0)
            {
                _cache.Insert(cacheKey, result, TimeSpan.FromMinutes(1));
            }

            return result;
        }

        public List<Contact> SearchContactsByEmail(string searchText, int maxCount)
        {
            var contacts = new List<Contact>();

            if (string.IsNullOrEmpty(searchText) || maxCount <= 0)
                return contacts;

            Func<SqlQuery> getBaseQuery = () =>
            {
                var query = new SqlQuery("crm_contact cc")
                    .Select(GetContactColumnsTable("cc"))
                    .InnerJoin("crm_contact_info cci",
                        Exp.EqColumns("cc.tenant_id", "cci.tenant_id") &
                        Exp.EqColumns("cc.id", "cci.contact_id"))
                    .Where("cci.type", (int) ContactInfoType.Email)
                    .Where("cc.tenant_id", TenantID);

                return query;
            };

            var ids = new List<int>();

            List<int> contactsIds;
            if (FactoryIndexer<EmailWrapper>.TrySelectIds(s => s.MatchAll(searchText), out contactsIds))
            {
                if (!contactsIds.Any())
                    return contacts;

                ids = contactsIds.Select(r => Convert.ToInt32(r)).ToList();
            }

            var isAdmin = CRMSecurity.IsAdmin;
            const int count_per_query = 100;

            var f = 0;
            do
            {
                var query = getBaseQuery();

                if (ids.Any())
                {
                    var partIds = ids.Skip(f).Take(count_per_query).ToList();

                    if (!partIds.Any())
                        break;

                    query
                        .Where(Exp.In("cc.id", partIds));
                }
                else
                {
                    query
                        .Where(Exp.Like("concat(cc.display_name, ' ', cci.data)", searchText, SqlLike.AnyWhere))
                        .SetFirstResult(f)
                        .SetMaxResults(count_per_query);
                }

                var partContacts = Db.ExecuteList(query)
                    .ConvertAll(ToContact);

                foreach (var partContact in partContacts)
                {
                    if (maxCount - contacts.Count == 0)
                        return contacts;

                    if (isAdmin || CRMSecurity.CanAccessTo(partContact))
                        contacts.Add(partContact);
                }

                if (maxCount - contacts.Count == 0 || !partContacts.Any() || partContacts.Count < count_per_query)
                    break;

                f += count_per_query;

            } while (true);

            return contacts;
        }

        public List<Contact> GetContacts(String searchText,
                                        IEnumerable<String> tags,
                                        int contactStage,
                                        int contactType,
                                        ContactListViewType contactListView,
                                        DateTime fromDate,
                                        DateTime toDate,
                                        int from,
                                        int count,
                                        OrderBy orderBy,
                                        Guid? responsibleId = null,
                                        bool? isShared = null)
        {
            if (CRMSecurity.IsAdmin)
            {
                if (!isShared.HasValue)
                {
                    return GetCrudeContacts(
                                            searchText,
                                            tags,
                                            contactStage,
                                            contactType,
                                            contactListView,
                                            fromDate,
                                            toDate,
                                            from,
                                            count,
                                            orderBy,
                                            responsibleId,
                                            isShared,
                                            false);
                }
                else
                {
                    var crudeContacts = GetCrudeContacts(
                                            searchText,
                                            tags,
                                            contactStage,
                                            contactType,
                                            contactListView,
                                            fromDate,
                                            toDate,
                                            0,
                                            from + count,
                                            orderBy,
                                            responsibleId,
                                            isShared,
                                            false);

                    if (crudeContacts.Count == 0) return crudeContacts;

                    var result = crudeContacts.Where(c => (isShared.Value == true ? c.ShareType != ShareType.None : c.ShareType == ShareType.None)).ToList();

                    if (result.Count == crudeContacts.Count) return result.Skip(from).ToList();

                    var localCount = count;
                    var localFrom = from + count;

                    while (true)
                    {
                        crudeContacts = GetCrudeContacts(
                                                searchText,
                                                tags,
                                                contactStage,
                                                contactType,
                                                contactListView,
                                                fromDate,
                                                toDate,
                                                localFrom,
                                                localCount,
                                                orderBy,
                                                responsibleId,
                                                isShared,
                                                false);

                        if (crudeContacts.Count == 0) break;

                        result.AddRange(crudeContacts.Where(c => (isShared.Value == true ? c.ShareType != ShareType.None : c.ShareType == ShareType.None)));

                       if ((result.Count >= count + from) || (crudeContacts.Count < localCount)) break;

                       localFrom += localCount;
                       localCount = localCount * 2;
                    }

                    return result.Skip(from).Take(count).ToList();
                }
            }
            else
            {
                var crudeContacts = GetCrudeContacts(
                                            searchText,
                                            tags,
                                            contactStage,
                                            contactType,
                                            contactListView,
                                            fromDate,
                                            toDate,
                                            0,
                                            from + count,
                                            orderBy,
                                            responsibleId,
                                            isShared,
                                            false);

                if (crudeContacts.Count == 0) return crudeContacts;

                var tmp = isShared.HasValue ? crudeContacts.Where(c => (isShared.Value == true ? c.ShareType != ShareType.None : c.ShareType == ShareType.None)).ToList() : crudeContacts;

                if (crudeContacts.Count < from + count)
                {
                    return tmp.FindAll(CRMSecurity.CanAccessTo).Skip(from).ToList();
                }

                var result = tmp.FindAll(CRMSecurity.CanAccessTo);

                if (result.Count == crudeContacts.Count) return result.Skip(from).ToList();

                var localCount = count;
                var localFrom = from + count;

                while (true)
                {
                    crudeContacts = GetCrudeContacts(
                                            searchText,
                                            tags,
                                            contactStage,
                                            contactType,
                                            contactListView,
                                            fromDate,
                                            toDate,
                                            localFrom,
                                            localCount,
                                            orderBy,
                                            responsibleId,
                                            isShared,
                                            false);

                    if (crudeContacts.Count == 0) break;

                    tmp = isShared.HasValue ? crudeContacts.Where(c => (isShared.Value == true ? c.ShareType != ShareType.None : c.ShareType == ShareType.None)).ToList() : crudeContacts;

                    result.AddRange(tmp.Where(CRMSecurity.CanAccessTo));

                    if ((result.Count >= count + from) || (crudeContacts.Count < localCount)) break;

                    localFrom += localCount;
                    localCount = localCount * 2;
                }

                return result.Skip(from).Take(count).ToList();
            }
        }

        private bool HasSearchParams(String searchText,
                                    IEnumerable<String> tags,
                                    int contactStage,
                                    int contactType,
                                    ContactListViewType contactListView,
                                    DateTime fromDate,
                                    DateTime toDate,
                                    Guid? responsibleid = null,
                                    bool? isShared = null)
        {
            var hasNoParams = String.IsNullOrEmpty(searchText) &&
                                    (tags == null || !tags.Any()) &&
                                    contactStage < 0 &&
                                    contactType < 0 &&
                                    contactListView == ContactListViewType.All &&
                                    !isShared.HasValue &&
                                    fromDate == DateTime.MinValue &&
                                    toDate == DateTime.MinValue &&
                                    !responsibleid.HasValue;

            return !hasNoParams;
        }

        private Exp WhereConditional(
            ICollection<int> exceptIDs,
            String searchText,
            IEnumerable<String> tags,
            int contactStage,
            int contactType,
            ContactListViewType contactListView,
            DateTime fromDate,
            DateTime toDate,
            Guid? responsibleid = null,
            bool? isShared = null)
        {
            var conditions = new List<Exp>();

            var ids = new List<int>();

            if (responsibleid.HasValue)
            {

                if (responsibleid != default(Guid))
                {
                    ids = CRMSecurity.GetContactsIdByManager(responsibleid.Value).ToList();
                    if (ids.Count == 0) return null;
                }
                else
                {
                    if (exceptIDs == null)
                        exceptIDs = new List<int>();

                    exceptIDs = exceptIDs.Union(CRMSecurity.GetContactsIdByManager(Guid.Empty)).ToList();

                    if (!exceptIDs.Any()) // HACK
                        exceptIDs.Add(0);
                }

            }

            if (!String.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim();

                var keywords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                   .ToArray();

                List<int> contactsIds;
                if (!BundleSearch.TrySelectContact(searchText, out contactsIds))
                {
                    _log.Debug("FullTextSearch.SupportModule('CRM.Contacts') = false");
                    conditions.Add(BuildLike(new[] { "display_name" }, keywords));
                }
                else
                {
                    _log.Debug("FullTextSearch.SupportModule('CRM.Contacts') = true");
                    _log.DebugFormat("FullTextSearch.Search: searchText = {0}", searchText);
                    var full_text_ids = contactsIds;

                    if (full_text_ids.Count == 0) return null;

                    if (ids.Count != 0)
                    {
                        ids = ids.Where(full_text_ids.Contains).ToList();
                        if (ids.Count == 0) return null;
                    }
                    else
                    {
                        ids = full_text_ids;
                    }

                    if (contactListView == ContactListViewType.All || contactListView == ContactListViewType.Person)
                    {
                        ids.AddRange(GetContactIDsByCompanyIds(ids));
                    }
                }
            }

            if (tags != null && tags.Any())
            {
                ids = SearchByTags(EntityType.Contact, ids.ToArray(), tags);

                if (ids.Count == 0) return null;
            }


            switch (contactListView)
            {
                case ContactListViewType.Company:
                    conditions.Add(Exp.Eq("is_company", true));
                    break;
                case ContactListViewType.Person:
                    conditions.Add(Exp.Eq("is_company", false));
                    break;
                case ContactListViewType.WithOpportunity:
                    if (ids.Count > 0)
                    {
                        ids = Db.ExecuteList(new SqlQuery("crm_entity_contact").Select("contact_id")
                                                .Distinct()
                                                .Where(Exp.In("contact_id", ids) & Exp.Eq("entity_type", (int)EntityType.Opportunity)))
                                                .ConvertAll(row => Convert.ToInt32(row[0]));
                    }
                    else
                    {
                        ids = Db.ExecuteList(new SqlQuery("crm_entity_contact").Select("contact_id")
                                                .Distinct()
                                                .Where(Exp.Eq("entity_type", (int)EntityType.Opportunity)))
                                                .ConvertAll(row => Convert.ToInt32(row[0]));
                    }

                    if (ids.Count == 0) return null;

                    break;
            }

            if (contactStage >= 0)
                conditions.Add(Exp.Eq("status_id", contactStage));
            if (contactType >= 0)
                conditions.Add(Exp.Eq("contact_type_id", contactType));

            if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                conditions.Add(Exp.Between("create_on", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate.AddDays(1).AddMinutes(-1))));
            else if (fromDate != DateTime.MinValue)
                conditions.Add(Exp.Ge("create_on", TenantUtil.DateTimeToUtc(fromDate)));
            else if (toDate != DateTime.MinValue)
                conditions.Add(Exp.Le("create_on", TenantUtil.DateTimeToUtc(toDate.AddDays(1).AddMinutes(-1))));

            if (isShared.HasValue)
            {
                if (isShared.Value == true) {
                    conditions.Add(Exp.Or(Exp.In("is_shared", new[]{(int)ShareType.Read , (int)ShareType.ReadWrite}), Exp.Eq("is_shared", null)));
                } else {
                    conditions.Add(Exp.Or(Exp.Eq("is_shared", (int)ShareType.None), Exp.Eq("is_shared", null)));
                }
            }

            if (ids.Count > 0)
            {
                if (exceptIDs.Count > 0)
                {
                    ids = ids.Except(exceptIDs).ToList();
                    if (ids.Count == 0) return null;
                }

                conditions.Add(Exp.In("id", ids));

            }
            else if (exceptIDs.Count > 0)
            {
                var _MaxCountForSQLNotIn = 500;

                if (exceptIDs.Count > _MaxCountForSQLNotIn)
                {
                    ids = Db.ExecuteList(Query("crm_contact").Select("id")).ConvertAll(row => Convert.ToInt32(row[0])); //get all contact ids
                    ids = ids.Except(exceptIDs).ToList();
                    if (ids.Count == 0) return null;

                    conditions.Add(Exp.In("id", ids));

                } else {
                    conditions.Add(!Exp.In("id", exceptIDs.ToArray()));
                }
            }

            if (conditions.Count == 0) return null;

            return conditions.Count == 1 ? conditions[0] : conditions.Aggregate((i, j) => i & j);
        }

        private List<Contact> GetCrudeContacts(
            String searchText,
            IEnumerable<String> tags,
            int contactStage,
            int contactType,
            ContactListViewType contactListView,
            DateTime fromDate,
            DateTime toDate,
            int from,
            int count,
            OrderBy orderBy,
            Guid? responsibleid = null,
            bool? isShared = null,
            bool selectIsSharedInNewScheme = true)
        {

            var sqlQuery = GetContactSqlQuery(null);

            var withParams = HasSearchParams(searchText,
                                            tags,
                                            contactStage,
                                            contactType,
                                            contactListView,
                                            fromDate,
                                            toDate,
                                            responsibleid,
                                            isShared);

            var whereConditional = WhereConditional(new List<int>(),
                                                    searchText,
                                                    tags,
                                                    contactStage,
                                                    contactType,
                                                    contactListView,
                                                    fromDate,
                                                    toDate,
                                                    responsibleid,
                                                    isShared);

            if (withParams && whereConditional == null)
                return new List<Contact>();

            sqlQuery.Where(whereConditional);

            if (0 < from && from < int.MaxValue) sqlQuery.SetFirstResult(from);
            if (0 < count && count < int.MaxValue) sqlQuery.SetMaxResults(count);

            if (orderBy != null)
            {

                if (!Enum.IsDefined(typeof(ContactSortedByType), orderBy.SortedBy.ToString()))
                    orderBy.SortedBy = ContactSortedByType.Created;

                switch ((ContactSortedByType)orderBy.SortedBy)
                {
                    case ContactSortedByType.DisplayName:
                        sqlQuery.OrderBy("display_name", orderBy.IsAsc);
                        break;
                    case ContactSortedByType.Created:
                        sqlQuery.OrderBy("create_on", orderBy.IsAsc);
                        break;
                    case ContactSortedByType.ContactType:
                        sqlQuery.OrderBy("status_id", orderBy.IsAsc);
                        break;
                    case ContactSortedByType.FirstName:
                        sqlQuery.OrderBy("first_name", orderBy.IsAsc);
                        sqlQuery.OrderBy("last_name", orderBy.IsAsc);
                        break;
                    case ContactSortedByType.LastName:
                        sqlQuery.OrderBy("last_name", orderBy.IsAsc);
                        sqlQuery.OrderBy("first_name", orderBy.IsAsc);
                        break;
                    case ContactSortedByType.History:
                        sqlQuery.Select("last_event_modifed_on");

                        var subSqlQuery = Query("crm_relationship_event")
                            .Select("contact_id")
                            .Select("max(last_modifed_on) as last_event_modifed_on")
                            .Where(Exp.Gt("contact_id", 0))
                            .GroupBy("contact_id");

                        sqlQuery.LeftOuterJoin(subSqlQuery, "evt", Exp.EqColumns("id", "evt.contact_id"));
                        sqlQuery.OrderBy("last_event_modifed_on", orderBy.IsAsc);
                        sqlQuery.OrderBy("create_on", orderBy.IsAsc);
                        break;
                    default:
                        sqlQuery.OrderBy("display_name", orderBy.IsAsc);
                        break;
                }
            }


            var data = Db.ExecuteList(sqlQuery);

            var contacts = data.ConvertAll(ToContact);

            return selectIsSharedInNewScheme && isShared.HasValue ?
                contacts.Where(c => (isShared.Value ? c.ShareType != ShareType.None : c.ShareType == ShareType.None)).ToList() :
                contacts;
        }

        public List<Contact> GetContactsByName(String title, bool isCompany)
        {
            if (String.IsNullOrEmpty(title)) return new List<Contact>();

            title = title.Trim();

            if (isCompany)
            {
                return Db.ExecuteList(
                    GetContactSqlQuery(Exp.Eq("display_name", title) & Exp.Eq("is_company", true)))
                        .ConvertAll(ToContact)
                        .FindAll(CRMSecurity.CanAccessTo);
            }
            else
            {
                var titleParts = title.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (titleParts.Length == 1 || titleParts.Length == 2)
                {
                    if (titleParts.Length == 1)
                        return Db.ExecuteList(
                            GetContactSqlQuery(Exp.Eq("display_name", title) & Exp.Eq("is_company", false)))
                                .ConvertAll(ToContact)
                                .FindAll(CRMSecurity.CanAccessTo);
                    else
                        return Db.ExecuteList(
                            GetContactSqlQuery(Exp.Eq("display_name", String.Concat(titleParts[0], _displayNameSeparator, titleParts[1])) & Exp.Eq("is_company", false)))
                                    .ConvertAll(ToContact)
                                    .FindAll(CRMSecurity.CanAccessTo);
                }
            }
            return GetContacts(title, null, -1, -1, isCompany ? ContactListViewType.Company : ContactListViewType.Person, DateTime.MinValue, DateTime.MinValue, 0, 0, null);
        }

        public void RemoveMember(int[] peopleID)
        {
            if ((peopleID == null) || (peopleID.Length == 0)) return;

            Db.ExecuteNonQuery(Update("crm_contact").Set("company_id", 0).Where(Exp.In("id", peopleID)));
            RemoveRelativeInDb(null, EntityType.Person, peopleID);
        }

        public void RemoveMember(int peopleID)
        {
            Db.ExecuteNonQuery(Update("crm_contact").Set("company_id", 0).Where("id", peopleID));
            RemoveRelative(0, EntityType.Person, peopleID);
        }

        public void AddMemberInDb(int peopleID, int companyID)
        {
            Db.ExecuteNonQuery(Update("crm_contact")
               .Set("company_id", companyID)
               .Where("id", peopleID));
            SetRelative(companyID, EntityType.Person, peopleID);
        }

        public void AddMember(int peopleID, int companyID)
        {
            AddMemberInDb(peopleID, companyID);
        }

        public void SetMembers(int companyID, params int[] peopleIDs)
        {
            if (companyID == 0)
                throw new ArgumentException();

            using (var tx = Db.BeginTransaction())
            {
                Db.ExecuteNonQuery(new SqlDelete("crm_entity_contact")
                                        .Where(Exp.Eq("entity_type", EntityType.Person) &
                                         Exp.Eq("contact_id", companyID)));

                Db.ExecuteNonQuery(Update("crm_contact")
                                       .Set("company_id", 0)
                                       .Where(Exp.Eq("company_id", companyID)));


                if (!(peopleIDs == null || peopleIDs.Length == 0))
                {
                    Db.ExecuteNonQuery(Update("crm_contact")
                        .Set("company_id", companyID)
                        .Where(Exp.In("id", peopleIDs)));
                    foreach (var peopleID in peopleIDs)
                        SetRelative(companyID, EntityType.Person, peopleID);
                }

                tx.Commit();

            }

        }

        public void SetRelativeContactProject(IEnumerable<int> contactid, int projectid)
        {
            using (var tx = Db.BeginTransaction())
            {
                foreach (var id in contactid)
                {

                    Db.ExecuteNonQuery(Insert("crm_projects")
                                                  .InColumnValue("contact_id", id)
                                                  .InColumnValue("project_id", projectid));
                }

                tx.Commit();
            }
        }

        public void RemoveRelativeContactProject(int contactid, int projectid)
        {
            Db.ExecuteNonQuery(Delete("crm_projects")
            .Where(Exp.Eq("contact_id", contactid) & Exp.Eq("project_id", projectid)));
        }


        public IEnumerable<Contact> GetContactsByProjectID(int projectid)
        {
            var contactIds = Db.ExecuteList(Query("crm_projects")
                            .Select("contact_id")
                            .Where("project_id", projectid)).Select(x => Convert.ToInt32(x[0]));


            if (!contactIds.Any()) return new List<Contact>();

            return GetContacts(contactIds.ToArray());
        }

        public List<Contact> GetMembers(int companyID)
        {
            return GetContacts(GetRelativeToEntity(companyID, EntityType.Person, null));
        }

        public List<Contact> GetRestrictedMembers(int companyID)
        {
            return GetRestrictedContacts(GetRelativeToEntity(companyID, EntityType.Person, null));
        }

        public Dictionary<int, int> GetMembersCount(int[] companyID)
        {

            var sqlQuery = new SqlQuery("crm_entity_contact")
                       .Select("contact_id")
                       .SelectCount()
                       .Where(Exp.In("contact_id", companyID) & Exp.Eq("entity_type", (int)EntityType.Person))
                       .GroupBy("contact_id");

            var sqlResult = Db.ExecuteList(sqlQuery);

            return sqlResult.ToDictionary(item => Convert.ToInt32(item[0]), item => Convert.ToInt32(item[1]));
        }


        public int GetMembersCount(int companyID)
        {
            return Db.ExecuteScalar<int>(
                new SqlQuery("crm_entity_contact")
                .SelectCount()
                .Where(Exp.Eq("contact_id", companyID) & Exp.Eq("entity_type", (int)EntityType.Person)));
        }

        public List<int> GetMembersIDs(int companyID)
        {
            return Db.ExecuteList(
                new SqlQuery("crm_entity_contact")
                .Select("entity_id")
                .Where(Exp.Eq("contact_id", companyID) & Exp.Eq("entity_type", (int)EntityType.Person)))
                .ConvertAll(row => (int)(row[0]));
        }

        public Dictionary<int, ShareType?> GetMembersIDsAndShareType(int companyID)
        {
            var result = new Dictionary<int, ShareType?>();

            var query = new SqlQuery("crm_entity_contact ec")
                .Select("ec.entity_id")
                .Select("c.is_shared")
                .LeftOuterJoin("crm_contact c", Exp.EqColumns("ec.entity_id", "c.id"))
                .Where(Exp.Eq("ec.contact_id", companyID) & Exp.Eq("ec.entity_type", (int)EntityType.Person));
            var rows = Db.ExecuteList(query);

            foreach (var row in rows) {
                if (row[1] == null)
                {
                    result.Add((int)(row[0]), null);
                }
                else
                {
                    result.Add((int)(row[0]), (ShareType)(Convert.ToInt32(row[1])));
                }
            }

            return result;
        }


        public virtual void UpdateContact(Contact contact)
        {
            UpdateContactFromDb(contact);
        }

        private void UpdateContactFromDb(Contact contact)
        {
            var originalContact = GetByID(contact.ID);
            if (originalContact == null) throw new ArgumentException();
            CRMSecurity.DemandEdit(originalContact);

            String firstName;
            String lastName;
            String companyName;
            String title;
            int companyID;

            var displayName = String.Empty;

            if (contact is Company)
            {
                firstName = String.Empty;
                lastName = String.Empty;
                title = String.Empty;
                companyName = ((Company)contact).CompanyName.Trim();
                companyID = 0;
                displayName = companyName;

                if (String.IsNullOrEmpty(companyName))
                    throw new ArgumentException();

            }
            else
            {
                var people = (Person)contact;

                firstName = people.FirstName.Trim();
                lastName = people.LastName.Trim();
                title = people.JobTitle;
                companyName = String.Empty;
                companyID = people.CompanyID;

                displayName = String.Concat(firstName, _displayNameSeparator, lastName);

                RemoveMember(people.ID);

                if (companyID > 0)
                {
                    AddMemberInDb(people.ID, companyID);
                }

                if (String.IsNullOrEmpty(firstName))// || String.IsNullOrEmpty(lastName)) lastname is not required field now
                    throw new ArgumentException();

            }

            if (!String.IsNullOrEmpty(title))
                title = title.Trim();

            if (!String.IsNullOrEmpty(contact.About))
                contact.About = contact.About.Trim();

            if (!String.IsNullOrEmpty(contact.Industry))
                contact.Industry = contact.Industry.Trim();


            Db.ExecuteNonQuery(
               Update("crm_contact")
                   .Set("first_name", firstName)
                   .Set("last_name", lastName)
                   .Set("company_name", companyName)
                   .Set("title", title)
                   .Set("notes", contact.About)
                   .Set("industry", contact.Industry)
                   .Set("status_id", contact.StatusID)
                   .Set("company_id", companyID)
                   .Set("last_modifed_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                   .Set("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                   .Set("display_name", displayName)
                   .Set("is_shared", (int)contact.ShareType)
                   .Set("contact_type_id", contact.ContactTypeID)
                   .Set("currency", contact.Currency)
                   .Where(Exp.Eq("id", contact.ID)));

            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "contacts.*"));
            FactoryIndexer<ContactsWrapper>.UpdateAsync(contact);
        }

        public void UpdateContactStatus(IEnumerable<int> contactid, int statusid)
        {
            Db.ExecuteNonQuery(
                Update("crm_contact")
                    .Set("status_id", statusid)
                    .Where(Exp.In("id", contactid.ToArray())));
            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "contacts.*"));
        }

        public List<int> FindDuplicateByEmail(List<ContactInfo> items, bool resultReal)
        {
            //resultReal - true => real, false => fake
            if (items.Count == 0) return new List<int>();

            var result = new List<int>();
            var emails = items.ConvertAll(i => i.Data).ToList();
            var sqlQuery = Query("crm_contact_info")
                    .Select("contact_id", "data")
                    .Where(Exp.In("data", emails) & Exp.Eq("type", (int)ContactInfoType.Email));

            if (resultReal)
            {
                result = Db.ExecuteList(sqlQuery).ConvertAll(row => Convert.ToInt32(row[0]));
            }
            else
            {
                List<string> emailsAlreadyExists;

                emailsAlreadyExists = Db.ExecuteList(sqlQuery).ConvertAll(row => Convert.ToString(row[1]));

                if (emailsAlreadyExists.Count != 0) {
                    result = items.Where(i => emailsAlreadyExists.Contains(i.Data)).Select(i => i.ContactID).ToList();
                }

            }

            return result;
        }

        public virtual Dictionary<int, int> SaveContactList(List<Contact> items)
        {
            using (var tx = Db.BeginTransaction())
            {
                var result = new Dictionary<int, int>();

                for (int index = 0; index < items.Count; index++)
                {
                    var item = items[index];

                    if (item.ID == 0)
                        item.ID = index;

                    result.Add(item.ID, SaveContactToDb(item));
                }

                tx.Commit();

                // Delete relative  keys
                _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "contacts.*"));

                return result;
            }
        }

        public virtual void UpdateContactList(List<Contact> items)
        {
            using (var tx = Db.BeginTransaction())
            {
                for (int index = 0; index < items.Count; index++)
                {
                    var item = items[index];

                    if (item.ID == 0)
                        item.ID = index;

                    UpdateContactFromDb(item);
                }

                tx.Commit();

                // Delete relative  keys
                _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "contacts.*"));
            }
        }

        public void MakePublic(int contactId, bool isShared)
        {
            if (contactId <= 0) throw new ArgumentException();

            Db.ExecuteNonQuery(
            Update("crm_contact")
            .Set("is_shared", isShared)
                .Where(Exp.Eq("id", contactId)));

        }

        public virtual int SaveContact(Contact contact)
        {
            var result = SaveContactToDb(contact);
            FactoryIndexer<ContactsWrapper>.IndexAsync(contact);

            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "contacts.*"));
            return result;
        }

        private int SaveContactToDb(Contact contact)
        {
            String firstName;
            String lastName;
            bool isCompany;
            String companyName;
            String title;
            int companyID;

            var displayName = String.Empty;

            if (contact is Company)
            {
                firstName = String.Empty;
                lastName = String.Empty;
                title = String.Empty;
                companyName = (((Company) contact).CompanyName ?? "").Trim();
                isCompany = true;
                companyID = 0;
                displayName = companyName;

                if (String.IsNullOrEmpty(companyName))
                    throw new ArgumentException();

            }
            else
            {
                var people = (Person)contact;

                firstName = people.FirstName.Trim();
                lastName = people.LastName.Trim();
                title = people.JobTitle;
                companyName = String.Empty;
                isCompany = false;

                if (IsExist(people.CompanyID))
                    companyID = people.CompanyID;
                else
                    companyID = 0;

                displayName = String.Concat(firstName, _displayNameSeparator, lastName);


                if (String.IsNullOrEmpty(firstName))// || String.IsNullOrEmpty(lastName)) lastname is not required field now
                    throw new ArgumentException();

            }

            if (!String.IsNullOrEmpty(title))
                title = title.Trim();

            if (!String.IsNullOrEmpty(contact.About))
                contact.About = contact.About.Trim();

            if (!String.IsNullOrEmpty(contact.Industry))
                contact.Industry = contact.Industry.Trim();

            var contactID = Db.ExecuteScalar<int>(
                Insert("crm_contact")
                   .InColumnValue("id", 0)
                   .InColumnValue("first_name", firstName)
                   .InColumnValue("last_name", lastName)
                   .InColumnValue("company_name", companyName)
                   .InColumnValue("title", title)
                   .InColumnValue("notes", contact.About)
                   .InColumnValue("is_company", isCompany)
                   .InColumnValue("industry", contact.Industry)
                   .InColumnValue("status_id", contact.StatusID)
                   .InColumnValue("company_id", companyID)
                   .InColumnValue("create_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                   .InColumnValue("create_on", TenantUtil.DateTimeToUtc(contact.CreateOn == DateTime.MinValue ? TenantUtil.DateTimeNow() : contact.CreateOn))
                   .InColumnValue("last_modifed_on", TenantUtil.DateTimeToUtc(contact.CreateOn == DateTime.MinValue ? TenantUtil.DateTimeNow() : contact.CreateOn))
                   .InColumnValue("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                   .InColumnValue("display_name", displayName)
                   .InColumnValue("is_shared", (int)contact.ShareType)
                   .InColumnValue("contact_type_id", contact.ContactTypeID)
                   .InColumnValue("currency", contact.Currency)
                   .Identity(1, 0, true));

            contact.ID = contactID;

            if (companyID > 0)
                AddMemberInDb(contactID, companyID);

            return contactID;
        }

        public Boolean IsExist(int contactID)
        {
            return Db.ExecuteScalar<bool>("select exists(select 1 from crm_contact where tenant_id = @tid and id = @id)", 
                new { tid = TenantID, id = contactID });
        }

        public Boolean CanDelete(int contactID)
        {
            return Db.ExecuteScalar<int>("select count(*) from crm_invoice where tenant_id = @tid and (contact_id = @id or consignee_id = @id)",
                new { tid = TenantID, id = contactID }) == 0;
        }

        public Dictionary<int, bool> CanDelete(int[] contactID)
        {
            var result = new Dictionary<int, bool>();
            if (contactID.Length == 0) return result;

            var contactIDs = contactID.Distinct().ToList();
            var contactIDsDBString = "";
            contactIDs.ForEach(id => contactIDsDBString += String.Format("{0},", id));
            contactIDsDBString = contactIDsDBString.TrimEnd(',');

            List<int> hasInvoiceIDs;

            var query = String.Format(
                @"select distinct contact_id from crm_invoice where tenant_id = {0} and contact_id in ({1})
                union all
                select distinct consignee_id from crm_invoice where tenant_id = {0} and consignee_id in ({1})", TenantID, contactIDsDBString);

            hasInvoiceIDs = Db.ExecuteList(query).ConvertAll(row => Convert.ToInt32(row[0]));

            foreach (var cid in contactIDs)
            {
                result.Add(cid, !hasInvoiceIDs.Contains(cid));
            }

            return result;
        }

        public virtual Contact GetByID(int contactID)
        {
            return GetByIDFromDb(contactID);
        }

        public Contact GetByIDFromDb(int contactID)
        {
            SqlQuery sqlQuery = GetContactSqlQuery(Exp.Eq("id", contactID));

            var contacts = Db.ExecuteList(sqlQuery).ConvertAll(row => ToContact(row));

            if (contacts.Count == 0) return null;

            return contacts[0];
        }

        public List<Contact> GetContacts(int[] contactID)
        {
            if (contactID == null || contactID.Length == 0) return new List<Contact>();

            SqlQuery sqlQuery = GetContactSqlQuery(Exp.In("id", contactID));

            return Db.ExecuteList(sqlQuery).ConvertAll(row => ToContact(row)).FindAll(CRMSecurity.CanAccessTo);
        }

        public List<Contact> GetRestrictedContacts(int[] contactID)
        {
            if (contactID == null || contactID.Length == 0) return new List<Contact>();

            SqlQuery sqlQuery = GetContactSqlQuery(Exp.In("id", contactID));

            return Db.ExecuteList(sqlQuery).ConvertAll(row => ToContact(row)).FindAll(cont => !CRMSecurity.CanAccessTo(cont));
        }

        public List<Contact> GetRestrictedAndAccessedContacts(int[] contactID)
        {
            if (contactID == null || contactID.Length == 0) return new List<Contact>();

            SqlQuery sqlQuery = GetContactSqlQuery(Exp.In("id", contactID));

            return Db.ExecuteList(sqlQuery).ConvertAll(row => ToContact(row));
        }

        public virtual List<Contact> DeleteBatchContact(int[] contactID)
        {
            if (contactID == null || contactID.Length == 0) return null;

            var contacts = GetContacts(contactID).Where(CRMSecurity.CanDelete).ToList();
            if (!contacts.Any()) return contacts;

            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "contacts.*"));

            DeleteBatchContactsExecute(contacts);

            return contacts;
        }

        public virtual List<Contact> DeleteBatchContact(List<Contact> contacts)
        {
            contacts = contacts.FindAll(CRMSecurity.CanDelete).ToList();
            if (!contacts.Any()) return contacts;

            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "contacts.*"));

            DeleteBatchContactsExecute(contacts);

            return contacts;
        }

        public virtual Contact DeleteContact(int contactID)
        {
            if (contactID <= 0) return null;

            var contact = GetByID(contactID);
            if (contact == null) return null;

            CRMSecurity.DemandDelete(contact);

            DeleteBatchContactsExecute(new List<Contact>() { contact });

            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "contacts.*"));

            var person = contact as Person;
            if (person != null)
            {
                FactoryIndexer<ContactsWrapper>.DeleteAsync(person);
            }
            else
            {
                FactoryIndexer<ContactsWrapper>.DeleteAsync(contact as Company);
            }

            return contact;
        }

        private void DeleteBatchContactsExecute(List<Contact> contacts)
        {
            var personsID = new List<int>();
            var companyID = new List<int>();
            var newContactID = new List<int>();

            foreach (var contact in contacts)
            {
                newContactID.Add(contact.ID);

                if (contact is Company)
                    companyID.Add(contact.ID);
                else
                    personsID.Add(contact.ID);
            }

            var contactID = newContactID.ToArray();
            var filesIDs = new object[0];

            using (var tx = Db.BeginTransaction())
            using (var tagdao = FilesIntegration.GetTagDao())
            {
                var tagNames = Db.ExecuteList(Query("crm_relationship_event").Select("id").Where(Exp.In("contact_id", contactID) & Exp.Eq("have_files", true))).Select(row => String.Format("RelationshipEvent_{0}", row[0])).ToArray();
                if (0 < tagNames.Length)
                {
                    filesIDs = tagdao.GetTags(tagNames, TagType.System).Where(t => t.EntryType == FileEntryType.File).Select(t => t.EntryId).ToArray();
                }

                Db.ExecuteNonQuery(Delete("crm_field_value").Where(Exp.In("entity_id", contactID) & Exp.In("entity_type", new[] { (int)EntityType.Contact, (int)EntityType.Person, (int)EntityType.Company })));
                Db.ExecuteNonQuery(Delete("crm_task").Where(Exp.In("contact_id", contactID)));
                Db.ExecuteNonQuery(new SqlDelete("crm_entity_tag").Where(Exp.In("entity_id", contactID) & Exp.Eq("entity_type", (int)EntityType.Contact)));

                Db.ExecuteNonQuery(Delete("crm_relationship_event").Where(Exp.In("contact_id", contactID)));
                Db.ExecuteNonQuery(Update("crm_deal").Set("contact_id", 0).Where(Exp.In("contact_id", contactID)));

                if (companyID.Count > 0)
                {
                    Db.ExecuteNonQuery(Update("crm_contact").Set("company_id", 0).Where(Exp.In("company_id", companyID)));
                }
                if (personsID.Count > 0)
                {
                    RemoveRelativeInDb(null, EntityType.Person, personsID.ToArray());
                }
                RemoveRelativeInDb(contactID, EntityType.Any, null);

                Db.ExecuteNonQuery(Delete("crm_contact_info").Where(Exp.In("contact_id", contactID)));
                Db.ExecuteNonQuery(Delete("crm_contact").Where(Exp.In("id", contactID)));

                tx.Commit();
            }

            contacts.ForEach(contact => CoreContext.AuthorizationManager.RemoveAllAces(contact));

            using (var filedao = FilesIntegration.GetFileDao())
            {
                foreach (var filesID in filesIDs)
                {
                    filedao.DeleteFile(filesID);
                }
            }

            //todo: remove indexes
        }

        private void MergeContactInfo(Contact fromContact, Contact toContact)
        {

            if ((toContact is Person) && (fromContact is Person))
            {
                var fromPeople = (Person)fromContact;
                var toPeople = (Person)toContact;

                if (toPeople.CompanyID == 0)
                    toPeople.CompanyID = fromPeople.CompanyID;

                if (String.IsNullOrEmpty(toPeople.JobTitle))
                    toPeople.JobTitle = fromPeople.JobTitle;
            }

            if (String.IsNullOrEmpty(toContact.Industry))
                toContact.Industry = fromContact.Industry;

            if (toContact.StatusID == 0)
                toContact.StatusID = fromContact.StatusID;
            if (toContact.ContactTypeID == 0)
                toContact.ContactTypeID = fromContact.ContactTypeID;

            if (String.IsNullOrEmpty(toContact.About))
                toContact.About = fromContact.About;

            UpdateContactFromDb(toContact);
        }

        public void MergeDublicate(int fromContactID, int toContactID)
        {
            if (fromContactID == toContactID)
            {
                if (GetByID(fromContactID) == null)
                    throw new ArgumentException();
                return;
            }

            var fromContact = GetByID(fromContactID);
            var toContact = GetByID(toContactID);

            if (fromContact == null || toContact == null)
                throw new ArgumentException();

            using (var tx = Db.BeginTransaction())
            {
                ISqlInstruction q = Update("crm_task")
                    .Set("contact_id", toContactID)
                    .Where(Exp.Eq("contact_id", fromContactID));
                Db.ExecuteNonQuery(q);

                // crm_entity_contact
                q = new SqlQuery("crm_entity_contact l")
                    .From("crm_entity_contact r")
                    .Select("l.entity_id", "l.entity_type", "l.contact_id")
                    .Where(Exp.EqColumns("l.entity_id", "r.entity_id") & Exp.EqColumns("l.entity_type", "r.entity_type"))
                    .Where("l.contact_id", fromContactID)
                    .Where("r.contact_id", toContactID);
                Db.ExecuteList(q)
                    .ForEach(row =>
                        Db.ExecuteNonQuery(new SqlDelete("crm_entity_contact").Where("entity_id", row[0]).Where("entity_type", row[1]).Where("contact_id", row[2]))
                 );

                q = new SqlUpdate("crm_entity_contact")
                    .Set("contact_id", toContactID)
                    .Where("contact_id", fromContactID);
                Db.ExecuteNonQuery(q);

                // crm_deal
                q = Update("crm_deal")
                    .Set("contact_id", toContactID)
                    .Where("contact_id", fromContactID);
                Db.ExecuteNonQuery(q);

                // crm_invoice
                q = Update("crm_invoice")
                    .Set("contact_id", toContactID)
                    .Where("contact_id", fromContactID);
                Db.ExecuteNonQuery(q);

                // crm_projects
                q = new SqlQuery("crm_projects p")
                    .Select("p.project_id")
                    .From("crm_projects r")
                    .Where(Exp.EqColumns("p.project_id", "r.project_id"))
                    .Where("p.contact_id", fromContactID)
                    .Where("r.contact_id", toContactID);
                var dublicateProjectID = Db.ExecuteList(q).ConvertAll(row => row[0]);

                q = new SqlDelete("crm_projects").Where(Exp.Eq("contact_id", fromContactID) & Exp.In("project_id", dublicateProjectID));
                Db.ExecuteNonQuery(q);

                q = new SqlUpdate("crm_projects").Set("contact_id", toContactID).Where("contact_id", fromContactID);
                Db.ExecuteNonQuery(q);

                // crm_relationship_event
                q = Update("crm_relationship_event")
                    .Set("contact_id", toContactID)
                    .Where("contact_id", fromContactID);
                Db.ExecuteNonQuery(q);

                // crm_entity_tag
                q = new SqlQuery("crm_entity_tag l")
                    .Select("l.tag_id")
                    .From("crm_entity_tag r")
                    .Where(Exp.EqColumns("l.tag_id", "r.tag_id") & Exp.EqColumns("l.entity_type", "r.entity_type"))
                    .Where("l.entity_id", fromContactID)
                    .Where("r.entity_id", toContactID);
                var dublicateTagsID = Db.ExecuteList(q).ConvertAll(row => row[0]);

                q = new SqlDelete("crm_entity_tag").Where(Exp.Eq("entity_id", fromContactID) & Exp.Eq("entity_type", (int)EntityType.Contact) & Exp.In("tag_id", dublicateTagsID));
                Db.ExecuteNonQuery(q);

                q = new SqlUpdate("crm_entity_tag").Set("entity_id", toContactID).Where("entity_id", fromContactID).Where("entity_type", (int)EntityType.Contact);
                Db.ExecuteNonQuery(q);

                // crm_field_value
                q = Query("crm_field_value l")
                    .From("crm_field_value r")
                    .Select("l.field_id")
                    .Where(Exp.EqColumns("l.tenant_id", "r.tenant_id") & Exp.EqColumns("l.field_id", "r.field_id") & Exp.EqColumns("l.entity_type", "r.entity_type"))
                    .Where("l.entity_id", fromContactID)
                    .Where("r.entity_id", toContactID);
                var dublicateCustomFieldValueID = Db.ExecuteList(q).ConvertAll(row => row[0]);

                q = Delete("crm_field_value")
                    .Where("entity_id", fromContactID)
                    .Where(Exp.In("entity_type", new[] { (int)EntityType.Contact, (int)EntityType.Person, (int)EntityType.Company }))
                    .Where(Exp.In("field_id", dublicateCustomFieldValueID));
                Db.ExecuteNonQuery(q);

                q = Update("crm_field_value")
                    .Set("entity_id", toContactID)
                    .Where("entity_id", fromContactID)
                    .Where("entity_type", (int)EntityType.Contact);
                Db.ExecuteNonQuery(q);


                // crm_contact_info

                q = Query("crm_contact_info l")
                   .From("crm_contact_info r")
                   .Select("l.id")
                   .Where(Exp.Eq("l.is_primary", true))
                   .Where(Exp.Eq("r.is_primary", true))
                   .Where(Exp.EqColumns("l.tenant_id", "r.tenant_id"))
                   .Where(Exp.EqColumns("l.type", "r.type"))
                   .Where(Exp.EqColumns("l.category", "r.category"))
                   .Where(!Exp.EqColumns("l.data", "r.data"))
                   .Where("l.contact_id", fromContactID)
                   .Where("r.contact_id", toContactID);
                var dublicatePrimaryContactInfoID = Db.ExecuteList(q).ConvertAll(row => row[0]);

                q = Update("crm_contact_info")
                    .Set("is_primary", false)
                    .Where("contact_id", fromContactID)
                    .Where(Exp.In("id", dublicatePrimaryContactInfoID));
                Db.ExecuteNonQuery(q);

                q = Query("crm_contact_info l")
                    .From("crm_contact_info r")
                    .Select("l.id")
                    .Where(Exp.EqColumns("l.tenant_id", "r.tenant_id"))
                    .Where(Exp.EqColumns("l.type", "r.type"))
                    .Where(Exp.EqColumns("l.is_primary", "r.is_primary"))
                    .Where(Exp.EqColumns("l.category", "r.category"))
                    .Where(Exp.EqColumns("l.data", "r.data"))
                    .Where("l.contact_id", fromContactID)
                    .Where("r.contact_id", toContactID);
                var dublicateContactInfoID = Db.ExecuteList(q).ConvertAll(row => row[0]);

                q = Delete("crm_contact_info")
                    .Where("contact_id", fromContactID)
                    .Where(Exp.In("id", dublicateContactInfoID));
                Db.ExecuteNonQuery(q);


                q = Update("crm_contact_info")
                    .Set("contact_id", toContactID)
                    .Where("contact_id", fromContactID);
                Db.ExecuteNonQuery(q);


                MergeContactInfo(fromContact, toContact);

                // crm_contact
                if ((fromContact is Company) && (toContact is Company))
                {
                    q = Update("crm_contact")
                        .Set("company_id", toContactID)
                        .Where("company_id", fromContactID);
                    Db.ExecuteNonQuery(q);
                }

                q = Delete("crm_contact").Where("id", fromContactID);
                Db.ExecuteNonQuery(q);

                tx.Commit();
            }

            CoreContext.AuthorizationManager.RemoveAllAces(fromContact);
        }

        public List<int> GetContactIDsByCompanyIds(List<int> companyIDs)
        {
            return Db.ExecuteList(Query("crm_contact")
                .Select("id")
                .Distinct()
                .Where(Exp.In("company_id", companyIDs) & Exp.Eq("is_company", false)))
                .ConvertAll(row => Convert.ToInt32(row[0]));
        }

        public List<int> GetContactIDsByContactInfo(ContactInfoType infoType, String data, int? category, bool? isPrimary)
        {
            List<int> ids;
            var q = Query("crm_contact_info")
                  .Select("contact_id")
                  .Where(Exp.Eq("type", (int)infoType));

            if (!string.IsNullOrWhiteSpace(data))
            {
                q = q.Where(Exp.Like("data", data, SqlLike.AnyWhere));
            }

            if (category.HasValue)
            {
                q = q.Where("category", category.Value);
            }
            if (isPrimary.HasValue)
            {
                q = q.Where("is_primary", isPrimary.Value);
            }

            ids = Db.ExecuteList(q).ConvertAll(row => (int)row[0]);
            return ids;
        }

        protected static Contact ToContact(object[] row)
        {
            Contact contact;

            var isCompany = Convert.ToBoolean(row[6]);

            if (isCompany)
                contact = new Company
                           {
                               CompanyName = Convert.ToString(row[3])
                           };
            else
                contact = new Person
                       {
                           FirstName = Convert.ToString(row[1]),
                           LastName = Convert.ToString(row[2]),
                           JobTitle = Convert.ToString(row[4]),
                           CompanyID = Convert.ToInt32(row[9])
                       };

            contact.ID = Convert.ToInt32(row[0]);
            contact.About = Convert.ToString(row[5]);
            contact.Industry = Convert.ToString(row[7]);
            contact.StatusID = Convert.ToInt32(row[8]);
            contact.CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[10]));
            contact.CreateBy = ToGuid(row[11]);
            contact.ContactTypeID = Convert.ToInt32(row[14]);
            contact.Currency = Convert.ToString(row[15]);

            if (row[13] == null)
            {
                var accessSubjectToContact = CRMSecurity.GetAccessSubjectTo(contact);

                contact.ShareType = !accessSubjectToContact.Any() ? ShareType.ReadWrite : ShareType.None;
            }
            else
                contact.ShareType = (ShareType)(Convert.ToInt32(row[13]));

            return contact;
        }

        private static string[] GetContactColumnsTable(String alias)
        {
            if (!String.IsNullOrEmpty(alias))
                alias = alias + ".";

            var result = new List<String>
                             {
                                 "id",
                                 "first_name",
                                 "last_name",
                                 "company_name",
                                 "title",
                                 "notes",
                                 "is_company",
                                 "industry",
                                 "status_id",
                                 "company_id",
                                 "create_on",
                                 "create_by",
                                 "display_name",
                                 "is_shared",
                                 "contact_type_id",
                                 "currency"
                             };

            return string.IsNullOrEmpty(alias) ? result.ToArray() : result.ConvertAll(item => string.Concat(alias, item)).ToArray();
        }

        private SqlQuery GetContactSqlQuery(Exp where)
        {
            var sqlQuery = Query("crm_contact").UseIndex("company_id");

            sqlQuery.Select(GetContactColumnsTable(String.Empty));

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;

        }


        public void ReassignContactsResponsible(Guid fromUserId, Guid toUserId)
        {
            var ids = CRMSecurity.GetContactsIdByManager(fromUserId).ToList();

            foreach (var id in ids)
            {
                var contact = GetByID(id);

                if (contact == null) continue;

                var responsibles = CRMSecurity.GetAccessSubjectGuidsTo(contact);

                if (!responsibles.Any()) continue;

                responsibles.Remove(fromUserId);
                responsibles.Add(toUserId);

                CRMSecurity.SetAccessTo(contact, responsibles.Distinct().ToList());
            }
        }


        /// <summary>
        /// Test method
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="creationDate"></param>
        public void SetContactCreationDate(int contactId, DateTime creationDate)
        {
            Db.ExecuteNonQuery(
                Update("crm_contact")
                    .Set("create_on", TenantUtil.DateTimeToUtc(creationDate))
                    .Where(Exp.Eq("id", contactId)));
            // Delete relative keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "contacts.*"));
        }

        /// <summary>
        /// Test method
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="lastModifedDate"></param>
        public void SetContactLastModifedDate(int contactId, DateTime lastModifedDate)
        {
            Db.ExecuteNonQuery(
                Update("crm_contact")
                    .Set("last_modifed_on", TenantUtil.DateTimeToUtc(lastModifedDate))
                    .Where(Exp.Eq("id", contactId)));
            // Delete relative keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "contacts.*"));
        }
    }
}