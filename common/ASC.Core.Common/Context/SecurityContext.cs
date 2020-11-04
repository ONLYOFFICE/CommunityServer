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
using System.Collections.Generic;
using System.Security;
using System.Security.Authentication;
using System.Security.Principal;
using System.Threading;
using System.Web;
using ASC.Common.Logging;
using ASC.Common.Security;
using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;
using ASC.Core.Billing;
using ASC.Core.Security.Authentication;
using ASC.Core.Security.Authorizing;
using ASC.Core.Tenants;
using ASC.Core.Users;

namespace ASC.Core
{
    public static class SecurityContext
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Core");


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


        public static string AuthenticateMe(string login, string passwordHash)
        {
            if (login == null) throw new ArgumentNullException("login");
            if (passwordHash == null) throw new ArgumentNullException("passwordHash");

            var tenantid = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var u = CoreContext.UserManager.GetUsersByPasswordHash(tenantid, login, passwordHash);

            return AuthenticateMe(new UserAccount(u, tenantid));
        }

        public static bool AuthenticateMe(string cookie)
        {
            if (!string.IsNullOrEmpty(cookie))
            {
                int tenant;
                Guid userid;
                int indexTenant;
                DateTime expire;
                int indexUser;

                if (cookie.Equals("Bearer", StringComparison.InvariantCulture))
                {
                    var ipFrom = string.Empty;
                    var address = string.Empty;
                    if (HttpContext.Current != null)
                    {
                        var request = HttpContext.Current.Request;
                        ipFrom = "from " + (request.Headers["X-Forwarded-For"] ?? request.UserHostAddress);
                        address = "for " + request.GetUrlRewriter();
                    }
                    log.InfoFormat("Empty Bearer cookie: {0} {1}", ipFrom, address);
                }
                else if (CookieStorage.DecryptCookie(cookie, out tenant, out userid, out indexTenant, out expire, out indexUser))
                {
                    if (tenant != CoreContext.TenantManager.GetCurrentTenant().TenantId)
                    {
                        return false;
                    }

                    var settingsTenant = TenantCookieSettings.GetForTenant(tenant);
                    if (indexTenant != settingsTenant.Index)
                    {
                        return false;
                    }

                    if (expire != DateTime.MaxValue && expire < DateTime.UtcNow)
                    {
                        return false;
                    }

                    try
                    {
                        var settingsUser = TenantCookieSettings.GetForUser(userid);
                        if (indexUser != settingsUser.Index)
                        {
                            return false;
                        }

                        AuthenticateMe(new UserAccount(new UserInfo { ID = userid }, tenant));
                        return true;
                    }
                    catch (InvalidCredentialException ice)
                    {
                        log.DebugFormat("{0}: cookie {1}, tenant {2}, userid {3}",
                                        ice.Message, cookie, tenant, userid);
                    }
                    catch (SecurityException se)
                    {
                        log.DebugFormat("{0}: cookie {1}, tenant {2}, userid {3}",
                                        se.Message, cookie, tenant, userid);
                    }
                    catch (Exception err)
                    {
                        log.ErrorFormat("Authenticate error: cookie {0}, tenant {1}, userid {2}: {5}",
                                        cookie, tenant, userid, err);
                    }
                }
                else
                {
                    var ipFrom = string.Empty;
                    var address = string.Empty;
                    if (HttpContext.Current != null)
                    {
                        var request = HttpContext.Current.Request;
                        address = "for " + request.GetUrlRewriter();
                        ipFrom = "from " + (request.Headers["X-Forwarded-For"] ?? request.UserHostAddress);
                    }
                    log.WarnFormat("Can not decrypt cookie: {0} {1} {2}", cookie, ipFrom, address);
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
                cookie = CookieStorage.EncryptCookie(CoreContext.TenantManager.GetCurrentTenant().TenantId, account.ID);
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

        public static void SetUserPasswordHash(Guid userID, string passwordHash)
        {
            var tenantid = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var u = CoreContext.UserManager.GetUsersByPasswordHash(tenantid, userID.ToString(), passwordHash);
            if (!Equals(u, Users.Constants.LostUser))
            {
                throw new PasswordException("A new password must be used");
            }

            CoreContext.Authentication.SetUserPasswordHash(userID, passwordHash);
        }

        public class PasswordException : Exception
        {
            public PasswordException(string message) : base(message)
            {
            }
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