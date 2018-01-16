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


using ASC.Core.Billing;
using ASC.Core.Tenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Web;


namespace ASC.Core
{
    public class TenantManager
    {
        public const string CURRENT_TENANT = "CURRENT_TENANT";
        private readonly ITenantService tenantService;
        private readonly IQuotaService quotaService;
        private readonly ITariffService tariffService;
        private readonly List<string> thisCompAddresses = new List<string>();


        public TenantManager(ITenantService tenantService, IQuotaService quotaService, ITariffService tariffService)
        {
            this.tenantService = tenantService;
            this.quotaService = quotaService;
            this.tariffService = tariffService;

            thisCompAddresses.Add("localhost");
            thisCompAddresses.Add(Dns.GetHostName().ToLowerInvariant());
            thisCompAddresses.AddRange(Dns.GetHostAddresses("localhost").Select(a => a.ToString()));
            try
            {
                thisCompAddresses.AddRange(Dns.GetHostAddresses(Dns.GetHostName()).Select(a => a.ToString()));
            }
            catch
            {
                // ignore
            }
        }


        public List<Tenant> GetTenants()
        {
            return tenantService.GetTenants(default(DateTime)).ToList();
        }

        public Tenant GetTenant(int tenantId)
        {
            return tenantService.GetTenant(tenantId);
        }

        public Tenant GetTenant(string domain)
        {
            if (string.IsNullOrEmpty(domain)) return null;

            Tenant t = null;
            if (thisCompAddresses.Contains(domain, StringComparer.InvariantCultureIgnoreCase))
            {
                t = tenantService.GetTenant("localhost");
            }
            var isAlias = false;
            if (t == null)
            {
                var baseUrl = CoreContext.Configuration.BaseDomain;
                if (!String.IsNullOrEmpty(baseUrl) && domain.EndsWith("." + baseUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    isAlias = true;
                    t = tenantService.GetTenant(domain.Substring(0, domain.Length - baseUrl.Length - 1));
                }
            }
            if (t == null)
            {
                t = tenantService.GetTenant(domain);
            }
            if (t == null && CoreContext.Configuration.Standalone && !isAlias)
            {
                t = tenantService.GetTenantForStandaloneWithoutAlias(domain);
            }
            return t;
        }

        public Tenant SetTenantVersion(Tenant tenant, int version)
        {
            if (tenant == null) throw new ArgumentNullException("tenant");
            if (tenant.Version != version)
            {
                tenant.Version = version;
                SaveTenant(tenant);
            }
            else
            {
                throw new ArgumentException("This is current version already");
            }
            return tenant;
        }

        public Tenant SaveTenant(Tenant tenant)
        {
            var newTenant = tenantService.SaveTenant(tenant);

            var oldTenant = CallContext.GetData(CURRENT_TENANT) as Tenant;
            if (oldTenant != null) SetCurrentTenant(newTenant);

            return newTenant;
        }

        public void RemoveTenant(int tenantId, bool auto = false)
        {
            tenantService.RemoveTenant(tenantId, auto);
        }

        public Tenant GetCurrentTenant()
        {
            return GetCurrentTenant(true);
        }

        public Tenant GetCurrentTenant(bool throwIfNotFound)
        {
            Tenant tenant = null;
            if (HttpContext.Current != null)
            {
                tenant = HttpContext.Current.Items[CURRENT_TENANT] as Tenant;
                if (tenant == null && HttpContext.Current.Request != null)
                {
                    tenant = GetTenant(HttpContext.Current.Request.GetUrlRewriter().Host);
                    HttpContext.Current.Items[CURRENT_TENANT] = tenant;
                }
            }
            if (tenant == null)
            {
                tenant = CallContext.GetData(CURRENT_TENANT) as Tenant;
            }
            if (tenant == null && throwIfNotFound)
            {
                throw new Exception("Could not resolve current tenant :-(.");
            }
            return tenant;
        }

        public void SetCurrentTenant(Tenant tenant)
        {
            if (tenant != null)
            {
                CallContext.SetData(CURRENT_TENANT, tenant);
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items[CURRENT_TENANT] = tenant;
                }
                Thread.CurrentThread.CurrentCulture = tenant.GetCulture();
                Thread.CurrentThread.CurrentUICulture = tenant.GetCulture();
            }
        }

        public void SetCurrentTenant(int tenantId)
        {
            SetCurrentTenant(GetTenant(tenantId));
        }

        public void SetCurrentTenant(string domain)
        {
            SetCurrentTenant(GetTenant(domain));
        }

        public void CheckTenantAddress(string address)
        {
            tenantService.ValidateDomain(address);
        }

        public IEnumerable<TenantVersion> GetTenantVersions()
        {
            return tenantService.GetTenantVersions();
        }


        public IEnumerable<TenantQuota> GetTenantQuotas()
        {
            return GetTenantQuotas(false);
        }

        public IEnumerable<TenantQuota> GetTenantQuotas(bool all)
        {
            return quotaService.GetTenantQuotas().Where(q => q.Id < 0 && (all || q.Visible)).OrderByDescending(q => q.Id).ToList();
        }

        public TenantQuota GetTenantQuota(int tenant)
        {
            // если в tenants_quota есть строка, с данным идентификатором портала, то в качестве квоты берется именно она
            var q = quotaService.GetTenantQuota(tenant) ?? quotaService.GetTenantQuota(Tenant.DEFAULT_TENANT) ?? TenantQuota.Default;
            if (q.Id != tenant && tariffService != null)
            {
                var tariffQuota = quotaService.GetTenantQuota(tariffService.GetTariff(tenant).QuotaId);
                if (tariffQuota != null)
                {
                    return tariffQuota;
                }
            }
            return q;
        }

        public IDictionary<string, IEnumerable<Tuple<string, decimal>>> GetProductPriceInfo(bool all = true)
        {
            var productIds = GetTenantQuotas(all)
                .Select(p => p.AvangateId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToArray();
            return tariffService.GetProductPriceInfo(productIds);
        }

        public TenantQuota SaveTenantQuota(TenantQuota quota)
        {
            quota = quotaService.SaveTenantQuota(quota);
            return quota;
        }

        public void SetTenantQuotaRow(TenantQuotaRow row, bool exchange)
        {
            quotaService.SetTenantQuotaRow(row, exchange);
        }

        public List<TenantQuotaRow> FindTenantQuotaRows(TenantQuotaRowQuery query)
        {
            return quotaService.FindTenantQuotaRows(query).ToList();
        }
    }
}
