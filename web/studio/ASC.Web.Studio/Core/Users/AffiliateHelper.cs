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
using System.Configuration;

using ASC.Core.Users;

namespace ASC.Web.Studio.Core.Users
{
    public class AffiliateHelper
    {
        public static string JoinAffilliateLink
        {
            get { return ConfigurationManagerExtension.AppSettings["web.affiliates.link"]; }

        }

        private static bool Available(UserInfo user)
        {
            return !String.IsNullOrEmpty(JoinAffilliateLink) &&
                   user.ActivationStatus == EmployeeActivationStatus.Activated &&
                   user.Status == EmployeeStatus.Active;
        }

        public static bool ButtonAvailable(UserInfo user)
        {
            return Available(user) && user.IsMe();
        }

        public static string Join()
        {
            if (!string.IsNullOrEmpty(JoinAffilliateLink))
            {
                return JoinAffilliateLink;

                /*                var request = WebRequest.Create(string.Format("{2}/Account/Register?uid={1}&tenantAlias={0}",
                                                                    CoreContext.TenantManager.GetCurrentTenant().TenantAlias, SecurityContext.CurrentAccount.ID,
                                                                    JoinAffilliateLink));
                                request.Method = "PUT";
                                request.ContentLength = 0;
                                using (var response = (HttpWebResponse)request.GetResponse())
                                {
                                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                                    {
                                        var origin = streamReader.ReadToEnd();
                                        if (response.StatusCode != HttpStatusCode.BadRequest)
                                        {
                                            return string.Format("{0}/home/Account/SignIn?ticketKey={1}", JoinAffilliateLink, origin);
                                        }
                                    }
                                }*/
            }

            return "";
        }
    }
}
