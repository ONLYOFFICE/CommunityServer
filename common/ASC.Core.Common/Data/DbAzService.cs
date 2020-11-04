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


using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Security.Authorizing;
using ASC.Core.Tenants;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ASC.Core.Data
{
    public class DbAzService : DbBaseService, IAzService
    {
        public DbAzService(ConnectionStringSettings connectionString)
            : base(connectionString, "tenant")
        {
        }

        public IEnumerable<AzRecord> GetAces(int tenant, DateTime from)
        {
            // row with tenant = -1 - common for all tenants, but equal row with tenant != -1 escape common row for the portal
            var q = new SqlQuery("core_acl")
                .Select("subject", "action", "object", "acetype")
                .Where(Exp.Eq("tenant", Tenant.DEFAULT_TENANT));

            var commonAces = ExecList(q)
                .ConvertAll(r => ToAzRecord(r, tenant))
                .ToDictionary(a => string.Concat(a.Tenant.ToString(), a.SubjectId.ToString(), a.ActionId.ToString(), a.ObjectId));

            q = new SqlQuery("core_acl")
                .Select("subject", "action", "object", "acetype")
                .Where(Exp.Eq("tenant", tenant));

            var tenantAces = ExecList(q)
                .ConvertAll(r => new AzRecord(new Guid((string)r[0]), new Guid((string)r[1]), (AceType)Convert.ToInt32(r[3]), string.Empty.Equals(r[2]) ? null : (string)r[2]) { Tenant = tenant });

            // remove excaped rows
            foreach (var a in tenantAces.ToList())
            {
                var key = string.Concat(a.Tenant.ToString(), a.SubjectId.ToString(), a.ActionId.ToString(), a.ObjectId);
                if (commonAces.ContainsKey(key))
                {
                    var common = commonAces[key];
                    commonAces.Remove(key);
                    if (common.Reaction == a.Reaction)
                    {
                        tenantAces.Remove(a);
                    }
                }
            }

            return commonAces.Values.Concat(tenantAces);
        }

        public AzRecord SaveAce(int tenant, AzRecord r)
        {
            r.Tenant = tenant;
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                if (!ExistEscapeRecord(db, r))
                {
                    InsertRecord(db, r);
                }
                else
                {
                    // unescape
                    DeleteRecord(db, r);
                }
                tx.Commit();
            }

            return r;
        }

        public void RemoveAce(int tenant, AzRecord r)
        {
            r.Tenant = tenant;
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                if (ExistEscapeRecord(db, r))
                {
                    // escape
                    InsertRecord(db, r);
                }
                else
                {
                    DeleteRecord(db, r);
                }
                tx.Commit();
            }
        }


        private bool ExistEscapeRecord(IDbManager db, AzRecord r)
        {
            var q = Query("core_acl", Tenant.DEFAULT_TENANT)
                .SelectCount()
                .Where("subject", r.SubjectId.ToString())
                .Where("action", r.ActionId.ToString())
                .Where("object", r.ObjectId ?? string.Empty)
                .Where("acetype", r.Reaction);
            return db.ExecuteScalar<int>(q) != 0;
        }

        private void DeleteRecord(IDbManager db, AzRecord r)
        {
            var q = Delete("core_acl", r.Tenant)
                .Where("subject", r.SubjectId.ToString())
                .Where("action", r.ActionId.ToString())
                .Where("object", r.ObjectId ?? string.Empty)
                .Where("acetype", r.Reaction);
            db.ExecuteNonQuery(q);
        }

        private void InsertRecord(IDbManager db, AzRecord r)
        {
            var q = Insert("core_acl", r.Tenant)
                .InColumnValue("subject", r.SubjectId.ToString())
                .InColumnValue("action", r.ActionId.ToString())
                .InColumnValue("object", r.ObjectId ?? string.Empty)
                .InColumnValue("acetype", r.Reaction);
            db.ExecuteNonQuery(q);
        }

        private AzRecord ToAzRecord(object[] r, int tenant)
        {
            return new AzRecord(
                new Guid((string)r[0]),
                new Guid((string)r[1]),
                (AceType)Convert.ToInt32(r[3]),
                string.Empty.Equals(r[2]) ? null : (string)r[2]) { Tenant = tenant };
        }
    }
}
