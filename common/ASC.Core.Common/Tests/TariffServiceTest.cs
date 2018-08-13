/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
