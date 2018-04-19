/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Linq;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.Common.Utils;
using ASC.VoipService.Dao;

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
                    name = x.Name
                }).ToList();

            var currentTenant = CoreContext.TenantManager.GetCurrentTenant();
            var hubToken = Signature.Create(string.Join(",", currentTenant.TenantId, SecurityContext.CurrentAccount.ID, currentTenant.TenantAlias));
            var hubUrl = ConfigurationManager.AppSettings["web.hub"] ?? string.Empty;
            if (hubUrl != string.Empty)
            {
                if (!hubUrl.EndsWith("/"))
                {
                    hubUrl += "/";
                }
            }
            var hubLogging = ConfigurationManager.AppSettings["web.chat.logging"] ?? "false";
            var webChat = ConfigurationManager.AppSettings["web.chat"] ?? "false";
            var voipEnabled = VoipDao.ConfigSettingsExist;

            return new List<KeyValuePair<string, object>>(1)
                   {
                       RegisterObject(
                            new
                                {
                                    Hub = new { Token = hubToken, Url = hubUrl, WebChat = webChat, VoipEnabled = voipEnabled, Logging = hubLogging },
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
                groups = CoreContext.UserManager.GetUserGroups(userInfo.ID).Select(x => new
                {
                    id = x.ID,
                    name = x.Name,
                    manager = CoreContext.UserManager.GetUsers(CoreContext.UserManager.GetDepartmentManager(x.ID)).UserName
                }).ToList(),
                isPending = userInfo.ActivationStatus == EmployeeActivationStatus.Pending,
                isActivated = userInfo.ActivationStatus == EmployeeActivationStatus.Activated,
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