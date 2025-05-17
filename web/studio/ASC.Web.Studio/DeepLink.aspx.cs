/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Web;

using ASC.Web.Core.Mobile;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls;

namespace ASC.Web.Studio
{
    public partial class DeepLink : MainPage
    {
        public static bool MustRedirect(HttpRequest request)
        {
            var userAgent = request.UserAgent.ToString().ToLower();
            HttpCookie deeplinkCookie = request.Cookies.Get("deeplink");
            var deepLink = ConfigurationManagerExtension.AppSettings["deeplink.documents.url"];

            return deepLink != null && MobileDetector.IsMobile
                && ((!userAgent.Contains("version/") && userAgent.Contains("android")) || !userAgent.Contains("android")) &&    //check webkit
                ((request[DeepLinking.WithoutDeeplinkRedirect] == null && deeplinkCookie == null) ||
                        request[DeepLinking.WithoutDeeplinkRedirect] == null && deeplinkCookie != null && deeplinkCookie.Value == "app");
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            Master.DisabledSidePanel = true;
            Master.TopStudioPanel.DisableUserInfo = true;
            Master.TopStudioPanel.DisableProductNavigation = true;
            Master.TopStudioPanel.DisableSearch = true;
            Master.TopStudioPanel.DisableSettings = true;
            Master.TopStudioPanel.DisableTariff = true;
            Master.TopStudioPanel.DisableLoginPersonal = true;
            Master.TopStudioPanel.DisableGift = true;

            var deepLinkingControl = (DeepLinking)LoadControl(DeepLinking.Location);
            DeepLinkingHolder.Controls.Add(deepLinkingControl);
        }
    }

}