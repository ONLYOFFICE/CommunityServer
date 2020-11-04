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
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.Common.Logging;

namespace ASC.Core.Billing
{
    public class BillingClient : ClientBase<IService>, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC");
        private readonly bool _test;


        public BillingClient() : this(false)
        {
        }

        public BillingClient(bool test)
        {
            _test = test;
        }


        public PaymentLast GetLastPayment(string portalId)
        {
            var result = Request("GetLatestActiveResourceEx", portalId);
            var xelement = ToXElement("<root>" + result + "</root>");
            var dedicated = xelement.Element("dedicated-resource");
            var payment = xelement.Element("payment");

            if (!_test && GetValueString(payment.Element("status")) == "4")
            {
                throw new BillingException("Can not accept test payment.", new { PortalId = portalId });
            }

            var autorenewal = string.Empty;
            try
            {
                autorenewal = Request("GetLatestAvangateLicenseRecurringStatus", portalId);
            }
            catch (BillingException err)
            {
                Log.Debug(err); // ignore
            }

            return new PaymentLast
                {
                    ProductId = GetValueString(dedicated.Element("product-id")),
                    EndDate = GetValueDateTime(dedicated.Element("end-date")),
                    Autorenewal = "enabled".Equals(autorenewal, StringComparison.InvariantCultureIgnoreCase),
                };
        }

        public IEnumerable<PaymentInfo> GetPayments(string portalId, DateTime from, DateTime to)
        {
            string result;
            if (from == DateTime.MinValue && to == DateTime.MaxValue)
            {
                result = Request("GetPayments", portalId);
            }
            else
            {
                result = Request("GetListOfPaymentsByTimeSpan", portalId, Tuple.Create("StartDate", from.ToString("yyyy-MM-dd HH:mm:ss")), Tuple.Create("EndDate", to.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            var xelement = ToXElement(result);
            return xelement.Elements("payment").Select(ToPaymentInfo);
        }

        public IDictionary<string, Tuple<Uri, Uri>> GetPaymentUrls(string portalId, string[] products, string affiliateId = null, string campaign = null, string currency = null, string language = null, string customerId = null)
        {
            var urls = new Dictionary<string, Tuple<Uri, Uri>>();

            var additionalParameters = new List<Tuple<string, string>>(2) { Tuple.Create("PaymentSystemId", "1") };
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

            var parameters = products
                .Distinct()
                .Select(p => Tuple.Create("ProductId", p))
                .Concat(additionalParameters)
                .ToArray();

            //max 100 products
            var paymentUrls = ToXElement(Request("GetBatchPaymentSystemUrl", portalId, parameters))
                .Elements()
                .ToDictionary(e => e.Attribute("id").Value, e => ToUrl(e.Attribute("value").Value));

            var upgradeUrls = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(portalId))
            {
                try
                {
                    //max 100 products
                    upgradeUrls = ToXElement(Request("GetBatchPaymentSystemUpgradeUrl", portalId, parameters))
                        .Elements()
                        .ToDictionary(e => e.Attribute("id").Value, e => ToUrl(e.Attribute("value").Value));
                }
                catch (BillingException)
                {
                }
            }

            foreach (var p in products)
            {
                string url;
                var paymentUrl = (Uri)null;
                var upgradeUrl = (Uri)null;
                if (paymentUrls.TryGetValue(p, out url) && !string.IsNullOrEmpty(url))
                {
                    paymentUrl = new Uri(url);
                }
                if (upgradeUrls.TryGetValue(p, out url) && !string.IsNullOrEmpty(url))
                {
                    upgradeUrl = new Uri(url);
                }
                urls[p] = Tuple.Create(paymentUrl, upgradeUrl);
            }
            return urls;
        }

        public Invoice GetInvoice(string paymentId)
        {
            var result = Request("GetInvoice", null, Tuple.Create("PaymentId", paymentId));
            var xelement = ToXElement(result);
            return new Invoice
                {
                    Sale = GetValueString(xelement.Element("sale")),
                    Refund = GetValueString(xelement.Element("refund")),
                };
        }

        public IDictionary<string, IEnumerable<Tuple<string, decimal>>> GetProductPriceInfo(params string[] productIds)
        {
            if (productIds == null)
            {
                throw new ArgumentNullException("productIds");
            }

            var responce = Request("GetBatchAvangateProductPriceInfo", null, productIds.Select(pid => Tuple.Create("ProductId", pid)).ToArray());
            var xelement = ToXElement(responce);
            return productIds
                .Select(p =>
                    {
                        var prices = Enumerable.Empty<Tuple<string, decimal>>();
                        var product = xelement.XPathSelectElement(string.Format("/avangate-product/internal-id[text()=\"{0}\"]", p));
                        if (product != null)
                        {
                            prices = product.Parent.Element("prices").Elements("price-item")
                                            .Select(e => Tuple.Create(e.Element("currency").Value, decimal.Parse(e.Element("amount").Value)));
                        }
                        return new { ProductId = p, Prices = prices, };
                    })
                .ToDictionary(e => e.ProductId, e => e.Prices);
        }


        private string Request(string method, string portalId, params Tuple<string, string>[] parameters)
        {
            var request = new XElement(method);
            if (!string.IsNullOrEmpty(portalId))
            {
                request.Add(new XElement("PortalId", portalId));
            }
            request.Add(parameters.Select(p => new XElement(p.Item1, p.Item2)).ToArray());

            var responce = Channel.Request(new Message { Type = MessageType.Data, Content = request.ToString(SaveOptions.DisableFormatting), });
            if (responce.Content == null)
            {
                throw new BillingNotConfiguredException("Billing response is null");
            }
            if (responce.Type == MessageType.Data)
            {
                var result = responce.Content;
                var invalidChar = ((char)65279).ToString();
                return result.Contains(invalidChar) ? result.Replace(invalidChar, string.Empty) : result;
            }

            var @params = (parameters ?? Enumerable.Empty<Tuple<string, string>>()).Select(p => string.Format("{0}: {1}", p.Item1, p.Item2));
            var info = new { Method = method, PortalId = portalId, Params = string.Join(", ", @params) };
            if (responce.Content.Contains("error: cannot find "))
            {
                throw new BillingNotFoundException(responce.Content, info);
            }
            throw new BillingException(responce.Content, info);
        }

        private static XElement ToXElement(string xml)
        {
            return XElement.Parse(xml);
        }

        private static PaymentInfo ToPaymentInfo(XElement x)
        {
            return new PaymentInfo
                {
                    ID = (int)GetValueDecimal(x.Element("id")),
                    Status = (int)GetValueDecimal(x.Element("status")),
                    PaymentType = GetValueString(x.Element("reserved-str-2")),
                    ExchangeRate = (double)GetValueDecimal(x.Element("exch-rate")),
                    GrossSum = (double)GetValueDecimal(x.Element("gross-sum")),
                    Name = (GetValueString(x.Element("fname")) + " " + GetValueString(x.Element("lname"))).Trim(),
                    Email = GetValueString(x.Element("email")),
                    Date = GetValueDateTime(x.Element("payment-date")),
                    Price = GetValueDecimal(x.Element("price")),
                    Currency = GetValueString(x.Element("payment-currency")),
                    Method = GetValueString(x.Element("payment-method")),
                    CartId = GetValueString(x.Element("cart-id")),
                    ProductId = GetValueString(x.Element("product-ref")),
                    TenantID = GetValueString(x.Element("customer-id")),
                    Country = GetValueString(x.Element("country")),
                    DiscountSum = GetValueDecimal(x.Element("discount-sum"))
                };
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

        private static string GetValueString(XElement xelement)
        {
            return xelement != null ? HttpUtility.HtmlDecode(xelement.Value) : default(string);
        }

        private static DateTime GetValueDateTime(XElement xelement)
        {
            return xelement != null ?
                       DateTime.ParseExact(xelement.Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) :
                       default(DateTime);
        }

        private static Decimal GetValueDecimal(XElement xelement)
        {
            if (xelement == null || string.IsNullOrEmpty(xelement.Value))
            {
                return default(Decimal);
            }
            var sep = CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator;
            decimal value;
            return Decimal.TryParse(xelement.Value.Replace(".", sep).Replace(",", sep), NumberStyles.Currency, CultureInfo.InvariantCulture, out value) ? value : default(Decimal);
        }

        void IDisposable.Dispose()
        {
            try
            {
                Close();
            }
            catch (CommunicationException)
            {
                Abort();
            }
            catch (TimeoutException)
            {
                Abort();
            }
            catch (Exception)
            {
                Abort();
                throw;
            }
        }
    }


    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        Message Request(Message message);
    }

    [DataContract(Name = "Message", Namespace = "http://schemas.datacontract.org/2004/07/BillingService")]
    [Serializable]
    public class Message
    {
        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public MessageType Type { get; set; }
    }

    [DataContract(Name = "MessageType", Namespace = "http://schemas.datacontract.org/2004/07/BillingService")]
    public enum MessageType
    {
        [EnumMember]
        Undefined = 0,

        [EnumMember]
        Data = 1,

        [EnumMember]
        Error = 2,
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