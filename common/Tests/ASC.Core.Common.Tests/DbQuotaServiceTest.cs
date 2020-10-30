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
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using ASC.Common.Data.Sql;
    using ASC.Core.Billing;
    using ASC.Core.Data;
    using ASC.Core.Tenants;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ASC.Common.Data;

    [TestClass]
    public class DbQuotaServiceTest : DbBaseTest<DbQuotaService>
    {
        [TestInitialize]
        public void ClearData()
        {
            Service.RemoveTenantQuota(Tenant);
            foreach (var row in Service.FindTenantQuotaRows(new TenantQuotaRowQuery(Tenant) { Path = "path" }))
            {
                DeleteQuotaRow(row);
            }
        }

        [TestMethod]
        public void QuotaMethod()
        {
            var quota1 = new TenantQuota(Tenant)
            {
                MaxFileSize = 3,
                MaxTotalSize = 4,
                ActiveUsers = 30,
            };
            Service.SaveTenantQuota(quota1);
            CompareQuotas(quota1, Service.GetTenantQuota(quota1.Id));

            Service.RemoveTenantQuota(Tenant);
            Assert.IsNull(Service.GetTenantQuota(quota1.Id));

            var row = new TenantQuotaRow { Tenant = this.Tenant, Path = "path", Counter = 1000, Tag = "tag" };
            Service.SetTenantQuotaRow(row, false);

            var rows = Service.FindTenantQuotaRows(new TenantQuotaRowQuery(Tenant).WithPath("path")).ToList();
            CompareQuotaRows(row, rows.Find(r => r.Tenant == row.Tenant && r.Tag == row.Tag));

            Service.SetTenantQuotaRow(row, true);
            row.Counter += 1000;
            rows = Service.FindTenantQuotaRows(new TenantQuotaRowQuery(Tenant).WithPath("path")).ToList();
            CompareQuotaRows(row, rows.Find(r => r.Tenant == row.Tenant && r.Tag == row.Tag));

            DeleteQuotaRow(row);
        }

        [TestMethod]
        public void SerializeTest()
        {
            var quota1 = new TenantQuota(Tenant)
            {
                AvangateId = "1",
                Features = "trial,year",
                Name = "quota1",
                Price = 12.5m,
                Price2 = 45.23m,
                Visible = true,
                MaxFileSize = 3,
                MaxTotalSize = 4,
                ActiveUsers = 30,
            };

            var serializer = new DataContractJsonSerializer(quota1.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, quota1);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Assert.AreEqual("{\"Id\":1024,\"Name\":\"quota1\",\"MaxFileSize\":3,\"MaxTotalSize\":4,\"ActiveUsers\":30,\"Features\":\"trial,year\",\"Price\":12.5,\"Price2\":45.23,\"AvangateId\":\"1\",\"Visible\":true}", json);
            }
        }

        [TestMethod]
        public void SyncTest()
        {
            var client = new TariffSyncClient();
            var quotas = client.GetTariffs(1, "key");
            Assert.AreNotEqual(0, quotas.Count());
        }

        private void CompareQuotas(TenantQuota q1, TenantQuota q2)
        {
            Assert.AreEqual(q1.Id, q2.Id);
            Assert.AreEqual(q1.Name, q2.Name);
            Assert.AreEqual(q1.MaxFileSize, q2.MaxFileSize);
            Assert.AreEqual(q1.MaxTotalSize, q2.MaxTotalSize);
            Assert.AreEqual(q1.ActiveUsers, q2.ActiveUsers);
            Assert.AreEqual(q1.Features, q2.Features);
            Assert.AreEqual(q1.Price, q2.Price);
            Assert.AreEqual(q1.Price2, q2.Price2);
            Assert.AreEqual(q1.AvangateId, q2.AvangateId);
            Assert.AreEqual(q1.Visible, q2.Visible);
        }

        private void CompareQuotaRows(TenantQuotaRow r1, TenantQuotaRow r2)
        {
            Assert.AreEqual(r1.Path, r2.Path);
            Assert.AreEqual(r1.Tag, r2.Tag);
            Assert.AreEqual(r1.Tenant, r2.Tenant);
            Assert.AreEqual(r1.Counter, r2.Counter);
        }

        private void DeleteQuotaRow(TenantQuotaRow row)
        {
            var d = new SqlDelete(DbQuotaService.tenants_quotarow).Where("tenant", row.Tenant).Where("path", row.Path);
            new DbManager("core").ExecuteNonQuery(d);
        }
    }
}
#endif
