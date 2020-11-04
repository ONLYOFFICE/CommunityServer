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
using System.Diagnostics;
using System.IO;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Utils;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Security.Cryptography;

namespace ASC.Core.Data
{
    public class DbTenantService : DbBaseService, ITenantService
    {
        private static TimeZoneInfo defaultTimeZone;
        private List<string> forbiddenDomains;


        public DbTenantService(ConnectionStringSettings connectionString)
            : base(connectionString, null)
        {
        }


        public void ValidateDomain(string domain)
        {
            using (var db = GetDb())
            {
                ValidateDomain(db, domain, Tenant.DEFAULT_TENANT, true);
            }
        }

        public IEnumerable<Tenant> GetTenants(DateTime from, bool active = true)
        {
            return GetTenants(
                Exp.And(
                    active
                        ? Exp.Eq("t.status", (int)TenantStatus.Active)
                        : Exp.Empty,
                    from != default(DateTime)
                        ? Exp.Ge("last_modified", from)
                        : Exp.Empty
                    )
                );
        }

        public IEnumerable<Tenant> GetTenants(string login, string passwordHash)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentNullException("login");

            if (passwordHash == null)
            {
                var q = TenantsQuery(Exp.Empty)
                    .InnerJoin("core_user u", Exp.EqColumns("t.id", "u.tenant"))
                    .InnerJoin("core_usersecurity s", Exp.EqColumns("u.id", "s.userid"))
                    .Where("t.status", (int)TenantStatus.Active)
                    .Where(login.Contains('@') ? "u.email" : "u.id", login)
                    .Where("u.status", EmployeeStatus.Active)
                    .Where("u.removed", false);

                return ExecList(q).ConvertAll(ToTenant);
            }

            Guid userId;
            if (Guid.TryParse(login, out userId))
            {
                var q = TenantsQuery(Exp.Empty)
                    .InnerJoin("core_user u", Exp.EqColumns("t.id", "u.tenant"))
                    .InnerJoin("core_usersecurity s", Exp.EqColumns("u.id", "s.userid"))
                    .Where("t.status", (int)TenantStatus.Active)
                    .Where("u.id", userId)
                    .Where("u.status", EmployeeStatus.Active)
                    .Where("u.removed", false)
                    .Where(Exp.Or(
                            Exp.Eq("s.pwdhash", GetPasswordHash(userId, passwordHash)),
                            Exp.Eq("s.pwdhash", Hasher.Base64Hash(passwordHash, HashAlg.SHA256)) //todo: remove old scheme
                        ));

                return ExecList(q).ConvertAll(ToTenant);
            }
            else
            {
                var q = TenantsQuery(Exp.Empty)
                    .InnerJoin("core_user u", Exp.EqColumns("t.id", "u.tenant"))
                    .InnerJoin("core_usersecurity s", Exp.EqColumns("u.id", "s.userid"))
                    .Where("t.status", (int)TenantStatus.Active)
                    .Where(login.Contains('@') ? "u.email" : "u.id", login)
                    .Where("u.status", EmployeeStatus.Active)
                    .Where("u.removed", false)
                    .Where("s.pwdhash", Hasher.Base64Hash(passwordHash, HashAlg.SHA256));

                //old password
                var result = ExecList(q).ConvertAll(ToTenant);

                var usersQuery = new SqlQuery("core_user u")
                    .Select("u.id")
                    .Where("u.email", login)
                    .Where("u.status", EmployeeStatus.Active)
                    .Where("u.removed", false);

                var passwordHashs = ExecList(usersQuery).ConvertAll(r =>
                        GetPasswordHash(new Guid((string)r[0]), passwordHash)
                    );

                q = TenantsQuery(Exp.Empty)
                    .InnerJoin("core_usersecurity s", Exp.EqColumns("t.id", "s.tenant"))
                    .Where(Exp.In("s.pwdhash", passwordHashs));

                //new password
                result = result.Concat(ExecList(q).ConvertAll(ToTenant)).ToList();
                result.Distinct();

                return result;
            }
        }

        public Tenant GetTenant(int id)
        {
            return GetTenants(Exp.Eq("id", id))
                .SingleOrDefault();
        }

        public Tenant GetTenant(string domain)
        {
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException("domain");

            return GetTenants(Exp.Eq("alias", domain.ToLowerInvariant()) | Exp.Eq("mappeddomain", domain.ToLowerInvariant()))
                .OrderBy(a => a.Status == TenantStatus.Restoring ? TenantStatus.Active : a.Status)
                .ThenByDescending(a => a.Status == TenantStatus.Restoring ? 0 : a.TenantId)
                .FirstOrDefault();
        }

        public Tenant GetTenantForStandaloneWithoutAlias(string ip)
        {
            return GetTenants(Exp.Empty)
                .OrderBy(a => a.Status)
                .ThenByDescending(a => a.TenantId)
                .FirstOrDefault();
        }

        public Tenant SaveTenant(Tenant t)
        {
            if (t == null) throw new ArgumentNullException("tenant");

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                if (!string.IsNullOrEmpty(t.MappedDomain))
                {
                    var baseUrl = TenantUtil.GetBaseDomain(t.HostedRegion);

                    if (baseUrl != null && t.MappedDomain.EndsWith("." + baseUrl, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ValidateDomain(db, t.MappedDomain.Substring(0, t.MappedDomain.Length - baseUrl.Length - 1), t.TenantId, false);
                    }
                    else
                    {
                        ValidateDomain(db, t.MappedDomain, t.TenantId, false);
                    }
                }

                if (t.TenantId == Tenant.DEFAULT_TENANT)
                {
                    var q = new SqlQuery("tenants_version")
                        .Select("id")
                        .Where(Exp.Eq("default_version", 1) | Exp.Eq("id", 0))
                        .OrderBy(1, false)
                        .SetMaxResults(1);
                    t.Version = db.ExecuteScalar<int>(q);

                    var i = new SqlInsert("tenants_tenants")
                        .InColumnValue("id", t.TenantId)
                        .InColumnValue("alias", t.TenantAlias.ToLowerInvariant())
                        .InColumnValue("mappeddomain", !string.IsNullOrEmpty(t.MappedDomain) ? t.MappedDomain.ToLowerInvariant() : null)
                        .InColumnValue("version", t.Version)
                        .InColumnValue("version_changed", t.VersionChanged)
                        .InColumnValue("name", t.Name ?? t.TenantAlias)
                        .InColumnValue("language", t.Language)
                        .InColumnValue("timezone", t.TimeZone.Id)
                        .InColumnValue("owner_id", t.OwnerId.ToString())
                        .InColumnValue("trusteddomains", t.GetTrustedDomains())
                        .InColumnValue("trusteddomainsenabled", (int)t.TrustedDomainsType)
                        .InColumnValue("creationdatetime", t.CreatedDateTime)
                        .InColumnValue("status", (int)t.Status)
                        .InColumnValue("statuschanged", t.StatusChangeDate)
                        .InColumnValue("payment_id", t.PaymentId)
                        .InColumnValue("last_modified", t.LastModified = DateTime.UtcNow)
                        .InColumnValue("industry", (int)t.Industry)
                        .InColumnValue("spam", t.Spam)
                        .InColumnValue("calls", t.Calls)
                        .Identity<int>(0, t.TenantId, true);


                    t.TenantId = db.ExecuteScalar<int>(i);
                }
                else
                {
                    var u = new SqlUpdate("tenants_tenants")
                        .Set("alias", t.TenantAlias.ToLowerInvariant())
                        .Set("mappeddomain", !string.IsNullOrEmpty(t.MappedDomain) ? t.MappedDomain.ToLowerInvariant() : null)
                        .Set("version", t.Version)
                        .Set("version_changed", t.VersionChanged)
                        .Set("name", t.Name ?? t.TenantAlias)
                        .Set("language", t.Language)
                        .Set("timezone", t.TimeZone.Id)
                        .Set("owner_id", t.OwnerId.ToString())
                        .Set("trusteddomains", t.GetTrustedDomains())
                        .Set("trusteddomainsenabled", (int)t.TrustedDomainsType)
                        .Set("creationdatetime", t.CreatedDateTime)
                        .Set("status", (int)t.Status)
                        .Set("statuschanged", t.StatusChangeDate)
                        .Set("payment_id", t.PaymentId)
                        .Set("last_modified", t.LastModified = DateTime.UtcNow)
                        .Set("industry", (int)t.Industry)
                        .Set("spam", t.Spam)
                        .Set("calls", t.Calls)
                        .Where("id", t.TenantId);

                    db.ExecuteNonQuery(u);
                }

                if (string.IsNullOrEmpty(t.PartnerId) && string.IsNullOrEmpty(t.AffiliateId) && string.IsNullOrEmpty(t.Campaign))
                {
                    var d = new SqlDelete("tenants_partners").Where("tenant_id", t.TenantId);
                    db.ExecuteNonQuery(d);
                }
                else
                {
                    var i = new SqlInsert("tenants_partners", true)
                        .InColumnValue("tenant_id", t.TenantId)
                        .InColumnValue("partner_id", t.PartnerId)
                        .InColumnValue("affiliate_id", t.AffiliateId)
                        .InColumnValue("campaign", t.Campaign);
                    db.ExecuteNonQuery(i);
                }

                tx.Commit();
            }
            //CalculateTenantDomain(t);
            return t;
        }

        public void RemoveTenant(int id, bool auto = false)
        {
            var postfix = auto ? "_auto_deleted" : "_deleted";

            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                var alias = db.ExecuteScalar<string>(new SqlQuery("tenants_tenants").Select("alias").Where("id", id));
                var count = db.ExecuteScalar<int>(new SqlQuery("tenants_tenants").SelectCount().Where(Exp.Like("alias", alias + postfix, SqlLike.StartWith)));
                db.ExecuteNonQuery(
                    new SqlUpdate("tenants_tenants")
                        .Set("alias", alias + postfix + (count > 0 ? count.ToString() : ""))
                        .Set("status", TenantStatus.RemovePending)
                        .Set("statuschanged", DateTime.UtcNow)
                        .Set("last_modified", DateTime.UtcNow)
                        .Where("id", id));

                tx.Commit();
            }
        }

        public IEnumerable<TenantVersion> GetTenantVersions()
        {
            var q = new SqlQuery("tenants_version")
                .Select("id", "version")
                .Where("visible", true);
            return ExecList(q)
                .ConvertAll(r => new TenantVersion(Convert.ToInt32(r[0]), (string)r[1]));
        }


        public byte[] GetTenantSettings(int tenant, string key)
        {
            return ExecScalar<byte[]>(new SqlQuery("core_settings").Select("value").Where("tenant", tenant).Where("id", key));
        }

        public void SetTenantSettings(int tenant, string key, byte[] data)
        {
            var i = data == null || data.Length == 0 ?
                (ISqlInstruction)new SqlDelete("core_settings").Where("tenant", tenant).Where("id", key) :
                (ISqlInstruction)new SqlInsert("core_settings", true).InColumns("tenant", "id", "value").Values(tenant, key, data);
            ExecNonQuery(i);
        }

        private IEnumerable<Tenant> GetTenants(Exp where)
        {
            var q = TenantsQuery(where);
            return ExecList(q).ConvertAll(ToTenant);
        }

        private static SqlQuery TenantsQuery(Exp where)
        {
            return new SqlQuery("tenants_tenants t")
                .Select("t.id", "t.alias", "t.mappeddomain", "t.version", "t.version_changed", "t.name", "t.language", "t.timezone", "t.owner_id")
                .Select("t.trusteddomains", "t.trusteddomainsenabled", "t.creationdatetime", "t.status", "t.statuschanged", "t.payment_id", "t.last_modified")
                .Select("p.partner_id", "p.affiliate_id", "p.campaign")
                .Select("t.industry", "t.spam", "t.calls")
                .LeftOuterJoin("tenants_partners p", Exp.EqColumns("t.id", "p.tenant_id"))
                .Where(where);
        }

        private Tenant ToTenant(object[] r)
        {
            var tenant = new Tenant(Convert.ToInt32(r[0]), (string)r[1])
            {
                MappedDomain = (string)r[2],
                Version = Convert.ToInt32(r[3]),
                VersionChanged = Convert.ToDateTime(r[4]),
                Name = (string)r[5],
                Language = (string)r[6],
                TimeZone = GetTimeZone((string)r[7]),
                OwnerId = r[8] != null ? new Guid((string)r[8]) : Guid.Empty,
                TrustedDomainsType = (TenantTrustedDomainsType)Convert.ToInt32(r[10]),
                CreatedDateTime = (DateTime)r[11],
                Status = (TenantStatus)Convert.ToInt32(r[12]),
                StatusChangeDate = r[13] != null ? (DateTime)r[13] : default(DateTime),
                PaymentId = (string)r[14],
                LastModified = (DateTime)r[15],
                PartnerId = (string)r[16],
                AffiliateId = (string)r[17],
                Campaign = (string)r[18],
                Industry = r[18] != null ? (TenantIndustry)Convert.ToInt32(r[19]) : TenantIndustry.Other,
                Spam = Convert.ToBoolean(r[20]),
                Calls = Convert.ToBoolean(r[21]),
            };
            tenant.SetTrustedDomains((string)r[9]);

            return tenant;
        }

        private TimeZoneInfo GetTimeZone(string zoneId)
        {
            if (defaultTimeZone == null)
            {
                try
                {
                    var tz = TimeZoneInfo.Local;
                    if (Path.DirectorySeparatorChar == '/')
                    {
                        if (tz.StandardName == "UTC" || tz.StandardName == "UCT")
                        {
                            tz = TimeZoneInfo.Utc;
                        }
                        else
                        {
                            var id = string.Empty;
                            if (File.Exists("/etc/timezone"))
                            {
                                id = File.ReadAllText("/etc/timezone").Trim();
                            }

                            if(string.IsNullOrEmpty(id))
                            {
                                var psi = new ProcessStartInfo
                                {
                                    FileName = "/bin/bash",
                                    Arguments = "date +%Z",
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                };
                                using (var p = Process.Start(psi))
                                {
                                    if (p.WaitForExit(1000))
                                    {
                                        id = p.StandardOutput.ReadToEnd();
                                    }
                                    p.Close();
                                }
                            }
                            if (!string.IsNullOrEmpty(id))
                            {
                                tz = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(z => z.Id == id) ?? tz;
                            }
                        }
                    }
                    defaultTimeZone = tz;
                }
                catch(Exception)
                {
                    // ignore
                    defaultTimeZone = TimeZoneInfo.Utc;
                }
            }

            return string.IsNullOrEmpty(zoneId) ? defaultTimeZone : TimeZoneConverter.GetTimeZone(zoneId);
        }

        private void ValidateDomain(IDbManager db, string domain, int tenantId, bool validateCharacters)
        {
            // size
            TenantDomainValidator.ValidateDomainLength(domain);

            // characters
            if (validateCharacters)
            {
                TenantDomainValidator.ValidateDomainCharacters(domain);
            }

            // forbidden or exists
            var exists = false;
            domain = domain.ToLowerInvariant();
            if (!exists)
            {
                if (forbiddenDomains == null)
                {
                    forbiddenDomains = ExecList(new SqlQuery("tenants_forbiden").Select("address")).Select(r => (string)r[0]).ToList();
                }
                exists = tenantId != 0 && forbiddenDomains.Contains(domain);
            }
            if (!exists)
            {
                exists = 0 < db.ExecuteScalar<int>(new SqlQuery("tenants_tenants").SelectCount().Where(Exp.Eq("alias", domain) & !Exp.Eq("id", tenantId)));
            }
            if (!exists)
            {
                exists = 0 < db.ExecuteScalar<int>(new SqlQuery("tenants_tenants").SelectCount()
                    .Where(Exp.Eq("mappeddomain", domain) & !Exp.Eq("id", tenantId) & !Exp.In("status", new []{(int)TenantStatus.RemovePending, (int) TenantStatus.Restoring})));
            }
            if (exists)
            {
                // cut number suffix
                while (true)
                {
                    if (6 < domain.Length && char.IsNumber(domain, domain.Length - 1))
                    {
                        domain = domain.Substring(0, domain.Length - 1);
                    }
                    else
                    {
                        break;
                    }
                }

                var q = new SqlQuery("tenants_forbiden").Select("address").Where(Exp.Like("address", domain, SqlLike.StartWith))
                    .Union(new SqlQuery("tenants_tenants").Select("alias").Where(Exp.Like("alias", domain, SqlLike.StartWith) & !Exp.Eq("id", tenantId)))
                    .Union(new SqlQuery("tenants_tenants").Select("mappeddomain")
                        .Where(Exp.Like("mappeddomain", domain, SqlLike.StartWith) & !Exp.Eq("id", tenantId) & !Exp.Eq("status", (int)TenantStatus.RemovePending)));

                var existsTenants = db.ExecuteList(q).ConvertAll(r => (string)r[0]);
                throw new TenantAlreadyExistsException("Address busy.", existsTenants);
            }
        }
    }
}
