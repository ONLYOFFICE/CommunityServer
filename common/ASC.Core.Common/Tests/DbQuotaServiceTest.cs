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
            new DbExecuter(ConfigurationManager.ConnectionStrings["core"]).ExecNonQuery(d);
        }
    }
}
#endif
