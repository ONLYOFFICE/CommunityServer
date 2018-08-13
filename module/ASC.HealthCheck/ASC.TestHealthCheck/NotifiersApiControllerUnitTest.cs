/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
