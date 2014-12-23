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
        internal const string tenants_quotarow = "tenants_quotarow";


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
                .InColumnValue("max_file_size", quota.MaxFileSize / 1024 / 1024) // save in MB
                .InColumnValue("max_total_size", quota.MaxTotalSize / 1024 / 1024) // save in MB
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

            using (var db = GetDb())
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


        private long GetInBytes(long bytes)
        {
            const long MB = 1024 * 1024;
            return bytes < MB ? bytes * MB : bytes;
        }
    }
}
