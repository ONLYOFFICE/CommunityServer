/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


        public IEnumerable<TenantQuota> GetTenantQuotas(bool useCache = false)
        {
            return GetTenantQuotas(Exp.Empty);
        }

        public TenantQuota GetTenantQuota(int id, bool useCache = false)
        {
            return GetTenantQuotas(Exp.Eq("tenant", id))
                .SingleOrDefault();
        }

        private IEnumerable<TenantQuota> GetTenantQuotas(Exp where)
        {
            var q = new SqlQuery(tenants_quota)
                .Select("tenant", "name", "max_file_size", "max_total_size", "active_users", "features", "price", "avangate_id", "visible")
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
                    AvangateId = (string)r[7],
                    Visible = Convert.ToBoolean(r[8]),
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

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {

                var userQuotaRow = FindUserQuotaRow(row.Tenant, row.UserId, row.Path);
                if(userQuotaRow == null)
                {
                    db.ExecuteNonQuery(Insert(tenants_quotarow, row.Tenant)
                        .InColumnValue("path", row.Path)
                        .InColumnValue("counter", row.Counter)
                        .InColumnValue("tag", row.Tag)
                        .InColumnValue("user_id", row.UserId)
                        .InColumnValue("last_modified", DateTime.UtcNow));
                }
                else
                {
                    var update = new SqlUpdate(tenants_quotarow)
                                            .Set("counter", exchange ? userQuotaRow.Counter + row.Counter : row.Counter)
                                            .Set("last_modified", DateTime.UtcNow)
                                            .Where("tenant", row.Tenant)
                                            .Where("path", row.Path)
                                            .Where("user_id", row.UserId);

                    db.ExecuteNonQuery(update);
                }

                tx.Commit();
            }
        }

        public IEnumerable<TenantQuotaRow> FindTenantQuotaRows(int tenantId)
        {
            var q = new SqlQuery(tenants_quotarow).Select("tenant", "path", "counter", "tag", "user_id");
            var where = Exp.Empty;

            if (tenantId != Tenant.DEFAULT_TENANT)
            {
                where &= Exp.Eq("tenant", tenantId);
                where &= Exp.Eq("user_id", Guid.Empty);
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

        public IEnumerable<TenantQuotaRow> FindUserQuotaRows(int tenantId, Guid userId, bool useCache)
        {
            var q = new SqlQuery(tenants_quotarow).Select("tenant", "user_id", "path", "counter", "tag");
            var where = Exp.Empty;

            if (tenantId != Tenant.DEFAULT_TENANT)
            {
                where &= Exp.Eq("tenant", tenantId);
                where &= Exp.Eq("user_id", userId);
            }

            if (where != Exp.Empty)
            {
                q.Where(where);
            }

            return ExecList(q)
                .ConvertAll(r => new TenantQuotaRow
                {
                    Tenant = Convert.ToInt32(r[0]),
                    UserId = Guid.Parse((string)r[1]),
                    Path = (string)r[2],
                    Counter = Convert.ToInt64(r[3]),
                    Tag = (string)r[4],
                });
        }

        public TenantQuotaRow FindUserQuotaRow(int tenantId, Guid userId, Guid tag)
        {
            var q = new SqlQuery(tenants_quotarow).Select("tenant", "user_id", "path", "counter", "tag");
            var where = Exp.Empty;

            if (tenantId != Tenant.DEFAULT_TENANT)
            {
                where &= Exp.Eq("tenant", tenantId);
                where &= Exp.Eq("user_id", userId);
                where &= Exp.Eq("tag", tag);
            }

            if (where != Exp.Empty)
            {
                q.Where(where);
            }

            return ExecList(q)
                .ConvertAll(r => new TenantQuotaRow
                {
                    Tenant = Convert.ToInt32(r[0]),
                    UserId = Guid.Parse((string)r[1]),
                    Path = (string)r[2],
                    Counter = Convert.ToInt64(r[3]),
                    Tag = (string)r[4],
                }).SingleOrDefault();
        }

        public TenantQuotaRow FindUserQuotaRow(int tenantId, Guid userId, string path)
        {
            var q = new SqlQuery(tenants_quotarow).Select("tenant", "user_id", "path", "counter", "tag");
            var where = Exp.Empty;

            if (tenantId != Tenant.DEFAULT_TENANT)
            {
                where &= Exp.Eq("tenant", tenantId);
                where &= Exp.Eq("user_id", userId);
                where &= Exp.Eq("path", path);
            }

            if (where != Exp.Empty)
            {
                q.Where(where);
            }

            return ExecList(q)
                .ConvertAll(r => new TenantQuotaRow
                {
                    Tenant = Convert.ToInt32(r[0]),
                    UserId = Guid.Parse((string)r[1]),
                    Path = (string)r[2],
                    Counter = Convert.ToInt64(r[3]),
                    Tag = (string)r[4],
                }).SingleOrDefault();
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
