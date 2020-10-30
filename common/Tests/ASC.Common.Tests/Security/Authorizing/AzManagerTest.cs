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
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ASC.Common.Tests.Security.Authorizing
{
    [TestClass]
    public class AzManagerTest
    {
        [TestMethod]
        public void CollectInheritSubjects_Test()
        {
            AzManager azMan = new AzManager(Domain.RoleProvider, Domain.PermissionProvider);

            var subjects = new List<ISubject>(azMan.GetSubjects(Domain.roleNET, null, null));
            Assert.IsNotNull(subjects);
            CollectionAssert.Contains(subjects, Domain.roleNET);
            CollectionAssert.Contains(subjects, Domain.roleAVS);
            CollectionAssert.Contains(subjects, Constants.Everyone);
            Assert.AreEqual(3, subjects.Count);

            subjects = new List<ISubject>(azMan.GetSubjects(Domain.accountValery, null, null));
            Assert.IsNotNull(subjects);
            CollectionAssert.Contains(subjects, Domain.accountValery);
            CollectionAssert.Contains(subjects, Domain.roleNET);
            CollectionAssert.Contains(subjects, Domain.roleAVS);
            CollectionAssert.Contains(subjects, Constants.Everyone);
            CollectionAssert.Contains(subjects, Constants.User);
            Assert.AreEqual(5, subjects.Count);

            subjects = new List<ISubject>(azMan.GetSubjects(Domain.accountLev, null, null));
            Assert.IsNotNull(subjects);
            CollectionAssert.Contains(subjects, Domain.accountLev);
            CollectionAssert.Contains(subjects, Domain.roleAdministration);
            CollectionAssert.Contains(subjects, Domain.roleAVS);
            CollectionAssert.Contains(subjects, Domain.roleHR);
            CollectionAssert.Contains(subjects, Constants.Everyone);
            CollectionAssert.Contains(subjects, Constants.User);
            Assert.AreEqual(6, subjects.Count);

            subjects = new List<ISubject>(azMan.GetSubjects(Domain.accountAlient, null, null));
            Assert.IsNotNull(subjects);
            CollectionAssert.Contains(subjects, Domain.accountAlient);
            CollectionAssert.Contains(subjects, Constants.Everyone);
            CollectionAssert.Contains(subjects, Constants.User);
            Assert.AreEqual(3, subjects.Count);

            subjects = new List<ISubject>(azMan.GetSubjects(Domain.accountMessangerService, null, null));
            Assert.IsNotNull(subjects);
            CollectionAssert.Contains(subjects, Domain.accountMessangerService);
            CollectionAssert.Contains(subjects, Constants.Everyone);
            //CollectionAssert.Contains(subjects, Constants.Service);
            Assert.AreEqual(3, subjects.Count);
        }

        [TestMethod]
        public void GetAzManagerAcl()
        {

            AzManager azMan = new AzManager(Domain.RoleProvider, Domain.PermissionProvider);
            AzManager.AzManagerAcl acl = null;

            acl = azMan.GetAzManagerAcl(Constants.Admin, Domain.actionAddUser, null, null);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Constants.Everyone, Domain.actionAddUser, null, null);
            Assert.IsFalse(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Constants.Owner, Domain.actionAddUser, null, null);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Constants.Self, Domain.actionAddUser, null, null);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Constants.User, Domain.actionAddUser, null, null);
            Assert.IsFalse(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.roleAVS, Domain.actionAddUser, null, null);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.roleHR, Domain.actionAddUser, null, null);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.roleNET, Domain.actionAddUser, null, null);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.roleAdministration, Domain.actionAddUser, null, null);
            Assert.IsFalse(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountAlient, Domain.actionAddUser, null, null);
            Assert.IsFalse(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountAnton, Domain.actionAddUser, null, null);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountKat, Domain.actionAddUser, null, null);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountLev, Domain.actionAddUser, null, null);
            Assert.IsFalse(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountNik, Domain.actionAddUser, null, null);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountValery, Domain.actionAddUser, null, null);
            Assert.IsFalse(acl.IsAllow);
        }

        [TestMethod]
        public void GetAzManagerObjectAcl()
        {
            AzManager azMan = new AzManager(Domain.RoleProvider, Domain.PermissionProvider);
            AzManager.AzManagerAcl acl = null;

            var c1 = new Class1(1);
            var c2 = new Class1(2);
            var sop = new Class1SecurityProvider();

            var c1Id = new SecurityObjectId(c1.Id, typeof(Class1));
            var c2Id = new SecurityObjectId(c2.Id, typeof(Class1));

            Domain.PermissionProvider.SetObjectAcesInheritance(c1Id, false);
            Domain.PermissionProvider.SetObjectAcesInheritance(c2Id, false);
            Domain.PermissionProvider.AddAce(Constants.Owner, Domain.actionAddUser, c1Id, AceType.Allow);

            acl = azMan.GetAzManagerAcl(Domain.accountNik, Domain.actionAddUser, c1Id, sop);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountNik, Domain.actionAddUser, c2Id, sop);
            Assert.IsFalse(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountAnton, Domain.actionAddUser, c1Id, sop);
            Assert.IsFalse(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountAnton, Domain.actionAddUser, c2Id, sop);
            Assert.IsFalse(acl.IsAllow);

            Domain.PermissionProvider.SetObjectAcesInheritance(c2Id, true);

            acl = azMan.GetAzManagerAcl(Domain.accountNik, Domain.actionAddUser, c2Id, sop);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountAnton, Domain.actionAddUser, c1Id, sop);
            Assert.IsFalse(acl.IsAllow);

            Domain.PermissionProvider.SetObjectAcesInheritance(c1Id, true);

            acl = azMan.GetAzManagerAcl(Domain.accountNik, Domain.actionAddUser, c2Id, sop);
            Assert.IsTrue(acl.IsAllow);

            acl = azMan.GetAzManagerAcl(Domain.accountLev, Domain.actionAddUser, c2Id, sop);
            Assert.IsFalse(acl.IsAllow);
        }
    }
}
#endif