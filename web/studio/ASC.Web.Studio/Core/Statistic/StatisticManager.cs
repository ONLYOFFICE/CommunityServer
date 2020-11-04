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
using System.Data;
using System.Web;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Web.Studio.Core.Statistic
{
    public class StatisticManager
    {
        private const string dbId = "default";
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
                    .ConvertAll(r => new Guid((string) r[0]));
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
                    .ConvertAll(
                        r =>
                            new UserVisit {VisitDate = Convert.ToDateTime(r[0]), VisitCount = Convert.ToInt32(r[1])});
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
                    .ConvertAll(
                        r =>
                            new UserVisit
                            {
                                VisitDate = Convert.ToDateTime(r[0]),
                                UserID = new Guid(Convert.ToString(r[1]))
                            });
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

            using(var db = GetDb())
            using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                foreach (var v in visits)
                {
                    var sql =
                        "insert into webstudio_uservisit(tenantid, productid, userid, visitdate, firstvisittime, lastvisittime, visitcount) values " +
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

        private static IDbManager GetDb()
        {
            return DbManager.FromHttpContext(dbId);
        }
    }
}