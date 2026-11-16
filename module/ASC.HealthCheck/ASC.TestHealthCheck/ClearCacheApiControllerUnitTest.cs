/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Configuration;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace ASC.TestHealthCheck
{
    [TestClass]
    public class ClearCacheApiControllerUnitTest
    {
        // healthCheck/portals-web-config-path in App.config
        private static readonly string[] PortalConfigs = { @"ClearCache\Portal.config", @"ClearCache\Portal2.config" };

        [TestMethod]
        public void CheckClearCache()
        {
            ClearCacheApiController controller = null;
            try
            {
                controller = new ClearCacheApiController();
                controller.Request = new HttpRequestMessage();
                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                var resetKeys = ReadResetKeys();

                HttpResponseMessage response = controller.ClearCache();
                dynamic status;
                response.TryGetContentValue(out status);

                Assert.IsTrue(response.IsSuccessStatusCode);
                Assert.AreEqual(1, status.code);
                var newResetKeys = ReadResetKeys();
                for (var i = 0; i < PortalConfigs.Length; i++)
                {
                    Assert.AreNotEqual(resetKeys[i], newResetKeys[i], PortalConfigs[i]);
                }
            }
            finally
            {
                if (controller != null)
                {
                    controller.Dispose();
                }
            }
        }

        private static string[] ReadResetKeys()
        {
            var keys = new string[PortalConfigs.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                var configuration = ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap { ExeConfigFilename = PortalConfigs[i] }, ConfigurationUserLevel.None);
                keys[i] = configuration.AppSettings.Settings["web.client.cache.resetkey"].Value;
            }
            return keys;
        }
    }
}
