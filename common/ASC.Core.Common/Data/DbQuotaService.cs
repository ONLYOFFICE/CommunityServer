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
