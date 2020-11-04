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
using System.Drawing;
using System.Text;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Core.Users
{
    public static class UserInfoExtension
    {
        public static string DisplayUserName(this UserInfo userInfo)
        {
            return DisplayUserName(userInfo, true);
        }

        public static string DisplayUserName(this UserInfo userInfo, bool withHtmlEncode)
        {
            return DisplayUserSettings.GetFullUserName(userInfo, withHtmlEncode);
        }

        public static List<UserInfo> SortByUserName(this IEnumerable<UserInfo> userInfoCollection)
        {
            if (userInfoCollection == null) return new List<UserInfo>();

            var users = new List<UserInfo>(userInfoCollection);
            users.Sort(UserInfoComparer.Default);
            return users;
        }

        public static bool HasAvatar(this UserInfo userInfo)
        {
            return UserPhotoManager.UserHasAvatar(userInfo.ID);
        }

        public static Size GetPhotoSize(this UserInfo userInfo)
        {
            return UserPhotoManager.GetPhotoSize(userInfo.ID);
        }

        public static string GetPhotoURL(this UserInfo userInfo)
        {
            return UserPhotoManager.GetPhotoAbsoluteWebPath(userInfo.ID);
        }

        public static string GetRetinaPhotoURL(this UserInfo userInfo)
        {
            return UserPhotoManager.GetRetinaPhotoURL(userInfo.ID);
        }

        public static string GetMaxPhotoURL(this UserInfo userInfo)
        {
            return UserPhotoManager.GetMaxPhotoURL(userInfo.ID);
        }

        public static string GetBigPhotoURL(this UserInfo userInfo)
        {
            return UserPhotoManager.GetBigPhotoURL(userInfo.ID);
        }

        public static string GetMediumPhotoURL(this UserInfo userInfo)
        {
            return UserPhotoManager.GetMediumPhotoURL(userInfo.ID);
        }

        public static string GetSmallPhotoURL(this UserInfo userInfo)
        {
            return UserPhotoManager.GetSmallPhotoURL(userInfo.ID);
        }

        public static string RenderProfileLinkBase(this UserInfo userInfo)
        {
            var sb = new StringBuilder();

            //check for removed users
            if (userInfo.ID == Constants.LostUser.ID)
            {
                sb.AppendFormat("<span class='userLink text-medium-describe' style='white-space:nowrap;'>{0}</span>", userInfo.DisplayUserName());
            }
            else
            {
                var popupID = Guid.NewGuid();
                sb.AppendFormat("<span class=\"userLink\" style='white-space:nowrap;' id='{0}' data-uid='{1}'>", popupID, userInfo.ID);
                sb.AppendFormat("<a class='linkDescribe' href=\"{0}\">{1}</a>", userInfo.GetUserProfilePageURLGeneral(), userInfo.DisplayUserName());
                sb.Append("</span>");

                sb.AppendFormat("<script language='javascript'> StudioUserProfileInfo.RegistryElement('{0}','\"{1}\"); </script>", popupID, userInfo.ID);
            }
            return sb.ToString();
        }

        /// <summary>
        /// return absolute profile link
        /// </summary>
        /// <param name="userInfo"></param>        
        /// <returns></returns>
        private static string GetUserProfilePageURLGeneral(this UserInfo userInfo)
        {
            return CommonLinkUtility.GetUserProfile(userInfo.ID);
        }
    }
}