/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

                if (currentProductId == WebItemManager.DocumentsProductID
                    && !ThirdPartyBannerSettings.CheckClosed("docusign")
                    && !DocuSignLoginProvider.Instance.IsEnabled)
                {
                    yield return new Tuple<string, string>("docusign", UserControlsCommonResource.BannerDocuSign);
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
                    //if(!ThirdPartyBannerSettings.CheckClosed("twilio"))
                    //    yield return new Tuple<string, string>("twilio", UserControlsCommonResource.BannerTwilio);

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