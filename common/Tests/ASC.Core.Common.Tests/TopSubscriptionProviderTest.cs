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