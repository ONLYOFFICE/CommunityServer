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


using ASC.ActiveDirectory;
using ASC.ActiveDirectory.BuiltIn;
using ASC.ActiveDirectory.DirectoryServices;
using ASC.ActiveDirectory.Novell;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Web.Studio.Core.Import
{
    public static class ActiveDirectoryUserImporter
    {
        private static ILog log = LogManager.GetLogger(typeof(ActiveDirectoryUserImporter));

        public static bool TryGetLdapUserInfo(string login, string password, out UserInfo userInfo)
        {
            userInfo = ASC.Core.Users.Constants.LostUser;

            if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString())
                || CoreContext.Configuration.Standalone && !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap)
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
                    LdapSettingsChecker ldapSettingsChecker;
                    string currentLogin;
                    if (!WorkContext.IsMono)
                    {
                        ldapSettingsChecker = new SystemLdapSettingsChecker();
                        currentLogin = login;
                    }
                    else
                    {
                        currentLogin = GetEntryDN(settings, login);
                        if (currentLogin == null)
                        {
                            return false;
                        }
                        ldapSettingsChecker = new NovellLdapSettingsChecker();
                    }
                    ldapSettingsChecker.CheckCredentials(currentLogin, password, settings.Server, settings.PortNumber, settings.StartTls);
                }
                catch (Exception)
                {
                    return false;
                }

                if (login.Contains("\\"))
                {
                    login = login.Split('\\')[1];
                }
                var sid = importer.GetSidOfCurrentUser(login, settings);
                if (sid == null)
                {
                    return false;
                }
                List<GroupInfo> existingGroups;
                importer.GetDiscoveredGroupsByAttributes(settings, out existingGroups);
                if (importer.GetDiscoveredUser(settings, sid).Equals(ASC.Core.Users.Constants.LostUser))
                {
                    return false;
                }

                userInfo = CoreContext.UserManager.GetUserBySid("l" + sid);
                if (userInfo.Equals(ASC.Core.Users.Constants.LostUser))
                {
                    userInfo = CoreContext.UserManager.GetUserBySid(sid);
                    if (userInfo.Equals(ASC.Core.Users.Constants.LostUser))
                    {
                        userInfo = importer.GetDiscoveredUser(settings, sid);
                        if (userInfo.Equals(ASC.Core.Users.Constants.LostUser))
                        {
                            return false;
                        }
                        if (userInfo.FirstName == string.Empty)
                        {
                            userInfo.FirstName = Resource.FirstName;
                        }
                        if (userInfo.LastName == string.Empty)
                        {
                            userInfo.LastName = Resource.LastName;
                        }
                        try
                        {
                            SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                            var asVisitor = TenantStatisticsProvider.GetUsersCount() >= TenantExtra.GetTenantQuota().ActiveUsers;
                            userInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword(), true, false, asVisitor);

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
                    userInfo.Sid = sid;
                    try
                    {
                        SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                        var asVisitor = TenantStatisticsProvider.GetUsersCount() >= TenantExtra.GetTenantQuota().ActiveUsers;

                        userInfo = UserManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword(), true, false, asVisitor, false, false);
                    }
                    finally
                    {
                        SecurityContext.Logout();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unexpected error: {0}", e);
                userInfo = ASC.Core.Users.Constants.LostUser;
                return false;
            }
        }

        private static string GetEntryDN(LDAPSupportSettings settings, string login)
        {
            LdapHelper ldapHelper = new NovellLdapHelper();
            var users = ldapHelper.GetUsersByAttributesAndFilter(
                settings, "(" + settings.LoginAttribute + "=" + login + ")");
            if (users.Count == 0)
            {
                return null;
            }
            var currentUser = users.FirstOrDefault(user => user != null);
            return currentUser == null ? null : currentUser.DistinguishedName;
        }

        public static bool LdapIsEnable
        {
            get
            {
                return SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()) &&
                       (!CoreContext.Configuration.Standalone || CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap) &&
                       SettingsManager.Instance.LoadSettings<LDAPSupportSettings>(TenantProvider.CurrentTenantID).EnableLdapAuthentication;
            }
        }
    }
}