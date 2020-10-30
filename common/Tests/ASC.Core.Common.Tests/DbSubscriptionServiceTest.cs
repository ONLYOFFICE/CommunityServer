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
