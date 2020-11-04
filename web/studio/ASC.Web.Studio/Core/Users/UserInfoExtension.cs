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
using System.Text;
using ASC.Web.Core;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Core.Users
{
    public static class StudioUserInfoExtension
    {
        public static string GetUserProfilePageURL(this UserInfo userInfo)
        {
            return userInfo == null ? "" : CommonLinkUtility.GetUserProfile(userInfo.ID);
        }

        public static string RenderProfileLink(this UserInfo userInfo, Guid productID)
        {
            var sb = new StringBuilder();

            if (userInfo == null || !CoreContext.UserManager.UserExists(userInfo.ID))
            {
                sb.Append("<span class='userLink text-medium-describe'>");
                sb.Append(Resource.ProfileRemoved);
                sb.Append("</span>");
            }
            else if (Array.Exists(Configuration.Constants.SystemAccounts, a => a.ID == userInfo.ID))
            {
                sb.Append("<span class='userLink text-medium-describe'>");
                sb.Append(userInfo.LastName);
                sb.Append("</span>");
            }
            else
            {
                sb.AppendFormat("<span class=\"userLink\" id=\"{0}\" data-uid=\"{1}\" data-pid=\"{2}\">", Guid.NewGuid(), userInfo.ID, productID);
                sb.AppendFormat("<a class='linkDescribe' href=\"{0}\">{1}</a>", userInfo.GetUserProfilePageURL(), userInfo.DisplayUserName());
                sb.Append("</span>");
            }
            return sb.ToString();
        }

        public static string RenderCustomProfileLink(this UserInfo userInfo, String containerCssClass, String linkCssClass)
        {
            var containerCss = string.IsNullOrEmpty(containerCssClass) ? "userLink" : "userLink " + containerCssClass;
            var linkCss = string.IsNullOrEmpty(linkCssClass) ? "" : linkCssClass;
            var sb = new StringBuilder();

            if (userInfo == null || !CoreContext.UserManager.UserExists(userInfo.ID))
            {
                sb.AppendFormat("<span class='{0}'>", containerCss);
                sb.Append(Resource.ProfileRemoved);
                sb.Append("</span>");
            }
            else if (Array.Exists(Configuration.Constants.SystemAccounts, a => a.ID == userInfo.ID))
            {
                sb.AppendFormat("<span class='{0}'>", containerCss);
                sb.Append(userInfo.LastName);
                sb.Append("</span>");
            }
            else
            {
                sb.AppendFormat("<span class=\"{0}\" id=\"{1}\" data-uid=\"{2}\" >", containerCss, Guid.NewGuid(), userInfo.ID);
                sb.AppendFormat("<a class='{0}' href=\"{1}\">{2}</a>", linkCss, userInfo.GetUserProfilePageURL(), userInfo.DisplayUserName());
                sb.Append("</span>");
            }
            return sb.ToString();
        }

        public static List<string> GetListAdminModules(this UserInfo ui)
        {
            var products = WebItemManager.Instance.GetItemsAll().Where(i => i is IProduct || i.ID == WebItemManager.MailProductID);

            return (from product in products where WebItemSecurity.IsProductAdministrator(product.ID, ui.ID) select product.ProductClassName).ToList();
        }
    }
}