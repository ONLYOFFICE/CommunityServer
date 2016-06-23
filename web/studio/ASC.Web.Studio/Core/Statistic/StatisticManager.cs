/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System;
using System.Collections.Generic;
using System.Data;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Web.Studio.Core.Statistic
{
    public class StatisticManager
    {
        private const string dbId = "webstudio";
        private static DateTime lastSave = DateTime.UtcNow;
        private static readonly TimeSpan cacheTime = TimeSpan.FromMinutes(2);
        private static readonly IDictionary<string, UserVisit> cache = new Dictionary<string, UserVisit>();


        public static void SaveUserVisit(int tenantID, Guid userID, Guid productID)
        {
            var now = DateTime.UtcNow;
            var key = string.Format("{0}|{1}|{2}|{3}", tenantID, userID, productID, now.Date);

            lock (cache)
            {
                var visit = cache.ContainsKey(key) ?
                                cache[key] :
                                new UserVisit
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
            using (var db = GetDb())
            {
                var users = db
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
        }

        public static List<UserVisit> GetHitsByPeriod(int tenantID, DateTime startDate, DateTime endPeriod)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(new SqlQuery("webstudio_uservisit")
                                          .Select("VisitDate")
                                          .SelectSum("VisitCount")
                                          .Where(Exp.Between("VisitDate", startDate, endPeriod))
                                          .Where("TenantID", tenantID)
                                          .GroupBy("VisitDate")
                                          .OrderBy("VisitDate", true))
                         .ConvertAll(r => new UserVisit { VisitDate = Convert.ToDateTime(r[0]), VisitCount = Convert.ToInt32(r[1]) });
            }
        }

        public static List<UserVisit> GetHostsByPeriod(int tenantID, DateTime startDate, DateTime endPeriod)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(new SqlQuery("webstudio_uservisit")
                                          .Select("VisitDate", "UserId")
                                          .Where(Exp.Between("VisitDate", startDate, endPeriod))
                                          .Where("TenantID", tenantID)
                                          .GroupBy("UserId", "VisitDate")
                                          .OrderBy("VisitDate", true))
                         .ConvertAll(r => new UserVisit { VisitDate = Convert.ToDateTime(r[0]), UserID = new Guid(Convert.ToString(r[1])) });
            }
        }

        private static void FlushCache()
        {
            if (cache.Count == 0) return;

            List<UserVisit> visits;
            lock (cache)
            {
                visits = new List<UserVisit>(cache.Values);
                cache.Clear();
                lastSave = DateTime.UtcNow;
            }

            using (var db = GetDb())
            using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                foreach (var v in visits)
                {
                    var sql = "insert into webstudio_uservisit(tenantid, productid, userid, visitdate, firstvisittime, lastvisittime, visitcount) values " +
                        "(@TenantId, @ProductId, @UserId, @VisitDate, @FirstVisitTime, @LastVisitTime, @VisitCount) " +
                        "on duplicate key update lastvisittime = @LastVisitTime, visitcount = visitcount + @VisitCount";

                    db.ExecuteNonQuery(sql, new
                    {
                        TenantId = v.TenantID,
                        ProductId = v.ProductID.ToString(),
                        UserId = v.UserID.ToString(),
                        VisitDate = v.VisitDate.Date,
                        FirstVisitTime = v.VisitDate,
                        LastVisitTime = v.LastVisitTime,
                        VisitCount = v.VisitCount,
                    });
                }
                tx.Commit();
            }
        }

        private static DbManager GetDb()
        {
            return new DbManager(dbId);
        }
    }
}