/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
