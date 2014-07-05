/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Web;
using ASC.Core.Billing;
using ASC.Core.Common.Logging;
using ASC.Core.Tenants;


namespace ASC.Core
{
    class ClientTenantManager : ITenantManagerClient
    {
        private const string CURRENT_TENANT = "CURRENT_TENANT";
        private readonly ITenantService tenantService;
        private readonly IQuotaService quotaService;
        private readonly ITariffService tariffService;
        private readonly List<string> thisCompAddresses = new List<string>();


        public ClientTenantManager(ITenantService tenantService, IQuotaService quotaService, ITariffService tariffService)
        {
            this.tenantService = tenantService;
            this.quotaService = quotaService;
            this.tariffService = tariffService;

            thisCompAddresses.Add("localhost");
            thisCompAddresses.Add(Dns.GetHostName().ToLowerInvariant());
            thisCompAddresses.AddRange(Dns.GetHostAddresses("localhost").Select(a => a.ToString()));
            thisCompAddresses.AddRange(Dns.GetHostAddresses(Dns.GetHostName()).Select(a => a.ToString()));
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
                t = tenantService.GetTenant(0);
            }
            if (t == null)
            {
                var baseUrl = ConfigurationManager.AppSettings["core.base-domain"];
                if (baseUrl != null && domain.EndsWith("." + baseUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    t = tenantService.GetTenant(domain.Substring(0, domain.Length - baseUrl.Length - 1));
                }
            }
            if (t == null)
            {
                t = tenantService.GetTenant(domain);
            }
            if (t == null && CoreContext.Configuration.Standalone)
            {
                t = GetTenants().FirstOrDefault();
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
                AdminLog.PostAction("Settings: switched portal version settings to <{0}>", version);
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

        public void RemoveTenant(int tenantId)
        {
            tenantService.RemoveTenant(tenantId);
        }

        public Tenant GetCurrentTenant()
        {
            return GetCurrentTenant(true);
        }

        public Tenant GetCurrentTenant(bool throwIfNotFound)
        {
            var tenant = CallContext.GetData(CURRENT_TENANT) as Tenant;
            if (tenant == null && HttpContext.Current != null)
            {
                tenant = HttpContext.Current.Items[CURRENT_TENANT] as Tenant;
                if (tenant == null && HttpContext.Current.Request != null)
                {
                    tenant = GetTenant(HttpContext.Current.Request.GetUrlRewriter().Host);
                    HttpContext.Current.Items[CURRENT_TENANT] = tenant;
                }
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
            // ���� � tenants_quota ���� ������, � ������ ��������������� �������, �� � �������� ����� ������� ������ ���
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
