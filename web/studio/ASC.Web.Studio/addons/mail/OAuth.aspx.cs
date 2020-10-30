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


using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using ASC.Web.Core;

namespace ASC.Web.Mail
{
    public partial class OAuth : BasePage
    {
        public static string Location
        {
            get { return MailAddon.BaseVirtualPath + "OAuth.aspx"; }
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
                                                                        string.Concat(GoogleLoginProvider.GoogleScopeMail, " ", GoogleLoginProvider.Instance.Scopes),
                                                                        new Dictionary<string, string>
                                                                            {
                                                                                { "access_type", "offline" },
                                                                                { "prompt", "consent" }
                                                                            });
                }
                else
                {
                    SubmitData(code);
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                SubmitData(null, ex.Message);
            }
        }

        private const string CallbackJavascript =
            @"window.onload = function(){{try {{window.opener.accountsModal.onGetOAuthInfo(""{0}"", {1});}}catch(ex){{}}window.close();}};";

        private void SubmitData(string token, string errorMessage = null)
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "posttoparent",
                                                   string.Format(CallbackJavascript, token, EncodeJsString(errorMessage)),
                                                   true);
        }

        private static string EncodeJsString(string s)
        {
            if (string.IsNullOrEmpty(s)) return "null";
            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (var c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        var i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }
    }
}