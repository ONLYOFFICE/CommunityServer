/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
    using System.Configuration;
    using System.Linq;
    using ASC.Core.Caching;
    using ASC.Core.Data;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CachedSubscriptionServiceTest
    {
        private readonly ISubscriptionService service;


        public CachedSubscriptionServiceTest()
        {
            service = new CachedSubscriptionService(new DbSubscriptionService(ConfigurationManager.ConnectionStrings["core"]));
        }


        [ClassInitialize]
        public void ClearData()
        {
            service.RemoveSubscriptions(2024, "sourceId2", "actionId2");
            service.RemoveSubscriptions(2024, "sourceId3", "actionId3", "objectId5");
            service.RemoveSubscriptions(2024, "sourceId1", "actionId1", "objectId1");

            var sm1 = new SubscriptionMethod { Tenant = 2024, ActionId = "actionId11", SourceId = "sourceId11", RecipientId = "recipientId11", };
            service.SetSubscriptionMethod(sm1);

            var sm2 = new SubscriptionMethod { Tenant = 2024, ActionId = "actionId22", SourceId = "sourceId22", RecipientId = "recipientId22", };
            service.SetSubscriptionMethod(sm2);
        }

        [TestMethod]
        public void CachedSubscriptionMethod()
        {
            var sb1 = new SubscriptionRecord { Tenant = 2024, ActionId = "actionId1", SourceId = "sourceId1", ObjectId = "objectId1", RecipientId = "recipientId1", Subscribed = false };
            service.SaveSubscription(sb1);

            var sb2 = new SubscriptionRecord { Tenant = 2024, ActionId = "actionId2", SourceId = "sourceId2", ObjectId = "objectId2", RecipientId = "recipientId2", Subscribed = false };
            service.SaveSubscription(sb2);

            var sb3 = new SubscriptionRecord { Tenant = 2024, ActionId = "actionId2", SourceId = "sourceId2", ObjectId = "objectId3", RecipientId = "recipientId3", Subscribed = false };
            service.SaveSubscription(sb3);

            var sb4 = new SubscriptionRecord { Tenant = 2024, ActionId = "actionId2", SourceId = "sourceId2", ObjectId = "", RecipientId = "recipientId4", Subscribed = false };
            service.SaveSubscription(sb4);

            var subscriptions = service.GetSubscriptions(2024, "sourceId1", "actionId1", "recipientId1", "objectId1");
            Assert.AreEqual(subscriptions.Count(), 1);

            subscriptions = service.GetSubscriptions(2024, "sourceId1", "actionId1", null, "objectId1");
            Assert.AreEqual(subscriptions.Count(), 1);

            subscriptions = service.GetSubscriptions(2024, "sourceId1", "actionId1", null, null);
            Assert.AreEqual(subscriptions.Count(), 0);

            subscriptions = service.GetSubscriptions(2024, "sourceId2", "actionId2");
            Assert.AreEqual(subscriptions.Count(), 3);

            var subscription = service.GetSubscription(2024, "sourceId2", "actionId2", "recipientId3", "objectId3");
            CompareSubscriptions(sb3, subscription);

            var sb5 = new SubscriptionRecord { Tenant = 2024, ActionId = "actionId3", SourceId = "sourceId3", ObjectId = "objectId5", RecipientId = "recipientId5", Subscribed = false };

            subscription = service.GetSubscription(2024, "sourceId3", "actionId3", "recipientId5", "objectId5");
            Assert.IsNull(subscription);

            service.SaveSubscription(sb5);

            subscription = service.GetSubscription(2024, "sourceId3", "actionId3", "recipientId5", "objectId5");
            CompareSubscriptions(sb5, subscription);

            service.RemoveSubscriptions(2024, "sourceId2", "actionId2");

            subscriptions = service.GetSubscriptions(2024, "sourceId2", "actionId2");
            Assert.AreEqual(0, subscriptions.Count());

            service.RemoveSubscriptions(2024, "sourceId3", "actionId3", "objectId5");
            service.RemoveSubscriptions(2024, "sourceId1", "actionId1", "objectId1");

            subscription = service.GetSubscription(2024, "sourceId3", "actionId3", "recipientId5", "objectId5");
            Assert.IsNull(subscription);

            subscription = service.GetSubscription(2024, "sourceId1", "actionId1", "recipientId1", "objectId1");
            Assert.IsNull(subscription);

            var sm1 = new SubscriptionMethod { Tenant = 2024, ActionId = "actionId11", SourceId = "sourceId11", RecipientId = "recipientId11", Methods = new string[] { "1", "2" } };
            service.SetSubscriptionMethod(sm1);

            var sm2 = new SubscriptionMethod { Tenant = 2024, ActionId = "actionId22", SourceId = "sourceId22", RecipientId = "recipientId22", Methods = new string[] { "3", "4" } };
            service.SetSubscriptionMethod(sm2);

            var methods = service.GetSubscriptionMethods(2024, "sourceId11", "actionId11", "recipientId11");
            Assert.AreEqual(methods.Count(), 1);
            CompareSubscriptionMethods(methods.ElementAt(0), sm1);

            methods = service.GetSubscriptionMethods(2024, "sourceId22", "actionId22", "recipientId22");
            Assert.AreEqual(methods.Count(), 1);
            CompareSubscriptionMethods(methods.ElementAt(0), sm2);

            sm2.Methods = null;

            service.SetSubscriptionMethod(sm2);

            methods = service.GetSubscriptionMethods(2024, "sourceId22", "actionId22", "recipientId11");
            Assert.AreEqual(0, methods.Count());

            sm1.Methods = null;

            service.SetSubscriptionMethod(sm1);

            methods = service.GetSubscriptionMethods(2024, "sourceId22", "actionId22", "recipientId22");
            Assert.AreEqual(0, methods.Count());
        }

        private void CompareSubscriptions(SubscriptionRecord sb1, SubscriptionRecord sb2)
        {
            Assert.AreEqual(sb1.Tenant, sb2.Tenant);
            Assert.AreEqual(sb1.ActionId, sb2.ActionId);
            Assert.AreEqual(sb1.SourceId, sb2.SourceId);
            Assert.AreEqual(sb1.RecipientId, sb2.RecipientId);
            Assert.AreEqual(sb1.ObjectId, sb1.ObjectId);
            Assert.AreEqual(sb1.Subscribed, sb2.Subscribed);
        }

        private void CompareSubscriptionMethods(SubscriptionMethod sm1, SubscriptionMethod sm2)
        {
            Assert.AreEqual(sm1.Tenant, sm2.Tenant);
            Assert.AreEqual(sm1.ActionId, sm2.ActionId);
            Assert.AreEqual(sm1.SourceId, sm2.SourceId);
            Assert.AreEqual(sm1.RecipientId, sm2.RecipientId);
            CollectionAssert.AreEqual(sm1.Methods, sm2.Methods);
        }
    }
}
#endif