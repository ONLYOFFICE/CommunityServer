/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

#if DEBUG
namespace ASC.Core.Common.Tests
{
    using System;
    using System.Linq;
    using ASC.Core.Billing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var p = billingClient.GetLastPayment("3b0d4e19-f539-4829-b090-269ce12e0a96");
            Assert.AreEqual(p.ProductId, "1");
            Assert.AreEqual(p.StartDate, new DateTime(2012, 4, 8, 13, 36, 30));
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
            var result = billingClient.GetPaymentUrls("0", new[] { "78", "79", "80", "0", "-2" });
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
    }
}
#endif
