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


using ASC.Common.Security;
using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;
using ASC.Core.Billing;
using ASC.Core.Security.Authentication;
using ASC.Core.Security.Authorizing;
using ASC.Core.Users;
using ASC.Security.Cryptography;
using log4net;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Authentication;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace ASC.Core
{
    public static class SecurityContext
    {
        private readonly static ILog log = LogManager.GetLogger("ASC.Core");


        public static IAccount CurrentAccount
        {
            get { return Principal.Identity is IAccount ? (IAccount)Principal.Identity : Configuration.Constants.Guest; }
        }

        public static bool IsAuthenticated
        {
            get { return CurrentAccount.IsAuthenticated; }
        }

        public static IPermissionResolver PermissionResolver { get; private set; }


        static SecurityContext()
        {
            var azManager = new AzManager(new RoleProvider(), new PermissionProvider());
            PermissionResolver = new PermissionResolver(azManager);
        }


        public static string AuthenticateMe(string login, string password)
        {
            if (login == null) throw new ArgumentNullException("login");
            if (password == null) throw new ArgumentNullException("password");

            var tenantid = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var u = CoreContext.UserManager.GetUsers(tenantid, login, Hasher.Base64Hash(password, HashAlg.SHA256));

            return AuthenticateMe(new UserAccount(u, tenantid));
        }

        public static bool AuthenticateMe(string cookie)
        {
            if (!string.IsNullOrEmpty(cookie))
            {
                int tenant;
                Guid userid;
                string login;
                string password;
                if (CookieStorage.DecryptCookie(cookie, out tenant, out userid, out login, out password))
                {
                    if (tenant != CoreContext.TenantManager.GetCurrentTenant().TenantId)
                    {
                        return false;
                    }

                    try
                    {
                        if (userid != Guid.Empty)
                        {
                            AuthenticateMe(new UserAccount(new UserInfo { ID = userid }, tenant));
                        }
                        else
                        {
                            AuthenticateMe(login, password);
                        }
                        return true;
                    }
                    catch (InvalidCredentialException ice)
                    {
                        log.DebugFormat("{0}: cookie {1}, tenant {2}, userid {3}, login {4}, pass {5}",
                            ice.Message, cookie, tenant, userid, login, password);
                    }
                    catch (SecurityException se)
                    {
                        log.DebugFormat("{0}: cookie {1}, tenant {2}, userid {3}, login {4}, pass {5}",
                            se.Message, cookie, tenant, userid, login, password);
                    }
                    catch (Exception err)
                    {
                        log.ErrorFormat("Authenticate error: cookie {0}, tenant {1}, userid {2}, login {3}, pass {4}: {5}",
                            cookie, tenant, userid, login, password, err);
                    }
                }
                else
                {
                    log.WarnFormat("Can not decrypt cookie: {0}", cookie);
                }
            }
            return false;
        }

        public static string AuthenticateMe(IAccount account)
        {
            if (account == null || account.Equals(Configuration.Constants.Guest)) throw new InvalidCredentialException("account");

            var roles = new List<string> { Role.Everyone };
            string cookie = null;


            if (account is ISystemAccount && account.ID == Configuration.Constants.CoreSystem.ID)
            {
                roles.Add(Role.System);
            }

            if (account is IUserAccount)
            {
                var u = CoreContext.UserManager.GetUsers(account.ID);

                if (u.ID == Users.Constants.LostUser.ID)
                {
                    throw new InvalidCredentialException("Invalid username or password.");
                }
                if (u.Status != EmployeeStatus.Active)
                {
                    throw new SecurityException("Account disabled.");
                }
                // for LDAP users only
                if (u.Sid != null)
                {
                    if (u.Sid.StartsWith("l"))
                    {
                        throw new SecurityException("LDAP settings were changed.");
                    }
                    if (!CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId).Ldap)
                    {
                        throw new BillingException("Your tariff plan does not support this option.", "Ldap");
                    }
                }
                if (CoreContext.UserManager.IsUserInGroup(u.ID, Users.Constants.GroupAdmin.ID))
                {
                    roles.Add(Role.Administrators);
                }
                roles.Add(Role.Users);

                account = new UserAccount(u, CoreContext.TenantManager.GetCurrentTenant().TenantId);
                cookie = CookieStorage.EncryptCookie(CoreContext.TenantManager.GetCurrentTenant().TenantId, account.ID, null, null);
            }

            Principal = new GenericPrincipal(account, roles.ToArray());

            return cookie;
        }

        public static string AuthenticateMe(Guid userId)
        {
            return AuthenticateMe(CoreContext.Authentication.GetAccountByID(userId));
        }

        public static void Logout()
        {
            Principal = null;
        }

        public static void SetUserPassword(Guid userID, string password)
        {
            CoreContext.Authentication.SetUserPassword(userID, password);
        }


        public static bool CheckPermissions(params IAction[] actions)
        {
            return PermissionResolver.Check(CurrentAccount, actions);
        }

        public static bool CheckPermissions(ISecurityObject securityObject, params IAction[] actions)
        {
            return CheckPermissions(securityObject, null, actions);
        }

        public static bool CheckPermissions(ISecurityObjectId objectId, ISecurityObjectProvider securityObjProvider, params IAction[] actions)
        {
            return PermissionResolver.Check(CurrentAccount, objectId, securityObjProvider, actions);
        }

        public static void DemandPermissions(params IAction[] actions)
        {
            PermissionResolver.Demand(CurrentAccount, actions);
        }

        public static void DemandPermissions(ISecurityObject securityObject, params IAction[] actions)
        {
            DemandPermissions(securityObject, null, actions);
        }

        public static void DemandPermissions(ISecurityObjectId objectId, ISecurityObjectProvider securityObjProvider, params IAction[] actions)
        {
            PermissionResolver.Demand(CurrentAccount, objectId, securityObjProvider, actions);
        }


        private static IPrincipal Principal
        {
            get { return Thread.CurrentPrincipal; }
            set
            {
                Thread.CurrentPrincipal = value;
                if (HttpContext.Current != null) HttpContext.Current.User = value;
            }
        }
    }
}