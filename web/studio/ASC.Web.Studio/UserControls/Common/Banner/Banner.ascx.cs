/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Collections.Generic;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using Resources;

using AjaxPro;


namespace ASC.Web.Studio.UserControls.Common.Banner
{
    [AjaxNamespace("AjaxPro.BannerController")]
    public partial class Banner : UserControl
    {
        public static string Location { get { return "~/usercontrols/common/banner/banner.ascx"; } }
       
        protected static string LanguageName
        {
            get
            {
                return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            }
        }
        protected List<BannerType> banners;

        public class BannerType
        {
            public string Id { get; set; }
            public string Url { get; set; }
            public string Title { get; set; }
            public string Img { get; set; }
            public string ImgType { get; set; }
            public string ImgUrl
            {
                get
                {
                    return WebPath.Exists("/skins/default/images/banner/" + Img + LanguageName + ImgType)
                       ? WebPath.GetPath("/skins/default/images/banner/" + Img + LanguageName + ImgType).ToLowerInvariant()
                       : WebPath.GetPath("/skins/default/images/banner/" + Img + "en" + ImgType).ToLowerInvariant();
                }
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/banner/js/banner.js"));
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            banners = new List<BannerType>();

            if (CoreContext.Configuration.Personal)
            {
                banners.Add(new BannerType
                {
                    Id = "joinTeamlabBanner",
                    Url = "https://www.onlyoffice.com/" + LanguageName + "/highlights.aspx",
                    Title = Resource.BannerRegistration,
                    Img = "banner_try_",
                    ImgType = ".png"
                });
                banners.Add(new BannerType
                {
                    Id = "chromeStoreBanner",
                    Url = "https://chrome.google.com/webstore/detail/teamlab-personal/iohfebkcjhlelaoibebeohcgkohkcgpn?hl=" + LanguageName,
                    Title = Resource.ChromeStoreBannerTitle,
                    Img = "banner_chrome_store_",
                    ImgType = ".png"

                });
            }
            else if (AffiliateHelper.BannerAvailable)
            {
                banners.Add(new BannerType
                {
                    Id = "joinAffilliateBanner",
                    Url = "javascript:void(0)",
                    Title = Resource.WeUseTeamLabOnlineOffice,
                    Img = "banner_portal_",
                    ImgType = ".png"
                });
            }
        }


        #region AjaxMethod
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse JoinToAffiliateProgram()
        {
            var resp = new AjaxResponse();
            try
            {
                resp.rs1 = "1";
                resp.rs2 = AffiliateHelper.Join();
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }
            return resp;

        }
        #endregion

    }
}