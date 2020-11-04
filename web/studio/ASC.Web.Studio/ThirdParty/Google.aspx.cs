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
using System.Collections.Generic;
using System.Threading;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.ThirdParty
{
    public partial class Google : BasePage
    {
        public static string Location
        {
            get { return CommonLinkUtility.ToAbsolute("~/ThirdParty/Google.aspx"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var error = Request["error"];
                if (!string.IsNullOrEmpty(error))
                {
                    if (error == "access_denied")
                    {
                        error = "Canceled at provider";
                    }
                    throw new Exception(error);
                }

                var code = Request["code"];
                if (string.IsNullOrEmpty(code))
                {
                    OAuth20TokenHelper.RequestCode<GoogleLoginProvider>(HttpContext.Current,
                                                                        GoogleLoginProvider.GoogleScopeDrive,
                                                                        new Dictionary<string, string>
                                                                            {
                                                                                { "access_type", "offline" },
                                                                                { "prompt", "consent" }
                                                                            });
                }
                else
                {
                    Master.SubmitCode(code);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Master.SubmitError(ex.Message);
            }
        }
    }
}