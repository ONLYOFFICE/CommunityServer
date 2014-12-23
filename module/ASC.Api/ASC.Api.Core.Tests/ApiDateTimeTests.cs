/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

#if DEBUG
using System;
using ASC.Specific;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASC.Api.Core.Tests
{
    [TestClass]
    public class ApiDateTimeTests
    {
        [TestMethod]
        public void TestParsing()
        {
            const string parseTime = "2012-01-11T07:01:00.0000001Z";
            var apiDateTime1 = ApiDateTime.Parse(parseTime);
            var dateTime = (DateTime) apiDateTime1;
            var utcTime = apiDateTime1.UtcTime;
            Assert.AreEqual(dateTime.Kind,utcTime.Kind);
            Assert.AreEqual(dateTime,utcTime);
            Assert.AreEqual(apiDateTime1.ToString(),parseTime);
        }

        [TestMethod]
        public void TestNull()
        {
            var apiDateTime = (ApiDateTime) null;
            Assert.IsNull(apiDateTime);
        }

        [TestMethod]
        public void TestLocal2()
        {

            var apiDateTime = new ApiDateTime(DateTime.Now,TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
            var stringv =apiDateTime.ToString();
        }


        [TestMethod]
        public void TestParsing2()
        {
            const string parseTime = "2012-01-31T20:00:00.0000000Z";
            var apiDateTime1 = ApiDateTime.Parse(parseTime);
            var dateTime = (DateTime)apiDateTime1;
            var utcTime = apiDateTime1.UtcTime;
            Assert.AreEqual(dateTime.Kind, utcTime.Kind);
            Assert.AreEqual(dateTime, utcTime);
            Assert.AreEqual(apiDateTime1.ToString(), parseTime);
        }

        [TestMethod]
        public void TestUtc()
        {
            var apiDateTime1 = new ApiDateTime(DateTime.Now,TimeZoneInfo.Utc);
            var dateTime = (DateTime)apiDateTime1;
            var utcTime = apiDateTime1.UtcTime;
            Assert.AreEqual(dateTime.Kind, utcTime.Kind);
            Assert.AreEqual(dateTime, utcTime);
        }

        [TestMethod]
        public void TestLocal()
        {
            var apiDateTime1 = new ApiDateTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
            var dateTime = (DateTime)apiDateTime1;
            var utcTime = apiDateTime1.UtcTime;
            Assert.AreEqual(dateTime.Kind, utcTime.Kind);
            Assert.AreEqual(dateTime, utcTime);
        }

        [TestMethod]
        public void Test00()
        {
            var apiDateTime1 = new ApiDateTime(DateTime.Now);
            var stringrep = apiDateTime1.ToString();
            var dateTime = (DateTime)apiDateTime1;
            var utcTime = apiDateTime1.UtcTime;
            Assert.AreEqual(dateTime.Kind, utcTime.Kind);
            Assert.AreEqual(dateTime, utcTime);
        }
    }
}
#endif