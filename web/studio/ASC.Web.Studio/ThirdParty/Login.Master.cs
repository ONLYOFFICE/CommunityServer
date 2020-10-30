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
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.ThirdParty
{
    public partial class Login : MasterPage
    {
        private const string CallbackSuccessJavascript =
            "function snd(){{try{{window.opener.OAuthCallback(\"{0}\");}}catch(err){{alert(err);}}window.close();}} window.onload = snd;";

        private const string CallbackErrorJavascript =
            "function snd(){{try{{window.opener.OAuthError(\"{0}\");}}catch(err){{alert(err);}}window.close();}} window.onload = snd;";

        protected override void OnInit(EventArgs e)
        {
            HttpContext.Current.PushRewritenUri();
            base.OnInit(e);
        }

        public void SubmitCode(string code)
        {
            var urlRedirect = Request["redirect"];
            if (!string.IsNullOrEmpty(urlRedirect))
            {
                Response.Redirect(AppendCode(urlRedirect, code), true);
            }

            Page.ClientScript.RegisterClientScriptBlock(GetType(), "posttoparent",
                                                        String.Format(CallbackSuccessJavascript, code.Replace("\"", "\\\"")), true);
        }

        public void SubmitError(string error)
        {
            var urlRedirect = Request["redirect"];
            if (!string.IsNullOrEmpty(urlRedirect))
            {
                Response.Redirect(AppendCode(urlRedirect, null, error), true);
            }

            Page.ClientScript.RegisterClientScriptBlock(GetType(), "posterrortoparent",
                                                        String.Format(CallbackErrorJavascript, error.Replace("\"", "\\\"")), true);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private static string AppendCode(string url, string code = null, string error = null)
        {
            url += (url.Contains("#") ? "&" : "#")
                   + (string.IsNullOrEmpty(error)
                          ? (string.IsNullOrEmpty(code)
                                 ? string.Empty
                                 : "code=" + HttpUtility.UrlEncode(code))
                          : ("error/" + HttpUtility.UrlEncode(error)));

            return url;
        }
    }
}