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
using ASC.Geolocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASC.Common.Tests.Geolocation
{
    [TestClass]
    public class GeolocationTest
    {
        [TestMethod]
        public void GetIPGeolocationTest()
        {
            var helper = new GeolocationHelper("db");
            var info = helper.GetIPGeolocation("62.213.10.13");
            Assert.AreEqual("Nizhny Novgorod", info.City);
            Assert.AreEqual("062.213.011.127", info.IPEnd);
            Assert.AreEqual("062.213.008.240", info.IPStart);
            Assert.AreEqual("RU", info.Key);
            Assert.AreEqual("Europe/Moscow", info.TimezoneName);
            Assert.AreEqual(4d, info.TimezoneOffset);

            info = helper.GetIPGeolocation("");
            Assert.AreEqual(IPGeolocationInfo.Default.City, info.City);
            Assert.AreEqual(IPGeolocationInfo.Default.IPEnd, info.IPEnd);
            Assert.AreEqual(IPGeolocationInfo.Default.IPStart, info.IPStart);
            Assert.AreEqual(IPGeolocationInfo.Default.Key, info.Key);
            Assert.AreEqual(IPGeolocationInfo.Default.TimezoneName, info.TimezoneName);
            Assert.AreEqual(IPGeolocationInfo.Default.TimezoneOffset, info.TimezoneOffset);
        }
    }
}
#endif