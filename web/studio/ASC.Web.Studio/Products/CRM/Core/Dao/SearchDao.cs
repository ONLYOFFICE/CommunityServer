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

using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Utils;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;
using ASC.Web.CRM.Core.Search;

namespace ASC.CRM.Core.Dao
{
    public class SearchDao : AbstractDao
    {
        #region Members

        private Dictionary<EntityType, IEnumerable<int>> _findedIDs;
        private bool _fullTextSearchEnable;
        private DaoFactory DaoFactory { get; set; }

        #endregion

        #region Constructor

        public SearchDao(int tenantID, DaoFactory daoFactory)
            : base(tenantID)
        {
            DaoFactory = daoFactory;
        }

        #endregion

        #region Methods

        #region Public
        
        public SearchResultItem[] Search(String searchText)
        {
            var keywords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
               .ToArray();

            if (keywords.Length == 0) return new List<SearchResultItem>().ToArray();

            _fullTextSearchEnable = BundleSearch.Support(EntityType.Case)
                                    && BundleSearch.Support(EntityType.Contact)
                                    && BundleSearch.Support(EntityType.Opportunity)
                                    && BundleSearch.Support(EntityType.Task)
                                    && BundleSearch.Support(EntityType.Invoice);
                            
            if (_fullTextSearchEnable)
            {
                _findedIDs = new Dictionary<EntityType, IEnumerable<int>>();

                List<int> casesId;
                if (BundleSearch.TrySelectCase(searchText, out casesId))
                {
                    _findedIDs.Add(EntityType.Case, casesId);
                }

                List<int> contactsId;
                if (BundleSearch.TrySelectContact(searchText, out contactsId))
                {
                    _findedIDs.Add(EntityType.Contact, contactsId);
                }

                List<int> dealsId;
                if (BundleSearch.TrySelectOpportunity(searchText, out dealsId))
                {
                    _findedIDs.Add(EntityType.Opportunity, dealsId);
                }

                List<int> tasksId;
                if (FactoryIndexer<TasksWrapper>.TrySelectIds(r => r.MatchAll(searchText), out tasksId))
                {
                    _findedIDs.Add(EntityType.Task, tasksId);
                }

                List<int> invoicesId;
                if (FactoryIndexer<InvoicesWrapper>.TrySelectIds(r => r.MatchAll(searchText), out invoicesId))
                {
                    _findedIDs.Add(EntityType.Invoice, invoicesId);
                }
            }
            else
            {
                _findedIDs = SearchByCustomFields(keywords)
                                     .Union(SearchByRelationshipEvent(keywords))
                                     .Union(SearchByContactInfos(keywords))
                                     .ToLookup(pair => pair.Key, pair => pair.Value)
                                     .ToDictionary(group => group.Key, group => group.First());
            }


            var searchQuery = GetSearchQuery(keywords);

            if (searchQuery == null) return new SearchResultItem[0];

            return ToSearchResultItem(Db.ExecuteList(searchQuery));
        }

        #endregion

        #region Private

        private Dictionary<EntityType, IEnumerable<int>> SearchByRelationshipEvent(String[] keywords)
        {
            var historyQuery = Query("crm_relationship_event")
                        .Select(
                                "contact_id",
                                "entity_id",
                                "entity_type")
                         .Distinct()
                        .Where(BuildLike(new[] { "content" }, keywords));

            return Db.ExecuteList(historyQuery).ConvertAll(row =>
                {
                    var entityID = Convert.ToInt32(row[1]);

                    if (entityID > 0)
                        return new[] { row[1], row[2] };

                    return new[] { row[0], (int)EntityType.Contact };

                }).GroupBy(row => row[1])
                    .ToDictionary(x => (EntityType)x.Key, x => x.SelectMany(item => item).Select(Convert.ToInt32));
        }

        private Dictionary<EntityType, IEnumerable<int>> SearchByCustomFields(String[] keywords)
        {

            var customFieldQuery = Query("crm_field_value")
              .Select("entity_id",
                      "entity_type")
              .Distinct()
              .Where(BuildLike(new[] { "value" }, keywords));

            return Db.ExecuteList(customFieldQuery)
                    .GroupBy(row => row[1])
                    .ToDictionary(x => (EntityType)x.Key, x => x.SelectMany(item => item).Select(Convert.ToInt32));
        }

        private Dictionary<EntityType, IEnumerable<int>> SearchByContactInfos(String[] keywords)
        {
            var sqlResult = Db.ExecuteList(Query("crm_contact_info").Distinct()
                                            .Select("contact_id")
                                            .Where(BuildLike(new[] { "data" }, keywords))).Select(item => Convert.ToInt32(item[0]));


            return new Dictionary<EntityType, IEnumerable<int>> { { EntityType.Contact, sqlResult } };
        }

        private String ToColumn(EntityType entityType)
        {
            return String.Format("{0} as container_type", (int)entityType);
        }


        private Exp BuildWhereExp(EntityType entityType, String[] keywords)
        {
            Exp where = Exp.Empty;

            if (_findedIDs.ContainsKey(entityType))
                where = Exp.In("id", _findedIDs[entityType].ToArray());

            if (BundleSearch.Support(entityType)) return where;

            Exp byField;

            switch (entityType)
            {
                case EntityType.Contact:
                    byField = BuildLike(new[]
                                          {
                                              "first_name",
                                              "last_name",
                                              "company_name",
                                              "title",
                                              "notes"
                                          }, keywords);
                    break;
                case EntityType.Opportunity:
                    byField = BuildLike(new[]
                                          {
                                              "title", 
                                              "description"
                                          }, keywords);
                    break;
                case EntityType.Task:
                    byField = BuildLike(new[]
                                          {
                                              "title", 
                                              "description"
                                          }, keywords);
                    break;
                case EntityType.Case:
                    byField = BuildLike(new[]
                                          {
                                              "title"
                                          }, keywords);
                    break;
                 case EntityType.Invoice:
                        byField = BuildLike(new[]
                                          {
                                              "number",
                                              "description"
                                          }, keywords);
                    break;
                default:
                    throw new ArgumentException();

            }

            if (where != Exp.Empty)
                where &= byField;
            else
                where = byField;

            return where;
        }

        private bool IncludeToSearch(EntityType entityType)
        {
            return !BundleSearch.Support(entityType)  || _findedIDs.ContainsKey(entityType);
        }

        private SqlQuery GetSearchQuery(String[] keywords)
        {
            var queries = new List<SqlQuery>();

            if (IncludeToSearch(EntityType.Task))
                queries.Add(Query("crm_task")
                     .Select(ToColumn(EntityType.Task), "id", "title", "description", "contact_id", "entity_id",
                             "entity_type", "create_on")
                     .Where(BuildWhereExp(EntityType.Task, keywords)));

            if (IncludeToSearch(EntityType.Opportunity))
                queries.Add(Query("crm_deal")
                           .Select(ToColumn(EntityType.Opportunity), "id", "title", "description", "contact_id", "0 as entity_id", "0 as entity_type", "create_on")
                           .Where(BuildWhereExp(EntityType.Opportunity, keywords)));

            if (IncludeToSearch(EntityType.Contact))
                queries.Add(Query("crm_contact")
                            .Select(ToColumn(EntityType.Contact),
                                    "id",
                                      String.Format(@"case is_company
	                                  when 0 then
	                                  concat(first_name, ' ', last_name)
	                                  else
	                                   company_name
	                                  end as title"),
                                     "notes as description", "0 as contact_id", "0 as entity_id", "0 as entity_type", "create_on")
                            .Where(BuildWhereExp(EntityType.Contact, keywords)));


            if (IncludeToSearch(EntityType.Case))
                queries.Add(Query("crm_case")
                            .Select(ToColumn(EntityType.Case), "id", "title", "'' as description", "0 as contact_id", "0 as entity_id", "0 as entity_type", "create_on")
                            .Where(BuildWhereExp(EntityType.Case, keywords)));

            if (IncludeToSearch(EntityType.Invoice))
                queries.Add(Query("crm_invoice")
                            .Select(ToColumn(EntityType.Invoice), "id", "number as title", "description", "contact_id", "entity_id", "entity_type", "create_on")
                            .Where(BuildWhereExp(EntityType.Invoice, keywords)));


            if (queries.Count == 0) return null;
            if (queries.Count == 1) return queries[0];
            return queries[0].UnionAll(queries.Skip(1).ToArray());
        }

        private SearchResultItem[] ToSearchResultItem(IEnumerable<object[]> rows)
        {
            var result = new List<SearchResultItem>();

            foreach (var row in rows)
            {
                var containerType = ((EntityType)Convert.ToInt32(row[0]));
                var id = row[1];
                string imageRef;
                String url;

                switch (containerType)
                {
                    case EntityType.Contact:
                        {

                            var contact = DaoFactory.ContactDao.GetByID(Convert.ToInt32(id));

                            if (contact == null || !CRMSecurity.CanAccessTo(contact)) continue;

                            url = String.Format("Default.aspx?id={0}", id);

                            if (contact is Company)
                                imageRef = WebImageSupplier.GetAbsoluteWebPath("companies_widget.png",
                                                                         ProductEntryPoint.ID);
                            else
                                imageRef = WebImageSupplier.GetAbsoluteWebPath("people_widget.png",
                                                  ProductEntryPoint.ID);

                            break;
                        }
                    case EntityType.Opportunity:
                        {

                            var deal = DaoFactory.DealDao.GetByID(Convert.ToInt32(id));

                            if (deal == null || !CRMSecurity.CanAccessTo(deal)) continue;

                            url = String.Format("Deals.aspx?id={0}", id);

                            imageRef = WebImageSupplier.GetAbsoluteWebPath("deal_widget.png",
                                                                           ProductEntryPoint.ID);
                            break;
                        }
                    case EntityType.Case:
                        {
                            var cases = DaoFactory.CasesDao.GetByID(Convert.ToInt32(id));

                            if (cases == null || !CRMSecurity.CanAccessTo(cases)) continue;

                            url = String.Format("Cases.aspx?id={0}", id);

                            imageRef = WebImageSupplier.GetAbsoluteWebPath("cases_widget.png",
                                                                           ProductEntryPoint.ID);

                            break;
                        }
                    case EntityType.Task:
                        {
                            var task = DaoFactory.TaskDao.GetByID(Convert.ToInt32(id));

                            if (task == null || !CRMSecurity.CanAccessTo(task)) continue;

                            url = "";

                            imageRef = WebImageSupplier.GetAbsoluteWebPath("tasks_widget.png",
                                                                         ProductEntryPoint.ID);
                            break;
                        }
                    case EntityType.Invoice:
                        {
                            var invoice = DaoFactory.InvoiceDao.GetByID(Convert.ToInt32(id));

                            if (invoice == null || !CRMSecurity.CanAccessTo(invoice)) continue;

                            url = String.Format("Invoices.aspx?id={0}", id);

                            imageRef = WebImageSupplier.GetAbsoluteWebPath("invoices_widget.png",
                                             ProductEntryPoint.ID);

                            break;
                        }
                    default:
                        throw new ArgumentException();
                }

                result.Add(new SearchResultItem
                {
                    Name = Convert.ToString(row[2]),
                    Description = HtmlUtil.GetText(Convert.ToString(row[3]), 120),
                    URL = !string.IsNullOrEmpty(url) ? String.Concat(PathProvider.BaseAbsolutePath, url) : string.Empty,
                    Date = TenantUtil.DateTimeFromUtc(DateTime.Parse(Convert.ToString(row[7]))),
                    Additional = new Dictionary<String, Object> 
                                        { { "imageRef", imageRef },
                                          {"relativeInfo", GetPath(
                                              Convert.ToInt32(row[4]),
                                              Convert.ToInt32(row[5]), 
                                              (EntityType)Convert.ToInt32(row[6]))},
                                          {"typeInfo", containerType.ToLocalizedString()}
                                        }
                });
            }

            return result.ToArray();

        }

        private String GetPath(int contactID, int entityID, EntityType entityType)
        {

            if (contactID == 0) return String.Empty;

            if (entityID == 0)
                return DaoFactory.ContactDao.GetByID(contactID).GetTitle();

            switch (entityType)
            {
                case EntityType.Company:
                case EntityType.Person:
                case EntityType.Contact:
                    var contact = DaoFactory.ContactDao.GetByID(contactID);
                    return contact == null ? string.Empty : contact.GetTitle();
                case EntityType.Opportunity:
                    var opportunity = DaoFactory.DealDao.GetByID(entityID);
                    return opportunity == null ? string.Empty : opportunity.Title;
                case EntityType.Case:
                    var @case = DaoFactory.CasesDao.GetByID(entityID);
                    return @case == null ? string.Empty : @case.Title;
                default:
                    throw new ArgumentException();
            }
        }

        #endregion

        #endregion
    }
}