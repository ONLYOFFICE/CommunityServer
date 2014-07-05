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
using System.Data;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Web.Studio.Core.Statistic
{
    class StatisticManager
    {
        private static readonly string dbId = "webstudio";
        private static DateTime lastSave = DateTime.UtcNow;
        private static TimeSpan cacheTime = TimeSpan.FromMinutes(2);
        private static IDictionary<string, UserVisit> cache = new Dictionary<string, UserVisit>();


        public static void SaveUserVisit(int tenantID, Guid userID, Guid productID)
        {
            var now = DateTime.UtcNow;
            var key = string.Format("{0}|{1}|{2}|{3}", tenantID, userID, productID, now.Date);

            lock (cache)
            {
                var visit = cache.ContainsKey(key) ?
                    cache[key] :
                    new UserVisit()
                    {
                        TenantID = tenantID,
                        UserID = userID,
                        ProductID = productID,
                        VisitDate = now
                    };

                visit.VisitCount++;
                visit.LastVisitTime = now;
                cache[key] = visit;
            }

            if (cacheTime < DateTime.UtcNow - lastSave)
            {
                FlushCache();
            }
        }

        public static List<Guid> GetVisitorsToday(int tenantID, Guid productID)
        {
            var users = DbManager
                .ExecuteList(
                    new SqlQuery("webstudio_uservisit")
                    .Select("UserID")
                    .Where("VisitDate", DateTime.UtcNow.Date)
                    .Where("TenantID", tenantID)
                    .Where("ProductID", productID.ToString())
                    .GroupBy(1)
                    .OrderBy("FirstVisitTime", true)
                )
                .ConvertAll(r => new Guid((string)r[0]));
            lock (cache)
            {
                foreach (var visit in cache.Values)
                {
                    if (!users.Contains(visit.UserID) && visit.VisitDate.Date == DateTime.UtcNow.Date)
                    {
                        users.Add(visit.UserID);
                    }
                }
            }
            return users;
        }

        public static List<UserVisit> GetHitsByPeriod(int tenantID, DateTime startDate, DateTime endPeriod)
        {
            return DbManager.ExecuteList(new SqlQuery("webstudio_uservisit")
              .Select("VisitDate")
              .SelectSum("VisitCount")
              .Where(Exp.Between("VisitDate", startDate, endPeriod))
              .Where("TenantID", tenantID)
              .GroupBy("VisitDate")
              .OrderBy("VisitDate", true))
              .ConvertAll(r => new UserVisit { VisitDate = Convert.ToDateTime(r[0]), VisitCount = Convert.ToInt32(r[1]) });
        }

        public static List<UserVisit> GetHostsByPeriod(int tenantID, DateTime startDate, DateTime endPeriod)
        {
            return DbManager.ExecuteList(new SqlQuery("webstudio_uservisit")
              .Select("VisitDate", "UserId")
              .Where(Exp.Between("VisitDate", startDate, endPeriod))
              .Where("TenantID", tenantID)
              .GroupBy("UserId", "VisitDate")
              .OrderBy("VisitDate", true))
              .ConvertAll(r => new UserVisit { VisitDate = Convert.ToDateTime(r[0]), UserID = new Guid(Convert.ToString(r[1])) });
        }

        private static void FlushCache()
        {
            if (cache.Count == 0) return;

            List<UserVisit> visits = null;
            lock (cache)
            {
                visits = new List<UserVisit>(cache.Values);
                cache.Clear();
                lastSave = DateTime.UtcNow;
            }

            var db = DbManager;
            using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                foreach (var v in visits)
                {
                    var pk = Exp.Eq("TenantID", v.TenantID) & Exp.Eq("UserID", v.UserID.ToString()) & Exp.Eq("ProductID", v.ProductID.ToString()) & Exp.Eq("VisitDate", v.VisitDate.Date);

                    var affected = db.ExecuteNonQuery(
                            new SqlUpdate("webstudio_uservisit")
                            .Set("LastVisitTime", v.LastVisitTime)
                            .Set("VisitCount = VisitCount + " + v.VisitCount)
                            .Where(pk));

                    if (affected == 0)
                    {
                        db.ExecuteNonQuery(
                            new SqlInsert("webstudio_uservisit")
                            .InColumns("TenantID", "ProductID", "UserID", "VisitDate", "FirstVisitTime", "LastVisitTime", "VisitCount")
                            .Values(v.TenantID, v.ProductID.ToString(), v.UserID.ToString(), v.VisitDate.Date, v.VisitDate, v.LastVisitTime, v.VisitCount)
                        );
                    }
                }
                tx.Commit();
            }
        }

        private static DbManager DbManager
        {
            get { return DbManager.FromHttpContext(dbId); }
        }
    }
}