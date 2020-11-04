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
using System.Configuration;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;

namespace ASC.Core.Data
{
    public class DbQuotaService : DbBaseService, IQuotaService
    {
        private const string tenants_quota = "tenants_quota";
        public const string tenants_quotarow = "tenants_quotarow";


        public DbQuotaService(ConnectionStringSettings connectionString)
            : base(connectionString, "tenant")
        {
        }


        public IEnumerable<TenantQuota> GetTenantQuotas()
        {
            return GetTenantQuotas(Exp.Empty);
        }

        public TenantQuota GetTenantQuota(int id)
        {
            return GetTenantQuotas(Exp.Eq("tenant", id))
                .SingleOrDefault();
        }

        private IEnumerable<TenantQuota> GetTenantQuotas(Exp where)
        {
            var q = new SqlQuery(tenants_quota)
                .Select("tenant", "name", "max_file_size", "max_total_size", "active_users", "features", "price", "price2", "avangate_id", "visible")
                .Where(where);

            return ExecList(q)
                .ConvertAll(r => new TenantQuota(Convert.ToInt32(r[0]))
                {
                    Name = (string)r[1],
                    MaxFileSize = GetInBytes(Convert.ToInt64(r[2])),
                    MaxTotalSize = GetInBytes(Convert.ToInt64(r[3])),
                    ActiveUsers = Convert.ToInt32(r[4]) != 0 ? Convert.ToInt32(r[4]) : int.MaxValue,
                    Features = (string)r[5],
                    Price = Convert.ToDecimal(r[6]),
                    Price2 = Convert.ToDecimal(r[7]),
                    AvangateId = (string)r[8],
                    Visible = Convert.ToBoolean(r[9]),
                });
        }


        public TenantQuota SaveTenantQuota(TenantQuota quota)
        {
            if (quota == null) throw new ArgumentNullException("quota");

            var i = Insert(tenants_quota, quota.Id)
                .InColumnValue("name", quota.Name)
                .InColumnValue("max_file_size", GetInMBytes(quota.MaxFileSize))
                .InColumnValue("max_total_size", GetInMBytes(quota.MaxTotalSize))
                .InColumnValue("active_users", quota.ActiveUsers)
                .InColumnValue("features", quota.Features)
                .InColumnValue("price", quota.Price)
                .InColumnValue("price2", quota.Price2)
                .InColumnValue("avangate_id", quota.AvangateId)
                .InColumnValue("visible", quota.Visible);

            ExecNonQuery(i);
            return quota;
        }

        public void RemoveTenantQuota(int id)
        {
            var d = Delete(tenants_quota, id);
            ExecNonQuery(d);
        }


        public void SetTenantQuotaRow(TenantQuotaRow row, bool exchange)
        {
            if (row == null) throw new ArgumentNullException("row");

            using(var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                var counter = db.ExecuteScalar<long>(Query(tenants_quotarow, row.Tenant)
                    .Select("counter")
                    .Where("path", row.Path));

                db.ExecuteNonQuery(Insert(tenants_quotarow, row.Tenant)
                    .InColumnValue("path", row.Path)
                    .InColumnValue("counter", exchange ? counter + row.Counter : row.Counter)
                    .InColumnValue("tag", row.Tag)
                    .InColumnValue("last_modified", DateTime.UtcNow));

                tx.Commit();
            }
        }

        public IEnumerable<TenantQuotaRow> FindTenantQuotaRows(TenantQuotaRowQuery query)
        {
            if (query == null) throw new ArgumentNullException("query");

            var q = new SqlQuery(tenants_quotarow).Select("tenant", "path", "counter", "tag");
            var where = Exp.Empty;

            if (query.Tenant != Tenant.DEFAULT_TENANT)
            {
                where &= Exp.Eq("tenant", query.Tenant);
            }
            if (!string.IsNullOrEmpty(query.Path))
            {
                where &= Exp.Eq("path", query.Path);
            }
            if (query.LastModified != default(DateTime))
            {
                where &= Exp.Ge("last_modified", query.LastModified);
            }

            if (where != Exp.Empty)
            {
                q.Where(where);
            }

            return ExecList(q)
                .ConvertAll(r => new TenantQuotaRow
                {
                    Tenant = Convert.ToInt32(r[0]),
                    Path = (string)r[1],
                    Counter = Convert.ToInt64(r[2]),
                    Tag = (string)r[3],
                });
        }


        private static long GetInBytes(long bytes)
        {
            const long MB = 1024 * 1024;
            return bytes < MB ? bytes * MB : bytes;
        }

        private static long GetInMBytes(long bytes)
        {
            const long MB = 1024 * 1024;
            return bytes < MB * MB ? bytes / MB : bytes;
        }
    }
}
