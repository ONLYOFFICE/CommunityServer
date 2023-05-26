/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

using Newtonsoft.Json;

namespace ASC.Core.Billing
{
    public class BillingClient
    {
        public readonly static bool Configured = false;
        private readonly static string _billingDomain;
        private readonly static string _billingKey;
        private readonly static string _billingSecret;
        private readonly bool _test;

        public enum PaymentSystem
        {
            Avangate = 1,
            Stripe = 9
        };

        static BillingClient()
        {
            var billingDomain = ConfigurationManagerExtension.AppSettings["core.payment-url"];

            _billingDomain = (billingDomain ?? "").Trim().TrimEnd('/');
            if (!string.IsNullOrEmpty(_billingDomain))
            {
                _billingDomain += "/billing/";

                _billingKey = ConfigurationManager.AppSettings["core.payment-key"];
                _billingSecret = ConfigurationManager.AppSettings["core.payment-secret"];

                Configured = true;
            }
        }

        public BillingClient(bool test)
        {
            _test = test;
        }


        public PaymentLast GetLastPayment(string portalId)
        {
            var result = Request("GetActiveResource", portalId);
            var paymentLast = JsonConvert.DeserializeObject<PaymentLast>(result);

            if (!_test && paymentLast.PaymentStatus == 4)
            {
                throw new BillingException("Can not accept test payment.", new { PortalId = portalId });
            }

            return paymentLast;
        }

        public IEnumerable<PaymentInfo> GetPayments(string portalId)
        {
            string result = Request("GetPayments", portalId);
            var payments = JsonConvert.DeserializeObject<List<PaymentInfo>>(result);

            return payments;
        }

        public IDictionary<string, Uri> GetPaymentUrls(string portalId, string[] products, string affiliateId = null, string campaign = null, string currency = null, string language = null, string customerId = null, string quantity = null, PaymentSystem paymentSystem = PaymentSystem.Avangate)
        {
            var urls = new Dictionary<string, Uri>();

            var additionalParameters = new List<Tuple<string, string>>() { Tuple.Create("PaymentSystemId", ((int)paymentSystem).ToString()) };
            if (!string.IsNullOrEmpty(affiliateId))
            {
                additionalParameters.Add(Tuple.Create("AffiliateId", affiliateId));
            }
            if (!string.IsNullOrEmpty(campaign))
            {
                additionalParameters.Add(Tuple.Create("campaign", campaign));
            }
            if (!string.IsNullOrEmpty(currency))
            {
                additionalParameters.Add(Tuple.Create("Currency", currency));
            }
            if (!string.IsNullOrEmpty(language))
            {
                additionalParameters.Add(Tuple.Create("Language", language));
            }
            if (!string.IsNullOrEmpty(customerId))
            {
                additionalParameters.Add(Tuple.Create("CustomerID", customerId));
            }
            if (!string.IsNullOrEmpty(quantity))
            {
                additionalParameters.Add(Tuple.Create("Quantity", quantity));
            }

            var parameters = products
                .Distinct()
                .Select(p => Tuple.Create("ProductId", p))
                .Concat(additionalParameters)
                .ToArray();

            //max 100 products
            var result = Request("GetPaymentUrl", portalId, parameters);
            var paymentUrls = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

            foreach (var p in products)
            {
                string url;
                var paymentUrl = (Uri)null;
                if (paymentUrls.TryGetValue(p, out url) && !string.IsNullOrEmpty(url = ToUrl(url)))
                {
                    paymentUrl = new Uri(url);
                }
                urls[p] = paymentUrl;
            }
            return urls;
        }

        public string GetPaymentUrl(string portalId, string[] products, string affiliateId = null, string campaign = null, string currency = null, string language = null, string customerId = null, string customerEmail = null, string backUrl = null, string quantity = null, PaymentSystem paymentSystem = PaymentSystem.Avangate)
        {
            var additionalParameters = new List<Tuple<string, string>>() { Tuple.Create("PaymentSystemId", ((int)paymentSystem).ToString()) };
            if (!string.IsNullOrEmpty(affiliateId))
            {
                additionalParameters.Add(Tuple.Create("AffiliateId", affiliateId));
            }
            if (!string.IsNullOrEmpty(campaign))
            {
                additionalParameters.Add(Tuple.Create("campaign", campaign));
            }
            if (!string.IsNullOrEmpty(currency))
            {
                additionalParameters.Add(Tuple.Create("Currency", currency));
            }
            if (!string.IsNullOrEmpty(language))
            {
                additionalParameters.Add(Tuple.Create("Language", language));
            }
            if (!string.IsNullOrEmpty(customerId))
            {
                additionalParameters.Add(Tuple.Create("CustomerID", customerId));
            }
            if (!string.IsNullOrEmpty(customerEmail))
            {
                additionalParameters.Add(Tuple.Create("CustomerEmail", customerEmail));
            }
            if (!string.IsNullOrEmpty(backUrl))
            {
                additionalParameters.Add(Tuple.Create("BackRef", backUrl));
            }
            if (!string.IsNullOrEmpty(quantity))
            {
                additionalParameters.Add(Tuple.Create("Quantity", quantity));
            }

            var parameters = products
                .Distinct()
                .Select(p => Tuple.Create("ProductId", p))
                .Concat(additionalParameters)
                .ToArray();

            var result = Request("GetSinglePaymentUrl", portalId, parameters);
            var paymentUrl = JsonConvert.DeserializeObject<string>(result);

            return paymentUrl;
        }

        public IDictionary<string, Dictionary<string, decimal>> GetProductPriceInfo(string[] productIds, PaymentSystem paymentSystem = PaymentSystem.Avangate)
        {
            if (productIds == null)
            {
                throw new ArgumentNullException("productIds");
            }

            var parameters = productIds.Select(pid => Tuple.Create("ProductId", pid)).ToList();
            parameters.Add(Tuple.Create("PaymentSystemId", ((int)paymentSystem).ToString()));

            var result = Request("GetProductsPrices", null, parameters.ToArray());
            var prices = JsonConvert.DeserializeObject<Dictionary<PaymentSystem, Dictionary<string, Dictionary<string, decimal>>>>(result);

            if (prices.ContainsKey(paymentSystem))
            {
                var pricesPaymentSystem = prices[paymentSystem];

                return productIds.Select(productId =>
                    {
                        if (pricesPaymentSystem.ContainsKey(productId))
                        {
                            return new { ProductId = productId, Prices = pricesPaymentSystem[productId] };
                        }
                        return new { ProductId = productId, Prices = new Dictionary<string, decimal>() };
                    })
                    .ToDictionary(e => e.ProductId, e => e.Prices);
            }

            return new Dictionary<string, Dictionary<string, decimal>>();
        }


        private string CreateAuthToken(string pkey, string machinekey)
        {
            using (var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(machinekey)))
            {
                var now = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var hash = HttpServerUtility.UrlTokenEncode(hasher.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", now, pkey))));
                return string.Format("ASC {0}:{1}:{2}", pkey, now, hash);
            }
        }

        private string Request(string method, string portalId, params Tuple<string, string>[] parameters)
        {
            var url = _billingDomain + method;

            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 60000;
            request.ContentType = "application/json";

            if (!string.IsNullOrEmpty(_billingKey))
            {
                request.Headers.Add("Authorization", CreateAuthToken(_billingKey, _billingSecret));
            }

            var data = new Dictionary<string, List<string>>();
            if (!string.IsNullOrEmpty(portalId))
            {
                data.Add("PortalId", new List<string>() { portalId });
            }
            foreach (var parameter in parameters)
            {
                if (!data.ContainsKey(parameter.Item1))
                {
                    data.Add(parameter.Item1, new List<string>() { parameter.Item2 });
                }
                else
                {
                    data[parameter.Item1].Add(parameter.Item2);
                }
            }
            var body = JsonConvert.SerializeObject(data);

            var bytes = Encoding.UTF8.GetBytes(body ?? "");
            request.ContentLength = bytes.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            string result;
            try
            {
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    if (stream == null)
                    {
                        throw new BillingNotConfiguredException("Billing response is null");
                    }
                    using (var readStream = new StreamReader(stream))
                    {
                        result = readStream.ReadToEnd();
                    }
                }
            }
            catch (WebException)
            {
                request.Abort();
                throw;
            }

            if (string.IsNullOrEmpty(result))
            {
                throw new BillingNotConfiguredException("Billing response is null");
            }
            if (!result.StartsWith("{\"Message\":\"error"))
            {
                return result;
            }

            var @params = (parameters ?? Enumerable.Empty<Tuple<string, string>>()).Select(p => string.Format("{0}: {1}", p.Item1, p.Item2));
            var info = new { Method = method, PortalId = portalId, Params = string.Join(", ", @params) };
            if (result.Contains("{\"Message\":\"error: cannot find "))
            {
                throw new BillingNotFoundException(result, info);
            }
            throw new BillingException(result, info);
        }

        private string ToUrl(string s)
        {
            s = s.Trim();
            if (s.StartsWith("error", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }
            if (_test && !s.Contains("&DOTEST = 1"))
            {
                s += "&DOTEST=1";
            }
            return s;
        }
    }

    [Serializable]
    public class BillingException : Exception
    {
        public BillingException(string message, object debugInfo = null) : base(message + (debugInfo != null ? " Debug info: " + debugInfo : string.Empty))
        {
        }

        public BillingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BillingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class BillingNotFoundException : BillingException
    {
        public BillingNotFoundException(string message, object debugInfo = null) : base(message, debugInfo)
        {
        }

        protected BillingNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class BillingNotConfiguredException : BillingException
    {
        public BillingNotConfiguredException(string message, object debugInfo = null) : base(message, debugInfo)
        {
        }

        public BillingNotConfiguredException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BillingNotConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}