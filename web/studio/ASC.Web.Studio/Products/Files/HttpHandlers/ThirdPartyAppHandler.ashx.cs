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
using System.Threading;
using System.Web;
using ASC.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.ThirdPartyApp;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Files.HttpHandlers
{
    public class ThirdPartyAppHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public static string HandlerPath = CommonLinkUtility.ToAbsolute("~/thirdpartyapp");

        public void ProcessRequest(HttpContext context)
        {
            Global.Logger.Debug("ThirdPartyApp: handler request - " + context.Request.Url);

            var message = string.Empty;

            try
            {
                var app = ThirdPartySelector.GetApp(context.Request[ThirdPartySelector.AppAttr]);
                Global.Logger.Debug("ThirdPartyApp: app - " + app);

                if (app.Request(context))
                {
                    return;
                }
            }
            catch (ThreadAbortException)
            {
                //Thats is responce ending
                return;
            }
            catch (Exception e)
            {
                Global.Logger.Error("ThirdPartyApp", e);
                message = e.Message;
            }

            if (string.IsNullOrEmpty(message))
            {
                if ((context.Request["error"] ?? "").ToLower() == "access_denied")
                {
                    message = context.Request["error_description"] ?? FilesCommonResource.AppAccessDenied;
                }
            }

            var redirectUrl = CommonLinkUtility.GetDefault();
            if (!string.IsNullOrEmpty(message))
            {
                if (SecurityContext.IsAuthenticated)
                {
                    redirectUrl += "#error/" + HttpUtility.UrlEncode(message);
                }
                else
                {
                    redirectUrl = string.Format("~/Auth.aspx?am={0}", (int)Studio.Auth.MessageKey.Error);
                }
            }
            context.Response.Redirect(redirectUrl, true);
        }
    }
}