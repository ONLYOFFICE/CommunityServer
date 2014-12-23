/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
