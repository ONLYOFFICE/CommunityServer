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
    using System.Configuration;
    using System.Linq;

    using ASC.Core.Data;
    using ASC.Core.Tenants;
    using ASC.Core.Users;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DbTenantServiceTest : DbBaseTest<DbTenantService>
    {
        private readonly DbUserService userService;


        public DbTenantServiceTest()
        {
            userService = new DbUserService(ConfigurationManager.ConnectionStrings["core"]);
        }


        //[ClassInitialize]
        public void ClearData()
        {
            foreach (var t in Service.GetTenants(default(DateTime)))
            {
                if (t.Name == "nct5nct5" || t.Name == "google5" || t.TenantId == Tenant) Service.RemoveTenant(t.TenantId);
            }
            foreach (var u in userService.GetUsers(Tenant, default(DateTime)))
            {
                userService.RemoveUser(Tenant, u.Value.ID, true);
            }
            Service.SetTenantSettings(Tenant, "key1", null);
            Service.SetTenantSettings(Tenant, "key2", null);
            Service.SetTenantSettings(Tenant, "key3", null);
        }

        [TestMethod]
        public void TenantTest()
        {
            var t1 = new Tenant("nct5nct5");
            var t2 = new Tenant("google5") { MappedDomain = "domain" };
            t2.TrustedDomains.Add(null);
            t2.TrustedDomains.Add("microsoft");

            Service.SaveTenant(t1);
            Service.SaveTenant(t2);

            var tenants = Service.GetTenants(default(DateTime));
            CollectionAssert.Contains(tenants.ToList(), t1);
            CollectionAssert.Contains(tenants.ToList(), t2);

            var t = Service.GetTenant(t1.TenantId);
            CompareTenants(t, t1);

            t1.Version = 2;
            Service.SaveTenant(t1);
            t = Service.GetTenant(t1.TenantId);
            CompareTenants(t, t1);

            Assert.AreEqual(0, t.TrustedDomains.Count);
            CollectionAssert.AreEquivalent(new[] { "microsoft" }, Service.GetTenant(t2.TenantId).TrustedDomains);

            Service.RemoveTenant(t1.TenantId);
            Assert.IsNull(Service.GetTenant(t1.TenantId));

            Service.RemoveTenant(t2.TenantId);
            Assert.IsNull(Service.GetTenant(t2.MappedDomain));


            t1 = new Tenant("nct5nct5");
            Service.SaveTenant(t1);

            var user = new UserInfo
            {
                UserName = "username",
                FirstName = "first name",
                LastName = "last name",
                Email = "user@mail.ru"
            };
            userService.SaveUser(t1.TenantId, user);

            //var password = "password";
            //userService.SetUserPassword(t1.TenantId, user.ID, password);

            //tenants = Service.GetTenants(user.Email, Hasher.Base64Hash(password, HashAlg.SHA256));
            //CollectionAssert.AreEqual(new[] { t1 }, tenants.ToList());

            //tenants = Service.GetTenants(user.Email, null);
            //CollectionAssert.Contains(tenants.ToList(), t1);

            //Service.RemoveTenant(t1.TenantId);
            //tenants = Service.GetTenants(user.Email, Hasher.Base64Hash(password, HashAlg.SHA256));
            //Assert.AreEqual(0, tenants.Count());

            userService.RemoveUser(Tenant, user.ID, true);
        }

        [TestMethod]
        public void ValidateDomain()
        {
            ValidateDomain("12345", typeof(TenantTooShortException));
            ValidateDomain("123456", null);
            ValidateDomain("трала   лалала", typeof(TenantIncorrectCharsException));
            ValidateDomain("abc.defg", typeof(TenantIncorrectCharsException));
            ValidateDomain("abcdef", null);

            var t = new Tenant("nct5nct5") { MappedDomain = "nct5nct6" };
            t = Service.SaveTenant(t);
            ValidateDomain("nct5nct5", typeof(TenantAlreadyExistsException));
            ValidateDomain("NCT5NCT5", typeof(TenantAlreadyExistsException));
            ValidateDomain("nct5nct6", typeof(TenantAlreadyExistsException));
            ValidateDomain("NCT5NCT6", typeof(TenantAlreadyExistsException));
            ValidateDomain("nct5nct7", null);
            try
            {
                Service.ValidateDomain("nct5nct5");
            }
            catch (TenantAlreadyExistsException e)
            {
                CollectionAssert.AreEquivalent(e.ExistsTenants.ToList(), new[] { "nct5nct5", "nct5nct6" });
            }

            t.MappedDomain = "abc.defg";
            t = Service.SaveTenant(t);
            Service.RemoveTenant(Tenant);
        }

        [TestMethod]
        public void TenantSettings()
        {
            Service.SetTenantSettings(Tenant, "key1", null);
            Assert.IsNull(Service.GetTenantSettings(Tenant, "key1"));

            Service.SetTenantSettings(Tenant, "key2", new byte[] { });
            Assert.IsNull(Service.GetTenantSettings(Tenant, "key2"));

            var data = new byte[] { 0 };
            Service.SetTenantSettings(Tenant, "key3", data);
            CollectionAssert.AreEquivalent(data, Service.GetTenantSettings(Tenant, "key3"));

            Service.SetTenantSettings(Tenant, "key3", null);
            Assert.IsNull(Service.GetTenantSettings(Tenant, "key3"));
        }

        private void CompareTenants(Tenant t1, Tenant t2)
        {
            Assert.AreEqual(t1.Language, t2.Language);
            Assert.AreEqual(t1.MappedDomain, t2.MappedDomain);
            Assert.AreEqual(t1.Name, t2.Name);
            Assert.AreEqual(t1.OwnerId, t2.OwnerId);
            Assert.AreEqual(t1.PartnerId, t2.PartnerId);
            Assert.AreEqual(t1.PaymentId, t2.PaymentId);
            Assert.AreEqual(t1.Status, t2.Status);
            Assert.AreEqual(t1.TenantAlias, t2.TenantAlias);
            Assert.AreEqual(t1.TenantDomain, t2.TenantDomain);
            Assert.AreEqual(t1.TenantId, t2.TenantId);
            Assert.AreEqual(t1.TrustedDomains, t2.TrustedDomains);
            Assert.AreEqual(t1.TrustedDomainsType, t2.TrustedDomainsType);
            Assert.AreEqual(t1.TimeZone.Id, t2.TimeZone.Id);
            Assert.AreEqual(t1.Version, t2.Version);
            Assert.AreEqual(t1.VersionChanged, t2.VersionChanged);
        }

        private void ValidateDomain(string domain, Type expectException)
        {
            try
            {
                Service.ValidateDomain(domain);
            }
            catch (Exception ex)
            {
                if (expectException == null || !ex.GetType().Equals(expectException)) throw;
            }
        }
    }
}
#endif
