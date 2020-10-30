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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASC.Core.Notify;
using ASC.Notify.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ASC.Notify.Recipients;

namespace ASC.Core.Common.Tests
{
    [TestClass]
    class TopSubscriptionProviderTest
    {
        private TopSubscriptionProvider subProvider;
        private RecipientProviderImpl recProvider;
        private Tenants.Tenant tenant;
        private string sourceId;
        private string actionId;
        private string objectId;
        private string rndObj;
        private string rndObj2;
        private IRecipient everyone = new RecipientsGroup(String.Empty, String.Empty);
        private IRecipient otdel = new RecipientsGroup(String.Empty, String.Empty);
        private IRecipient testRec;
        private IRecipient testRec2;
        private NotifyAction nAction;

        [ClassInitialize]
        public void CreateProviders()
        {
            tenant = new Tenants.Tenant(0, "teamlab");
            sourceId = "6045b68c-2c2e-42db-9e53-c272e814c4ad";
            actionId = "NewCommentForTask";
            objectId = "Task_5946_457";
            nAction = new NotifyAction(actionId, actionId);
            testRec = new DirectRecipient("ff0c4e13-1831-43c2-91ce-7b7beb56179b", null); //Oliver Khan
            testRec2 = new DirectRecipient("0017794f-aeb7-49a5-8817-9e870e02bd3f", null); //Якутова Юлия


            recProvider = new RecipientProviderImpl();
            var directSubProvider = new DirectSubscriptionProvider(sourceId, CoreContext.SubscriptionManager, recProvider);
            subProvider = new TopSubscriptionProvider(recProvider, directSubProvider);
            CoreContext.TenantManager.SetCurrentTenant(tenant);
        }

        [TestMethod]
        public void TopSubProviderTest()
        {
            try
            {
                //0017794f-aeb7-49a5-8817-9e870e02bd3f - Якутова Юлия
                //ff0c4e13-1831-43c2-91ce-7b7beb56179b - Oliver Khan
                //cc8eea30-1260-427e-83c4-ff9e9680edba - Отдел интернет-приложений!!!;)

                IRecipient[] res;

                //GetRecipients
                res = subProvider.GetRecipients(nAction, objectId);
                var cnt = res.Count();

                //Subscribe
                subProvider.Subscribe(nAction, objectId, testRec);
                res = subProvider.GetRecipients(nAction, objectId);
                Assert.AreEqual(cnt + 1, res.Count());

                //UnSubscribe
                subProvider.UnSubscribe(nAction, testRec);
                res = subProvider.GetRecipients(nAction, objectId);
                Assert.AreEqual(cnt, res.Count());

                String[] objs;

                //GetSubscribtions

                //Получаем подписки юзера
                //for (int i = 0; i < 6; i++) subProvider.Subscribe(nAction, new Random().Next().ToString(), testRec2);
                objs = subProvider.GetSubscriptions(nAction, testRec2);
                Assert.AreNotEqual(0, objs.Count());
                CollectionAssert.AllItemsAreUnique(objs);

                //Получаем список групп к которым он принадлежит
                var parents = recProvider.GetGroups(testRec2);
                Assert.AreNotEqual(0, parents.Count());
                otdel = parents.First();
                everyone = parents.Last();

                var objsGroup = subProvider.GetSubscriptions(nAction, otdel);
                CollectionAssert.AllItemsAreUnique(objsGroup);

                //Подписываем весь отдел на объект
                rndObj = String.Concat("TestObject#", new Random().Next().ToString());
                subProvider.Subscribe(nAction, rndObj, otdel);
                //Проверяем подписался ли юзер вместе со всем отделом двумя способами.
                Assert.AreEqual(objsGroup.Count() + 1, subProvider.GetSubscriptions(nAction, otdel).Count());
                Assert.AreEqual(objs.Count() + 1, subProvider.GetSubscriptions(nAction, testRec2).Count());
                Assert.AreEqual(true, subProvider.IsSubscribed(nAction, testRec2, rndObj));

                //Подписываем Everybody
                rndObj2 = String.Concat("TestObject#", new Random().Next().ToString());
                objs = subProvider.GetSubscriptions(nAction, testRec2);
                subProvider.Subscribe(nAction, rndObj2, everyone);
                //Проверяем подписался ли user двумя способами.
                Assert.AreEqual(objs.Count() + 1, subProvider.GetSubscriptions(nAction, testRec2).Count());
                Assert.AreEqual(true, subProvider.IsSubscribed(nAction, testRec2, rndObj2));

            }
            finally
            {
                subProvider.UnSubscribe(nAction, objectId, testRec);
                subProvider.UnSubscribe(nAction, rndObj, otdel);
                subProvider.UnSubscribe(nAction, rndObj2, everyone);
            }
        }
    }
}
#endif