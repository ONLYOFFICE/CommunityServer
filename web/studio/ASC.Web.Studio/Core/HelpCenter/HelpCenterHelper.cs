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


using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.HelpCenter
{
    public static class HelpCenterHelper
    {
        public static List<VideoGuideItem> GetVideoGuides()
        {
            var data = GetVideoGuidesAll();
            var wathced = UserVideoSettings.GetUserVideoGuide();

            data.RemoveAll(r => r != null && wathced.Contains(r.Id));
            if (!UserHelpTourHelper.IsNewUser)
            {
                data.RemoveAll(r => r.Status == "default");
            }
            return data;
        }

        private static List<VideoGuideItem> GetVideoGuidesAll()
        {
            var baseUrl = CommonLinkUtility.GetHelpLink(false); ;
            if (string.IsNullOrEmpty(baseUrl))
            {
                return new List<VideoGuideItem>();
            }

            var storage = new BaseHelpCenterStorage<VideoGuideData>(HttpContext.Current.Server.MapPath("~/"),"videoguide.html", "videoguide");
            var storageData = storage.GetData(baseUrl, CommonLinkUtility.GetHelpLink() + "/video.aspx", baseUrl);
            var result = new List<VideoGuideItem>();
            if (storageData != null)
            {
                result.AddRange(storageData.ListItems);
            }
            return result;
        }


        public static List<HelpCenterItem> GetHelpCenter(string module, string helpLinkBlock)
        {
            var baseUrl = CommonLinkUtility.GetHelpLink(false);
            if (string.IsNullOrEmpty(baseUrl))
            {
                return null;
            }

            var storage = new BaseHelpCenterStorage<HelpCenterData>(HttpContext.Current.Server.MapPath("~/"), "helpcenter.html", "helpcenter");
            var storageData = storage.GetData(baseUrl, CommonLinkUtility.GetHelpLink() + "/gettingstarted/" + module, helpLinkBlock);

            return storageData != null ? storageData.ListItems : null;
        }
    }
}