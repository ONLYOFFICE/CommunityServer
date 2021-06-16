/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using ASC.Common.Caching;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core.Data;
using ASC.Core.Tenants;

namespace ASC.Core.Billing
{
    public class TariffService : DbBaseService, ITariffService
    {
        private const int DEFAULT_TRIAL_PERIOD = 30;
        private static readonly TimeSpan DEFAULT_CACHE_EXPIRATION = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan STANDALONE_CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

        private readonly static ICache cache;
        private readonly static ICacheNotify notify;

        private static readonly ILog log = LogManager.GetLogger("ASC");
        private readonly IQuotaService quotaService;
        private readonly ITenantService tenantService;
        private readonly CoreConfiguration config;
        private readonly bool test;
        private readonly int paymentDelay;

        public readonly static int ACTIVE_USERS_MIN;
        public readonly static int ACTIVE_USERS_MAX;


        public TimeSpan CacheExpiration { get; set; }


        static TariffService()
        {
            cache = AscCache.Memory;
            notify = AscCache.Notify;
            notify.Subscribe<TariffCacheItem>((i, a) =>
            {
                cache.Remove(GetTariffCacheKey(i.TenantId));
                cache.Remove(GetBillingUrlCacheKey(i.TenantId));
                cache.Remove(GetBillingPaymentCacheKey(i.TenantId)); // clear all payments
            });


            var range = (ConfigurationManager.AppSettings["core.payment-user-range"] ?? "").Split('-');
            if (!int.TryParse(range[0], out ACTIVE_USERS_MIN))
            {
                ACTIVE_USERS_MIN = 0;
            }
            if (range.Length < 2 || !int.TryParse(range[1], out ACTIVE_USERS_MAX))
            {
                ACTIVE_USERS_MAX = Users.Constants.MaxEveryoneCount;
            }
        }


        public TariffService(ConnectionStringSettings connectionString, IQuotaService quotaService, ITenantService tenantService)
            : base(connectionString, "tenant")
        {
            this.quotaService = quotaService;
            this.tenantService = tenantService;
            config = new CoreConfiguration(tenantService);
            CacheExpiration = DEFAULT_CACHE_EXPIRATION;
            test = ConfigurationManagerExtension.AppSettings["core.payment-test"] == "true";
            int.TryParse(ConfigurationManagerExtension.AppSettings["core.payment-delay"], out paymentDelay);
        }


        public Tariff GetTariff(int tenantId, bool withRequestToPaymentSystem = true)
        {
            //single tariff for all portals
            if (CoreContext.Configuration.Standalone)
                tenantId = -1;

            var key = GetTariffCacheKey(tenantId);
            var tariff = cache.Get<Tariff>(key);
            if (tariff == null)
            {
                tariff = GetBillingInfo(tenantId);

                tariff = CalculateTariff(tenantId, tariff);
                cache.Insert(key, tariff, DateTime.UtcNow.Add(GetCacheExpiration()));

                if (BillingClient.Configured && withRequestToPaymentSystem)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            var client = GetBillingClient();
                            var lastPayment = client.GetLastPayment(GetPortalId(tenantId));

                            var quota = quotaService.GetTenantQuotas().SingleOrDefault(q => q.AvangateId == lastPayment.ProductId);
                            if (quota == null)
                            {
                                throw new InvalidOperationException(string.Format("Quota with id {0} not found for portal {1}.", lastPayment.ProductId, GetPortalId(tenantId)));
                            }

                            var asynctariff = Tariff.CreateDefault();
                            asynctariff.QuotaId = quota.Id;
                            asynctariff.Autorenewal = lastPayment.Autorenewal;
                            asynctariff.DueDate = 9999 <= lastPayment.EndDate.Year ? DateTime.MaxValue : lastPayment.EndDate;

                            if (quota.ActiveUsers == -1
                                && lastPayment.Quantity < ACTIVE_USERS_MIN)
                            {
                                throw new BillingException(string.Format("The portal {0} is paid for {1} users", tenantId, lastPayment.Quantity));
                            }
                            asynctariff.Quantity = lastPayment.Quantity;

                            if (SaveBillingInfo(tenantId, asynctariff, false))
                            {
                                asynctariff = CalculateTariff(tenantId, asynctariff);
                                ClearCache(tenantId);
                                cache.Insert(key, asynctariff, DateTime.UtcNow.Add(GetCacheExpiration()));
                            }
                        }
                        catch (BillingNotFoundException)
                        {
                        }
                        catch (Exception error)
                        {
                            LogError(error, tenantId.ToString());
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

            SaveBillingInfo(tenantId, tariff);

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

        private static string GetBillingPaymentCacheKey(int tenantId)
        {
            return string.Format("{0}:{1}", tenantId, "billing:payments");
        }


        public void ClearCache(int tenantId)
        {
            notify.Publish(new TariffCacheItem { TenantId = tenantId }, CacheNotifyAction.Remove);
        }

        public IEnumerable<PaymentInfo> GetPayments(int tenantId)
        {
            var key = GetBillingPaymentCacheKey(tenantId);
            var payments = cache.Get<List<PaymentInfo>>(key);
            if (payments == null)
            {
                payments = new List<PaymentInfo>();
                if (BillingClient.Configured)
                {
                    try
                    {
                        var quotas = quotaService.GetTenantQuotas();
                        var client = GetBillingClient();
                        foreach (var pi in client.GetPayments(GetPortalId(tenantId)))
                        {
                            var quota = quotas.SingleOrDefault(q => q.AvangateId == pi.ProductRef);
                            if (quota != null)
                            {
                                pi.QuotaId = quota.Id;
                            }
                            payments.Add(pi);
                        }
                    }
                    catch (Exception error)
                    {
                        LogError(error, tenantId.ToString());
                    }
                }

                cache.Insert(key, payments, DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)));
            }

            return payments;
        }

        public Uri GetShoppingUri(int? tenant, int quotaId, string affiliateId, string currency = null, string language = null, string customerId = null, string quantity = null)
        {
            var quota = quotaService.GetTenantQuota(quotaId);
            if (quota == null) return null;

            var key = tenant.HasValue
                          ? GetBillingUrlCacheKey(tenant.Value)
                          : String.Format("notenant{0}", !string.IsNullOrEmpty(affiliateId) ? "_" + affiliateId : "");
            key += quota.Visible ? "" : "0";
            var urls = cache.Get<Dictionary<string, Tuple<Uri, Uri>>>(key) as IDictionary<string, Tuple<Uri, Uri>>;
            if (urls == null)
            {
                urls = new Dictionary<string, Tuple<Uri, Uri>>();
                if (BillingClient.Configured)
                {
                    try
                    {
                        var products = quotaService.GetTenantQuotas()
                                                   .Where(q => !string.IsNullOrEmpty(q.AvangateId) && q.Visible == quota.Visible)
                                                   .Select(q => q.AvangateId)
                                                   .ToArray();

                        var client = GetBillingClient();
                        urls =
                            client.GetPaymentUrls(
                                tenant.HasValue ? GetPortalId(tenant.Value) : null,
                                products,
                                tenant.HasValue ? GetAffiliateId(tenant.Value) : affiliateId,
                                tenant.HasValue ? GetCampaign(tenant.Value) : null,
                                !string.IsNullOrEmpty(currency) ? "__Currency__" : null,
                                !string.IsNullOrEmpty(language) ? "__Language__" : null,
                                !string.IsNullOrEmpty(customerId) ? "__CustomerID__" : null,
                                !string.IsNullOrEmpty(quantity) ? "__Quantity__" : null
                                );
                    }
                    catch (Exception error)
                    {
                        log.Error(error);
                    }
                }
                cache.Insert(key, urls, DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)));
            }

            ResetCacheExpiration();

            Tuple<Uri, Uri> tuple;
            if (!string.IsNullOrEmpty(quota.AvangateId) && urls.TryGetValue(quota.AvangateId, out tuple))
            {
                var result = tuple.Item2;

                if (result == null)
                {
                    result = tuple.Item1;
                }
                else
                {
                    var tariff = tenant.HasValue ? GetTariff(tenant.Value) : null;
                    if (tariff == null || tariff.QuotaId == quotaId || tariff.State >= TariffState.Delay)
                    {
                        result = tuple.Item1;
                    }
                }

                if (result == null) return null;

                result = new Uri(result.ToString()
                                       .Replace("__Currency__", HttpUtility.UrlEncode(currency ?? ""))
                                       .Replace("__Language__", HttpUtility.UrlEncode((language ?? "").ToLower()))
                                       .Replace("__CustomerID__", HttpUtility.UrlEncode(customerId ?? ""))
                                       .Replace("__Quantity__", HttpUtility.UrlEncode(quantity ?? "")));
                return result;
            }
            return null;
        }

        public IDictionary<string, Dictionary<string, decimal>> GetProductPriceInfo(params string[] productIds)
        {
            if (productIds == null)
            {
                throw new ArgumentNullException("productIds");
            }
            try
            {
                var key = "biling-prices" + string.Join(",", productIds);
                var result = cache.Get<IDictionary<string, Dictionary<string, decimal>>>(key);
                if (result == null)
                {
                    var client = GetBillingClient();
                    result = client.GetProductPriceInfo(productIds);
                    cache.Insert(key, result, DateTime.Now.AddHours(1));
                }
                return result;
            }
            catch (Exception error)
            {
                LogError(error);
                return productIds
                    .Select(p => new { ProductId = p, Prices = new Dictionary<string, decimal>() })
                    .ToDictionary(e => e.ProductId, e => e.Prices);
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


        private Tariff GetBillingInfo(int tenant)
        {
            var q = new SqlQuery("tenants_tariff")
                .Select("tariff", "stamp", "quantity")
                .Where("tenant", tenant)
                .OrderBy("id", false)
                .SetMaxResults(1);

            return ExecList(q)
                .ConvertAll(r =>
                {
                    var tariff = Tariff.CreateDefault();
                    tariff.QuotaId = Convert.ToInt32(r[0]);
                    tariff.DueDate = ((DateTime)r[1]).Year < 9999 ? (DateTime)r[1] : DateTime.MaxValue;
                    tariff.Quantity = Convert.ToInt32(r[2]);
                    return tariff;
                })
                .SingleOrDefault()
                ?? Tariff.CreateDefault();
        }

        private bool SaveBillingInfo(int tenant, Tariff tariffInfo, bool renewal = true)
        {
            var inserted = false;
            var currentTariff = GetBillingInfo(tenant);
            if (!tariffInfo.EqualsByParams(currentTariff))
            {
                using (var db = GetDb())
                using (var tx = db.BeginTransaction())
                {
                    // last record is not the same
                    var q = new SqlQuery("tenants_tariff")
                        .SelectCount()
                        .Where("tenant", tenant)
                        .Where("tariff", tariffInfo.QuotaId)
                        .Where("stamp", tariffInfo.DueDate)
                        .Where("quantity", tariffInfo.Quantity);

                    if (tariffInfo.DueDate == DateTime.MaxValue || renewal || db.ExecuteScalar<int>(q) == 0)
                    {
                        var i = new SqlInsert("tenants_tariff")
                            .InColumnValue("tenant", tenant)
                            .InColumnValue("tariff", tariffInfo.QuotaId)
                            .InColumnValue("stamp", tariffInfo.DueDate)
                            .InColumnValue("quantity", tariffInfo.Quantity);
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

        public void DeleteDefaultBillingInfo()
        {
            const int tenant = Tenant.DEFAULT_TENANT;
            using (var db = GetDb())
            {
                db.ExecuteNonQuery(new SqlUpdate("tenants_tariff")
                                       .Set("tenant", -2)
                                       .Where("tenant", tenant));
            }

            ClearCache(tenant);
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
                    if (q != null)
                    {
                        var defaultQuota = quotaService.GetTenantQuota(Tenant.DEFAULT_TENANT);
                        defaultQuota.Name = "overdue";

                        defaultQuota.Features = q.Features;
                        defaultQuota.Support = false;

                        quotaService.SaveTenantQuota(defaultQuota);
                    }

                    var unlimTariff = Tariff.CreateDefault();
                    unlimTariff.LicenseDate = tariff.DueDate;

                    tariff = unlimTariff;
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

        private string GetCampaign(int tenant)
        {
            return config.GetCampaign(tenant);
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

        private static void LogError(Exception error, string tenantId = null)
        {
            if (error is BillingNotFoundException)
            {
                log.DebugFormat("Payment tenant {0} not found: {1}", tenantId, error.Message);
            }
            else if (error is BillingNotConfiguredException)
            {
                log.DebugFormat("Billing tenant {0} not configured: {1}", tenantId, error.Message);
            }
            else
            {
                if (log.IsDebugEnabled)
                {
                    log.Error("Billing tenant " + tenantId);
                    log.Error(error);
                }
                else
                {
                    log.ErrorFormat("Billing tenant {0}: {1}", tenantId, error.Message);
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