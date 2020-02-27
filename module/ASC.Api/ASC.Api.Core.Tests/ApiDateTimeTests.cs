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