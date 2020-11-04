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


#region Import

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ASC.Collections;
using ASC.Web.CRM.Classes;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.CRM.Core.Dao
{

    public class CachedListItem : ListItemDao
    {

        #region Members

        private readonly HttpRequestDictionary<ListItem> _listItemCache = new HttpRequestDictionary<ListItem>("crm_list_item");

        #endregion

        #region Constructor

        public CachedListItem(int tenantID)
            : base(tenantID)
        {


        }

        #endregion

        #region Members

        public override void ChangeColor(int id, string newColor)
        {
            ResetCache(id);

            base.ChangeColor(id, newColor);
        }

        public override void DeleteItem(ListType listType, int itemID, int toItemID)
        {
            ResetCache(itemID);

            base.DeleteItem(listType, itemID, toItemID);
        }

        public override void ChangePicture(int id, string newPicture)
        {
            ResetCache(id);

            base.ChangePicture(id, newPicture);
        }

        public override void EditItem(ListType listType, ListItem enumItem)
        {
            ResetCache(enumItem.ID);

            base.EditItem(listType, enumItem);
        }

        public override void ReorderItems(ListType listType, string[] titles)
        {
            _listItemCache.Clear();

            base.ReorderItems(listType, titles);
        }

        public override ListItem GetByID(int id)
        {
            return _listItemCache.Get(id.ToString(), () => GetByIDBase(id));
        }

        private ListItem GetByIDBase(int id)
        {
            return base.GetByID(id);
        }

        private void ResetCache(int id)
        {
            _listItemCache.Reset(id.ToString());
        }

        #endregion


    }

    public class ListItemDao : AbstractDao
    {
        #region Constructor

        public ListItemDao(int tenantID)
            : base(tenantID)
        {


        }

        #endregion



        public bool IsExist(ListType listType, String title)
        {
            var q = new SqlQuery("crm_list_item")
                .Select("1")
                .Where("tenant_id", TenantID)
                .Where("list_type", (int)listType)
                .Where("title", title)
                .SetMaxResults(1);

            return Db.ExecuteScalar<bool>(q);
        }

        public bool IsExist(int id)
        {
            return Db.ExecuteScalar<bool>("select exists(select 1 from crm_list_item where tenant_id = @tid and id = @id)",
                            new { tid = TenantID, id = id });
        }

        public List<ListItem> GetItems()
        {
            var sqlQuery = GetListItemSqlQuery(null).OrderBy("sort_order", true);

            return Db.ExecuteList(sqlQuery).ConvertAll(ToListItem);
        }

        public List<ListItem> GetItems(ListType listType)
        {
            var sqlQuery = GetListItemSqlQuery(Exp.Eq("list_type", (int)listType))
                               .OrderBy("sort_order", true);

            return Db.ExecuteList(sqlQuery).ConvertAll(ToListItem);
        }

        public int GetItemsCount(ListType listType)
        {
            SqlQuery sqlQuery = Query("crm_list_item").SelectCount().Where(Exp.Eq("list_type", (int)listType))
                               .OrderBy("sort_order", true);

            return Db.ExecuteScalar<int>(sqlQuery);
        }

        public ListItem GetSystemListItem(int id)
        {
            switch (id)
            {
                case (int)HistoryCategorySystem.TaskClosed:
                    return new ListItem
                               {
                                   ID = -1,
                                   Title = HistoryCategorySystem.TaskClosed.ToLocalizedString(),
                                   AdditionalParams = "event_category_close.png"
                               };
                case (int)HistoryCategorySystem.FilesUpload:
                    return new ListItem
                               {
                                   ID = -2,
                                   Title = HistoryCategorySystem.FilesUpload.ToLocalizedString(),
                                   AdditionalParams = "event_category_attach_file.png"
                               };
                case (int)HistoryCategorySystem.MailMessage:
                    return  new ListItem
                        {
                            ID = -3,
                            Title = HistoryCategorySystem.MailMessage.ToLocalizedString(),
                            AdditionalParams = "event_category_email.png"
                        };
                default:
                    return null;
            }

        }

        public List<ListItem> GetSystemItems()
        {
            return new List<ListItem>
            {
                new ListItem
                        {
                            ID = (int)HistoryCategorySystem.TaskClosed,
                            Title = HistoryCategorySystem.TaskClosed.ToLocalizedString(),
                            AdditionalParams = "event_category_close.png"
                        },
                new ListItem
                        {
                            ID = (int)HistoryCategorySystem.FilesUpload,
                            Title = HistoryCategorySystem.FilesUpload.ToLocalizedString(),
                            AdditionalParams = "event_category_attach_file.png"
                        },
                new ListItem
                        {
                            ID =(int)HistoryCategorySystem.MailMessage,
                            Title = HistoryCategorySystem.MailMessage.ToLocalizedString(),
                            AdditionalParams = "event_category_email.png"
                        }
            };
        }

        public virtual ListItem GetByID(int id)
        {
            if (id < 0) return GetSystemListItem(id);

            var result = Db.ExecuteList(GetListItemSqlQuery(Exp.Eq("id", id))).ConvertAll(ToListItem);

            return result.Count > 0 ? result[0] : null;
        }

        public virtual List<ListItem> GetItems(int[] id)
        {
            var sqlResult = Db.ExecuteList(GetListItemSqlQuery(Exp.In("id", id))).ConvertAll(ToListItem);

            var systemItem = id.Where(item => item < 0).Select(GetSystemListItem);

            return systemItem.Any() ? sqlResult.Union(systemItem).ToList() : sqlResult;
        }

        public virtual List<ListItem> GetAll()
        {
            return Db.ExecuteList(GetListItemSqlQuery(null)).ConvertAll(ToListItem);
        }

        public virtual void ChangeColor(int id, string newColor)
        {
            Db.ExecuteNonQuery(Update("crm_list_item")
                                        .Set("color", newColor)
                                        .Where(Exp.Eq("id", id)));
        }

        public NameValueCollection GetColors(ListType listType)
        {
            var where = Exp.Eq("list_type", (int)listType);

            var result = new NameValueCollection();

            Db.ExecuteList(Query("crm_list_item")
                                        .Select("id", "color")
                                        .Where(where))
                                    .ForEach(row => result.Add(row[0].ToString(), row[1].ToString()));
            return result;

        }

        public ListItem GetByTitle(ListType listType, string title)
        {
            var result = Db.ExecuteList(GetListItemSqlQuery(Exp.Eq("title", title) & Exp.Eq("list_type", (int)listType))).ConvertAll(ToListItem);

            return result.Count > 0 ? result[0] : null;
        }

        public int GetRelativeItemsCount(ListType listType, int id)
        {

            SqlQuery sqlQuery;


            switch (listType)
            {
                case ListType.ContactStatus:
                    sqlQuery = Query("crm_contact")
                              .Select("count(*)")
                              .Where(Exp.Eq("status_id", id));
                    break;
                case ListType.ContactType:
                    sqlQuery = Query("crm_contact")
                              .Select("count(*)")
                              .Where(Exp.Eq("contact_type_id", id));
                    break;
                case ListType.TaskCategory:
                    sqlQuery = Query("crm_task")
                             .Select("count(*)")
                             .Where(Exp.Eq("category_id", id));
                    break;
                case ListType.HistoryCategory:
                    sqlQuery = Query("crm_relationship_event")
                              .Select("count(*)")
                              .Where(Exp.Eq("category_id", id));

                    break;
                default:
                    throw new ArgumentException();
                  
            }

            return Db.ExecuteScalar<int>(sqlQuery);
        }

        public Dictionary<int, int> GetRelativeItemsCount(ListType listType)
        {
            var sqlQuery = Query("crm_list_item tbl_list_item")
              .Where(Exp.Eq("tbl_list_item.list_type", (int)listType))
              .Select("tbl_list_item.id")
              .OrderBy("tbl_list_item.sort_order", true)
              .GroupBy("tbl_list_item.id");

            switch (listType)
            {
                case ListType.ContactStatus:
                    sqlQuery.LeftOuterJoin("crm_contact tbl_crm_contact",
                                            Exp.EqColumns("tbl_list_item.id", "tbl_crm_contact.status_id")
                                             & Exp.EqColumns("tbl_list_item.tenant_id", "tbl_crm_contact.tenant_id"))
                                          .Select("count(tbl_crm_contact.status_id)");
                    break;
                case ListType.ContactType:
                    sqlQuery.LeftOuterJoin("crm_contact tbl_crm_contact",
                                            Exp.EqColumns("tbl_list_item.id", "tbl_crm_contact.contact_type_id")
                                             & Exp.EqColumns("tbl_list_item.tenant_id", "tbl_crm_contact.tenant_id"))
                                          .Select("count(tbl_crm_contact.contact_type_id)");
                    break;
                case ListType.TaskCategory:
                    sqlQuery.LeftOuterJoin("crm_task tbl_crm_task",
                                            Exp.EqColumns("tbl_list_item.id", "tbl_crm_task.category_id")
                                              & Exp.EqColumns("tbl_list_item.tenant_id", "tbl_crm_task.tenant_id"))
                                           .Select("count(tbl_crm_task.category_id)");
                    break;
                case ListType.HistoryCategory:
                    sqlQuery.LeftOuterJoin("crm_relationship_event tbl_crm_relationship_event",
                                            Exp.EqColumns("tbl_list_item.id", "tbl_crm_relationship_event.category_id")
                                              & Exp.EqColumns("tbl_list_item.tenant_id", "tbl_crm_relationship_event.tenant_id"))
                                           .Select("count(tbl_crm_relationship_event.category_id)");

                    break;
                default:
                    throw new ArgumentException();
            }

            var queryResult = Db.ExecuteList(sqlQuery);

            return queryResult.ToDictionary(x => Convert.ToInt32(x[0]), y => Convert.ToInt32(y[1]));
        }

        public virtual int CreateItem(ListType listType, ListItem enumItem)
        {

            if (IsExist(listType, enumItem.Title))
                return GetByTitle(listType, enumItem.Title).ID;

            if (string.IsNullOrEmpty(enumItem.Title))
                throw new ArgumentException();

            if (listType == ListType.TaskCategory || listType == ListType.HistoryCategory)
                if (string.IsNullOrEmpty(enumItem.AdditionalParams))
                    throw new ArgumentException();
                else
                   enumItem.AdditionalParams = System.IO.Path.GetFileName(enumItem.AdditionalParams);
                
            if (listType == ListType.ContactStatus)
                if (string.IsNullOrEmpty(enumItem.Color))
                    throw new ArgumentException();

            var sortOrder = enumItem.SortOrder;

            if (sortOrder == 0)
                sortOrder = Db.ExecuteScalar<int>(Query("crm_list_item")
                                                        .Where(Exp.Eq("list_type", (int)listType))
                                                        .SelectMax("sort_order")) + 1;

            return Db.ExecuteScalar<int>(
                                                Insert("crm_list_item")
                                                .InColumnValue("id", 0)
                                                .InColumnValue("list_type", (int)listType)
                                                .InColumnValue("description", enumItem.Description)
                                                .InColumnValue("title", enumItem.Title)
                                                .InColumnValue("additional_params", enumItem.AdditionalParams)
                                                .InColumnValue("color", enumItem.Color)
                                                .InColumnValue("sort_order", sortOrder)
                                                .Identity(1, 0, true));
        }

        public virtual void EditItem(ListType listType, ListItem enumItem)
        {

            if (HaveRelativeItemsLink(listType, enumItem.ID))
            switch (listType)
            {
                case ListType.ContactStatus:
                case ListType.ContactType:
                    throw new ArgumentException(string.Format("{0}. {1}.", CRMErrorsResource.BasicCannotBeEdited, CRMErrorsResource.HasRelatedContacts));
                case ListType.TaskCategory:
                    throw new ArgumentException(string.Format("{0}. {1}.", CRMErrorsResource.BasicCannotBeEdited, CRMErrorsResource.TaskCategoryHasRelatedTasks));
                case ListType.HistoryCategory:
                    throw new ArgumentException(string.Format("{0}. {1}.", CRMErrorsResource.BasicCannotBeEdited, CRMErrorsResource.HistoryCategoryHasRelatedEvents));
                default:
                    throw new ArgumentException(string.Format("{0}.", CRMErrorsResource.BasicCannotBeEdited));
            }

            Db.ExecuteNonQuery(Update("crm_list_item")
                                        .Set("description", enumItem.Description)
                                        .Set("title", enumItem.Title)
                                        .Set("additional_params", enumItem.AdditionalParams)
                                        .Set("color", enumItem.Color)
                                        .Where(Exp.Eq("id", enumItem.ID)));
        }

        public virtual void ChangePicture(int id, String newPicture)
        {
            Db.ExecuteNonQuery(Update("crm_list_item")
                                    .Set("additional_params", newPicture)
                                    .Where(Exp.Eq("id", id)));
        }

        private bool HaveRelativeItemsLink(ListType listType, int itemID)
        {
            SqlQuery sqlQuery;

            switch (listType)
            {

                case ListType.ContactStatus:
                    sqlQuery = Query("crm_contact")
                               .Where(Exp.Eq("status_id", itemID));
                    break;
                case ListType.ContactType:
                    sqlQuery = Query("crm_contact")
                                .Where(Exp.Eq("contact_type_id", itemID));

                    break;
                case ListType.TaskCategory:
                    sqlQuery = Query("crm_task")
                              .Where(Exp.Eq("category_id", itemID));
                    break;
                case ListType.HistoryCategory:
                    sqlQuery = Query("crm_relationship_event")
                              .Where(Exp.Eq("category_id", itemID));
                    break;
                default:
                    throw new ArgumentException();
            }

            return Db.ExecuteScalar<int>(sqlQuery.SelectCount()) > 0;
        }

        public void ChangeRelativeItemsLink(ListType listType, int fromItemID, int toItemID)
        {
            if (!IsExist(fromItemID))
                throw new ArgumentException("", "toItemID");
           
            if (!HaveRelativeItemsLink(listType, fromItemID)) return;

            if (!IsExist(toItemID))
                throw new ArgumentException("", "toItemID");
           
            SqlUpdate sqlUpdate;
            
            switch (listType)
            {
                case ListType.ContactStatus:
                    sqlUpdate = Update("crm_contact")
                                .Set("status_id", toItemID)
                                .Where(Exp.Eq("status_id", fromItemID));
                    break;
                case ListType.ContactType:
                    sqlUpdate = Update("crm_contact")
                                .Set("contact_type_id", toItemID)
                                .Where(Exp.Eq("contact_type_id", fromItemID));
                    break;
                case ListType.TaskCategory:
                    sqlUpdate = Update("crm_task")
                               .Set("category_id", toItemID)
                               .Where(Exp.Eq("category_id", fromItemID));
                    break;
                case ListType.HistoryCategory:
                    sqlUpdate = Update("crm_relationship_event")
                               .Set("category_id", toItemID)  
                               .Where(Exp.Eq("category_id", fromItemID));
                    break;
                default:
                    throw new ArgumentException();
            }

            Db.ExecuteNonQuery(sqlUpdate);
        }

        public virtual void DeleteItem(ListType listType, int itemID, int toItemID)
        {
            if (HaveRelativeItemsLink(listType, itemID))
            {
                switch (listType)
                {
                    case ListType.ContactStatus:
                    case ListType.ContactType:
                        throw new ArgumentException(string.Format("{0}. {1}.", CRMErrorsResource.BasicCannotBeDeleted, CRMErrorsResource.HasRelatedContacts));
                    case ListType.TaskCategory:
                        var exMsg = string.Format("{0}. {1}.", CRMErrorsResource.BasicCannotBeDeleted, CRMErrorsResource.TaskCategoryHasRelatedTasks);
                        if (itemID == toItemID) throw new ArgumentException(exMsg);
                        ChangeRelativeItemsLink(listType, itemID, toItemID);
                        break;
                    case ListType.HistoryCategory:
                        throw new ArgumentException(string.Format("{0}. {1}.", CRMErrorsResource.BasicCannotBeDeleted, CRMErrorsResource.HistoryCategoryHasRelatedEvents));
                    default:
                        throw new ArgumentException(string.Format("{0}.", CRMErrorsResource.BasicCannotBeDeleted));
                }
            }

            Db.ExecuteNonQuery(Delete("crm_list_item").Where(Exp.Eq("id", itemID) & Exp.Eq("list_type", (int)listType)));
        }

        public virtual void ReorderItems(ListType listType, String[] titles)
        {
            using (var tx = Db.BeginTransaction())
            {
                for (int index = 0; index < titles.Length; index++)
                    Db.ExecuteNonQuery(Update("crm_list_item")
                                             .Set("sort_order", index)
                                             .Where(Exp.Eq("title", titles[index]) & Exp.Eq("list_type", (int)listType)));

                tx.Commit();
            }
        }

        private SqlQuery GetListItemSqlQuery(Exp where)
        {
            var result = Query("crm_list_item")
               .Select(
                   "id",
                   "title",
                   "description",
                   "color",
                   "sort_order",
                   "additional_params",
                   "list_type"
               );

            if (where != null)
                result.Where(where);

            return result;

        }

        public static ListItem ToListItem(object[] row)
        {
            var result = new ListItem
                       {
                           ID = Convert.ToInt32(row[0]),
                           Title = Convert.ToString(row[1]),
                           Description = Convert.ToString(row[2]),
                           Color = Convert.ToString(row[3]),
                           SortOrder = Convert.ToInt32(row[4]),
                           AdditionalParams = Convert.ToString(row[5])
                       };

            ListType listType;
            if (Enum.TryParse(Convert.ToString(row[6]), out listType))
            {
                result.ListType = listType;
            }

            return result;
        }
    }
}
