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

using ASC.Common.Security;
using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;
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
            if (cookie == null) throw new ArgumentNullException("cookie");

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
                catch (Exception err)
                {
                    LogManager.GetLogger("ASC.Core").ErrorFormat("Authenticate error: cookie {0}, tenant {1}, userid {2}, login {3}, pass {4}: {5}",
                        cookie, tenant, userid, login, password, err);
                }
            }
            else
            {
                LogManager.GetLogger("ASC.Core").WarnFormat("Can not decrypt cookie: {0}", cookie);
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
                if (u.Sid != null && u.Sid.StartsWith("l"))
                {
                    throw new SecurityException("LDAP settings were changed.");
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