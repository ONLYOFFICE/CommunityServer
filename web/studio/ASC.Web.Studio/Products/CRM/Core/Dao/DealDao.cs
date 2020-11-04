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
using System.Text.RegularExpressions;
using System.Globalization;

using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;
using ASC.Files.Core;
using ASC.Web.Files.Api;
using ASC.Web.CRM.Core.Search;
using OrderBy = ASC.CRM.Core.Entities.OrderBy;

namespace ASC.CRM.Core.Dao
{
    public class CachedDealDao : DealDao
    {
        private readonly HttpRequestDictionary<Deal> _dealCache = new HttpRequestDictionary<Deal>("crm_deal");

        public CachedDealDao(int tenantID)
            : base(tenantID)
        {
        }

        public override void EditDeal(Deal deal)
        {
            ResetCache(deal.ID);
            base.EditDeal(deal);
        }

        public override Deal GetByID(int dealID)
        {
            return _dealCache.Get(dealID.ToString(), () => GetByIDBase(dealID));
        }

        private Deal GetByIDBase(int dealID)
        {
            return base.GetByID(dealID);
        }

        public override Deal DeleteDeal(int dealID)
        {
            ResetCache(dealID);
            return base.DeleteDeal(dealID);
        }

        public override int CreateNewDeal(Deal deal)
        {
            deal.ID = base.CreateNewDeal(deal);
            _dealCache.Add(deal.ID.ToString(), deal);
            return deal.ID;
        }

        private void ResetCache(int dealID)
        {
            _dealCache.Reset(dealID.ToString());
        }
    }

    public class DealDao : AbstractDao
    {
        #region Constructor

        public DealDao(int tenantID)
            : base(tenantID)
        {
        }

        #endregion

        #region Methods

        public void AddMember(int dealID, int memberID)
        {
            SetRelative(memberID, EntityType.Opportunity, dealID);
        }

        public Dictionary<int, int[]> GetMembers(int[] dealID)
        {
            return GetRelativeToEntity(null, EntityType.Opportunity, dealID);
        }

        public int[] GetMembers(int dealID)
        {
            return GetRelativeToEntity(null, EntityType.Opportunity, dealID);
        }

        public void SetMembers(int dealID, int[] memberID)
        {
            SetRelative(memberID, EntityType.Opportunity, dealID);
        }

        public void RemoveMember(int dealID, int memberID)
        {
            RemoveRelative(memberID, EntityType.Opportunity, dealID);
        }

        public virtual List<Deal> GetDeals(int[] id)
        {
            if (id == null || !id.Any()) return new List<Deal>();

            return Db.ExecuteList(GetDealSqlQuery(Exp.In("tblDeal.id", id)))
                .ConvertAll(ToDeal).FindAll(CRMSecurity.CanAccessTo).ToList();
        }

        public virtual Deal GetByID(int dealID)
        {
            if (dealID <= 0) return null;

            var deals = GetDeals(new[] { dealID });

            return deals.Count == 0 ? null : deals[0];
        }

        public virtual int CreateNewDeal(Deal deal)
        {
            var result = CreateNewDealInDb(deal);

            deal.ID = result;

            FactoryIndexer<DealsWrapper>.IndexAsync(deal);

            return result;
        }

        private int CreateNewDealInDb(Deal deal)
        {
            if (String.IsNullOrEmpty(deal.Title) || deal.ResponsibleID == Guid.Empty || deal.DealMilestoneID <= 0)
                throw new ArgumentException();

            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "deals.*"));


            var dealID = Db.ExecuteScalar<int>(
                Insert("crm_deal")
                .InColumnValue("id", 0)
                .InColumnValue("title", deal.Title)
                .InColumnValue("description", deal.Description)
                .InColumnValue("responsible_id", deal.ResponsibleID)
                .InColumnValue("contact_id", deal.ContactID)
                .InColumnValue("bid_currency", deal.BidCurrency)
                .InColumnValue("bid_value", deal.BidValue)
                .InColumnValue("bid_type", deal.BidType)
                .InColumnValue("deal_milestone_id", deal.DealMilestoneID)
                .InColumnValue("deal_milestone_probability", deal.DealMilestoneProbability)
                .InColumnValue("expected_close_date", TenantUtil.DateTimeToUtc(deal.ExpectedCloseDate))
                .InColumnValue("actual_close_date", TenantUtil.DateTimeToUtc(deal.ActualCloseDate))
                .InColumnValue("per_period_value", deal.PerPeriodValue)
                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(deal.CreateOn == DateTime.MinValue ? TenantUtil.DateTimeNow() : deal.CreateOn))
                .InColumnValue("create_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                .InColumnValue("last_modifed_on", TenantUtil.DateTimeToUtc(deal.CreateOn == DateTime.MinValue ? TenantUtil.DateTimeNow() : deal.CreateOn))
                .InColumnValue("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                .Identity(1, 0, true));

            //    if (deal.ContactID > 0)
            //      AddMember(dealID, deal.ContactID);

            return dealID;
        }

        public virtual int[] SaveDealList(List<Deal> items)
        {
            using (var tx = Db.BeginTransaction())
            {
                var result = items.Select(item => CreateNewDealInDb(item)).ToArray();
                tx.Commit();
                foreach (var item in items)
                {
                    FactoryIndexer<DealsWrapper>.IndexAsync(item);
                }
                return result;
            }
        }

        public virtual void EditDeal(Deal deal)
        {
            CRMSecurity.DemandEdit(deal);

            //   var oldDeal = GetByID(deal.ID);

            //   if (oldDeal.ContactID > 0)
            //      RemoveMember(oldDeal.ID, oldDeal.ContactID);

            //    AddMember(deal.ID, deal.ContactID);

            Db.ExecuteNonQuery(
                Update("crm_deal")
                .Set("title", deal.Title)
                .Set("description", deal.Description)
                .Set("responsible_id", deal.ResponsibleID)
                .Set("contact_id", deal.ContactID)
                .Set("bid_currency", deal.BidCurrency)
                .Set("bid_value", deal.BidValue)
                .Set("bid_type", deal.BidType)
                .Set("deal_milestone_id", deal.DealMilestoneID)
                .Set("deal_milestone_probability", deal.DealMilestoneProbability)
                .Set("expected_close_date", TenantUtil.DateTimeToUtc(deal.ExpectedCloseDate))
                .Set("per_period_value", deal.PerPeriodValue)
                .Set("actual_close_date", TenantUtil.DateTimeToUtc(deal.ActualCloseDate))
                .Set("last_modifed_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                .Set("last_modifed_by", ASC.Core.SecurityContext.CurrentAccount.ID)
                .Where(Exp.Eq("id", deal.ID))
                );

            FactoryIndexer<DealsWrapper>.IndexAsync(deal);
        }


        public int GetDealsCount()
        {
            return GetDealsCount(String.Empty, Guid.Empty, 0, null, 0, null, null, DateTime.MinValue, DateTime.MinValue);
        }

        public List<Deal> GetAllDeals()
        {
            return GetDeals(String.Empty,
                            Guid.Empty,
                            0,
                            null,
                            0,
                            null,
                            null,
                            DateTime.MinValue,
                            DateTime.MinValue,
                            0,
                            0,
                            new OrderBy(DealSortedByType.Stage, true));
        }

        private Exp WhereConditional(
                                  ICollection<int> exceptIDs,
                                  String searchText,
                                  Guid responsibleID,
                                  int milestoneID,
                                  IEnumerable<String> tags,
                                  int contactID,
                                  DealMilestoneStatus? stageType,
                                  bool? contactAlsoIsParticipant)
        {
            var conditions = new List<Exp>();

            var ids = new List<int>();

            if (!String.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim();

                var keywords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                if (keywords.Length > 0)
                {
                    if (!BundleSearch.TrySelectOpportunity(searchText, out ids))
                    {
                        conditions.Add(BuildLike(new[] {"tblDeal.title", "tblDeal.description"}, keywords));
                    }
                    else if (ids.Count == 0) return null;
                }

            }

            if (tags != null && tags.Any())
            {
                ids = SearchByTags(EntityType.Opportunity, ids.ToArray(), tags);

                if (ids.Count == 0) return null;
            }

            if (contactID > 0)
            {
                if (contactAlsoIsParticipant.HasValue && contactAlsoIsParticipant.Value)
                {
                    var relativeContactsID = GetRelativeToEntity(contactID, EntityType.Opportunity, null).ToList();

                    if (relativeContactsID.Count == 0)
                    {
                        conditions.Add(Exp.Eq("tblDeal.contact_id", contactID));
                    }
                    else
                    {
                        if (ids.Count > 0)
                        {
                            ids = relativeContactsID.Intersect(ids).ToList();

                            if (ids.Count == 0) return null;
                        }
                        else
                        {
                            ids = relativeContactsID;
                        }
                    }
                }
                else
                {
                    conditions.Add(Exp.Eq("tblDeal.contact_id", contactID));
                }
            }

            if (0 < milestoneID && milestoneID < int.MaxValue)
            {
                conditions.Add(Exp.Eq("tblDeal.deal_milestone_id", milestoneID));
            }

            if (responsibleID != Guid.Empty)
            {
                conditions.Add(Exp.Eq("tblDeal.responsible_id", responsibleID));
            }

            if (stageType != null)
            {
                conditions.Add(Exp.Eq("tblDM.status", (int)stageType.Value));
            }

            if (ids.Count > 0)
            {
                if (exceptIDs.Count > 0)
                {
                    ids = ids.Except(exceptIDs).ToList();
                    if (ids.Count == 0) return null;
                }

                conditions.Add(Exp.In("tblDeal.id", ids));
            }
            else if (exceptIDs.Count > 0)
            {
                conditions.Add(!Exp.In("tblDeal.id", exceptIDs.ToArray()));
            }

            if (conditions.Count == 0) return null;

            return conditions.Count == 1 ? conditions[0] : conditions.Aggregate((i, j) => i & j);
        }

        public int GetDealsCount(String searchText,
                                  Guid responsibleID,
                                  int milestoneID,
                                  IEnumerable<String> tags,
                                  int contactID,
                                  DealMilestoneStatus? stageType,
                                  bool? contactAlsoIsParticipant,
                                  DateTime fromDate,
                                  DateTime toDate)
        {
            var cacheKey = TenantID.ToString(CultureInfo.InvariantCulture) +
                        "deals" +
                        SecurityContext.CurrentAccount.ID.ToString() +
                        searchText +
                        responsibleID +
                        milestoneID +
                        contactID;

            if (tags != null)
                cacheKey += String.Join("", tags.ToArray());

            if (stageType.HasValue)
                cacheKey += stageType.Value;

            if (contactAlsoIsParticipant.HasValue)
                cacheKey += contactAlsoIsParticipant.Value;

            if (fromDate != DateTime.MinValue)
                cacheKey += fromDate.ToString();

            if (toDate != DateTime.MinValue)
                cacheKey += toDate.ToString();

            var fromCache = _cache.Get<string>(cacheKey);

            if (fromCache != null) return Convert.ToInt32(fromCache);

            var withParams = !(String.IsNullOrEmpty(searchText) &&
                               responsibleID == Guid.Empty &&
                               milestoneID <= 0 &&
                               (tags == null || !tags.Any()) &&
                               contactID <= 0 &&
                               stageType == null &&
                               contactAlsoIsParticipant == null &&
                               fromDate == DateTime.MinValue &&
                               toDate == DateTime.MinValue);

            ICollection<int> exceptIDs = CRMSecurity.GetPrivateItems(typeof(Deal)).ToList();

            int result;

            if (withParams)
            {
                var whereConditional = WhereConditional(exceptIDs, searchText, responsibleID, milestoneID, tags,
                                                        contactID, stageType, contactAlsoIsParticipant);


                var sqlQuery = GetDealSqlQuery(whereConditional);

                if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                {
                    sqlQuery.Having(Exp.Between("close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)));

                    result = Db.ExecuteList(sqlQuery).Count;

                }
                else if (whereConditional == null)
                {
                    result = 0;
                }
                else
                {
                    result = Db.ExecuteList(sqlQuery).Count;
                }

            }
            else
            {

                var countWithoutPrivate = Db.ExecuteScalar<int>(Query("crm_deal").SelectCount());
                var privateCount = exceptIDs.Count;

                if (privateCount > countWithoutPrivate)
                {
                    _log.Error("Private deals count more than all deals");

                    privateCount = 0;
                }

                result = countWithoutPrivate - privateCount;
            }
            if (result > 0)
            {
                _cache.Insert(cacheKey, result, TimeSpan.FromSeconds(30));
            }
            return result;

        }

        public List<Deal> GetDeals(
                                  String searchText,
                                  Guid responsibleID,
                                  int milestoneID,
                                  IEnumerable<String> tags,
                                  int contactID,
                                  DealMilestoneStatus? stageType,
                                  bool? contactAlsoIsParticipant,
                                  DateTime fromDate,
                                  DateTime toDate,
                                  int from,
                                  int count,
                                  OrderBy orderBy)
        {

            if (CRMSecurity.IsAdmin)
                return GetCrudeDeals(searchText,
                                     responsibleID,
                                     milestoneID,
                                     tags,
                                     contactID,
                                     stageType,
                                     contactAlsoIsParticipant,
                                     fromDate,
                                     toDate,
                                     from,
                                     count,
                                     orderBy);

            var crudeDeals = GetCrudeDeals(searchText,
                                     responsibleID,
                                     milestoneID,
                                     tags,
                                     contactID,
                                     stageType,
                                     contactAlsoIsParticipant,
                                      fromDate,
                                      toDate,
                                      0,
                                      from + count,
                                      orderBy);

            if (crudeDeals.Count == 0) return crudeDeals;

            if (crudeDeals.Count < from + count) return crudeDeals.FindAll(CRMSecurity.CanAccessTo).Skip(from).ToList();

            var result = crudeDeals.FindAll(CRMSecurity.CanAccessTo);

            if (result.Count == crudeDeals.Count) return result.Skip(from).ToList();

            var localCount = count;
            var localFrom = from + count;

            while (true)
            {
                crudeDeals = GetCrudeDeals(searchText,
                                              responsibleID,
                                              milestoneID,
                                              tags,
                                              contactID,
                                              stageType,
                                              contactAlsoIsParticipant,
                                              fromDate,
                                              toDate,
                                              localFrom,
                                              localCount,
                                              orderBy);

                if (crudeDeals.Count == 0) break;

                result.AddRange(crudeDeals.Where(CRMSecurity.CanAccessTo));

                if ((result.Count >= count + from) || (crudeDeals.Count < localCount)) break;

                localFrom += localCount;
                localCount = localCount * 2;
            }

            return result.Skip(from).Take(count).ToList();
        }


        private List<Deal> GetCrudeDeals(
                                   String searchText,
                                   Guid responsibleID,
                                   int milestoneID,
                                   IEnumerable<String> tags,
                                   int contactID,
                                   DealMilestoneStatus? stageType,
                                   bool? contactAlsoIsParticipant,
                                   DateTime fromDate,
                                   DateTime toDate,
                                   int from,
                                   int count,
                                   OrderBy orderBy)
        {
            var sqlQuery = GetDealSqlQuery(null);

            var withParams = !(String.IsNullOrEmpty(searchText) &&
                           responsibleID == Guid.Empty &&
                           milestoneID <= 0 &&
                           (tags == null || !tags.Any()) &&
                           contactID <= 0 &&
                           stageType == null &&
                           contactAlsoIsParticipant == null &&
                           fromDate == DateTime.MinValue &&
                           toDate == DateTime.MinValue);

            var whereConditional = WhereConditional(new List<int>(),
                                                    searchText,
                                                    responsibleID,
                                                    milestoneID,
                                                    tags,
                                                    contactID,
                                                    stageType,
                                                    contactAlsoIsParticipant);



            if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                sqlQuery.Having(Exp.Between("close_date", TenantUtil.DateTimeToUtc(fromDate), TenantUtil.DateTimeToUtc(toDate)));
            else if (withParams && whereConditional == null)
                return new List<Deal>();

            sqlQuery.Where(whereConditional);

            if (0 < from && from < int.MaxValue)
                sqlQuery.SetFirstResult(from);

            if (0 < count && count < int.MaxValue)
                sqlQuery.SetMaxResults(count);

            if (orderBy != null && Enum.IsDefined(typeof(DealSortedByType), orderBy.SortedBy))
                switch ((DealSortedByType)orderBy.SortedBy)
                {
                    case DealSortedByType.Title:
                        sqlQuery.OrderBy("tblDeal.title", orderBy.IsAsc);
                        break;
                    case DealSortedByType.BidValue:
                        sqlQuery.OrderBy("tblDeal.bid_value", orderBy.IsAsc);
                        break;
                    case DealSortedByType.Responsible:

                        sqlQuery.OrderBy("tblDeal.responsible_id", orderBy.IsAsc)
                                .OrderBy("tblDM.sort_order", orderBy.IsAsc)
                                .OrderBy("tblDeal.contact_id", true)
                                .OrderBy("tblDeal.actual_close_date", false)
                                .OrderBy("tblDeal.expected_close_date", true)
                                .OrderBy("tblDeal.title", true);

                        break;
                    case DealSortedByType.Stage:
                        sqlQuery.OrderBy("tblDM.sort_order", orderBy.IsAsc)
                                .OrderBy("tblDeal.contact_id", true)
                                .OrderBy("tblDeal.actual_close_date", false)
                                .OrderBy("tblDeal.expected_close_date", true)
                                .OrderBy("tblDeal.title", true);
                        break;
                    case DealSortedByType.DateAndTime:
                        sqlQuery.OrderBy("close_date", orderBy.IsAsc);
                        break;
                    default:
                        throw new ArgumentException();
                }
            else
                sqlQuery.OrderBy("tblDM.sort_order", true)
                    .OrderBy("tblDeal.contact_id", true)
                    .OrderBy("tblDeal.title", true);

            return Db.ExecuteList(sqlQuery).ConvertAll(ToDeal);
        }

        public List<Deal> GetDealsByContactID(int contactID)
        {

            return GetDeals(String.Empty, Guid.Empty, 0, null, contactID, null, true, DateTime.MinValue,
                            DateTime.MinValue, 0, 0, new OrderBy(DealSortedByType.Title, true));

        }

        public List<Deal> GetDealsByPrefix(String prefix, int from, int count, int contactId = 0, bool internalSearch = true)
        {
            if (count == 0)
                throw new ArgumentException();

            prefix = prefix.Trim();

            var keywords = prefix.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var q = GetDealSqlQuery(null);

            if (keywords.Length == 1)
            {
                q.Where(Exp.Like("tblDeal.title", keywords[0]));
            }
            else
            {
                foreach (var k in keywords)
                {
                    q.Where(Exp.Like("tblDeal.title", k));
                }
            }

            if (0 < from && from < int.MaxValue) q.SetFirstResult(from);
            if (0 < count && count < int.MaxValue) q.SetMaxResults(count);

            if (contactId > 0)
            {
                var ids = GetRelativeToEntity(contactId, EntityType.Opportunity, null);

                if (internalSearch)
                    q.Where(Exp.Eq("tblDeal.contact_id", contactId) | Exp.In("tblDeal.id", ids));
                else
                    q.Where(!Exp.Eq("tblDeal.contact_id", contactId) & !Exp.In("tblDeal.id", ids));
            }

            var sqlResult = Db.ExecuteList(q).ConvertAll(ToDeal).FindAll(CRMSecurity.CanAccessTo);

            return sqlResult.OrderBy(deal => deal.Title).ToList();
        }

        public virtual Deal DeleteDeal(int dealID)
        {
            if (dealID <= 0) return null;

            var deal = GetByID(dealID);
            if (deal == null) return null;

            CRMSecurity.DemandDelete(deal);

            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "deals.*"));

            DeleteBatchDealsExecute(new List<Deal>() { deal });

            FactoryIndexer<DealsWrapper>.DeleteAsync(deal);

            return deal;
        }

        public List<Deal> DeleteBatchDeals(int[] dealID)
        {
            var deals = GetDeals(dealID).FindAll(CRMSecurity.CanDelete).ToList();
            if (!deals.Any()) return deals;

            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "deals.*"));

            DeleteBatchDealsExecute(deals);
            return deals;
        }

        public List<Deal> DeleteBatchDeals(List<Deal> deals)
        {
            deals = deals.FindAll(CRMSecurity.CanDelete).ToList();
            if (!deals.Any()) return deals;

            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "deals.*"));

            DeleteBatchDealsExecute(deals);
            return deals;
        }

        private void DeleteBatchDealsExecute(List<Deal> deals)
        {
            if (deals == null || !deals.Any()) return;

            var dealID = deals.Select(x => x.ID).ToArray();
            object[] filesIDs;

            using (var tagdao = FilesIntegration.GetTagDao())
            {
                var tagNames = Db.ExecuteList(Query("crm_relationship_event").Select("id")
                                    .Where(Exp.Eq("have_files", true) & Exp.In("entity_id", dealID) & Exp.Eq("entity_type", (int)EntityType.Opportunity)))
                                    .Select(row => String.Format("RelationshipEvent_{0}", row[0])).ToArray();
                filesIDs = tagdao.GetTags(tagNames, TagType.System).Where(t => t.EntryType == FileEntryType.File).Select(t => t.EntryId).ToArray();

                using (var tx = Db.BeginTransaction(true))
                {

                    Db.ExecuteNonQuery(Delete("crm_field_value").Where(Exp.In("entity_id", dealID) & Exp.Eq("entity_type", (int)EntityType.Opportunity)));
                    Db.ExecuteNonQuery(new SqlDelete("crm_entity_contact").Where(Exp.In("entity_id", dealID) & Exp.Eq("entity_type", EntityType.Opportunity)));
                    Db.ExecuteNonQuery(Delete("crm_relationship_event").Where(Exp.In("entity_id", dealID) & Exp.Eq("entity_type", EntityType.Opportunity)));
                    Db.ExecuteNonQuery(Delete("crm_task").Where(Exp.In("entity_id", dealID) & Exp.Eq("entity_type", EntityType.Opportunity)));
                    Db.ExecuteNonQuery(new SqlDelete("crm_entity_tag").Where(Exp.In("entity_id", dealID) & Exp.Eq("entity_type", EntityType.Opportunity)));
                    Db.ExecuteNonQuery(Delete("crm_deal").Where(Exp.In("id", dealID)));

                    tx.Commit();
                }

                deals.ForEach(deal => CoreContext.AuthorizationManager.RemoveAllAces(deal));
            }

            using (var filedao = FilesIntegration.GetFileDao())
            {
                foreach (var filesID in filesIDs)
                {
                    filedao.DeleteFile(filesID);
                }
            }
        }

        #region Private Methods

        private static Deal ToDeal(object[] row)
        {
            return new Deal
                {
                    ID = Convert.ToInt32(row[0]),
                    Title = Convert.ToString(row[1]),
                    Description = Convert.ToString(row[2]),
                    ResponsibleID = ToGuid(row[3]),
                    ContactID = Convert.ToInt32(row[4]),
                    BidCurrency = Convert.ToString(row[5]),
                    BidValue = Convert.ToDecimal(row[6]),
                    BidType = (BidType)Convert.ToInt32(row[7]),
                    DealMilestoneID = Convert.ToInt32(row[8]),
                    ExpectedCloseDate = Convert.ToDateTime(row[9]) == DateTime.MinValue ? DateTime.MinValue : TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[9])),
                    PerPeriodValue = Convert.ToInt32(row[10]),
                    DealMilestoneProbability = Convert.ToInt32(row[11]),
                    CreateOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[12])),
                    CreateBy = ToGuid(row[13]),
                    ActualCloseDate = Convert.ToDateTime(row[14]) == DateTime.MinValue ? DateTime.MinValue : TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[14]))
                };
        }

        private SqlQuery GetDealSqlQuery(Exp where)
        {

            SqlQuery sqlQuery = Query("crm_deal tblDeal")
                .Select(
                    "tblDeal.id",
                    "tblDeal.title",
                    "tblDeal.description",
                    "tblDeal.responsible_id",
                    "tblDeal.contact_id",
                    "tblDeal.bid_currency",
                    "tblDeal.bid_value",
                    "tblDeal.bid_type",
                    "tblDeal.deal_milestone_id",
                    "tblDeal.expected_close_date",
                    "tblDeal.per_period_value",
                    "tblDeal.deal_milestone_probability",
                    "tblDeal.create_on",
                    "tblDeal.create_by",
                    "tblDeal.actual_close_date"
                )
                .Select(@"case tblDM.status
                        when 0
                        then tblDeal.expected_close_date
                        else
                           tblDeal.actual_close_date
                        end as close_date")
                .LeftOuterJoin("crm_deal_milestone tblDM",
                               Exp.EqColumns("tblDeal.deal_milestone_id", "tblDM.id"));

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;
        }

        #endregion


        public void ReassignDealsResponsible(Guid fromUserId, Guid toUserId)
        {
            var deals = GetDeals(String.Empty,
                            fromUserId,
                            0,
                            null,
                            0,
                            DealMilestoneStatus.Open, 
                            null,
                            DateTime.MinValue,
                            DateTime.MinValue,
                            0,
                            0,
                            null);

            foreach (var deal in deals)
            {
                deal.ResponsibleID = toUserId;

                EditDeal(deal);

                var responsibles = CRMSecurity.GetAccessSubjectGuidsTo(deal);

                if (!responsibles.Any()) continue;

                responsibles.Remove(fromUserId);
                responsibles.Add(toUserId);

                CRMSecurity.SetAccessTo(deal, responsibles.Distinct().ToList());
            }
        }

        #endregion


        /// <summary>
        /// Test method
        /// </summary>
        /// <param name="opportunityid"></param>
        /// <param name="creationDate"></param>
        public void SetDealCreationDate(int opportunityid, DateTime creationDate)
        {
            Db.ExecuteNonQuery(
                Update("crm_deal")
                    .Set("create_on", TenantUtil.DateTimeToUtc(creationDate))
                    .Where(Exp.Eq("id", opportunityid)));
            // Delete relative keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "deals.*"));
        }

        /// <summary>
        /// Test method
        /// </summary>
        /// <param name="opportunityid"></param>
        /// <param name="lastModifedDate"></param>
        public void SetDealLastModifedDate(int opportunityid, DateTime lastModifedDate)
        {
            Db.ExecuteNonQuery(
                Update("crm_deal")
                    .Set("last_modifed_on", TenantUtil.DateTimeToUtc(lastModifedDate))
                    .Where(Exp.Eq("id", opportunityid)));
            // Delete relative keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "deals.*"));
        }
    }
}