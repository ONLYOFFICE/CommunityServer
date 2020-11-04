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