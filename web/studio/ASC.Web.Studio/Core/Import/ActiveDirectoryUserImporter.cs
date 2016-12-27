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
using ASC.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using log4net;
using System;
using Constants = ASC.ActiveDirectory.Constants;

namespace ASC.Web.Studio.Core.Import
{
    public static class ActiveDirectoryUserImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ActiveDirectoryUserImporter));

        public static bool TryGetLdapUserInfo(string login, string password, out UserInfo userInfo)
        {
            userInfo = ASC.Core.Users.Constants.LostUser;

            try
            {
                if (!LdapIsEnable)
                {
                    return false;
                }

                var settings = SettingsManager.Instance.LoadSettings<LDAPSupportSettings>(TenantProvider.CurrentTenantID);
                if (!settings.EnableLdapAuthentication)
                {
                    return false;
                }

                var importer = new LDAPUserImporter(settings);

                var ldapUserInfo = ASC.Core.Users.Constants.LostUser;
                try
                {
                    var ldapSettingsChecker = WorkContext.IsMono 
                        ? new NovellLdapSettingsChecker()
                        : new SystemLdapSettingsChecker() as LdapSettingsChecker;

                    var parsedLogin = ldapSettingsChecker.ParseLogin(login);

                    var ldapUsers = importer.FindLdapUsers(parsedLogin);

                    foreach (var ldapUser in ldapUsers)
                    {
                        try
                        {
                            ldapUserInfo = ldapUser.Key;
                            var ldapUserObject = ldapUser.Value;

                            if (ldapUserInfo.Equals(ASC.Core.Users.Constants.LostUser)
                                || ldapUserObject == null
                                || string.IsNullOrEmpty(ldapUserObject.DistinguishedName))
                            {
                                continue;
                            }

                            string currentLogin;

                            if (!WorkContext.IsMono)
                            {
                                currentLogin =
                                    ldapUserObject.InvokeGet(Constants.ADSchemaAttributes.ACCOUNT_NAME) as string;
                            }
                            else
                            {
                                currentLogin = ldapUserObject.DistinguishedName;
                            }

                            ldapSettingsChecker.CheckCredentials(currentLogin, password, settings.Server,
                                settings.PortNumber,
                                settings.StartTls);

                            break;

                        }
                        catch (Exception)
                        {
                            ldapUserInfo = ASC.Core.Users.Constants.LostUser;
                        }
                    }

                    if (ldapUserInfo.Equals(ASC.Core.Users.Constants.LostUser))
                        return false;

                }
                catch (Exception)
                {
                    return false;
                }

                if (settings.GroupMembership && !importer.IsUserExistsInGroups(ldapUserInfo))
                {
                    return false;
                }

                try
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                    userInfo = UserManagerWrapper.SyncUserLDAP(ldapUserInfo);

                    if (userInfo == null || userInfo.Equals(ASC.Core.Users.Constants.LostUser))
                    {
                        return false;
                    }

                    userInfo.Sid = ldapUserInfo.Sid;

                    importer.SyncUserGroupMembership(userInfo);
                }
                finally
                {
                    SecurityContext.Logout();
                }

                return true;
            }
            catch (Exception e)
            {
                Log.ErrorFormat("TryGetLdapUserInfo(login: '{0}') Unexpected error: {1}", login, e);
                userInfo = ASC.Core.Users.Constants.LostUser;
                return false;
            }
        }

        public static bool LdapIsEnable
        {
            get
            {
                try
                {
                    if (!SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString()) ||
                        (CoreContext.Configuration.Standalone &&
                         !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap))
                    {
                        return false;
                    }

                    var enabled =
                                SettingsManager.Instance.LoadSettings<LDAPSupportSettings>(
                                    TenantProvider.CurrentTenantID)
                                    .EnableLdapAuthentication;

                    return enabled;
                }
                catch(Exception ex)
                {
                    Log.Error("[LDAP] LdapIsEnable: " + ex);
                    return false;
                }
            }
        }    
    }
}