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

using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Caching;
using ASC.Core.Data;
using ASC.Core.Tenants;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Core.Billing
{
    class TariffService : DbBaseService, ITariffService
    {
        private const int DEFAULT_TRIAL_PERIOD = 45;
        private static readonly TimeSpan DEFAULT_CACHE_EXPIRATION = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan STANDALONE_CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

        private static readonly ILog log = LogManager.GetLogger(typeof(TariffService));
        private readonly IQuotaService quotaService;
        private readonly ITenantService tenantService;
        private readonly IConfigurationClient config;
        private readonly ICache cache;
        private readonly bool test;


        public TimeSpan CacheExpiration { get; set; }


        public TariffService(ConnectionStringSettings connectionString, IQuotaService quotaService, ITenantService tenantService)
            : base(connectionString, "tenant")
        {
            this.quotaService = quotaService;
            this.tenantService = tenantService;
            this.config = new ClientConfiguration(tenantService);
            this.cache = new AspCache();
            this.CacheExpiration = DEFAULT_CACHE_EXPIRATION;
            this.test = ConfigurationManager.AppSettings["core.payment-test"] == "true";
        }


        public Tariff GetTariff(int tenantId, bool withRequestToPaymentSystem = true)
        {
            var key = "tariff/" + tenantId;
            var tariff = cache.Get(key) as Tariff;
            if (tariff == null)
            {
                tariff = Tariff.CreateDefault();

                var cached = GetBillingInfo(tenantId);
                if (cached != null)
                {
                    tariff.QuotaId = cached.Item1;
                    tariff.DueDate = cached.Item2;
                }

                if (withRequestToPaymentSystem)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            using (var client = GetBillingClient())
                            {
                                try
                                {
                                    var p = client.GetLastPayment(GetPortalId(tenantId));
                                    var quota = quotaService.GetTenantQuotas().SingleOrDefault(q => q.AvangateId == p.ProductId);
                                    var asynctariff = Tariff.CreateDefault();
                                    asynctariff.QuotaId = quota.Id;
                                    asynctariff.Autorenewal = p.Autorenewal;
                                    asynctariff.DueDate = 9999 <= p.EndDate.Year ? DateTime.MaxValue : p.EndDate;

                                    if (SaveBillingInfo(tenantId, Tuple.Create(asynctariff.QuotaId, asynctariff.DueDate)))
                                    {
                                        asynctariff = CalculateTariff(tenantId, asynctariff);
                                        ClearCache(tenantId);
                                        cache.Insert(key, asynctariff, DateTime.UtcNow.Add(GetCacheExpiration()));
                                    }
                                }
                                finally
                                {
                                    if (config.Standalone)
                                    {
                                        var po = client.GetPaymentOffice(GetPortalId(tenantId));
                                        if (!string.IsNullOrEmpty(po.Key2) && !Equals(config.SKey, po.Key2))
                                        {
                                            config.SKey = po.Key2;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception error)
                        {
                            log.Error(error);
                        }
                    });
                }

                tariff = CalculateTariff(tenantId, tariff);
                cache.Insert(key, tariff, DateTime.UtcNow.Add(GetCacheExpiration()));
            }

            return tariff;
        }

        public void SetTariff(int tenantId, Tariff tariff)
        {
            if (tariff == null)
            {
                throw new ArgumentNullException("tariff");
            }

            var q = quotaService.GetTenantQuota(tariff.QuotaId);
            if (q == null) return;
            SaveBillingInfo(tenantId, Tuple.Create(tariff.QuotaId, tariff.DueDate));
            if (q.Trial)
            {
                // reset trial date
                var tenant = tenantService.GetTenant(tenantId);
                if (tenant != null)
                {
                    tenant.VersionChanged = DateTime.UtcNow;
                    tenantService.SaveTenant(tenant);
                }
            }

            ClearCache(tenantId);
        }

        public void ClearCache(int tenantId)
        {
            cache.Remove("tariff/" + tenantId);
            cache.Remove("billing/urls/" + tenantId);
            cache.Remove(string.Format("billing/payments/{0}{1:u}{2:u}", tenantId, DateTime.MinValue, DateTime.MaxValue)); // clear all payments
        }

        public IEnumerable<PaymentInfo> GetPayments(int tenantId, DateTime from, DateTime to)
        {
            from = from.Date;
            to = to.Date.AddTicks(TimeSpan.TicksPerDay - 1);
            var key = string.Format("billing/payments/{0}{1:u}{2:u}", tenantId, from, to);
            var payments = cache.Get(key) as List<PaymentInfo>;
            if (payments == null)
            {
                payments = new List<PaymentInfo>();
                var quotas = quotaService.GetTenantQuotas();
                try
                {
                    using (var client = GetBillingClient())
                    {
                        foreach (var pi in client.GetPayments(GetPortalId(tenantId), from, to))
                        {
                            var quota = quotas.SingleOrDefault(q => q.AvangateId == pi.ProductId);
                            if (quota != null)
                            {
                                pi.QuotaId = quota.Id;
                            }
                            payments.Add(pi);
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error(error);
                }

                cache.Insert(key, payments, DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)));
            }

            return payments;
        }

        public Uri GetShoppingUri(int tenant, int plan)
        {
            var key = "billing/urls/" + tenant;
            var urls = cache.Get(key) as IDictionary<string, Tuple<Uri, Uri>>;
            if (urls == null)
            {
                urls = new Dictionary<string, Tuple<Uri, Uri>>();
                try
                {
                    var products = quotaService.GetTenantQuotas()
                                               .Select(q => q.AvangateId)
                                               .Where(id => !string.IsNullOrEmpty(id))
                                               .ToArray();
                    using (var client = GetBillingClient())
                    {
                        urls = client.GetPaymentUrls(GetPortalId(tenant), products);
                    }
                }
                catch (Exception error)
                {
                    log.Error(error);
                }
                cache.Insert(key, urls, DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)));
            }

            ResetCacheExpiration();

            var quota = quotaService.GetTenantQuota(plan);
            Tuple<Uri, Uri> tuple;
            if (quota != null && !string.IsNullOrEmpty(quota.AvangateId) && urls.TryGetValue(quota.AvangateId, out tuple))
            {
                var tariff = GetTariff(tenant);
                if (tariff == null || tariff.QuotaId == plan || tariff.State == TariffState.NotPaid || tuple.Item2 == null)
                {
                    return tuple.Item1;
                }
                return tuple.Item2;
            }
            return null;
        }

        public Invoice GetInvoice(string paymentId)
        {
            try
            {
                using (var client = GetBillingClient())
                {
                    return client.GetInvoice(paymentId);
                }
            }
            catch (Exception error)
            {
                log.Error(error);
                return new Invoice();
            }
        }


        public string GetButton(int tariffId, string partnerId)
        {
            var q = new SqlQuery("tenants_buttons")
               .Select("button_url")
               .Where(Exp.Eq("tariff_id", tariffId) & Exp.Eq("partner_id", partnerId))
               .SetMaxResults(1);

            return ExecList(q).ConvertAll(r => (string)r[0]).SingleOrDefault();
        }

        public void SaveButton(int tariffId, string partnerId, string buttonUrl)
        {
            var q = new SqlInsert("tenants_buttons", true)
                .InColumnValue("tariff_id", tariffId)
                .InColumnValue("partner_id", partnerId)
                .InColumnValue("button_url", buttonUrl);

            ExecNonQuery(q);
        }


        private Tuple<int, DateTime> GetBillingInfo(int tenant)
        {
            var q = new SqlQuery("tenants_tariff")
                .Select("tariff", "stamp")
                .Where("tenant", tenant)
                .OrderBy("id", false)
                .SetMaxResults(1);

            return ExecList(q)
                .ConvertAll(r => Tuple.Create(Convert.ToInt32(r[0]), ((DateTime)r[1]).Year < 9999 ? (DateTime)r[1] : DateTime.MaxValue))
                .SingleOrDefault();
        }

        private bool SaveBillingInfo(int tenant, Tuple<int, DateTime> bi)
        {
            var inserted = false;
            ExecAction(db =>
                {
                    if (!Equals(bi, GetBillingInfo(tenant)))
                    {
                        // last record is not the same
                        var q = new SqlQuery("tenants_tariff").SelectCount().Where("tenant", tenant).Where("tariff", bi.Item1).Where("stamp", bi.Item2);
                        if (bi.Item2 == DateTime.MaxValue || db.ExecScalar<int>(q) == 0)
                        {
                            var i = new SqlInsert("tenants_tariff")
                                .InColumnValue("tenant", tenant)
                                .InColumnValue("tariff", bi.Item1)
                                .InColumnValue("stamp", bi.Item2);
                            db.ExecNonQuery(i);
                            cache.Remove("tariff/" + tenant);
                            inserted = true;
                        }
                    }
                });

            if (inserted)
            {
                var t = tenantService.GetTenant(tenant);
                if (t != null)
                {
                    // update tenant.LastModified to flush cache in documents
                    tenantService.SaveTenant(t);
                }
            }
            return inserted;
        }


        private Tariff CalculateTariff(int tenantId, Tariff tariff)
        {
            tariff.State = TariffState.Paid;
            var q = quotaService.GetTenantQuota(tariff.QuotaId);

            if (q == null || q.GetFeature("old"))
            {
                tariff.QuotaId = Tenant.DEFAULT_TENANT;
                q = quotaService.GetTenantQuota(tariff.QuotaId);
            }

            if (q != null && q.Trial)
            {
                tariff.State = TariffState.Trial;
                if (tariff.DueDate == DateTime.MinValue || tariff.DueDate == DateTime.MaxValue)
                {
                    var tenant = tenantService.GetTenant(tenantId);
                    if (tenant != null)
                    {
                        var fromDate = tenant.CreatedDateTime < tenant.VersionChanged ? tenant.VersionChanged : tenant.CreatedDateTime;
                        var trialPeriod = GetPeriod("TrialPeriod", DEFAULT_TRIAL_PERIOD);
                        if (fromDate == DateTime.MinValue) fromDate = DateTime.UtcNow.Date;
                        tariff.DueDate = trialPeriod != default(int) ? fromDate.Date.AddDays(trialPeriod) : DateTime.MaxValue;
                    }
                    else
                    {
                        tariff.DueDate = DateTime.MaxValue;
                    }
                }
            }
            if (tariff.DueDate.Date < DateTime.UtcNow.Date)
            {
                tariff.State = TariffState.NotPaid;

                if (config.Standalone)
                {
                    tariff = Tariff.CreateDefault();
                }
            }

            tariff.Prolongable = tariff.DueDate == DateTime.MinValue || tariff.DueDate == DateTime.MaxValue ||
                                 tariff.State == TariffState.Trial ||
                                 new DateTime(tariff.DueDate.Year, tariff.DueDate.Month, 1) <= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1);

            return tariff;
        }

        private int GetPeriod(string key, int defaultValue)
        {
            var settings = tenantService.GetTenantSettings(Tenant.DEFAULT_TENANT, key);
            return settings != null ? Convert.ToInt32(Encoding.UTF8.GetString(settings)) : defaultValue;
        }

        private BillingClient GetBillingClient()
        {
            return new BillingClient(test);
        }

        private string GetPortalId(int tenant)
        {
            return config.GetKey(tenant);
        }

        private TimeSpan GetCacheExpiration()
        {
            if (config.Standalone && CacheExpiration < STANDALONE_CACHE_EXPIRATION)
            {
                CacheExpiration = CacheExpiration.Add(TimeSpan.FromSeconds(30));
            }
            return CacheExpiration;
        }

        private void ResetCacheExpiration()
        {
            if (config.Standalone)
            {
                CacheExpiration = DEFAULT_CACHE_EXPIRATION;
            }
        }
    }
}