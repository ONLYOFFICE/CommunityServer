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
            var userInfoList = new List<UserInfo> {CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID)};
            var groupInfoList = new List<GroupInfo>();

            if (SecurityContext.IsAuthenticated && !CoreContext.Configuration.Personal)
            {
                userInfoList = CoreContext.UserManager.GetUsers(EmployeeStatus.Active).ToList();

                groupInfoList = CoreContext.UserManager.GetDepartments().ToList();
            }

            var user = PrepareUserInfo(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID));

            var users = userInfoList.Select(PrepareUserInfo).ToList();

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
                // hubUrl = CommonLinkUtility.GetFullAbsolutePath(hubUrl);
                if (!hubUrl.EndsWith("/"))
                {
                    hubUrl += "/";
                }
                hubUrl += "signalr";
            }
            var hubLogging = ConfigurationManager.AppSettings["web.chat.logging"] ?? "false";
            var webChat = ConfigurationManager.AppSettings["web.chat"] ?? "false";
            var voipEnabled = ConfigurationManager.AppSettings["voip.enabled"] ?? "false";
            yield return RegisterObject("Hub",
                new { Token = hubToken, Url = hubUrl, WebChat = webChat, VoipEnabled = voipEnabled, Logging = hubLogging });

            yield return RegisterObject("ApiResponsesMyProfile", new {response = user});
            yield return RegisterObject("ApiResponses_Profiles", new {response = users});
            yield return RegisterObject("ApiResponses_Groups", new {response = groups});
        }

        protected override string GetCacheHash()
        {
            /* return users and groups last mod time */
            return SecurityContext.CurrentAccount.ID +
                   (SecurityContext.IsAuthenticated && !CoreContext.Configuration.Personal
                        ? (CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture) +
                           CoreContext.GroupManager.GetMaxGroupsLastModified().Ticks.ToString(CultureInfo.InvariantCulture))
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
                isVisitor = userInfo.IsVisitor(),
                isOutsider = userInfo.IsOutsider(),
                isAdmin = userInfo.IsAdmin(),
                isOwner = userInfo.IsOwner(),
                contacts = GetContacts(userInfo),
                created = userInfo.CreateDate
            };
        }
    }
}