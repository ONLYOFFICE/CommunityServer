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
        [ClassInitialize]
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
