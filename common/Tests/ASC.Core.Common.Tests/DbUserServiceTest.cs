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
    using System.Linq;

    using ASC.Core.Data;
    using ASC.Core.Users;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DbUserServiceTest : DbBaseTest<DbUserService>
    {
        [ClassInitialize]
        public void ClearData()
        {
            Service.GetUsers(Tenant, default(DateTime))
                .ToList()
                .ForEach(u => Service.RemoveUser(Tenant, u.Value.ID, true));

            Service.GetGroups(Tenant, default(DateTime))
                .ToList()
                .ForEach(g => Service.RemoveGroup(Tenant, g.Value.Id, true));

            Service.GetUserGroupRefs(Tenant, default(DateTime))
                .ToList()
                .ForEach(r => Service.RemoveUserGroupRef(Tenant, r.Value.UserId, r.Value.GroupId, r.Value.RefType, true));
        }

        [TestMethod]
        public void CRUDUser()
        {
            var user1 = new UserInfo
            {
                UserName = "username1",
                FirstName = "first name",
                LastName = "last name",
                BirthDate = new DateTime(2011, 01, 01, 7, 8, 9),
                Sex = true,
                Email = "email@mail.ru",
                Location = "location",
                Notes = "notes",
                Status = EmployeeStatus.Active,
                Title = "title",
                WorkFromDate = new DateTime(2011, 01, 01, 7, 8, 9),
                TerminatedDate = new DateTime(2011, 01, 01, 7, 8, 9),
                CultureName = "de-DE",
            };
            user1.ContactsFromString("contacts");
            user1 = Service.SaveUser(Tenant, user1);
            CompareUsers(user1, Service.GetUser(Tenant, user1.ID));

            var user2 = new UserInfo
            {
                UserName = "username2",
                FirstName = "first name",
                LastName = "last name",
            };

            user2 = Service.SaveUser(Tenant, user2);
            user2 = Service.SaveUser(Tenant, user2);
            CompareUsers(user2, Service.GetUser(Tenant, user2.ID));

            var duplicateUsername = false;
            var user3 = new UserInfo
            {
                UserName = "username3",
                FirstName = "first name",
                LastName = "last name",
            };
            var user4 = new UserInfo
            {
                UserName = "username3",
                FirstName = "first name",
                LastName = "last name",
            };
            try
            {
                user3 = Service.SaveUser(Tenant, user3);
                user4 = Service.SaveUser(Tenant, user4);
            }
            catch (ArgumentOutOfRangeException)
            {
                duplicateUsername = true;
            }
            Assert.IsTrue(duplicateUsername);

            Service.RemoveUser(Tenant, user3.ID, false);
            user4 = Service.SaveUser(Tenant, user4);
            Service.RemoveUser(Tenant, user3.ID, true);
            Service.RemoveUser(Tenant, user4.ID, true);

            var users = Service.GetUsers(Tenant, new DateTime(1900, 1, 1)).Values;
            CollectionAssert.AreEquivalent(new[] { user1, user2 }, users.ToList());

            Service.RemoveUser(Tenant, user2.ID, true);

            Service.SetUserPhoto(Tenant, user1.ID, null);
            Assert.AreEqual(0, Service.GetUserPhoto(Tenant, user1.ID).Count());

            Service.SetUserPhoto(Tenant, user1.ID, new byte[0]);
            Assert.AreEqual(0, Service.GetUserPhoto(Tenant, user1.ID).Count());

            Service.SetUserPhoto(Tenant, user1.ID, new byte[] { 1, 2, 3 });
            CollectionAssert.AreEquivalent(new byte[] { 1, 2, 3 }, Service.GetUserPhoto(Tenant, user1.ID));

            //var password = "password";
            //Service.SetUserPassword(Tenant, user1.ID, password);
            //Assert.AreEqual(password, Service.GetUserPassword(Tenant, user1.ID));

            //CompareUsers(user1, Service.GetUser(Tenant, user1.Email, Hasher.Base64Hash(password, HashAlg.SHA256)));

            //Service.RemoveUser(Tenant, user1.ID);
            //Assert.IsTrue(Service.GetUser(Tenant, user1.ID).Removed);

            //Service.RemoveUser(Tenant, user1.ID, true);

            //Assert.AreEqual(0, Service.GetUserPhoto(Tenant, user1.ID).Count());
            //Assert.IsNull(Service.GetUserPassword(Tenant, user1.ID));
        }

        [TestMethod]
        public void CRUDGroup()
        {
            var g1 = new Group
            {
                Name = "group1",
                CategoryId = Guid.NewGuid(),
            };
            g1 = Service.SaveGroup(Tenant, g1);
            CompareGroups(g1, Service.GetGroup(Tenant, g1.Id));

            var now = DateTime.UtcNow;

            var g2 = new Group
            {
                Name = "group2",
                ParentId = g1.Id,
            };
            g2 = Service.SaveGroup(Tenant, g2);
            CompareGroups(g2, Service.GetGroup(Tenant, g2.Id));

            Service.RemoveGroup(Tenant, g1.Id);
            g1 = Service.GetGroup(Tenant, g1.Id);
            g2 = Service.GetGroup(Tenant, g2.Id);
            Assert.IsTrue(g1.Removed);
            Assert.IsTrue(g2.Removed);

            CollectionAssert.AreEquivalent(Service.GetGroups(Tenant, now).Values.ToList(), new[] { g1, g2 });

            Service.RemoveGroup(Tenant, g1.Id, true);
            Assert.AreEqual(0, Service.GetGroups(Tenant, new DateTime(1900, 1, 1)).Count());
        }

        [TestMethod]
        public void CRUDUserGroupRef()
        {
            Service.SaveUserGroupRef(Tenant, new UserGroupRef { UserId = Guid.Empty, GroupId = Guid.Empty, RefType = UserGroupRefType.Manager });

            Service.RemoveUserGroupRef(Tenant, Guid.Empty, Guid.Empty, UserGroupRefType.Manager);
            Assert.IsTrue(Service.GetUserGroupRefs(Tenant, new DateTime(1900, 1, 1)).First().Value.Removed);

            Service.RemoveUserGroupRef(Tenant, Guid.Empty, Guid.Empty, UserGroupRefType.Manager, true);
            Assert.AreEqual(0, Service.GetUserGroupRefs(Tenant, new DateTime(1900, 1, 1)).Count());

            var gu1 = Service.SaveUserGroupRef(Tenant, new UserGroupRef { UserId = Guid.Empty, GroupId = Guid.Empty, RefType = UserGroupRefType.Manager });
            var gu2 = Service.SaveUserGroupRef(Tenant, new UserGroupRef { UserId = new Guid("00000000-0000-0000-0000-000000000001"), GroupId = new Guid("00000000-0000-0000-0000-000000000002"), RefType = UserGroupRefType.Manager });
            CollectionAssert.AreEquivalent(new[] { gu1, gu2 }, Service.GetUserGroupRefs(Tenant, new DateTime(1900, 1, 1)).Values.ToList());
            Service.RemoveUserGroupRef(Tenant, gu1.UserId, gu1.GroupId, UserGroupRefType.Manager, true);
            Service.RemoveUserGroupRef(Tenant, gu2.UserId, gu2.GroupId, UserGroupRefType.Manager, true);


            var u = Service.SaveUser(Tenant, new UserInfo { UserName = "username", LastName = "lastname", FirstName = "firstname" });
            Service.SaveUserGroupRef(Tenant, new UserGroupRef { UserId = u.ID, GroupId = Guid.Empty, RefType = UserGroupRefType.Manager });

            Service.RemoveUser(Tenant, u.ID);
            Assert.IsTrue(Service.GetUserGroupRefs(Tenant, new DateTime(1900, 1, 1)).First().Value.Removed);

            Service.RemoveUser(Tenant, u.ID, true);
            Assert.AreEqual(0, Service.GetUserGroupRefs(Tenant, new DateTime(1900, 1, 1)).Count());


            var g = Service.SaveGroup(Tenant, new Group { Name = "group1" });
            u = Service.SaveUser(Tenant, new UserInfo { UserName = "username", LastName = "lastname", FirstName = "firstname" });
            Service.SaveUserGroupRef(Tenant, new UserGroupRef { UserId = u.ID, GroupId = g.Id, RefType = UserGroupRefType.Manager });
            u = Service.GetUser(Tenant, u.ID);

            Service.RemoveGroup(Tenant, g.Id);
            Assert.IsTrue(Service.GetUserGroupRefs(Tenant, new DateTime(1900, 1, 1)).First().Value.Removed);

            Service.SaveUser(Tenant, u);
            Service.RemoveGroup(Tenant, g.Id, true);
            Assert.AreEqual(0, Service.GetUserGroupRefs(Tenant, new DateTime(1900, 1, 1)).Count());
        }

        private void CompareUsers(UserInfo u1, UserInfo u2)
        {
            Assert.AreEqual(u1.ID, u2.ID);
            Assert.AreEqual(u1.BirthDate, u2.BirthDate);
            Assert.AreEqual(u1.ContactsToString(), u2.ContactsToString());
            Assert.AreEqual(u1.Email, u2.Email);
            Assert.AreEqual(u1.FirstName, u2.FirstName);
            Assert.AreEqual(u1.LastName, u2.LastName);
            Assert.AreEqual(u1.Location, u2.Location);
            Assert.AreEqual(u1.Notes, u2.Notes);
            Assert.AreEqual(u1.Sex, u2.Sex);
            Assert.AreEqual(u1.Status, u2.Status);
            Assert.AreEqual(u1.Title, u2.Title);
            Assert.AreEqual(u1.UserName, u2.UserName);
            Assert.AreEqual(u1.WorkFromDate, u2.WorkFromDate);
            Assert.AreEqual(u1.TerminatedDate, u2.TerminatedDate);
            Assert.AreEqual(u1.Removed, u2.Removed);
            Assert.AreEqual(u1.CultureName, u2.CultureName);
        }

        private void CompareGroups(Group g1, Group g2)
        {
            Assert.AreEqual(g1.CategoryId, g2.CategoryId);
            Assert.AreEqual(g1.Id, g2.Id);
            Assert.AreEqual(g1.Name, g2.Name);
            Assert.AreEqual(g1.ParentId, g2.ParentId);
            Assert.AreEqual(g1.Removed, g2.Removed);
        }
    }
}
#endif
