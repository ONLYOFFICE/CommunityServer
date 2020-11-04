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
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using ASC.Core.Billing;
using Newtonsoft.Json;


namespace ASC.Core
{
    public class PaymentManager
    {
        private readonly ITariffService tariffService;
        private readonly string partnerUrl;
        private readonly string partnerKey;


        public PaymentManager(ITariffService tariffService)
        {
            this.tariffService = tariffService;
            partnerUrl = (ConfigurationManagerExtension.AppSettings["core.payment-partners"] ?? "https://partners.onlyoffice.com/api").TrimEnd('/');
            partnerKey = (ConfigurationManagerExtension.AppSettings["core.machinekey"] ?? "C5C1F4E85A3A43F5B3202C24D97351DF");
        }


        public Tariff GetTariff(int tenantId)
        {
            return tariffService.GetTariff(tenantId);
        }

        public void SetTariff(int tenantId, Tariff tariff)
        {
            tariffService.SetTariff(tenantId, tariff);
        }

        public void DeleteDefaultTariff()
        {
            tariffService.DeleteDefaultBillingInfo();
        }

        public IEnumerable<PaymentInfo> GetTariffPayments(int tenant)
        {
            return GetTariffPayments(tenant, DateTime.MinValue, DateTime.MaxValue);
        }

        public IEnumerable<PaymentInfo> GetTariffPayments(int tenant, DateTime from, DateTime to)
        {
            return tariffService.GetPayments(tenant, from, to);
        }

        public Invoice GetPaymentInvoice(string paymentId)
        {
            return tariffService.GetInvoice(paymentId);
        }

        public IDictionary<string, IEnumerable<Tuple<string, decimal>>> GetProductPriceInfo(params string[] productIds)
        {
            return tariffService.GetProductPriceInfo(productIds);
        }

        public Uri GetShoppingUri(int tenant, int quotaId, string currency = null, string language = null, string customerId = null)
        {
            return tariffService.GetShoppingUri(tenant, quotaId, null, currency, language, customerId);
        }

        public Uri GetShoppingUri(int quotaId, bool forCurrentTenant = true, string affiliateId = null, string currency = null, string language = null, string customerId = null)
        {
            return tariffService.GetShoppingUri(forCurrentTenant ? CoreContext.TenantManager.GetCurrentTenant().TenantId : (int?)null, quotaId, affiliateId, currency, language, customerId);
        }

        public Uri GetShoppingUri(int quotaId, string affiliateId, string currency = null, string language = null, string customerId = null)
        {
            return tariffService.GetShoppingUri(null, quotaId, affiliateId, currency, language, customerId);
        }



        public void ActivateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            var now = DateTime.UtcNow;
            var actionUrl = "/partnerapi/ActivateKey?code=" + HttpUtility.UrlEncode(key) + "&portal=" + HttpUtility.UrlEncode(CoreContext.TenantManager.GetCurrentTenant().TenantAlias);
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Authorization", GetPartnerAuthHeader(actionUrl));
                try
                {
                    webClient.DownloadData(partnerUrl + actionUrl);
                }
                catch (WebException we)
                {
                    var error = GetException(we);
                    if (error != null)
                    {
                        throw error;
                    }
                    throw;
                }
                tariffService.ClearCache(CoreContext.TenantManager.GetCurrentTenant().TenantId);

                var timeout = DateTime.UtcNow - now - TimeSpan.FromSeconds(5);
                if (TimeSpan.Zero < timeout)
                {
                    // clear tenant cache
                    Thread.Sleep(timeout);
                }
                CoreContext.TenantManager.GetTenant(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            }
        }

        private string GetPartnerAuthHeader(string url)
        {
            using (var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(partnerKey)))
            {
                var now = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                var data = string.Join("\n", now, "/api/" + url.TrimStart('/')); //data: UTC DateTime (yyyy:MM:dd HH:mm:ss) + \n + url
                var hash = HttpServerUtility.UrlTokenEncode(hasher.ComputeHash(Encoding.UTF8.GetBytes(data)));
                return string.Format("ASC :{0}:{1}", now, hash);
            }
        }

        private static Exception GetException(WebException we)
        {
            var response = (HttpWebResponse)we.Response;
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var result = reader.ReadToEnd();
                    var excInfo = JsonConvert.DeserializeObject<ExceptionJson>(result);
                    return (Exception)Activator.CreateInstance(Type.GetType(excInfo.exceptionType, true), excInfo.exceptionMessage);
                }
            }
            return null;
        }


        private class ExceptionJson
        {
            public string message = null;
            public string exceptionMessage = null;
            public string exceptionType = null;
            public string stackTrace = null;
        }
    }
}
