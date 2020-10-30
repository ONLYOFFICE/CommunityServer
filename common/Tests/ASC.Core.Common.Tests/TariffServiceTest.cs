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
    using System;
    using System.Configuration;
    using ASC.Core.Billing;
    using ASC.Core.Data;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TariffServiceTest
    {
        private readonly ITariffService tariffService;


        public TariffServiceTest()
        {
            var cs = ConfigurationManager.ConnectionStrings["core"];
            tariffService = new TariffService(cs, new DbQuotaService(cs), new DbTenantService(cs));
        }


        [TestMethod]
        public void TestShoppingUriBatch()
        {
            var bc = new BillingClient(true);
            var result = bc.GetPaymentUrls("0", new[] { "12", "13", "14", "0", "-2" });
            Assert.AreEqual(5, result.Count);
        }

        [TestMethod]
        public void TestPaymentInfo()
        {
            var payments = tariffService.GetPayments(918, DateTime.MinValue, DateTime.MaxValue);
            Assert.IsNotNull(payments);
        }

        [TestMethod]
        public void TestTariff()
        {
            var tariff = tariffService.GetTariff(918);
            Assert.IsNotNull(tariff);
        }

        [TestMethod]
        public void TestSetTariff()
        {
            var duedate = DateTime.UtcNow.AddMonths(1);
            tariffService.SetTariff(0, new Tariff { QuotaId = -1, DueDate = DateTime.MaxValue });
            tariffService.SetTariff(0, new Tariff { QuotaId = -21, DueDate = duedate });
            tariffService.SetTariff(0, new Tariff { QuotaId = -21, DueDate = duedate });
            tariffService.SetTariff(0, new Tariff { QuotaId = -1, DueDate = DateTime.MaxValue });
        }

        [TestMethod]
        public void TestInvoice()
        {
            var payments = tariffService.GetPayments(918, DateTime.MinValue, DateTime.MaxValue);
            foreach (var p in payments)
            {
                var invoice = tariffService.GetInvoice(p.CartId);
                Assert.IsNotNull(invoice);
            }
        }
    }
}
#endif
