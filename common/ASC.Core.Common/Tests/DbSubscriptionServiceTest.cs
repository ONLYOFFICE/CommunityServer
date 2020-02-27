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
    using System.Linq;
    using ASC.Core.Data;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DbSubscriptionServiceTest : DbBaseTest<DbSubscriptionService>
    {
        [ClassInitialize]
        public void ClearData()
        {
            Service.SetSubscriptionMethod(new SubscriptionMethod { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", RecipientId = "recipientId", });
            Service.RemoveSubscriptions(Tenant, "sourceId", "actionId");
            Service.RemoveSubscriptions(Tenants.Tenant.DEFAULT_TENANT, "Good", "Bad", "Ugly");
            Service.RemoveSubscriptions(this.Tenant, "Good", "Bad", "Ugly");
            Service.RemoveSubscriptions(this.Tenant, "Good", "Bad", "NotUgly");
            Service.SetSubscriptionMethod(new SubscriptionMethod { Tenant = this.Tenant, SourceId = "Good", ActionId = "Bad", RecipientId = "Rec1", Methods = null });
            Service.SetSubscriptionMethod(new SubscriptionMethod { Tenant = Tenants.Tenant.DEFAULT_TENANT, SourceId = "Good", ActionId = "Bad", RecipientId = "Rec1", Methods = null });
        }

        [TestMethod]
        public void SubscriptionMethod()
        {
            Service.SetSubscriptionMethod(new SubscriptionMethod { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", RecipientId = "recipientId", Methods = new[] { "email.sender" } });
            var m = Service.GetSubscriptionMethods(Tenant, "sourceId", "actionId", "recipientId").First();
            Assert.AreEqual(m.Tenant, Tenant);
            Assert.AreEqual(m.SourceId, "sourceId");
            Assert.AreEqual(m.ActionId, "actionId");
            Assert.AreEqual(m.RecipientId, "recipientId");
            CollectionAssert.AreEquivalent(new[] { "email.sender" }, m.Methods);

            Service.SetSubscriptionMethod(new SubscriptionMethod { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", RecipientId = "recipientId", Methods = null });
            Assert.IsNull(Service.GetSubscriptionMethods(Tenant, "sourceId", "actionId", "recipientId").FirstOrDefault());

            Service.SaveSubscription(new SubscriptionRecord { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", ObjectId = "object1Id", RecipientId = "recipientId", Subscribed = false });
            Service.SaveSubscription(new SubscriptionRecord { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", ObjectId = "object2Id", RecipientId = "recipientId", Subscribed = true });
            var subs = Service.GetSubscriptions(Tenant, "sourceId", "actionId", "recipientId", null);
            Assert.AreEqual(subs.Count(), 2);
            subs = Service.GetSubscriptions(Tenant, "sourceId", "actionId", null, "object1Id");
            Assert.AreEqual(subs.Count(), 1);
            subs = Service.GetSubscriptions(Tenant, "sourceId", "actionId", null, "object1Id");
            Assert.AreEqual(subs.Count(), 1);

            Service.RemoveSubscriptions(Tenant, "sourceId", "actionId");
            subs = Service.GetSubscriptions(Tenant, "sourceId", "actionId", "recipientId", null);
            Assert.AreEqual(0, subs.Count());

            Service.SaveSubscription(new SubscriptionRecord { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", ObjectId = "objectId", RecipientId = "recipientId", Subscribed = true });
            Service.RemoveSubscriptions(Tenant, "sourceId", "actionId", "objectId");
            subs = Service.GetSubscriptions(Tenant, "sourceId", "actionId", "recipientId", null);
            Assert.AreEqual(0, subs.Count());

            Service.SaveSubscription(new SubscriptionRecord { Tenant = Tenants.Tenant.DEFAULT_TENANT, SourceId = "Good", ActionId = "Bad", RecipientId = "Rec1", ObjectId = "Ugly", Subscribed = true });
            subs = Service.GetSubscriptions(this.Tenant, "Good", "Bad", null, "Ugly");
            Assert.AreEqual(subs.Count(), 1);

            Service.SaveSubscription(new SubscriptionRecord { Tenant = Tenants.Tenant.DEFAULT_TENANT, SourceId = "Good", ActionId = "Bad", RecipientId = "Rec2", ObjectId = "Ugly", Subscribed = true });
            subs = Service.GetSubscriptions(this.Tenant, "Good", "Bad", null, "Ugly");
            Assert.AreEqual(subs.Count(), 2);

            Service.SaveSubscription(new SubscriptionRecord { Tenant = this.Tenant, SourceId = "Good", ActionId = "Bad", RecipientId = "Rec2", ObjectId = "Ugly", Subscribed = true });
            subs = Service.GetSubscriptions(this.Tenant, "Good", "Bad", null, "Ugly");
            Assert.AreEqual(subs.Count(), 2);

            Service.SaveSubscription(new SubscriptionRecord { Tenant = this.Tenant, SourceId = "Good", ActionId = "Bad", RecipientId = "Rec3", ObjectId = "NotUgly", Subscribed = true });
            subs = Service.GetSubscriptions(this.Tenant, "Good", "Bad", null, "Ugly");
            Assert.AreEqual(subs.Count(), 2);

            Service.SetSubscriptionMethod(new SubscriptionMethod { Tenant = Tenants.Tenant.DEFAULT_TENANT, SourceId = "Good", ActionId = "Bad", RecipientId = "Rec1", Methods = new[] { "s1" } });
            Service.SetSubscriptionMethod(new SubscriptionMethod { Tenant = this.Tenant, SourceId = "Good", ActionId = "Bad", RecipientId = "Rec1", Methods = new[] { "s2" } });
            var methods = Service.GetSubscriptionMethods(this.Tenant, "Good", "Bad", "Rec1");
            Assert.AreEqual(methods.Count(), 1);
            CollectionAssert.AreEquivalent(new[] { "s2" }, methods.ToArray()[0].Methods);
        }
    }
}
#endif
