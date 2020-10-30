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


using ASC.HealthCheck.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace ASC.TestHealthCheck
{
    [TestClass]
    public class DownloadLogsApiControllerUnitTest
    {
        [TestMethod]
        public void CheckDownloadLogs()
        {
            DownloadLogsApiController controller = null;
            try
            {
                controller = new DownloadLogsApiController();
                controller.Request = new HttpRequestMessage();
                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                HttpResponseMessage response = controller.DownloadLogs("2012-04-04", "2022-04-04");
                dynamic status;
                response.TryGetContentValue(out status);

                Assert.IsTrue(response.IsSuccessStatusCode);
                Assert.AreEqual(null, status);
            }
            finally
            {
                if (controller != null)
                {
                    controller.Dispose();
                }
            }
        }
    }
}
