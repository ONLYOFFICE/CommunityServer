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
    using ASC.Core.Tenants;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Configuration;
    using System.Linq;

    [TestClass]
    public class HostedSolutionTest
    {
        [TestMethod]
        public void FindTenants()
        {
            var h = new HostedSolution(ConfigurationManager.ConnectionStrings["core"]);
            var tenants = h.FindTenants("76ff727b-f987-4871-9834-e63d4420d6e9");
            Assert.AreNotEqual(0, tenants.Count);
        }

        [TestMethod]
        public void TenantUtilTest()
        {
            var date = TenantUtil.DateTimeNow(System.TimeZoneInfo.GetSystemTimeZones().First());
            Assert.IsNotNull(date);
        }

        [TestMethod]
        public void RegionsTest()
        {
            var regionSerice = new MultiRegionHostedSolution("site");

            var t1 = regionSerice.GetTenant("teamlab.com", 50001);
            Assert.AreEqual("alias_test2.teamlab.com", t1.TenantDomain);

            var t2 = regionSerice.GetTenant("teamlab.eu.com", 50001);
            Assert.AreEqual("tscherb.teamlab.eu.com", t2.TenantDomain);
        }
    }
}
#endif
