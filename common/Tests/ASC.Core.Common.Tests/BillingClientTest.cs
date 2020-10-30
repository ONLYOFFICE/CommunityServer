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


#if DEBUG
namespace ASC.Core.Common.Tests
{
    using Core.Billing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;

    [TestClass]
    public class BillingClientTest
    {
        private readonly BillingClient billingClient;


        public BillingClientTest()
        {
            billingClient = new BillingClient();
        }


        [TestMethod]
        public void GetLastPaymentTest()
        {
            var p = billingClient.GetLastPayment("208761");
            Assert.AreEqual(p.ProductId, "1");
            Assert.AreEqual(p.EndDate, new DateTime(2012, 5, 8, 13, 36, 30));
            Assert.IsFalse(p.Autorenewal);
        }

        [TestMethod]
        public void GetPaymentsTest()
        {
            var payments = billingClient.GetPayments("918", DateTime.MinValue, DateTime.MaxValue).ToList();
            Assert.AreEqual(10, payments.Count);
            Assert.AreEqual(payments[0].ProductId, "1");
            Assert.AreEqual(payments[0].CartId, "11806812");
            Assert.AreEqual(payments[0].Currency, "EUR");
            Assert.AreEqual(payments[0].Date, new DateTime(2012, 4, 8, 13, 36, 30));
            Assert.AreEqual(payments[0].Email, "digiredo@mac.com");
            Assert.AreEqual(payments[0].Method, "PayPal");
            Assert.AreEqual(payments[0].Name, "Erik van der Zijden");
            Assert.AreEqual(payments[0].Price, 37.5);
        }

        [TestMethod]
        public void ShoppingUriBatchTest()
        {
            var result = billingClient.GetPaymentUrls("55380i", new[] { "78", "79", "80", "107", "108" });
            Assert.AreEqual(5, result.Count);
            Assert.IsNotNull(result["12"].Item1);
            Assert.IsNotNull(result["13"].Item1);
            Assert.IsNotNull(result["14"].Item1);
            Assert.IsNull(result["0"].Item1);
            Assert.IsNull(result["-2"].Item1);

            Assert.IsNull(result["12"].Item2);
            Assert.IsNull(result["13"].Item2);
            Assert.IsNull(result["14"].Item2);
            Assert.IsNull(result["0"].Item2);
            Assert.IsNull(result["-2"].Item2);
        }

        [TestMethod]
        public void GetInvoiceTest()
        {
            var result = billingClient.GetInvoice("11806812");
            Assert.IsNotNull(result.Sale);
            Assert.IsNull(result.Refund);
        }

        [TestMethod]
        public void GetProductPriceInfoTest()
        {
            var result = billingClient.GetProductPriceInfo("36", "60", "131");
            Assert.IsNotNull(result);
        }
    }
}
#endif
