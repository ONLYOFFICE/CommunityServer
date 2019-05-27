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


#region Import

using System;
using System.Collections.Generic;
using System.Globalization;
using ASC.Collections;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.CRM.Classes;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.CRM.Core.Entities;


#endregion

namespace ASC.CRM.Core.Dao
{

    public class CachedListItemHistory : ListItemHistoryDao
    {

        #region Members

        private readonly HttpRequestDictionary<ListItemHistory> _listItemHistoryCache = new HttpRequestDictionary<ListItemHistory>("crm_list_item_history");

        #endregion

        #region Constructor

        public CachedListItemHistory(int tenantId, DaoFactory factory)
            : base(tenantId, factory)
        {

        }

        #endregion

        #region Members

        public override ListItemHistory GetByID(int id)
        {
            return _listItemHistoryCache.Get(id.ToString(CultureInfo.InvariantCulture), () => GetByIDBase(id));
        }

        private ListItemHistory GetByIDBase(int id)
        {
            return base.GetByID(id);
        }

        private void ResetCache(int id)
        {
            _listItemHistoryCache.Reset(id.ToString(CultureInfo.InvariantCulture));
        }

        #endregion


    }

    public class ListItemHistoryDao : AbstractDao
    {
        #region Constructor

        private DaoFactory DaoFactory { get; set; }

        public ListItemHistoryDao(int tenantId, DaoFactory daoFactory)
            : base(tenantId)
        {
            DaoFactory = daoFactory;
        }

        #endregion

        public List<ListItemHistory> GetItems(EntityType entityType, int statusID, DateTime? startDate, DateTime? endDate)
        {
            var sqlQuery = GetListItemSqlQuery(Exp.Eq("entity_type", (int)entityType) & Exp.Eq("status", statusID));

            if (startDate.HasValue)
                sqlQuery.Where(Exp.Gt("modifed_on", startDate));

            if (endDate.HasValue)
                sqlQuery.Where(Exp.Lt("modifed_on", endDate));

            return Db.ExecuteList(sqlQuery.OrderBy("entity_id", true))
                .ConvertAll(ToListItemHistory);
        }

        public int GetItemsCount(EntityType entityType, int statusID, DateTime? startDate, DateTime? endDate)
        {
            var sqlQuery = Query("crm_item_history")
                .SelectCount()
                .Where(Exp.Eq("entity_type", (int) entityType) & Exp.Eq("status", statusID));

            if (startDate.HasValue)
                sqlQuery.Where(Exp.Gt("modifed_on", startDate));

            if (endDate.HasValue)
                sqlQuery.Where(Exp.Lt("modifed_on", endDate));

            return Db.ExecuteScalar<int>(sqlQuery);
        }

        public virtual ListItemHistory GetByID(int id)
        {
            var sqlQuery = GetListItemSqlQuery(Exp.Eq("id", id));
            
            var result = Db.ExecuteList(sqlQuery).ConvertAll(ToListItemHistory);

            return result.Count > 0 ? result[0] : null;
        }

        public virtual List<ListItemHistory> GetAll(EntityType entityType)
        {
            var sqlQuery = GetListItemSqlQuery(Exp.Eq("entity_type", (int) entityType))
                .OrderBy("entity_id", true);

            return Db.ExecuteList(sqlQuery).ConvertAll(ToListItemHistory);
        }

        public virtual int CreateItem(ListItemHistory item)
        {
            if (item.EntityType != EntityType.Opportunity || item.EntityType != EntityType.Contact)
                throw new ArgumentException();

            if (item.EntityType == EntityType.Opportunity &&
                (DaoFactory.DealDao.GetByID(item.EntityID) == null ||
                 DaoFactory.DealMilestoneDao.GetByID(item.StatusID) == null))
                throw new ArgumentException();

            if (item.EntityType == EntityType.Contact &&
                (DaoFactory.ContactDao.GetByID(item.EntityID) == null ||
                 DaoFactory.ListItemDao.GetByID(item.StatusID) == null))
                throw new ArgumentException();

            var sqlQuery = Insert("crm_item_history")
                .InColumnValue("id", 0)
                .InColumnValue("entity_id", item.EntityID)
                .InColumnValue("entity_type", (int) item.EntityType)
                .InColumnValue("status", item.StatusID)
                .InColumnValue("modifed_on", TenantUtil.DateTimeToUtc(TenantUtil.DateTimeNow()))
                .InColumnValue("modifed_by", SecurityContext.CurrentAccount.ID)
                .Identity(1, 0, true);

            return Db.ExecuteScalar<int>(sqlQuery);
        }

        public virtual void DeleteItem(EntityType entityType, int statusID)
        {
            if (HaveRelativeItems(entityType, statusID))
                throw new ArgumentException();

            var sqlQuery = Delete("crm_item_history")
                .Where(Exp.Eq("entity_type", (int) entityType) & Exp.Eq("status", statusID));

            Db.ExecuteNonQuery(sqlQuery);
        }

        private bool HaveRelativeItems(EntityType entityType, int statusID)
        {
            SqlQuery sqlQuery;

            switch (entityType)
            {
                case EntityType.Contact:
                    sqlQuery = Query("crm_contact")
                               .Where(Exp.Eq("status_id", statusID));
                    break;
                case EntityType.Opportunity:
                    sqlQuery = Query("crm_deal")
                             .Where(Exp.Eq("deal_milestone_id", statusID));
                    break;
                default:
                    throw new ArgumentException();
            }

            return Db.ExecuteScalar<int>(sqlQuery.SelectCount()) > 0;
        }

        private SqlQuery GetListItemSqlQuery(Exp where)
        {
            var result = Query("crm_list_item_history")
               .Select(
                   "id",
                   "entity_id",
                   "entity_type",
                   "status",
                   "modifed_on",
                   "modifed_by"
               );

            if (where != null)
                result.Where(where);

            return result;

        }

        public static ListItemHistory ToListItemHistory(object[] row)
        {
            return new ListItemHistory
                       {
                           ID = Convert.ToInt32(row[0]),
                           EntityID = Convert.ToInt32(row[1]),
                           EntityType = (EntityType)Convert.ToInt32(row[2]),
                           StatusID = Convert.ToInt32(row[3]),
                           ModifedOn = TenantUtil.DateTimeFromUtc(DateTime.Parse(row[4].ToString())),
                           ModifedBy = ToGuid(row[5]),
                       };
        }
    }
}
