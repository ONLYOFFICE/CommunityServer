/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Runtime.Serialization;

using ASC.Api.Impl;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Api.Employee
{
    [DataContract(Name = "person", Namespace = "")]
    public class EmployeeWraper
    {
        protected EmployeeWraper()
        {
        }

        public EmployeeWraper(UserInfo userInfo)
            : this(userInfo, null)
        {
        }

        public EmployeeWraper(UserInfo userInfo, ApiContext context)
        {
            Id = userInfo.ID;
            DisplayName = DisplayUserSettings.GetFullUserName(userInfo);
            if (!string.IsNullOrEmpty(userInfo.Title))
            {
                Title = userInfo.Title;
            }

            if (EmployeeWraperFull.CheckContext(context, "avatarSmall"))
            {
                var smallPhotoUrl = UserPhotoManager.GetSmallPhotoURL(userInfo.ID);

                AvatarSmall = GetParametrizedPhotoUrl(smallPhotoUrl, userInfo);
            }
        }

        internal string GetParametrizedPhotoUrl(string photoUrl, UserInfo userInfo)
        {
            if (!UserPhotoManager.IsDeafaultPhoto(photoUrl))
            {
                photoUrl += "?_=" + userInfo.LastModified.GetHashCode();
            }

            return photoUrl;
        }

        ///<example>00000000-0000-0000-0000-000000000000</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        ///<example>Mike Zanyatski</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public string DisplayName { get; set; }

        ///<example>Manager</example>
        ///<order>11</order>
        [DataMember(Order = 11, EmitDefaultValue = false)]
        public string Title { get; set; }

        ///<example>url to small avatar</example>
        ///<order>20</order>
        [DataMember(Order = 20)]
        public string AvatarSmall { get; set; }

        ///<example>\/Products\/People\/Profile.aspx?user=administrator</example>
        ///<order>30</order>
        [DataMember(Order = 30)]
        public string ProfileUrl
        {
            get
            {
                if (Id == Guid.Empty) return string.Empty;
                var profileUrl = CommonLinkUtility.GetUserProfile(Id.ToString(), false);
                return CommonLinkUtility.GetFullAbsolutePath(profileUrl);
            }
        }

        public static EmployeeWraper Get(Guid userId)
        {
            try
            {
                return Get(CoreContext.UserManager.GetUsers(userId));
            }
            catch (Exception)
            {
                return Get(Core.Users.Constants.LostUser);
            }
        }

        public static EmployeeWraper Get(UserInfo userInfo)
        {
            return new EmployeeWraper(userInfo);
        }

        public static EmployeeWraper GetSample()
        {
            return new EmployeeWraper
            {
                Id = Guid.Empty,
                DisplayName = "Mike Zanyatski",
                Title = "Manager",
                AvatarSmall = "url to small avatar",
            };
        }
    }
}