/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

#endregion

namespace ASC.CRM.Core.Dao
{
    public class CachedDealMilestoneDao : DealMilestoneDao
    {
        private readonly HttpRequestDictionary<DealMilestone> _dealMilestoneCache =
            new HttpRequestDictionary<DealMilestone>("crm_deal_milestone");

        public CachedDealMilestoneDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
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

        public DealMilestoneDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {


        }

        #endregion

        public virtual void Reorder(int[] ids)
        {
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                for (int index = 0; index < ids.Length; index++)
                    db.ExecuteNonQuery(Update("crm_deal_milestone")
                                             .Set("sort_order", index)
                                             .Where(Exp.Eq("id", ids[index])));

                tx.Commit();
            }
        }

        public int GetCount()
        {
            using (var db = GetDb())
            {
                return db.ExecuteScalar<int>(Query("crm_deal_milestone").SelectCount());
            }
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

            using (var db = GetDb())
            {
                var queryResult = db.ExecuteList(sqlQuery);
                return queryResult.ToDictionary(x => Convert.ToInt32(x[0]), y => Convert.ToInt32(y[1]));
            }
        }

        public int GetRelativeItemsCount(int id)
        {

            var sqlQuery = Query("crm_deal")
                             .Select("count(*)")
                             .Where(Exp.Eq("deal_milestone_id", id));

            using (var db = GetDb())
            {
                return db.ExecuteScalar<int>(sqlQuery);
            }
        }

        public virtual int Create(DealMilestone item)
        {

            if (String.IsNullOrEmpty(item.Title) || String.IsNullOrEmpty(item.Color))
                throw new ArgumentException();

            int id;

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                if (item.SortOrder == 0)
                    item.SortOrder = db.ExecuteScalar<int>(Query("crm_deal_milestone")
                                                            .SelectMax("sort_order")) + 1;

                id = db.ExecuteScalar<int>(
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
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Update("crm_deal_milestone")
                                         .Set("color", newColor)
                                         .Where(Exp.Eq("id", id)));
            }
        }

        public virtual void Edit(DealMilestone item)
        {

            if (HaveContactLink(item.ID))
                throw new ArgumentException("Can't be edited. There are some opportunities at this stage.");

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Update("crm_deal_milestone")
                                        .Set("title", item.Title)
                                        .Set("description", item.Description)
                                        .Set("color", item.Color)
                                        .Set("probability", item.Probability)
                                        .Set("status", (int)item.Status)
                                        .Where(Exp.Eq("id", item.ID)));
            }
        }

        public bool HaveContactLink(int dealMilestoneID)
        {
            SqlQuery sqlQuery = Query("crm_deal")
                                .Where(Exp.Eq("deal_milestone_id", dealMilestoneID))
                                .SelectCount()
                                .SetMaxResults(1);

            using (var db = GetDb())
            {
                return db.ExecuteScalar<int>(sqlQuery)  >= 1;
            }
        }

        public virtual void Delete(int id)
        {
            if (HaveContactLink(id))
                throw new ArgumentException("Can't be deleted. There are some opportunities at this stage.");

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Delete("crm_deal_milestone").Where(Exp.Eq("id", id)));
            }
        }

        public virtual DealMilestone GetByID(int id)
        {
            using (var db = GetDb())
            {
                var dealMilestones = db.ExecuteList(GetDealMilestoneQuery(Exp.Eq("id", id))).ConvertAll(row => ToDealMilestone(row));

                if (dealMilestones.Count == 0)
                    return null;

                return dealMilestones[0];
            }
        }

        public Boolean IsExist(int id)
        {
            using (var db = GetDb())
            {
                return db.ExecuteScalar<bool>("select exists(select 1 from crm_deal_milestone where tenant_id = @tid and id = @id)",
                    new { tid = TenantID, id = id });
            }
        }

        public List<DealMilestone> GetAll(int[] id)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(GetDealMilestoneQuery(Exp.In("id", id))).ConvertAll(row => ToDealMilestone(row));
            }
        }

        public List<DealMilestone> GetAll()
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(GetDealMilestoneQuery(null)).ConvertAll(row => ToDealMilestone(row));
            }
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