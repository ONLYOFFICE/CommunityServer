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