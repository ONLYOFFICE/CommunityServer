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

using ASC.ActiveDirectory;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using System;

namespace ASC.Web.Studio.Core.Import
{
	public static class ActiveDirectoryUserImporter
    {
        public static bool TryLdapAuth(string login, string password)
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()))
            {
                return false;
            }
            
            var settings = SettingsManager.Instance.LoadSettings<LDAPSupportSettings>(TenantProvider.CurrentTenantID);
            if (!settings.EnableLdapAuthentication)
            {
                return false;
            }
            try
            {
                var importer = new LDAPUserImporter();
                try
                {
                    ADDomain.CheckCredentials(login, password, settings.Server, settings.PortNumber);
                }
                catch (Exception)
                {
                    return false;
                }

                var sid = importer.GetSidOfCurrentUser(login, settings);
                if (sid == null)
                {
                    return false;
                }
                importer.GetDiscoveredGroupsByAttributes(settings);
                var userInfo = CoreContext.UserManager.GetUserBySid( "l" + sid);
                if (userInfo == ASC.Core.Users.Constants.LostUser)
                {
                    userInfo = CoreContext.UserManager.GetUserBySid(sid);
                    if (userInfo == ASC.Core.Users.Constants.LostUser)
                    {
                        userInfo = importer.GetDiscoveredUser(settings, sid);
                        if (userInfo == ASC.Core.Users.Constants.LostUser)
                        {
                            return false;
                        }
                        try
                        {
                            SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                            if (TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers)
                            {
                                userInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword(), true, false);
                            }
                            else
                            {
                                userInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword(), true, false, true);
                            }
                            importer.AddUserIntoGroups(userInfo, settings);
                            importer.AddUserInCacheGroups(userInfo);
                        }
                        finally
                        {
                            SecurityContext.Logout();
                        }
                    }
                }
                else
                {
                    if (importer.GetDiscoveredUser(settings, sid) == ASC.Core.Users.Constants.LostUser)
                    {
                        return false;
                    }
                    userInfo.Sid = sid;
                    try
                    {
                        SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                        if (TenantStatisticsProvider.GetUsersCount() < TenantExtra.GetTenantQuota().ActiveUsers)
                        {
                            userInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword(), true, false);
                        }
                        else
                        {
                            userInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword(), true, false, true);
                        }
                    }
                    finally
                    {
                        SecurityContext.Logout();
                    }
                }
                var cookiesKey = SecurityContext.AuthenticateMe(userInfo.ID);
                CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
                return true;
            }
            catch (Exception e)
            {
                ADDomain.LogError(e.Message);
                return false;
            }
        }
    }
}