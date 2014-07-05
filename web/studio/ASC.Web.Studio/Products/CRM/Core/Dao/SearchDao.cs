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

#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Utils;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;
using ASC.FullTextIndex;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;

#endregion

namespace ASC.CRM.Core.Dao
{
    public class SearchDao : AbstractDao
    {
        #region Members

        private Dictionary<EntityType, IEnumerable<String>> _findedIDs;
        private bool _fullTextSearchEnable;

        #endregion

        #region Constructor

        public SearchDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
        {

        }

        #endregion

        #region Methods

        #region Public
        
        public SearchResultItem[] Search(String searchText)
        {
            var keywords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
               .ToArray();

            if (keywords.Length == 0) return new List<SearchResultItem>().ToArray();

            _fullTextSearchEnable = FullTextSearch.SupportModule(FullTextSearch.CRMCasesModule)
                                    && FullTextSearch.SupportModule(FullTextSearch.CRMContactsModule)
                                    && FullTextSearch.SupportModule(FullTextSearch.CRMDealsModule)
                                    && FullTextSearch.SupportModule(FullTextSearch.CRMTasksModule);
                            
            if (_fullTextSearchEnable)
            {
                _findedIDs = new Dictionary<EntityType, IEnumerable<string>>();

                _findedIDs.Add(EntityType.Case, FullTextSearch.Search(searchText, FullTextSearch.CRMCasesModule)
                                               .GetIdentifiers()
                                               .Select(item => item.Split('_')[1]).Distinct()
                              );

                _findedIDs.Add(EntityType.Contact, FullTextSearch.Search(searchText, FullTextSearch.CRMContactsModule)
                                                  .GetIdentifiers()
                                                  .Select(item => item.Split('_')[1]).Distinct()
                               );

                _findedIDs.Add(EntityType.Opportunity, FullTextSearch.Search(searchText, FullTextSearch.CRMDealsModule)
                                                      .GetIdentifiers()
                                                      .Select(item => item.Split('_')[1]).Distinct()
                               );

                _findedIDs.Add(EntityType.Task, FullTextSearch.Search(searchText, FullTextSearch.CRMTasksModule)
                                                .GetIdentifiers()
                              );


            }
            else
                _findedIDs = SearchByCustomFields(keywords)
                                     .Union(SearchByRelationshipEvent(keywords))
                                     .Union(SearchByContactInfos(keywords))
                                     .ToLookup(pair => pair.Key, pair => pair.Value)
                                     .ToDictionary(group => group.Key, group => group.First());


            var searchQuery = GetSearchQuery(keywords);

            if (searchQuery == null) return new SearchResultItem[0];


            using (var db = GetDb())
            {
                return ToSearchResultItem(db.ExecuteList(searchQuery));
            }
        }

        #endregion

        #region Private

        private Dictionary<EntityType, IEnumerable<String>> SearchByRelationshipEvent(String[] keywords)
        {
            var historyQuery = Query("crm_relationship_event")
                        .Select(
                                "contact_id",
                                "entity_id",
                                "entity_type")
                         .Distinct()
                        .Where(BuildLike(new[] { "content" }, keywords));

            using (var db = GetDb())
            {
                return db.ExecuteList(historyQuery).ConvertAll(row =>
                    {
                        var entityID = Convert.ToInt32(row[1]);

                        if (entityID > 0)
                            return new[] { row[1], row[2] };

                        return new[] { row[0], (int)EntityType.Contact };

                    }).GroupBy(row => row[1])
                       .ToDictionary(x => (EntityType)x.Key, x => x.Select(item => item.ToString()));
            }
        }

        private Dictionary<EntityType, IEnumerable<String>> SearchByCustomFields(String[] keywords)
        {

            var customFieldQuery = Query("crm_field_value")
              .Select("entity_id",
                      "entity_type")
              .Distinct()
              .Where(BuildLike(new[] { "value" }, keywords));

            using (var db = GetDb())
            {
                return db.ExecuteList(customFieldQuery)
                      .GroupBy(row => row[1])
                      .ToDictionary(x => (EntityType)x.Key, x => x.Select(item => item.ToString()));
            }
        }

        private Dictionary<EntityType, IEnumerable<String>> SearchByContactInfos(String[] keywords)
        {
            using (var db = GetDb())
            {
                var sqlResult = db.ExecuteList(Query("crm_contact_info").Distinct()
                                                .Select("contact_id")
                                                .Where(BuildLike(new[] { "data" }, keywords))).Select(item => Convert.ToString(item[0]));


                return new Dictionary<EntityType, IEnumerable<String>> { { EntityType.Contact, sqlResult } };
            }
        }

        private String GetFullTextSearchModule(EntityType entityType)
        {
            switch (entityType)
            {
                 case EntityType.Case: return FullTextSearch.CRMCasesModule;
                 case EntityType.Company: return FullTextSearch.CRMContactsModule;
                 case EntityType.Contact: return FullTextSearch.CRMContactsModule;
                 case EntityType.Person: return FullTextSearch.CRMContactsModule;
                 case EntityType.Opportunity: return FullTextSearch.CRMDealsModule;
                 case EntityType.Invoice: return FullTextSearch.CRMInvoicesModule;
                 case EntityType.RelationshipEvent: return FullTextSearch.CRMEventsModule;
                 case EntityType.Task: return FullTextSearch.CRMTasksModule;
            }

            throw new NotSupportedException();

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

            if (FullTextSearch.SupportModule(GetFullTextSearchModule(entityType))) return where;

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
            return !FullTextSearch.SupportModule(GetFullTextSearchModule(entityType)) || _findedIDs.ContainsKey(entityType);
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

                            var contact = Global.DaoFactory.GetContactDao().GetByID(Convert.ToInt32(id));

                            if (!CRMSecurity.CanAccessTo(contact)) continue;

                            url = String.Format("default.aspx?id={0}", id);

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

                            var deal = Global.DaoFactory.GetDealDao().GetByID(Convert.ToInt32(id));

                            if (!CRMSecurity.CanAccessTo(deal)) continue;

                            url = String.Format("deals.aspx?id={0}", id);

                            imageRef = WebImageSupplier.GetAbsoluteWebPath("deal_widget.png",
                                                                           ProductEntryPoint.ID);
                            break;
                        }
                    case EntityType.Case:
                        {
                            var cases = Global.DaoFactory.GetCasesDao().GetByID(Convert.ToInt32(id));

                            if (!CRMSecurity.CanAccessTo(cases)) continue;

                            url = String.Format("cases.aspx?id={0}", id);

                            imageRef = WebImageSupplier.GetAbsoluteWebPath("cases_widget.png",
                                                                           ProductEntryPoint.ID);

                            break;
                        }
                    case EntityType.Task:
                        {
                            var task = Global.DaoFactory.GetTaskDao().GetByID(Convert.ToInt32(id));

                            if (!CRMSecurity.CanAccessTo(task)) continue;

                            url = String.Format("tasks.aspx?id={0}", id);

                            imageRef = WebImageSupplier.GetAbsoluteWebPath("tasks_widget.png",
                                                                         ProductEntryPoint.ID);
                            break;
                        }
                    case EntityType.Invoice:
                        {
                            var invoice = Global.DaoFactory.GetInvoiceDao().GetByID(Convert.ToInt32(id));

                            if (!CRMSecurity.CanAccessTo(invoice)) continue;

                            url = String.Format("invoices.aspx?id={0}", id);

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
                    URL = String.Concat(PathProvider.BaseAbsolutePath, url),
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
                return Global.DaoFactory.GetContactDao().GetByID(contactID).GetTitle();

            switch (entityType)
            {
                case EntityType.Company:
                case EntityType.Person:
                case EntityType.Contact:
                    return Global.DaoFactory.GetContactDao().GetByID(contactID).GetTitle();
                case EntityType.Opportunity:
                    return Global.DaoFactory.GetDealDao().GetByID(entityID).Title;
                case EntityType.Case:
                    return Global.DaoFactory.GetCasesDao().GetByID(entityID).Title;
                default:
                    throw new ArgumentException();
            }
        }

        #endregion

        #endregion
    }
}