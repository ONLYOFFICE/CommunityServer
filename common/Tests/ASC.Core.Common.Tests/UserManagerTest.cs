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
            CoreContext.TenantManager.SetCurrentTenant(1);

            var users = CoreContext.UserManager.Search(null, EmployeeStatus.Active);
            Assert.AreEqual(0, users.Length);

            users = CoreContext.UserManager.Search("", EmployeeStatus.Active);
            Assert.AreEqual(0, users.Length);

            users = CoreContext.UserManager.Search("  ", EmployeeStatus.Active);
            Assert.AreEqual(0, users.Length);

            users = CoreContext.UserManager.Search("АбРаМсКй", EmployeeStatus.Active);
            Assert.AreEqual(0, users.Length);

            users = CoreContext.UserManager.Search("АбРаМсКий", EmployeeStatus.Active);
            Assert.AreEqual(0, users.Length);//Абрамский уволился

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
            CoreContext.TenantManager.SetCurrentTenant(1);

            var deps = CoreContext.UserManager.GetDepartments();
            var users = CoreContext.UserManager.GetUsers();

            var g1 = deps[0];
            var ceo = users[0];
            var u1 = users[1];
            var u2 = users[2];

            //проверка кэша ceo
            var ceoTemp = CoreContext.UserManager.GetCompanyCEO();
            CoreContext.UserManager.SetCompanyCEO(ceo.ID);
            ceoTemp = CoreContext.UserManager.GetCompanyCEO();
            Assert.AreEqual(ceo, ceoTemp);

            Thread.Sleep(TimeSpan.FromSeconds(6));
            ceoTemp = CoreContext.UserManager.GetCompanyCEO();
            Assert.AreEqual(ceo, ceoTemp);

            //установка манагеров
            CoreContext.UserManager.SetDepartmentManager(g1.ID, u1.ID);

            CoreContext.UserManager.SetDepartmentManager(g1.ID, u2.ID);
        }

        [TestMethod]
        public void UserGroupsPerformanceTest()
        {
            CoreContext.TenantManager.SetCurrentTenant(1);

            foreach (var u in CoreContext.UserManager.GetUsers())
            {
                var groups = CoreContext.UserManager.GetGroups(Guid.Empty);
                Assert.IsNotNull(groups);
                foreach (var g in CoreContext.UserManager.GetUserGroups(u.ID))
                {
                    var manager = CoreContext.UserManager.GetUsers(CoreContext.UserManager.GetDepartmentManager(g.ID)).UserName;
                }
            }
            var stopwatch = Stopwatch.StartNew();
            foreach (var u in CoreContext.UserManager.GetUsers())
            {
                var groups = CoreContext.UserManager.GetGroups(Guid.Empty);
                Assert.IsNotNull(groups);
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
            Assert.IsNotNull(users);
            Assert.IsNotNull(visitors);
            Assert.IsNotNull(all);
            stopwatch.Stop();
        }
    }
}
#endif
