/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Settings;

namespace ASC.Web.Core.Users
{
    [Serializable]
    [DataContract]
    public class DisplayUserSettings : ISettings
    {
        public Guid ID
        {
            get { return new Guid("2EF59652-E1A7-4814-BF71-FEB990149428"); }
        }

        [DataMember(Name = "IsDisableGettingStarted")]
        public bool IsDisableGettingStarted { get; set; }


        public ISettings GetDefault()
        {
            return new DisplayUserSettings
                {
                    IsDisableGettingStarted = false,
                };
        }

        public static string GetFullUserName(Guid userID)
        {
            return GetFullUserName(CoreContext.UserManager.GetUsers(userID));
        }

        public static string GetFullUserName(Guid userID, bool withHtmlEncode)
        {
            return GetFullUserName(CoreContext.UserManager.GetUsers(userID), withHtmlEncode);
        }

        public static string GetFullUserName(UserInfo userInfo)
        {
            return GetFullUserName(userInfo, DisplayUserNameFormat.Default, true);
        }

        public static string GetFullUserName(UserInfo userInfo, bool withHtmlEncode)
        {
            return GetFullUserName(userInfo, DisplayUserNameFormat.Default, withHtmlEncode);
        }

        public static string GetFullUserName(UserInfo userInfo, DisplayUserNameFormat format, bool withHtmlEncode)
        {
            if (userInfo == null)
            {
                return string.Empty;
            }
            if (!userInfo.ID.Equals(Guid.Empty) && !CoreContext.UserManager.UserExists(userInfo.ID))
            {
                return "profile removed";
            }
            var result = UserFormatter.GetUserName(userInfo, format);
            return withHtmlEncode ? result.HtmlEncode() : result;
        }
    }
}