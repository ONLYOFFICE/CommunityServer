/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
    using Billing;
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
            Assert.AreEqual(p.StartDate, new DateTime(2012, 4, 8, 13, 36, 30));
            Assert.AreEqual(p.EndDate, new DateTime(2012, 5, 8, 13, 36, 30));
            Assert.IsFalse(p.Autorenewal);
        }

        [TestMethod]
        public void GetLastPaymentByEmail()
        {
            var arr = billingClient.GetLastPaymentByEmail("david@bluetigertech.com.au");
            var p = arr.ElementAt(0);
            Assert.AreEqual(p.ProductName, "1-30 users - Teamlab Server Enterprise Edition One Year Subscription");
            Assert.AreEqual(p.StartDate, new DateTime(2014, 3, 28, 13, 4, 24));
            Assert.AreEqual(p.PaymentDate, new DateTime(2014, 3, 28, 13, 4, 24));
            Assert.AreEqual(p.EndDate, new DateTime(2015, 3, 28, 13, 4, 24));
            Assert.IsTrue(p.SAAS);
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
        public void GetPaymentUrlTest()
        {
            var result = billingClient.GetPaymentUrl("49b9c8c2-70d0-4e16-bb1b-a5106af81e52", "61", "en-EN");
            Assert.AreNotEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetInvoiceTest()
        {
            var result = billingClient.GetInvoice("11806812");
            Assert.IsNotNull(result.Sale);
            Assert.IsNull(result.Refund);
        }

        [TestMethod]
        public void GetPaymentOfficeTest()
        {
            var p = billingClient.GetPaymentOffice("144448");
            Assert.AreEqual(p.Key1, "144448");
            Assert.AreEqual(p.Key2, "0a589014-2feb-4022-ab7d-ffb7ee5eb135");
            Assert.AreEqual(p.StartDate, new DateTime(2013, 2, 4, 11, 52, 41));
            Assert.AreEqual(p.EndDate, new DateTime(2013, 3, 4, 11, 52, 41));
            Assert.AreEqual(p.UsersCount, 20);
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
