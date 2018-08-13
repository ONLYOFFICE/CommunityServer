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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.FederatedLogin.LoginProviders;
using ASC.Thrdparty.Configuration;
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
            get { return SecurityContext.CheckPermissions(SecutiryConstants.EditPortalSettings) && GetBanner.Any(); }
        }

        protected Tuple<string, string> CurrentBanner;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterStyle("~/usercontrols/common/thirdpartybanner/css/thirdpartybanner.css");

            var tmp = GetBanner;
            var i = new Random().Next(0, tmp.Count());
            CurrentBanner = GetBanner.ToArray()[i];
        }

        private static IEnumerable<Tuple<string, string>> GetBanner
        {
            get
            {
                if (CoreContext.Configuration.Personal || HttpContext.Current.Request.DesktopApp())
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

                if (currentProductId == WebItemManager.DocumentsProductID
                    && !ThirdPartyBannerSettings.CheckClosed("docusign")
                    && (string.IsNullOrEmpty(DocuSignLoginProvider.DocuSignOAuth20ClientId)
                        || string.IsNullOrEmpty(DocuSignLoginProvider.DocuSignOAuth20ClientSecret)))
                {
                    yield return new Tuple<string, string>("docusign", UserControlsCommonResource.BannerDocuSign);
                }

                if ((currentProductId == WebItemManager.CRMProductID
                     || currentProductId == WebItemManager.PeopleProductID)
                    && !ThirdPartyBannerSettings.CheckClosed("social")
                    && (string.IsNullOrEmpty(TwitterLoginProvider.TwitterKey)
                        || string.IsNullOrEmpty(TwitterLoginProvider.TwitterSecret)
                        || string.IsNullOrEmpty(FacebookLoginProvider.FacebookOAuth20ClientId)
                        || string.IsNullOrEmpty(FacebookLoginProvider.FacebookOAuth20ClientSecret)
                        || string.IsNullOrEmpty(LinkedInLoginProvider.LinkedInOAuth20ClientId)
                        || string.IsNullOrEmpty(LinkedInLoginProvider.LinkedInOAuth20ClientSecret)
                        || string.IsNullOrEmpty(GoogleLoginProvider.GoogleOAuth20ClientId)
                        || string.IsNullOrEmpty(GoogleLoginProvider.GoogleOAuth20ClientSecret)))
                {
                    yield return new Tuple<string, string>("social", UserControlsCommonResource.BannerSocial);
                }

                if (currentProductId == WebItemManager.DocumentsProductID
                    && !ThirdPartyBannerSettings.CheckClosed("storage")
                    && (string.IsNullOrEmpty(BoxLoginProvider.BoxOAuth20ClientId)
                        || string.IsNullOrEmpty(BoxLoginProvider.BoxOAuth20ClientSecret)
                        || string.IsNullOrEmpty(DropboxLoginProvider.DropboxOAuth20ClientId)
                        || string.IsNullOrEmpty(DropboxLoginProvider.DropboxOAuth20ClientSecret)
                        || string.IsNullOrEmpty(OneDriveLoginProvider.OneDriveOAuth20ClientId)
                        || string.IsNullOrEmpty(OneDriveLoginProvider.OneDriveOAuth20ClientSecret)))
                {
                    yield return new Tuple<string, string>("storage", UserControlsCommonResource.BannerStorage);
                }

                if (currentProductId == WebItemManager.CRMProductID
                    && !ThirdPartyBannerSettings.CheckClosed("twilio")
                    && !VoipDao.ConfigSettingsExist
                    && KeyStorage.CanSet("twilioAccountSid"))
                {
                    yield return new Tuple<string, string>("twilio", UserControlsCommonResource.BannerTwilio);
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