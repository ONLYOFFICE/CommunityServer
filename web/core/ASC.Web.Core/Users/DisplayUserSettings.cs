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
using System.Configuration;
using System.Reflection;
using System.Runtime.Serialization;

using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Core.Users;

namespace ASC.Web.Core.Users
{
    [Serializable]
    [DataContract]
    public class DisplayUserSettings : BaseSettings<DisplayUserSettings>
    {
        private static readonly string RemovedProfileName = ConfigurationManagerExtension.AppSettings["web.removed-profile-name"] ?? "profile removed";

        public override Guid ID
        {
            get { return new Guid("2EF59652-E1A7-4814-BF71-FEB990149428"); }
        }

        [DataMember(Name = "IsDisableGettingStarted")]
        public bool IsDisableGettingStarted { get; set; }


        public override ISettings GetDefault()
        {
            return new DisplayUserSettings
            {
                IsDisableGettingStarted = false,
            };
        }

        public static string GetFullUserName(Guid userID, bool withHtmlEncode = true)
        {
            return GetFullUserName(CoreContext.UserManager.GetUsers(userID), withHtmlEncode);
        }

        public static string GetFullUserName(UserInfo userInfo, bool withHtmlEncode = true)
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
                try
                {
                    var resourceType = Type.GetType("Resources.Resource, ASC.Web.Studio");
                    var resourceProperty = resourceType.GetProperty("ProfileRemoved", BindingFlags.Static | BindingFlags.Public);
                    var resourceValue = (string)resourceProperty.GetValue(null);

                    return string.IsNullOrEmpty(resourceValue) ? RemovedProfileName : resourceValue;
                }
                catch (Exception)
                {
                    return RemovedProfileName;
                }
            }
            var result = UserFormatter.GetUserName(userInfo, format);
            return withHtmlEncode ? result.HtmlEncode() : result;
        }
    }
}