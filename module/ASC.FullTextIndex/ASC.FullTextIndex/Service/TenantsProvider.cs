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

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Caching;
using ASC.Core.Tenants;
using System;
using System.Collections.Generic;

namespace ASC.FullTextIndex.Service
{
    class TenantsProvider
    {
        private readonly string dbid;
        private readonly int userActivityDays;
        private readonly ICache cache;
        private DateTime last;


        public TenantsProvider(string dbid, int userActivityDays)
        {
            this.dbid = dbid;
            this.userActivityDays = userActivityDays;
            this.cache = AscCache.Default;
        }


        public Tenant GetTenant(int tenantId)
        {
            var key = "fullTextSearch/tenant/" + tenantId;
            var tenant = cache.Get(key) as Tenant;
            if (tenant == null)
            {
                using (var db = new DbManager(dbid))
                {
                    var q = new SqlQuery("tenants_tenants")
                        .Select("language")
                        .Where("id", tenantId);
                    var language = db.ExecuteScalar<string>(q);
                    tenant = new Tenant(tenantId, string.Empty) { Language = language };
                    cache.Insert(key, tenant, DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
                }
            }
            return tenant;
        }

        public List<Tenant> GetTenants()
        {
            var result = new List<Tuple<int, string, string>>();

            if (last == DateTime.MinValue)
            {
                // first start
                if (userActivityDays == 0)
                {
                    // not use user_activity
                    using (var db = new DbManager(dbid))
                    {
                        var q = new SqlQuery("tenants_tenants")
                            .Select("id", "alias", "language")
                            .Where("status", (int)TenantStatus.Active);
                        result = db.ExecuteList(q).ConvertAll(r => Tuple.Create(Convert.ToInt32(r[0]), (string)r[1], (string)r[2]));
                    }
                }
                else
                {
                    // use user_activity
                    using (var db = new DbManager(dbid))
                    {
                        var q = new SqlQuery("webstudio_uservisit")
                            .Select("tenantid")
                            .Select("t.alias", "t.language")
                            .InnerJoin("tenants_tenants t", Exp.EqColumns("tenantid", "t.id"))
                            .Where(Exp.Ge("visitdate", DateTime.UtcNow.Date.AddDays(-userActivityDays)))
                            .Where("t.status", (int)TenantStatus.Active)
                            .GroupBy(1, 2);
                        var ids = db
                            .ExecuteList(q)
                            .ConvertAll(r => Convert.ToInt32(r[0]));
                        result = db.ExecuteList(q).ConvertAll(r => Tuple.Create(Convert.ToInt32(r[0]), (string)r[1], (string)r[2]));
                    }
                }
            }
            else
            {
                using (var db = new DbManager(dbid))
                {
                    var q = new SqlQuery("webstudio_uservisit")
                        .Select("tenantid")
                        .Select("t.alias", "t.language")
                        .InnerJoin("tenants_tenants t", Exp.EqColumns("tenantid", "t.id"))
                        .Where("visitdate", DateTime.UtcNow.Date)
                        .Where(Exp.Ge("lastvisittime", last.AddHours(-1)))
                        .Where("t.status", (int)TenantStatus.Active)
                        .GroupBy(1, 2);
                    result = db.ExecuteList(q).ConvertAll(r => Tuple.Create(Convert.ToInt32(r[0]), (string)r[1], (string)r[2]));
                }
            }
            last = DateTime.UtcNow;

            return result.ConvertAll(r => new Tenant(r.Item1, r.Item2) { Language = r.Item3 });
        }
    }
}
