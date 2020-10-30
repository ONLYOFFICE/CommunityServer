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
    using ASC.Core.Billing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Linq;

    [TestClass]
    public class TariffSyncServiceTest
    {
        private readonly ITariffSyncService tariffSyncService;


        public TariffSyncServiceTest()
        {
            tariffSyncService = new TariffSyncService();
        }

        [TestMethod]
        public void GetTeriffsTest()
        {
            var tariff = tariffSyncService.GetTariffs(70, null).FirstOrDefault(t => t.Id == -38);
            Assert.AreEqual(1024 * 1024 * 1024, tariff.MaxFileSize);
            tariff = tariffSyncService.GetTariffs(74, null).FirstOrDefault(t => t.Id == -38);
            Assert.AreEqual(100 * 1024 * 1024, tariff.MaxFileSize);
        }

        [TestMethod]
        public void SyncTest()
        {
            using (var wcfClient = new TariffSyncClient())
            {
                var tariffs = wcfClient.GetTariffs(74, null);
                Assert.IsTrue(tariffs.Any());
            }
        }
    }
}
#endif
