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
using ASC.HealthCheck.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace ASC.TestHealthCheck
{
    /// <summary>
    /// Summary description for PortsCheckApiControllerUnitTest
    /// </summary>
    [TestClass]
    public class PortsCheckApiControllerUnitTest
    {
        [TestMethod]
        public void CheckPorts()
        {
            PortsCheckApiController controller = null;
            try
            {
                controller = new PortsCheckApiController { Request = new HttpRequestMessage() };
                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                IList<Port> ports = controller.GetPortList();

                foreach (var port in ports)
                {
                    // HTTP
                    if (port.Number == 80)
                    {
                        //Assert.AreEqual(PortStatus.Open, port.PortStatus);
                        break;
                    }
                }
            }
            finally
            {
                if(controller != null)
                {
                    controller.Dispose();
                }
            }
        }
    }
}
