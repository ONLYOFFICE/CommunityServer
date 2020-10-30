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


using ASC.HealthCheck.Models;
using ASC.HealthCheck.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ASC.TestHealthCheck
{
    [TestClass]
    public class ServiceRepositoryUnitTest
    {
        private const string JabberService = "OnlyOfficeSvcJabber";

        [TestMethod]
        public void ServiceRepositoryAddRemove()
        {
            IServiceRepository serviceRepository = new ServiceRepository();
            serviceRepository.Add(JabberService);
            Assert.AreEqual(true, serviceRepository.HasAtempt(JabberService));
            var service = serviceRepository.GetService(JabberService);
            Assert.AreEqual(0, service.Attempt);
            Assert.AreEqual(JabberService, service.ServiceName);
            serviceRepository.Remove(JabberService);
            try
            {
                serviceRepository.GetService(JabberService);
            }
            catch (InvalidOperationException)
            {
                // Expected exception
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            Assert.Fail("Error! Never get this command.");
        }

        [TestMethod]
        public void ServiceRepositorySetStates()
        {
            var serviceRepository = new ServiceRepository();
            serviceRepository.Add(JabberService);
            serviceRepository.SetStates(JabberService, HealthCheckResource.StatusServiceStoped, HealthCheckResource.ServiceStop);
            var service = serviceRepository.GetService(JabberService);
            Assert.AreEqual(0, service.Attempt);
            Assert.AreEqual(JabberService, service.ServiceName);
            Assert.AreEqual(HealthCheckResource.ServiceStop, service.Message);
        }

        [TestMethod]
        public void ServiceRepositoryDropAttemptsShouldRestart()
        {
            var serviceRepository = new ServiceRepository();
            serviceRepository.Add(JabberService);
            Assert.AreEqual(true, serviceRepository.HasAtempt(JabberService));
            var service = serviceRepository.GetService(JabberService);
            Assert.AreEqual(0, service.Attempt);
            Assert.AreEqual(false, serviceRepository.ShouldRestart(JabberService));
            Assert.AreEqual(1, service.Attempt);
            serviceRepository.DropAttempt(JabberService);
            Assert.AreEqual(0, service.Attempt);
        }
    }
}
