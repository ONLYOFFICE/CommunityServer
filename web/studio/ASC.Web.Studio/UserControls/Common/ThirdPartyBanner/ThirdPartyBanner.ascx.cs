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
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.FederatedLogin.LoginProviders;
using ASC.VoipService.Dao;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Resources;

namespace ASC.Web.Studio.UserControls.Common.ThirdPartyBanner
{
    [AjaxNamespace("ThirdPartyBanner")]
    public partial class ThirdPartyBanner : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/ThirdPartyBanner/ThirdPartyBanner.ascx"; }
        }

        public static bool Display
        {
            get { return SetupInfo.ThirdPartyBannerEnabled && SecurityContext.CheckPermissions(SecutiryConstants.EditPortalSettings) && GetBanner.Any(); }
        }

        protected Tuple<string, string> CurrentBanner;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterStyle("~/UserControls/Common/ThirdPartyBanner/css/thirdpartybanner.css");

            var tmp = GetBanner;
            var i = new Random().Next(0, tmp.Count());
            CurrentBanner = GetBanner.ToArray()[i];
        }

        private static IEnumerable<Tuple<string, string>> GetBanner
        {
            get
            {
                if (CoreContext.Configuration.Personal
                    || CoreContext.Configuration.CustomMode
                    || HttpContext.Current.Request.DesktopApp())
                {
                    yield break;
                }

                var currentProductId = CommonLinkUtility.GetProductID();

                if ((currentProductId == WebItemManager.DocumentsProductID
                     || currentProductId == WebItemManager.PeopleProductID)
                    && !ThirdPartyBannerSettings.CheckClosed("bitly")
                    && !BitlyLoginProvider.Enabled)
                {
                    yield return new Tuple<string, string>("bitly", UserControlsCommonResource.BannerBitly);
                }

                if ((currentProductId == WebItemManager.CRMProductID
                     || currentProductId == WebItemManager.PeopleProductID)
                    && !ThirdPartyBannerSettings.CheckClosed("social")
                    && 
                    (!TwitterLoginProvider.Instance.IsEnabled || 
                    !FacebookLoginProvider.Instance.IsEnabled || 
                    !LinkedInLoginProvider.Instance.IsEnabled || 
                    !GoogleLoginProvider.Instance.IsEnabled))
                {
                    yield return new Tuple<string, string>("social", UserControlsCommonResource.BannerSocial);
                }

                if (currentProductId == WebItemManager.DocumentsProductID
                    && !ThirdPartyBannerSettings.CheckClosed("storage")
                    && (!BoxLoginProvider.Instance.IsEnabled
                        || !DropboxLoginProvider.Instance.IsEnabled
                        || !OneDriveLoginProvider.Instance.IsEnabled))
                {
                    yield return new Tuple<string, string>("storage", UserControlsCommonResource.BannerStorage);
                }

                if (currentProductId == WebItemManager.CRMProductID
                    && !VoipDao.ConfigSettingsExist
                    && VoipDao.Consumer.CanSet)
                {
                    if(!ThirdPartyBannerSettings.CheckClosed("twilio2"))
                        yield return new Tuple<string, string>("twilio2", UserControlsCommonResource.BannerTwilio2);
                }
            }
        }

        [AjaxMethod]
        public void CloseBanner(string banner)
        {
            ThirdPartyBannerSettings.Close(banner);
        }
    }
}