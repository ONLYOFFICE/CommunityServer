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