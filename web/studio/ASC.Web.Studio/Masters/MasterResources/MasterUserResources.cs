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
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.VoipService.Dao;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Masters.MasterResources
{
    public class MasterUserResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Resources.Master"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var activeUserInfoList = new List<UserInfo>();
            var disabledUserInfoList = new List<UserInfo>();
            var groupInfoList = new List<GroupInfo>();

            if (SecurityContext.IsAuthenticated && !CoreContext.Configuration.Personal)
            {
                var allUsers = CoreContext.UserManager.GetUsers(EmployeeStatus.All);

                foreach (var userInfo in allUsers)
                {
                    if (userInfo.Status == EmployeeStatus.Active)
                        activeUserInfoList.Add(userInfo);
                    else
                        disabledUserInfoList.Add(userInfo);
                }

                groupInfoList = CoreContext.UserManager.GetDepartments().ToList();
            }
            else
            {
                activeUserInfoList.Add(user);
            }

            var activeUsers = activeUserInfoList.Select(PrepareUserInfo);
            var disabledUsers = disabledUserInfoList.Select(PrepareUserInfo);

            var groups = groupInfoList.Select(x => new
            {
                id = x.ID,
                name = x.Name,
                manager = CoreContext.UserManager.GetDepartmentManager(x.ID)
            });

            var hubUrl = ConfigurationManagerExtension.AppSettings["web.hub"] ?? string.Empty;
            if (hubUrl != string.Empty)
            {
                if (!hubUrl.EndsWith("/"))
                {
                    hubUrl += "/";
                }
            }
            var hubLogging = ConfigurationManagerExtension.AppSettings["web.chat.logging"] ?? "false";
            var webChat = ConfigurationManagerExtension.AppSettings["web.chat"] ?? "false";
            var voipAllowed = SetupInfo.VoipEnabled;
            var voipEnabled = voipAllowed && VoipDao.ConfigSettingsExist;

            return new List<KeyValuePair<string, object>>(1)
                   {
                       RegisterObject(
                            new
                                {
                                    Hub = new { Url = hubUrl, WebChat = webChat, VoipAllowed = voipAllowed, VoipEnabled = voipEnabled, Logging = hubLogging },
                                    ApiResponsesMyProfile = new {response = PrepareUserInfo(user)},
                                    ApiResponsesRemovedProfile = new {response = PrepareUserInfo(Constants.LostUser)},
                                    ApiResponses_ActiveProfiles = new {response = activeUsers},
                                    ApiResponses_DisabledProfiles = new {response = disabledUsers},
                                    ApiResponses_Groups = new {response = groups}
                                })
                   };
        }

        protected override string GetCacheHash()
        {
            /* return users and groups last mod time */
            return SecurityContext.CurrentAccount.ID +
                   (SecurityContext.IsAuthenticated && !CoreContext.Configuration.Personal
                        ? (CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture) +
                           CoreContext.UserManager.GetMaxGroupsLastModified().Ticks.ToString(CultureInfo.InvariantCulture))
                        : string.Empty);
        }

        private static List<object> GetContacts(UserInfo userInfo)
        {
            var contacts = new List<object>();
            for (var i = 0; i < userInfo.Contacts.Count; i += 2)
            {
                if (i + 1 < userInfo.Contacts.Count)
                {
                    contacts.Add(new {type = userInfo.Contacts[i], value = userInfo.Contacts[i + 1]});
                }
            }

            return contacts.Any() ? contacts : null;
        }

        private static object PrepareUserInfo(UserInfo userInfo)
        {
            return new
            {
                id = userInfo.ID,
                displayName = DisplayUserSettings.GetFullUserName(userInfo),
                title = userInfo.Title,
                avatarSmall = UserPhotoManager.GetSmallPhotoURL(userInfo.ID),
                avatarBig = UserPhotoManager.GetBigPhotoURL(userInfo.ID),
                profileUrl = CommonLinkUtility.ToAbsolute(CommonLinkUtility.GetUserProfile(userInfo.ID.ToString(), false)),
                groups = CoreContext.UserManager.GetUserGroupsId(userInfo.ID),
                isPending = userInfo.ActivationStatus == EmployeeActivationStatus.Pending,
                isActivated = userInfo.ActivationStatus.HasFlag(EmployeeActivationStatus.Activated),
                isVisitor = userInfo.IsVisitor(),
                isOutsider = userInfo.IsOutsider(),
                isAdmin = userInfo.IsAdmin(),
                isOwner = userInfo.IsOwner(),
                contacts = GetContacts(userInfo),
                created = userInfo.CreateDate,
                email = userInfo.Email,
                isLDAP = userInfo.IsLDAP(),
                isSSO = userInfo.IsSSO(),
                isTerminated = userInfo.Status == EmployeeStatus.Terminated
            };
        }
    }
}