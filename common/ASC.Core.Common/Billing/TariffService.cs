/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.Common.Caching;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Data;
using ASC.Core.Tenants;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Core.Billing
{
    class TariffService : DbBaseService, ITariffService
    {
        private const int DEFAULT_TRIAL_PERIOD = 30;
        private static readonly TimeSpan DEFAULT_CACHE_EXPIRATION = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan STANDALONE_CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

        private readonly static ICache cache;
        private readonly static ICacheNotify notify;

        private static readonly ILog log = LogManager.GetLogger(typeof(TariffService));
        private readonly IQuotaService quotaService;
        private readonly ITenantService tenantService;
        private readonly CoreConfiguration config;
        private readonly bool test;
        private readonly int paymentDelay;


        public TimeSpan CacheExpiration { get; set; }


        static TariffService()
        {
            cache = AscCache.Memory;
            notify = AscCache.Notify;
            notify.Subscribe<TariffCacheItem>((i, a) =>
            {
                cache.Remove(GetTariffCacheKey(i.TenantId));
                cache.Remove(GetBillingUrlCacheKey(i.TenantId));
                cache.Remove(GetBillingPaymentCacheKey(i.TenantId, DateTime.MinValue, DateTime.MaxValue)); // clear all payments
            });
        }


        public TariffService(ConnectionStringSettings connectionString, IQuotaService quotaService, ITenantService tenantService)
            : base(connectionString, "tenant")
        {
            this.quotaService = quotaService;
            this.tenantService = tenantService;
            config = new CoreConfiguration(tenantService);
            CacheExpiration = DEFAULT_CACHE_EXPIRATION;
            test = ConfigurationManager.AppSettings["core.payment-test"] == "true";
            int.TryParse(ConfigurationManager.AppSettings["core.payment-delay"], out paymentDelay);
        }


        public Tariff GetTariff(int tenantId, bool withRequestToPaymentSystem = true)
        {
            var key = GetTariffCacheKey(tenantId);
            var tariff = cache.Get<Tariff>(key);
            if (tariff == null)
            {
                tariff = Tariff.CreateDefault();

                var cached = GetBillingInfo(tenantId);
                if (cached != null)
                {
                    tariff.QuotaId = cached.Item1;
                    tariff.DueDate = cached.Item2;
                }

                tariff = CalculateTariff(tenantId, tariff);
                cache.Insert(key, tariff, DateTime.UtcNow.Add(GetCacheExpiration()));

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
                                    if (quota == null)
                                    {
                                        throw new InvalidOperationException(string.Format("Quota with id {0} not found for portal {1}.", p.ProductId, GetPortalId(tenantId)));
                                    }
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
                            LogError(error);
                        }
                    });
                }
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

        private static string GetTariffCacheKey(int tenantId)
        {
            return string.Format("{0}:{1}", tenantId, "tariff");
        }

        private static string GetBillingUrlCacheKey(int tenantId)
        {
            return string.Format("{0}:{1}", tenantId, "billing:urls");
        }

        private static string GetBillingPaymentCacheKey(int tenantId, DateTime from, DateTime to)
        {
            return string.Format("{0}:{1}:{2}-{3}", tenantId, "billing:payments", from.ToString("yyyyMMddHHmmss"), to.ToString("yyyyMMddHHmmss"));
        }


        public void ClearCache(int tenantId)
        {
            notify.Publish(new TariffCacheItem { TenantId = tenantId }, CacheNotifyAction.Remove);
        }

        public IEnumerable<PaymentInfo> GetPayments(int tenantId, DateTime from, DateTime to)
        {
            from = from.Date;
            to = to.Date.AddTicks(TimeSpan.TicksPerDay - 1);
            var key = GetBillingPaymentCacheKey(tenantId, from, to);
            var payments = cache.Get<List<PaymentInfo>>(key);
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
                    LogError(error);
                }

                cache.Insert(key, payments, DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)));
            }

            return payments;
        }

        public Uri GetShoppingUri(int? tenant, int plan, string affiliateId)
        {
            var key = tenant.HasValue
                ? GetBillingUrlCacheKey(tenant.Value)
                : String.Format("notenant{0}", !string.IsNullOrEmpty(affiliateId) ? "_" + affiliateId : "");
            var urls = cache.Get<Dictionary<string, Tuple<Uri, Uri>>>(key) as IDictionary<string, Tuple<Uri, Uri>>;
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
                        urls = tenant.HasValue ?
                            client.GetPaymentUrls(GetPortalId(tenant.Value), products, GetAffiliateId(tenant.Value)) :
                            client.GetPaymentUrls(null, products, !string.IsNullOrEmpty(affiliateId) ? affiliateId : null);
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
                var tariff = tenant.HasValue ? GetTariff(tenant.Value) : null;
                if (tariff == null || tariff.QuotaId == plan || tariff.State >= TariffState.Delay || tuple.Item2 == null)
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
                LogError(error);
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
            if (!Equals(bi, GetBillingInfo(tenant)))
            {
                using (var db = GetDb())
                using (var tx = db.BeginTransaction())
                {
                    // last record is not the same
                    var q = new SqlQuery("tenants_tariff").SelectCount().Where("tenant", tenant).Where("tariff", bi.Item1).Where("stamp", bi.Item2);
                    if (bi.Item2 == DateTime.MaxValue || db.ExecuteScalar<int>(q) == 0)
                    {
                        var i = new SqlInsert("tenants_tariff")
                            .InColumnValue("tenant", tenant)
                            .InColumnValue("tariff", bi.Item1)
                            .InColumnValue("stamp", bi.Item2);
                        db.ExecuteNonQuery(i);
                        cache.Remove(GetTariffCacheKey(tenant));
                        inserted = true;
                    }
                    tx.Commit();
                }
            }

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

            var delay = 0;
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
            else
            {
                delay = paymentDelay;
            }

            if (tariff.DueDate != DateTime.MinValue && tariff.DueDate.Date < DateTime.Today && delay > 0)
            {
                tariff.State = TariffState.Delay;

                tariff.DelayDueDate = tariff.DueDate.Date.AddDays(delay);
            }

            if (tariff.DueDate == DateTime.MinValue ||
                tariff.DueDate != DateTime.MaxValue && tariff.DueDate.Date.AddDays(delay) < DateTime.Today)
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
            try
            {
                return new BillingClient(test);
            }
            catch (InvalidOperationException ioe)
            {
                throw new BillingNotConfiguredException(ioe.Message, ioe);
            }
            catch (ReflectionTypeLoadException rtle)
            {
                log.ErrorFormat("{0}{1}LoaderExceptions: {2}",
                    rtle,
                    Environment.NewLine,
                    string.Join(Environment.NewLine, rtle.LoaderExceptions.Select(e => e.ToString())));
                throw;
            }
        }

        private string GetPortalId(int tenant)
        {
            return config.GetKey(tenant);
        }

        private string GetAffiliateId(int tenant)
        {
            return config.GetAffiliateId(tenant);
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

        private static void LogError(Exception error)
        {
            if (error is BillingNotFoundException)
            {
                log.DebugFormat("Payment not found: {0}", error.Message);
            }
            else if (error is BillingNotConfiguredException)
            {
                log.DebugFormat("Billing not configured: {0}", error.Message);
            }
            else
            {
                if (log.IsDebugEnabled)
                {
                    log.Error(error);
                }
                else
                {
                    log.Error(error.Message);
                }
            }
        }


        [Serializable]
        class TariffCacheItem
        {
            public int TenantId { get; set; }
        }
    }
}