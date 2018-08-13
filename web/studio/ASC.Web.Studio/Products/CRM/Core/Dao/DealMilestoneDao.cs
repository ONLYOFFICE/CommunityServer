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
using System.Linq;
using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.CRM.Core.Dao
{
    public class CachedDealMilestoneDao : DealMilestoneDao
    {
        private readonly HttpRequestDictionary<DealMilestone> _dealMilestoneCache =
            new HttpRequestDictionary<DealMilestone>("crm_deal_milestone");

        public CachedDealMilestoneDao(int tenantID)
            : base(tenantID)
        {

        }

        private void ResetCache(int id)
        {
            _dealMilestoneCache.Reset(id.ToString());
        }

        public override int Create(DealMilestone item)
        {
            item.ID = base.Create(item);

            _dealMilestoneCache.Add(item.ID.ToString(), item);

            return item.ID;
        }

        public override void Delete(int id)
        {
            ResetCache(id);

            base.Delete(id);
        }

        public override void Edit(DealMilestone item)
        {
            ResetCache(item.ID);

            base.Edit(item);
        }


        private DealMilestone GetByIDBase(int id)
        {
            return base.GetByID(id);
        }

        public override DealMilestone GetByID(int id)
        {
            return _dealMilestoneCache.Get(id.ToString(), () => GetByIDBase(id));
        }

        public override void Reorder(int[] ids)
        {
            _dealMilestoneCache.Clear();

            base.Reorder(ids);
        }
    }

    public class DealMilestoneDao : AbstractDao
    {

        #region Constructor

        public DealMilestoneDao(int tenantID)
            : base(tenantID)
        {


        }

        #endregion

        public virtual void Reorder(int[] ids)
        {
            using (var tx = Db.BeginTransaction())
            {
                for (int index = 0; index < ids.Length; index++)
                    Db.ExecuteNonQuery(Update("crm_deal_milestone")
                                             .Set("sort_order", index)
                                             .Where(Exp.Eq("id", ids[index])));

                tx.Commit();
            }
        }

        public int GetCount()
        {
            return Db.ExecuteScalar<int>(Query("crm_deal_milestone").SelectCount());
        }


        public Dictionary<int, int> GetRelativeItemsCount()
        {
            var sqlQuery = Query("crm_deal_milestone tbl_deal_milestone")
                          .Select("tbl_deal_milestone.id")
                          .OrderBy("tbl_deal_milestone.sort_order", true)
                          .GroupBy("tbl_deal_milestone.id");

            sqlQuery.LeftOuterJoin("crm_deal tbl_crm_deal",
                                      Exp.EqColumns("tbl_deal_milestone.id", "tbl_crm_deal.deal_milestone_id"))
                .Select("count(tbl_crm_deal.deal_milestone_id)");

            var queryResult = Db.ExecuteList(sqlQuery);
            return queryResult.ToDictionary(x => Convert.ToInt32(x[0]), y => Convert.ToInt32(y[1]));
        }

        public int GetRelativeItemsCount(int id)
        {

            var sqlQuery = Query("crm_deal")
                             .Select("count(*)")
                             .Where(Exp.Eq("deal_milestone_id", id));

            return Db.ExecuteScalar<int>(sqlQuery);
        }

        public virtual int Create(DealMilestone item)
        {

            if (String.IsNullOrEmpty(item.Title) || String.IsNullOrEmpty(item.Color))
                throw new ArgumentException();

            int id;

            using (var tx = Db.BeginTransaction())
            {
                if (item.SortOrder == 0)
                    item.SortOrder = Db.ExecuteScalar<int>(Query("crm_deal_milestone")
                                                            .SelectMax("sort_order")) + 1;

                id = Db.ExecuteScalar<int>(
                                  Insert("crm_deal_milestone")
                                 .InColumnValue("id", 0)
                                 .InColumnValue("title", item.Title)
                                 .InColumnValue("description", item.Description)
                                 .InColumnValue("color", item.Color)
                                 .InColumnValue("probability", item.Probability)
                                 .InColumnValue("status", (int)item.Status)
                                 .InColumnValue("sort_order", item.SortOrder)
                                 .Identity(1, 0, true));
                tx.Commit();
            }

            return id;

        }


        public virtual void ChangeColor(int id, String newColor)
        {
            Db.ExecuteNonQuery(Update("crm_deal_milestone")
                                        .Set("color", newColor)
                                        .Where(Exp.Eq("id", id)));
        }

        public virtual void Edit(DealMilestone item)
        {

            if (HaveContactLink(item.ID))
                throw new ArgumentException(String.Format("{0}. {1}.", CRMErrorsResource.BasicCannotBeEdited, CRMErrorsResource.DealMilestoneHasRelatedDeals));

            Db.ExecuteNonQuery(Update("crm_deal_milestone")
                                    .Set("title", item.Title)
                                    .Set("description", item.Description)
                                    .Set("color", item.Color)
                                    .Set("probability", item.Probability)
                                    .Set("status", (int)item.Status)
                                    .Where(Exp.Eq("id", item.ID)));
        }

        public bool HaveContactLink(int dealMilestoneID)
        {
            SqlQuery sqlQuery = Query("crm_deal")
                                .Where(Exp.Eq("deal_milestone_id", dealMilestoneID))
                                .SelectCount()
                                .SetMaxResults(1);

            return Db.ExecuteScalar<int>(sqlQuery) >= 1;
        }

        public virtual void Delete(int id)
        {
            if (HaveContactLink(id))
                throw new ArgumentException(String.Format("{0}. {1}.", CRMErrorsResource.BasicCannotBeDeleted, CRMErrorsResource.DealMilestoneHasRelatedDeals));

            Db.ExecuteNonQuery(Delete("crm_deal_milestone").Where(Exp.Eq("id", id)));
        }

        public virtual DealMilestone GetByID(int id)
        {
            var dealMilestones = Db.ExecuteList(GetDealMilestoneQuery(Exp.Eq("id", id))).ConvertAll(row => ToDealMilestone(row));

            if (dealMilestones.Count == 0)
                return null;

            return dealMilestones[0];
        }

        public Boolean IsExist(int id)
        {
            return Db.ExecuteScalar<bool>("select exists(select 1 from crm_deal_milestone where tenant_id = @tid and id = @id)",
                new { tid = TenantID, id = id });
        }

        public List<DealMilestone> GetAll(int[] id)
        {
            return Db.ExecuteList(GetDealMilestoneQuery(Exp.In("id", id))).ConvertAll(row => ToDealMilestone(row));
        }

        public List<DealMilestone> GetAll()
        {
            return Db.ExecuteList(GetDealMilestoneQuery(null)).ConvertAll(row => ToDealMilestone(row));
        }

        private SqlQuery GetDealMilestoneQuery(Exp where)
        {
            SqlQuery sqlQuery = Query("crm_deal_milestone")
                .Select("id",
                        "title",
                        "description",
                        "color",
                        "probability",
                        "status",
                        "sort_order")
                        .OrderBy("sort_order", true);

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;

        }

        private static DealMilestone ToDealMilestone(object[] row)
        {
            return new DealMilestone
            {
                ID = Convert.ToInt32(row[0]),
                Title = Convert.ToString(row[1]),
                Description = Convert.ToString(row[2]),
                Color = Convert.ToString(row[3]),
                Probability = Convert.ToInt32(row[4]),
                Status = (DealMilestoneStatus)Convert.ToInt32(row[5]),
                SortOrder = Convert.ToInt32(row[6])
            };
        }



    }
}