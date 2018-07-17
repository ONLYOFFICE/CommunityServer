/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Controllers;
using ASC.HealthCheck.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace ASC.TestHealthCheck
{
    [TestClass]
    public class CheckServiceApiControllerUnitTest
    {
        private const string JabberService = "OnlyOfficeSvcJabber";

        [TestMethod]
        public void CheckServer()
        {
            CheckServiceApiController controller = null;
            try
            {
                controller = new CheckServiceApiController { Request = new HttpRequestMessage() };
                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                HealthCheckRunner.ServiceNames = new List<string>();
                HttpResponseMessage response = controller.GetState();
                dynamic status;
                response.TryGetContentValue<dynamic>(out status);

                Assert.IsTrue(response.IsSuccessStatusCode);
                Assert.AreEqual(1, status.code);
            }
            finally
            {
                if(controller != null)
                {
                    controller.Dispose();
                }
            }
        }

        [TestMethod]
        public void CheckJabber()
        {
            CheckServiceApiController controller = null;
            try
            {
                controller = new CheckServiceApiController();
                controller.Request = new HttpRequestMessage();
                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                IServiceRepository serviceRepository = new ServiceRepository();
                serviceRepository.Add(JabberService);
                controller.ServiceRepository = serviceRepository;
                HealthCheckRunner.ServiceNames = new List<string> { JabberService };

                HttpResponseMessage response = controller.GetState();
                dynamic status;
                response.TryGetContentValue(out status);

                Assert.IsTrue(response.IsSuccessStatusCode);
                Assert.AreEqual(1, status.code);
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
