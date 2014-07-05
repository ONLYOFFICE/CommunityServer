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
    using System.Threading;
    using ASC.Core.Users;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Diagnostics;

    [TestClass]
    public class UserManagerTest
    {
        [TestMethod]
        public void SearchUsers()
        {
            CoreContext.TenantManager.SetCurrentTenant(0);

            var users = CoreContext.UserManager.Search(null, EmployeeStatus.Active);
            Assert.AreEqual(0, users.Length);

            users = CoreContext.UserManager.Search("", EmployeeStatus.Active);
            Assert.AreEqual(0, users.Length);

            users = CoreContext.UserManager.Search("  ", EmployeeStatus.Active);
            Assert.AreEqual(0, users.Length);

            users = CoreContext.UserManager.Search("АбРаМсКй", EmployeeStatus.Active);
            Assert.AreEqual(0, users.Length);

            users = CoreContext.UserManager.Search("АбРаМсКий", EmployeeStatus.Active);
            Assert.AreEqual(0, users.Length);

            users = CoreContext.UserManager.Search("АбРаМсКий", EmployeeStatus.All);
            Assert.AreNotEqual(0, users.Length);

            users = CoreContext.UserManager.Search("иванов николай", EmployeeStatus.Active);
            Assert.AreNotEqual(0, users.Length);

            users = CoreContext.UserManager.Search("ведущий програм", EmployeeStatus.Active);
            Assert.AreNotEqual(0, users.Length);

            users = CoreContext.UserManager.Search("баннов лев", EmployeeStatus.Active, new Guid("613fc896-3ddd-4de1-a567-edbbc6cf1fc8"));
            Assert.AreNotEqual(0, users.Length);

            users = CoreContext.UserManager.Search("иванов николай", EmployeeStatus.Active, new Guid("613fc896-3ddd-4de1-a567-edbbc6cf1fc8"));
            Assert.AreEqual(0, users);
        }

        [TestMethod]
        public void DepartmentManagers()
        {
            CoreContext.TenantManager.SetCurrentTenant(1024);

            var deps = CoreContext.UserManager.GetDepartments();
            var users = CoreContext.UserManager.GetUsers();

            var g1 = deps[0];
            var ceo = users[0];
            var u1 = users[1];
            var u2 = users[2];

            
            var ceoTemp = CoreContext.UserManager.GetCompanyCEO();
            CoreContext.UserManager.SetCompanyCEO(ceo.ID);
            ceoTemp = CoreContext.UserManager.GetCompanyCEO();
            Assert.AreEqual(ceo, ceoTemp);

            Thread.Sleep(TimeSpan.FromSeconds(6));
            ceoTemp = CoreContext.UserManager.GetCompanyCEO();
            Assert.AreEqual(ceo, ceoTemp);

            
            CoreContext.UserManager.SetDepartmentManager(g1.ID, u1.ID);

            CoreContext.UserManager.SetDepartmentManager(g1.ID, u2.ID);
        }

        [TestMethod]
        public void UserGroupsPerformanceTest()
        {
            CoreContext.TenantManager.SetCurrentTenant(0);

            foreach (var u in CoreContext.UserManager.GetUsers())
            {
                var groups = CoreContext.GroupManager.GetGroups(Guid.Empty);
                foreach (var g in CoreContext.UserManager.GetUserGroups(u.ID))
                {
                    var manager = CoreContext.UserManager.GetUsers(CoreContext.UserManager.GetDepartmentManager(g.ID)).UserName;
                }
            }
            var stopwatch = Stopwatch.StartNew();
            foreach (var u in CoreContext.UserManager.GetUsers())
            {
                var groups = CoreContext.GroupManager.GetGroups(Guid.Empty);
                foreach (var g in CoreContext.UserManager.GetUserGroups(u.ID))
                {
                    var manager = CoreContext.UserManager.GetUsers(CoreContext.UserManager.GetDepartmentManager(g.ID)).UserName;
                }
            }
            stopwatch.Stop();

            stopwatch.Restart();
            var users = CoreContext.UserManager.GetUsersByGroup(Constants.GroupUser.ID);
            var visitors = CoreContext.UserManager.GetUsersByGroup(Constants.GroupVisitor.ID);
            var all = CoreContext.UserManager.GetUsers();
            stopwatch.Stop();
        }
    }
}
#endif
