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