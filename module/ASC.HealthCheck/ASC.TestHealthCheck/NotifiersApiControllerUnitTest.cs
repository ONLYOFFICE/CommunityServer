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
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace ASC.TestHealthCheck
{
    [TestClass]
    public class NotifiersApiControllerUnitTest
    {
        [TestMethod]
        public void GetNotifiersUnitTest()
        {
            NotifiersApiController controller = null;
            try
            {
                controller = new NotifiersApiController { Request = new HttpRequestMessage() };
                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                HttpResponseMessage response = controller.GetNotifiers();
                
                IList<string> emails = new List<string>(2) { "qwerty@qwerty.com", "asdfg@asdfg.com" };
                IList<string> numbers = new List<string>(2) { "000", "111" };
                dynamic status;
                response.TryGetContentValue(out status);

                Assert.IsTrue(response.IsSuccessStatusCode);
                Assert.AreEqual(1, status.code);
                for (int i = 0; i < status.emails.Count; i++)
                {
                    Assert.AreEqual(emails[i], status.emails[i]);
                }
                for (int i = 0; i < status.numbers.Count; i++)
                {
                    Assert.AreEqual(numbers[i], status.numbers[i]);
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

        [TestMethod]
        public void GetNotifySettingsUnitTest()
        {
            NotifiersApiController controller = null;
            try
            {
                controller = new NotifiersApiController { Request = new HttpRequestMessage() };
                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                HttpResponseMessage response = controller.GetNotifySettings();

                dynamic status;
                response.TryGetContentValue(out status);

                Assert.IsTrue(response.IsSuccessStatusCode);
                Assert.AreEqual(1, status.code);
                Assert.AreEqual(false, status.sendNotify);
                Assert.AreEqual(0, status.sendEmailSms);
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
