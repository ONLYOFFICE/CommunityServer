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
            testRec2 = new DirectRecipient("0017794f-aeb7-49a5-8817-9e870e02bd3f", null); 


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
                
                //ff0c4e13-1831-43c2-91ce-7b7beb56179b - Oliver Khan
                

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

                
                //for (int i = 0; i < 6; i++) subProvider.Subscribe(nAction, new Random().Next().ToString(), testRec2);
                objs = subProvider.GetSubscriptions(nAction, testRec2);
                Assert.AreNotEqual(0, objs.Count());
                CollectionAssert.AllItemsAreUnique(objs);

                
                var parents = recProvider.GetGroups(testRec2);
                Assert.AreNotEqual(0, parents.Count());
                otdel = parents.First();
                everyone = parents.Last();

                var objsGroup = subProvider.GetSubscriptions(nAction, otdel);
                CollectionAssert.AllItemsAreUnique(objsGroup);

                
                rndObj = String.Concat("TestObject#", new Random().Next().ToString());
                subProvider.Subscribe(nAction, rndObj, otdel);
                
                Assert.AreEqual(objsGroup.Count() + 1, subProvider.GetSubscriptions(nAction, otdel).Count());
                Assert.AreEqual(objs.Count() + 1, subProvider.GetSubscriptions(nAction, testRec2).Count());
                Assert.AreEqual(true, subProvider.IsSubscribed(nAction, testRec2, rndObj));

                
                rndObj2 = String.Concat("TestObject#", new Random().Next().ToString());
                objs = subProvider.GetSubscriptions(nAction, testRec2);
                subProvider.Subscribe(nAction, rndObj2, everyone);
                
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