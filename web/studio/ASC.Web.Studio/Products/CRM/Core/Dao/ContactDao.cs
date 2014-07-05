/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Caching;
using ASC.Collections;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;
using ASC.Files.Core;
using ASC.FullTextIndex;
using ASC.Web.CRM.Core.Enums;
using ASC.Web.Files.Api;
using OrderBy = ASC.CRM.Core.Entities.OrderBy;

namespace ASC.CRM.Core.Dao
{
    public class CachedContactDao : ContactDao
    {

        private readonly HttpRequestDictionary<Contact> _contactCache = new HttpRequestDictionary<Contact>("crm_contact");

        public CachedContactDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
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

        public ContactDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
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

            prefix = prefix.Trim();

            var keywords = prefix.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var q = GetContactSqlQuery(null);

            switch (searchType)
            {
                case 0: // Company
                    q.Where(Exp.Eq("is_company", true));
                    break;
                case 1: // Persons
                    q.Where(Exp.Eq("is_company", false));
                    break;
                case 2: // PersonsWithoutCompany
                    q.Where(Exp.Eq("company_id", 0) & Exp.Eq("is_company", false));

                    break;
                case 3: // CompaniesAndPersonsWithoutCompany
                    q.Where(Exp.Eq("company_id", 0));

                    break;
            }

            if (keywords.Length == 1)
            {
                q.Where(Exp.Like("display_name", keywords[0]));
            }
            else
            {
                foreach (var k in keywords)
                {
                    q.Where(Exp.Like("display_name", k));
                }
            }

            if (0 < from && from < int.MaxValue) q.SetFirstResult(from);
            if (0 < count && count < int.MaxValue) q.SetMaxResults(count);

            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(q).ConvertAll(row => ToContact(row)).FindAll(CRMSecurity.CanAccessTo);
                return sqlResult.OrderBy(contact => contact.GetTitle()).ToList();
            }
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

            var fromCache = _cache.Get(cacheKey);

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

            using (var db = GetDb())
            {
                if (withParams)
                {
                    ICollection<int> excludedContactIDs;

                    switch (contactListView)
                    {
                        case ContactListViewType.Person:
                            excludedContactIDs = CRMSecurity.GetPrivateItems(typeof(Person))
                                                .Except(db.ExecuteList(Query("crm_contact").Select("id")
                                                        .Where(Exp.Eq("is_shared", true) & Exp.Eq("is_company", false)))
                                                  .Select(x => Convert.ToInt32(x[0]))).ToList();
                            break;
                        case ContactListViewType.Company:
                            excludedContactIDs = CRMSecurity.GetPrivateItems(typeof(Company))
                                               .Except(db.ExecuteList(Query("crm_contact").Select("id")
                                                      .Where(Exp.Eq("is_shared", true) & Exp.Eq("is_company", true)))
                                                      .Select(x => Convert.ToInt32(x[0]))).ToList();

                            break;
                        default:
                            excludedContactIDs = CRMSecurity.GetPrivateItems(typeof(Company)).Union(CRMSecurity.GetPrivateItems(typeof(Person)))
                                        .Except(db.ExecuteList(Query("crm_contact").Select("id").Where("is_shared", true))
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
                            result = db.ExecuteScalar<int>(sqlQuery);
                        }
                        else
                        {
                            var sqlQuery = Query("crm_contact").Select("id, is_company, is_shared").Where(whereConditional);
                            var sqlResultRows = db.ExecuteList(sqlQuery);

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
                    var countWithoutPrivate = db.ExecuteScalar<int>(Query("crm_contact").SelectCount());

                    var privateCount = CRMSecurity.GetPrivateItemsCount(typeof(Person)) +
                                       CRMSecurity.GetPrivateItemsCount(typeof(Company)) -
                                       db.ExecuteScalar<int>(Query("crm_contact").Where("is_shared", true).SelectCount());

                    if (privateCount < 0)
                        privateCount = 0;

                    if (privateCount > countWithoutPrivate)
                    {
                        _log.Error("Private contacts count more than all contacts");

                        privateCount = 0;
                    }

                    result = countWithoutPrivate - privateCount;
                }
            }
            if (result > 0)
                _cache.Insert(cacheKey, result, new CacheDependency(null, new[] { _contactCacheKey }), Cache.NoAbsoluteExpiration,
                                      TimeSpan.FromMinutes(1));

            return result;
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

                if (!FullTextSearch.SupportModule(FullTextSearch.CRMContactsModule))
                {
                    conditions.Add(BuildLike(new[] { "display_name" }, keywords));
                }
                else
                {
                    ids = FullTextSearch.Search(searchText, FullTextSearch.CRMContactsModule)
                                 .GetIdentifiers()
                                 .Select(item => Convert.ToInt32(item.Split('_')[1])).Distinct()
                                 .ToList();

                    if (ids.Count == 0) return null;

                }
            }

            if (tags != null && tags.Any())
            {
                ids = SearchByTags(EntityType.Contact, ids.ToArray(), tags);

                if (ids.Count == 0) return null;
            }

            using (var db = GetDb())
            {
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
                            ids = db.ExecuteList(new SqlQuery("crm_entity_contact").Select("contact_id")
                                                 .Distinct()
                                                 .Where(Exp.In("contact_id", ids) & Exp.Eq("entity_type", (int)EntityType.Opportunity)))
                                                 .ConvertAll(row => Convert.ToInt32(row[0]));
                        }
                        else
                        {
                            ids = db.ExecuteList(new SqlQuery("crm_entity_contact").Select("contact_id")
                                                 .Distinct()
                                                 .Where(Exp.Eq("entity_type", (int)EntityType.Opportunity)))
                                                 .ConvertAll(row => Convert.ToInt32(row[0]));
                        }

                        if (ids.Count == 0) return null;

                        break;
                }
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
                conditions.Add(!Exp.In("id", exceptIDs.ToArray()));
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
                    default:
                        sqlQuery.OrderBy("display_name", orderBy.IsAsc);
                        break;
                }
            }

            using (var db = GetDb())
            {
                var contacts = db.ExecuteList(sqlQuery).ConvertAll(ToContact);
                return selectIsSharedInNewScheme && isShared.HasValue ?
                    contacts.Where(c => (isShared.Value ? c.ShareType != ShareType.None : c.ShareType == ShareType.None)).ToList() :
                    contacts;
            }
        }

        public List<Contact> GetContactsByName(String title)
        {
            if (String.IsNullOrEmpty(title)) return new List<Contact>();

            title = title.Trim();

            var titleParts = title.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            using (var db = GetDb())
            {
                if (titleParts.Length == 1)
                    return db.ExecuteList(GetContactSqlQuery(Exp.Eq("display_name", title)))
                           .ConvertAll(ToContact)
                           .FindAll(CRMSecurity.CanAccessTo);
                else if (titleParts.Length == 2)
                    return db.ExecuteList(GetContactSqlQuery(Exp.Eq("display_name", String.Concat(titleParts[0], _displayNameSeparator, titleParts[1]))))
                              .ConvertAll(ToContact)
                              .FindAll(CRMSecurity.CanAccessTo);
            }
            return GetContacts(title, null, -1, -1, ContactListViewType.All, DateTime.MinValue, DateTime.MinValue, 0, 0, null);
        }

        public void RemoveMember(int[] peopleID)
        {
            if ((peopleID == null) || (peopleID.Length == 0)) return;

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Update("crm_contact").Set("company_id", 0).Where(Exp.In("id", peopleID)));
                RemoveRelative(null, EntityType.Person, peopleID, db);
            }
        }

        public void RemoveMember(int peopleID)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Update("crm_contact").Set("company_id", 0).Where("id", peopleID));
                RemoveRelative(0, EntityType.Person, peopleID, db);
            }
        }

        public void AddMember(int peopleID, int companyID, DbManager db)
        {
            db.ExecuteNonQuery(Update("crm_contact")
               .Set("company_id", companyID)
               .Where("id", peopleID));
            SetRelative(companyID, EntityType.Person, peopleID, db);
        }

        public void AddMember(int peopleID, int companyID)
        {
            using (var db = GetDb())
            {
                AddMember(peopleID, companyID, db);
            }
        }

        public void SetMembers(int companyID, params int[] peopleIDs)
        {
            if (companyID == 0)
                throw new ArgumentException();

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                db.ExecuteNonQuery(new SqlDelete("crm_entity_contact")
                                        .Where(Exp.Eq("entity_type", EntityType.Person) &
                                         Exp.Eq("contact_id", companyID)));

                db.ExecuteNonQuery(Update("crm_contact")
                                       .Set("company_id", 0)
                                       .Where(Exp.Eq("company_id", companyID)));

                if (!(peopleIDs == null || peopleIDs.Length == 0))
                {
                    db.ExecuteNonQuery(Update("crm_contact")
                        .Set("company_id", companyID)
                        .Where(Exp.In("id", peopleIDs)));
                    foreach (var peopleID in peopleIDs)
                        SetRelative(companyID, EntityType.Person, peopleID, db);
                }

                tx.Commit();

            }

        }

        public void SetRelativeContactProject(IEnumerable<int> contactid, int projectid)
        {
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                foreach (var id in contactid)
                {

                    db.ExecuteNonQuery(Insert("crm_projects")
                                                  .InColumnValue("contact_id", id)
                                                  .InColumnValue("project_id", projectid));
                }

                tx.Commit();
            }
        }

        public void RemoveRelativeContactProject(int contactid, int projectid)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Delete("crm_projects")
                .Where(Exp.Eq("contact_id", contactid) & Exp.Eq("project_id", projectid)));
            }

        }


        public IEnumerable<Contact> GetContactsByProjectID(int projectid)
        {
            using (var db = GetDb())
            {
                var contactIds = db.ExecuteList(Query("crm_projects")
                                .Select("contact_id")
                                .Where("project_id", projectid)).Select(x => Convert.ToInt32(x[0]));


                if (!contactIds.Any()) return new List<Contact>();

                return GetContacts(contactIds.ToArray());
            }
        }

        public List<Contact> GetMembers(int companyID)
        {
            return GetContacts(GetRelativeToEntity(companyID, EntityType.Person, null));
        }

        public Dictionary<int, int> GetMembersCount(int[] companyID)
        {

            var sqlQuery = new SqlQuery("crm_entity_contact")
                       .Select("contact_id")
                       .SelectCount()
                       .Where(Exp.In("contact_id", companyID) & Exp.Eq("entity_type", (int)EntityType.Person))
                       .GroupBy("contact_id");

            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(sqlQuery);

                return sqlResult.ToDictionary(item => Convert.ToInt32(item[0]), item => Convert.ToInt32(item[1]));
            }
        }

        public int GetMembersCount(int companyID)
        {
            using (var db = GetDb())
            {
                return db.ExecuteScalar<int>(
                    new SqlQuery("crm_entity_contact")
                   .SelectCount()
                   .Where(Exp.Eq("contact_id", companyID) & Exp.Eq("entity_type", (int)EntityType.Person)));
            }
        }

        public virtual void UpdateContact(Contact contact)
        {
            using (var db = GetDb())
            {
                UpdateContact(contact, db);
            }
        }

        private void UpdateContact(Contact contact, DbManager db)
        {
            var originalContact = GetByID(contact.ID, db);
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
                    AddMember(people.ID, companyID, db);
                }

                if (String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName))
                    throw new ArgumentException();

            }

            if (!String.IsNullOrEmpty(title))
                title = title.Trim();

            if (!String.IsNullOrEmpty(contact.About))
                contact.About = contact.About.Trim();

            if (!String.IsNullOrEmpty(contact.Industry))
                contact.Industry = contact.Industry.Trim();


            db.ExecuteNonQuery(
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
            _cache.Insert(_contactCacheKey, String.Empty);
        }

        public void UpdateContactStatus(List<Contact> contacts, int statusid)
        {
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                   Update("crm_contact")
                       .Set("status_id", statusid)
                       .Where(Exp.In("id", contacts.ConvertAll(c => c.ID))));
            }
            // Delete relative  keys
            _cache.Insert(_contactCacheKey, String.Empty);
        }

        public List<Object[]> FindDuplicateByEmail(List<ContactInfo> items)
        {

            if (items.Count == 0) return new List<Object[]>();

            var result = new List<Object[]>();

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {

                var sqlQueryStr = @"
                                    CREATE  TEMPORARY TABLE IF NOT EXISTS `crm_dublicated` (
                                    `contact_id` INT(11) NOT NULL,
                                    `email` VARCHAR(255) NOT NULL DEFAULT '0',
                                    `tenant_id` INT(11) NOT NULL DEFAULT '0'	
                                );";

                db.ExecuteNonQuery(sqlQueryStr);

                db.ExecuteNonQuery(Delete("crm_dublicated"));

                foreach (var item in items)
                    db.ExecuteNonQuery(
                         Insert("crm_dublicated")
                        .InColumnValue("contact_id", item.ContactID)
                        .InColumnValue("email", item.Data));

                var sqlQuery = Query("crm_dublicated tblLeft")
                               .Select("tblLeft.contact_id",
                                       "tblLeft.email",
                                       "tblRight.contact_id",
                                       "tblRight.data")
                               .LeftOuterJoin("crm_contact_info tblRight", Exp.EqColumns("tblLeft.tenant_id", "tblRight.tenant_id") & Exp.EqColumns("tblLeft.email", "tblRight.data"))
                               .Where(Exp.Eq("tblRight.tenant_id", TenantID) & Exp.Eq("tblRight.type", (int)ContactInfoType.Email));

                result = db.ExecuteList(sqlQuery);


                tx.Commit();

            }

            return result;


        }

        public virtual Dictionary<int, int> SaveContactList(List<Contact> items)
        {
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                var result = new Dictionary<int, int>();

                for (int index = 0; index < items.Count; index++)
                {
                    var item = items[index];

                    if (item.ID == 0)
                        item.ID = index;

                    result.Add(item.ID, SaveContact(item, db));
                }

                tx.Commit();

                // Delete relative  keys
                _cache.Insert(_contactCacheKey, String.Empty);

                return result;
            }
        }

        public virtual void UpdateContactList(List<Contact> items)
        {
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                for (int index = 0; index < items.Count; index++)
                {
                    var item = items[index];

                    if (item.ID == 0)
                        item.ID = index;

                    UpdateContact(item, db);
                }

                tx.Commit();

                // Delete relative  keys
                _cache.Insert(_contactCacheKey, String.Empty);
            }
        }

        public void MakePublic(int contactId, bool isShared)
        {
            if (contactId <= 0) throw new ArgumentException();

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(
                Update("crm_contact")
               .Set("is_shared", isShared)
                    .Where(Exp.Eq("id", contactId)));

            }

        }

        public virtual int SaveContact(Contact contact)
        {
            using (var db = GetDb())
            {
                var result = SaveContact(contact, db);
                // Delete relative  keys
                _cache.Insert(_contactCacheKey, String.Empty);
                return result;
            }
        }

        private int SaveContact(Contact contact, DbManager db)
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
                companyName = ((Company)contact).CompanyName.Trim();
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


                if (String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName))
                    throw new ArgumentException();

            }

            if (!String.IsNullOrEmpty(title))
                title = title.Trim();

            if (!String.IsNullOrEmpty(contact.About))
                contact.About = contact.About.Trim();

            if (!String.IsNullOrEmpty(contact.Industry))
                contact.Industry = contact.Industry.Trim();

            var contactID = db.ExecuteScalar<int>(
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
                   .InColumnValue("create_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                   .InColumnValue("last_modifed_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                   .InColumnValue("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                   .InColumnValue("display_name", displayName)
                   .InColumnValue("is_shared", (int)contact.ShareType)
                   .InColumnValue("contact_type_id", contact.ContactTypeID)
                   .InColumnValue("currency", contact.Currency)
                   .Identity(1, 0, true));

            contact.ID = contactID;

            if (companyID > 0)
                AddMember(contactID, companyID, db);

            return contactID;
        }

        public Boolean IsExist(int contactID)
        {
            var q = new SqlExp(
                    string.Format(@"select exists(select 1 from crm_contact where tenant_id = {0} and id = {1})",
                                TenantID,
                                contactID));

            using (var db = GetDb())
            {
                return db.ExecuteScalar<bool>(q);
            }
        }

        public Boolean CanDelete(int contactID)
        {
            var q = new SqlExp(
                string.Format(@"select count(*) from crm_invoice where tenant_id = {0} and (contact_id = {1} or consignee_id = {1})",
                              TenantID,
                              contactID));

            using (var db = GetDb())
            {
                return db.ExecuteScalar<int>(q) == 0;
            }
        }

        public virtual Contact GetByID(int contactID)
        {
            using (var db = GetDb())
            {
                return GetByID(contactID, db);
            }
        }

        public Contact GetByID(int contactID, DbManager db)
        {
            SqlQuery sqlQuery = GetContactSqlQuery(Exp.Eq("id", contactID));

            var contacts = db.ExecuteList(sqlQuery).ConvertAll(row => ToContact(row));

            if (contacts.Count == 0) return null;

            return contacts[0];
        }

        public List<Contact> GetContacts(int[] contactID)
        {
            if (contactID == null || contactID.Length == 0) return new List<Contact>();

            SqlQuery sqlQuery = GetContactSqlQuery(Exp.In("id", contactID));

            using (var db = GetDb())
            {
                return db.ExecuteList(sqlQuery).ConvertAll(row => ToContact(row)).FindAll(CRMSecurity.CanAccessTo);
            }
        }

        public virtual List<Contact> DeleteBatchContact(int[] contactID)
        {
            if (contactID == null || contactID.Length == 0) return null;

            var contacts = GetContacts(contactID).Where(CRMSecurity.CanDelete).ToList();
            if (!contacts.Any()) return contacts;

            // Delete relative  keys
            _cache.Insert(_contactCacheKey, String.Empty);

            DeleteBatchContactsExecute(contacts);

            return contacts;
        }

        public virtual List<Contact> DeleteBatchContact(List<Contact> contacts)
        {
            contacts = contacts.FindAll(CRMSecurity.CanDelete).ToList();
            if (!contacts.Any()) return contacts;

            // Delete relative  keys
            _cache.Insert(_contactCacheKey, String.Empty);

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
            _cache.Insert(_invoiceCacheKey, String.Empty);

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

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            using (var tagdao = FilesIntegration.GetTagDao())
            {
                var tagNames = db.ExecuteList(Query("crm_relationship_event").Select("id").Where(Exp.In("contact_id", contactID) & Exp.Eq("have_files", true))).Select(row => String.Format("RelationshipEvent_{0}", row[0])).ToArray();
                if (0 < tagNames.Length)
                {
                    filesIDs = tagdao.GetTags(tagNames, TagType.System).Where(t => t.EntryType == FileEntryType.File).Select(t => t.EntryId).ToArray();
                }

                db.ExecuteNonQuery(Delete("crm_field_value").Where(Exp.In("entity_id", contactID) & Exp.In("entity_type", new[] { (int)EntityType.Contact, (int)EntityType.Person, (int)EntityType.Company })));
                db.ExecuteNonQuery(Delete("crm_task").Where(Exp.In("contact_id", contactID)));
                db.ExecuteNonQuery(new SqlDelete("crm_entity_tag").Where(Exp.In("entity_id", contactID) & Exp.Eq("entity_type", (int)EntityType.Contact)));

                db.ExecuteNonQuery(Delete("crm_relationship_event").Where(Exp.In("contact_id", contactID)));
                db.ExecuteNonQuery(Update("crm_deal").Set("contact_id", 0).Where(Exp.In("contact_id", contactID)));

                if (companyID.Count > 0)
                {
                    db.ExecuteNonQuery(Update("crm_contact").Set("company_id", 0).Where(Exp.In("company_id", companyID)));
                }
                if (personsID.Count > 0)
                {
                    RemoveRelative(null, EntityType.Person, personsID.ToArray(), db);
                }
                RemoveRelative(contactID, EntityType.Any, null, db);

                db.ExecuteNonQuery(Delete("crm_contact_info").Where(Exp.In("contact_id", contactID)));
                db.ExecuteNonQuery(Delete("crm_contact").Where(Exp.In("id", contactID)));

                tx.Commit();
            }

            contacts.ForEach(contact => CoreContext.AuthorizationManager.RemoveAllAces(contact));

            using (var filedao = FilesIntegration.GetFileDao())
            {
                foreach (var filesID in filesIDs)
                {
                    filedao.DeleteFolder(filesID);
                    filedao.DeleteFile(filesID);
                }
            }
        }

        private void MergeContactInfo(Contact fromContact, Contact toContact, DbManager db)
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

            UpdateContact(toContact, db);
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

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                ISqlInstruction q = Update("crm_task")
                    .Set("contact_id", toContactID)
                    .Where(Exp.Eq("contact_id", fromContactID));
                db.ExecuteNonQuery(q);

                // crm_entity_contact
                q = new SqlQuery("crm_entity_contact l")
                    .From("crm_entity_contact r")
                    .Select("l.entity_id", "l.entity_type", "l.contact_id")
                    .Where(Exp.EqColumns("l.entity_id", "r.entity_id") & Exp.EqColumns("l.entity_type", "r.entity_type"))
                    .Where("l.contact_id", fromContactID)
                    .Where("r.contact_id", toContactID);
                db.ExecuteList(q)
                    .ForEach(row =>
                        db.ExecuteNonQuery(new SqlDelete("crm_entity_contact").Where("entity_id", row[0]).Where("entity_type", row[1]).Where("contact_id", row[2]))
                 );

                q = new SqlUpdate("crm_entity_contact")
                    .Set("contact_id", toContactID)
                    .Where("contact_id", fromContactID);
                db.ExecuteNonQuery(q);

                // crm_deal
                q = Update("crm_deal")
                    .Set("contact_id", toContactID)
                    .Where("contact_id", fromContactID);
                db.ExecuteNonQuery(q);

                // crm_invoice
                q = Update("crm_invoice")
                    .Set("contact_id", toContactID)
                    .Where("contact_id", fromContactID);
                db.ExecuteNonQuery(q);

                // crm_projects
                q = new SqlQuery("crm_projects p")
                    .Select("p.project_id")
                    .From("crm_projects r")
                    .Where(Exp.EqColumns("p.project_id", "r.project_id"))
                    .Where("p.contact_id", fromContactID)
                    .Where("r.contact_id", toContactID);
                var dublicateProjectID = db.ExecuteList(q).ConvertAll(row => row[0]);

                q = new SqlDelete("crm_projects").Where(Exp.Eq("contact_id", fromContactID) & Exp.In("project_id", dublicateProjectID));
                db.ExecuteNonQuery(q);

                q = new SqlUpdate("crm_projects").Set("contact_id", toContactID).Where("contact_id", fromContactID);
                db.ExecuteNonQuery(q);

                // crm_relationship_event
                q = Update("crm_relationship_event")
                    .Set("contact_id", toContactID)
                    .Where("contact_id", fromContactID);
                db.ExecuteNonQuery(q);

                // crm_entity_tag
                q = new SqlQuery("crm_entity_tag l")
                    .Select("l.tag_id")
                    .From("crm_entity_tag r")
                    .Where(Exp.EqColumns("l.tag_id", "r.tag_id") & Exp.EqColumns("l.entity_type", "r.entity_type"))
                    .Where("l.entity_id", fromContactID)
                    .Where("r.entity_id", toContactID);
                var dublicateTagsID = db.ExecuteList(q).ConvertAll(row => row[0]);

                q = new SqlDelete("crm_entity_tag").Where(Exp.Eq("entity_id", fromContactID) & Exp.Eq("entity_type", (int)EntityType.Contact) & Exp.In("tag_id", dublicateTagsID));
                db.ExecuteNonQuery(q);

                q = new SqlUpdate("crm_entity_tag").Set("entity_id", toContactID).Where("entity_id", fromContactID).Where("entity_type", (int)EntityType.Contact);
                db.ExecuteNonQuery(q);

                // crm_field_value
                q = Query("crm_field_value l")
                    .From("crm_field_value r")
                    .Select("l.field_id")
                    .Where(Exp.EqColumns("l.tenant_id", "r.tenant_id") & Exp.EqColumns("l.field_id", "r.field_id") & Exp.EqColumns("l.entity_type", "r.entity_type"))
                    .Where("l.entity_id", fromContactID)
                    .Where("r.entity_id", toContactID);
                var dublicateCustomFieldValueID = db.ExecuteList(q).ConvertAll(row => row[0]);

                q = Delete("crm_field_value")
                    .Where("entity_id", fromContactID)
                    .Where(Exp.In("entity_type", new[] { (int)EntityType.Contact, (int)EntityType.Person, (int)EntityType.Company }))
                    .Where(Exp.In("field_id", dublicateCustomFieldValueID));
                db.ExecuteNonQuery(q);

                q = Update("crm_field_value")
                    .Set("entity_id", toContactID)
                    .Where("entity_id", fromContactID)
                    .Where("entity_type", (int)EntityType.Contact);
                db.ExecuteNonQuery(q);

                // crm_contact_info
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
                var dublicateContactInfoID = db.ExecuteList(q).ConvertAll(row => row[0]);

                q = Delete("crm_contact_info")
                    .Where("contact_id", fromContactID)
                    .Where(Exp.In("id", dublicateContactInfoID));
                db.ExecuteNonQuery(q);

                q = Update("crm_contact_info")
                    .Set("contact_id", toContactID)
                    .Where("contact_id", fromContactID);
                db.ExecuteNonQuery(q);


                MergeContactInfo(fromContact, toContact, db);

                // crm_contact
                if ((fromContact is Company) && (toContact is Company))
                {
                    q = Update("crm_contact")
                        .Set("company_id", toContactID)
                        .Where("company_id", fromContactID);
                    db.ExecuteNonQuery(q);
                }

                q = Delete("crm_contact").Where("id", fromContactID);
                db.ExecuteNonQuery(q);

                tx.Commit();
            }

            CoreContext.AuthorizationManager.RemoveAllAces(fromContact);
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

        private SqlQuery GetContactSqlQuery(Exp where, String alias)
        {
            var sqlQuery = Query("crm_contact");

            if (!String.IsNullOrEmpty(alias))
            {
                sqlQuery = new SqlQuery(String.Concat("crm_contact ", alias))
                           .Where(Exp.Eq(alias + ".tenant_id", TenantID));
                sqlQuery.Select(GetContactColumnsTable(alias));

            }
            else
                sqlQuery.Select(GetContactColumnsTable(String.Empty));


            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;

        }

        private SqlQuery GetContactSqlQuery(Exp where)
        {
            return GetContactSqlQuery(where, String.Empty);

        }
    }
}