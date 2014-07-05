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
                var popupID = Guid.NewGuid();
                sb.AppendFormat("<span class=\"userLink\" id=\"{0}\" data-uid=\"{1}\" data-pid=\"{2}\">", popupID, userInfo.ID, productID);
                sb.AppendFormat("<a class='linkDescribe' href=\"{0}\">{1}</a>", userInfo.GetUserProfilePageURL(), userInfo.DisplayUserName());
                sb.Append("</span>");
            }
            return sb.ToString();
        }

        public static string RenderCustomProfileLink(this UserInfo userInfo, Guid productID, String containerCssClass, String linkCssClass)
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
                var popupID = Guid.NewGuid();
                sb.AppendFormat("<span class=\"{0}\" id=\"{1}\" data-uid=\"{2}\" data-pid=\"{3}\">", containerCss, popupID, userInfo.ID, productID);
                sb.AppendFormat("<a class='{0}' href=\"{1}\">{2}</a>", linkCss, userInfo.GetUserProfilePageURL(), userInfo.DisplayUserName());
                sb.Append("</span>");
            }
            return sb.ToString();
        }

        public static List<string> GetListAdminModules(this UserInfo ui)
        {
            var listModules = new List<string>();

            var productsForAccessSettings = WebItemManager.Instance.GetItemsAll<IProduct>().Where(n => String.Compare(n.GetSysName(), "people") != 0).ToList();

            foreach (var product in productsForAccessSettings)
            {
                if (WebItemSecurity.IsProductAdministrator(product.ID, ui.ID))
                {
                    listModules.Add(product.ProductClassName);
                }
            }

            return listModules;
        }
    }
}