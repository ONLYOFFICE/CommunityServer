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


using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;

using ASC.Common.Logging;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;


namespace ASC.Web.Studio.UserControls.FirstTime
{
    public static class FirstTimeTenantSettings
    {
        public static void SendInstallInfo(UserInfo user)
        {
            if (!TenantExtra.Opensource) return;
            if (!WizardSettings.Load().Analytics) return;

            try
            {
                var url = ConfigurationManagerExtension.AppSettings["web.install-url"];
                if (string.IsNullOrEmpty(url)) return;

                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var q = new MailQuery
                {
                    Email = user.Email,
                    Id = CoreContext.Configuration.GetKey(tenant.TenantId),
                    Alias = tenant.TenantDomain,
                };

                var index = url.IndexOf("?v=", StringComparison.InvariantCultureIgnoreCase);
                if (0 < index)
                {
                    q.Version = url.Substring(index + 3) + Environment.OSVersion;
                    url = url.Substring(0, index);
                }

                using (var webClient = new WebClient())
                {
                    var values = new NameValueCollection
                        {
                            {"query", Signature.Create(q, "4be71393-0c90-41bf-b641-a8d9523fba5c")}
                        };
                    webClient.UploadValues(url, values);
                }
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Web").Error(error);
            }
        }

        private class MailQuery
        {
            public string Email { get; set; }
            public string Version { get; set; }
            public string Id { get; set; }
            public string Alias { get; set; }
        }
    }
}