/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Data.Common;
using System.Linq;
using System.Security;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Core.Billing;
using ASC.Core.Tenants;

namespace ASC.Core
{
    public class MultiRegionHostedSolution
    {
        private readonly object locker = new object();
        private readonly Dictionary<string, HostedSolution> regions = new Dictionary<string, HostedSolution>();
        private readonly String dbid;
        private volatile bool initialized = false;


        public MultiRegionHostedSolution(string dbid)
        {
            this.dbid = dbid;
        }

        public List<Tenant> GetTenants(DateTime from)
        {
            return GetRegionServices()
                .SelectMany(r => r.GetTenants(from))
                .ToList();
        }

        public List<Tenant> FindTenants(string login)
        {
            return FindTenants(login, null);
        }

        public List<Tenant> FindTenants(string login, string password)
        {
            var result = new List<Tenant>();
            Exception error = null;

            foreach (var service in GetRegionServices())
            {
                try
                {
                    result.AddRange(service.FindTenants(login, password));
                }
                catch (SecurityException exception)
                {
                    error = exception;
                }
            }
            if (!result.Any() && error != null)
            {
                throw error;
            }
            return result;
        }

        public void RegisterTenant(string region, TenantRegistrationInfo ri, out Tenant tenant)
        {
            ri.HostedRegion = region;
            GetRegionService(region).RegisterTenant(ri, out tenant);
        }

        public Tenant GetTenant(string domain)
        {
            foreach (var service in GetRegionServices())
            {
                var tenant = service.GetTenant(domain);
                if (tenant != null)
                {
                    return tenant;
                }
            }
            return null;
        }

        public Tenant GetTenant(string region, int tenantId)
        {
            return GetRegionService(region).GetTenant(tenantId);
        }

        public Tenant SaveTenant(string region, Tenant tenant)
        {
            return GetRegionService(region).SaveTenant(tenant);
        }


        public string CreateAuthenticationCookie(string region, int tenantId, string login, string password)
        {
            return GetRegionService(region).CreateAuthenticationCookie(tenantId, login, password);
        }

        public string CreateAuthenticationCookie(string region, int tenantId, Guid userId)
        {
            return GetRegionService(region).CreateAuthenticationCookie(tenantId, userId);
        }


        public Tariff GetTariff(string region, int tenantId, bool withRequestToPaymentSystem = true)
        {
            return GetRegionService(region).GetTariff(tenantId, withRequestToPaymentSystem);
        }

        public void SetTariff(string region, int tenant, bool paid)
        {
            GetRegionService(region).SetTariff(tenant, paid);
        }

        public void SetTariff(string region, int tenant, Tariff tariff)
        {
            GetRegionService(region).SetTariff(tenant, tariff);
        }

        public void SaveButton(string region, int tariffId, string partnerId, string buttonUrl)
        {
            GetRegionService(region).SaveButton(tariffId, partnerId, buttonUrl);
        }

        public TenantQuota GetTenantQuota(string region, int tenant)
        {
            return GetRegionService(region).GetTenantQuota(tenant);
        }

        public void CheckTenantAddress(string address)
        {
            foreach (var service in GetRegionServices())
            {
                service.CheckTenantAddress(address);
            }
        }

        public IEnumerable<string> GetRegions()
        {
            return GetRegionServices().Select(s => s.Region).ToList();
        }

        public IDbManager GetRegionDb(string region)
        {
            return new DbManager(GetRegionService(region).DbId);
        }

        public IDbManager GetMultiRegionDb()
        {
            return new MultiRegionalDbManager(GetRegions().Select(r => new DbManager(GetRegionService(r).DbId)));
        }

        public ConnectionStringSettings GetRegionConnectionString(string region)
        {
            return DbRegistry.GetConnectionString(GetRegionService(region).DbId);
        }


        private IEnumerable<HostedSolution> GetRegionServices()
        {
            Initialize();

            return regions.Where(x => !String.IsNullOrEmpty(x.Key))
                   .Select(x => x.Value);
        }

        private HostedSolution GetRegionService(string region)
        {
            Initialize();
            return regions[region];
        }

        private void Initialize()
        {
            if (!initialized)
            {
                lock (locker)
                {
                    if (!initialized)
                    {
                        initialized = true;


                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["core.multi-hosted.config-only"] ?? "false"))
                        {
                            foreach (ConnectionStringSettings cs in ConfigurationManager.ConnectionStrings)
                            {
                                if (cs.Name.StartsWith(dbid + "."))
                                {
                                    var name = cs.Name.Substring(dbid.Length + 1);
                                    regions[name] = new HostedSolution(cs, name);
                                }
                            }

                            regions[dbid] = new HostedSolution(ConfigurationManager.ConnectionStrings[dbid]);
                            if (!regions.ContainsKey(string.Empty))
                            {
                                regions[string.Empty] = new HostedSolution(ConfigurationManager.ConnectionStrings[dbid]);
                            }
                        }
                        else
                        {

                            var find = false;
                            foreach (ConnectionStringSettings cs in ConfigurationManager.ConnectionStrings)
                            {
                                if (cs.Name.StartsWith(dbid + "."))
                                {
                                    var name = cs.Name.Substring(dbid.Length + 1);
                                    regions[name] = new HostedSolution(cs, name);
                                    find = true;
                                }
                            }
                            if (find)
                            {
                                regions[dbid] = new HostedSolution(ConfigurationManager.ConnectionStrings[dbid]);
                                if (!regions.ContainsKey(string.Empty))
                                {
                                    regions[string.Empty] = new HostedSolution(ConfigurationManager.ConnectionStrings[dbid]);
                                }
                            }
                            else
                            {
                                foreach (ConnectionStringSettings connectionString in ConfigurationManager.ConnectionStrings)
                                {
                                    try
                                    {
                                        using (var db = new DbManager(connectionString.Name))
                                        {
                                            var q = new SqlQuery("regions")
                                                .Select("region")
                                                .Select("connection_string")
                                                .Select("provider");
                                            db.ExecuteList(q)
                                                .ForEach(r =>
                                                {
                                                    var cs = new ConnectionStringSettings((string)r[0], (string)r[1], (string)r[2]);
                                                    if (!DbRegistry.IsDatabaseRegistered(cs.Name))
                                                    {
                                                        DbRegistry.RegisterDatabase(cs.Name, cs);
                                                    }

                                                    if (!regions.ContainsKey(string.Empty))
                                                    {
                                                        regions[string.Empty] = new HostedSolution(cs, cs.Name);
                                                    }

                                                    regions[cs.Name] = new HostedSolution(cs, cs.Name);
                                                });
                                        }
                                    }
                                    catch (DbException) { }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}