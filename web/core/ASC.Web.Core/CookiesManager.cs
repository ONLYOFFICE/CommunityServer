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
using System.Security;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Core
{
    public enum CookiesType
    {
        AuthKey,
        SocketIO
    }

    public class CookiesManager
    {
        private const string AuthCookiesName = "asc_auth_key";
        private const string SocketIOCookiesName = "socketio.sid";

        private static string GetCookiesName(CookiesType type)
        {
            switch (type)
            {
                case CookiesType.AuthKey: return AuthCookiesName;
                case CookiesType.SocketIO: return SocketIOCookiesName;
            }

            return string.Empty;
        }

        public static string GetRequestVar(CookiesType type)
        {
            if (HttpContext.Current == null) return "";

            var cookie = HttpContext.Current.Request.QueryString[GetCookiesName(type)] ?? HttpContext.Current.Request.Form[GetCookiesName(type)];

            return string.IsNullOrEmpty(cookie) ? GetCookies(type) : cookie;
        }

        public static void SetCookies(CookiesType type, string value, bool session = false)
        {
            if (HttpContext.Current == null) return;

            HttpContext.Current.Response.Cookies[GetCookiesName(type)].Value = value;
            HttpContext.Current.Response.Cookies[GetCookiesName(type)].Expires = GetExpiresDate(session);

            if (type == CookiesType.AuthKey)
            {
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].HttpOnly = true;

                if (HttpContext.Current.Request.GetUrlRewriter().Scheme == "https")
                    HttpContext.Current.Response.Cookies[GetCookiesName(type)].Secure = true;

            }
        }

        public static void SetCookies(CookiesType type, string value, string domain, bool session = false)
        {
            if (HttpContext.Current == null) return;

            HttpContext.Current.Response.Cookies[GetCookiesName(type)].Value = value;
            HttpContext.Current.Response.Cookies[GetCookiesName(type)].Domain = domain;
            HttpContext.Current.Response.Cookies[GetCookiesName(type)].Expires = GetExpiresDate(session);

            if (type == CookiesType.AuthKey)
            {
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].HttpOnly = true;

                if (HttpContext.Current.Request.GetUrlRewriter().Scheme == "https")
                    HttpContext.Current.Response.Cookies[GetCookiesName(type)].Secure = true;
            }
        }

        public static string GetCookies(CookiesType type)
        {
            if (HttpContext.Current != null)
            {
                var cookieName = GetCookiesName(type);

                if (HttpContext.Current.Request.Cookies[cookieName] != null)
                    return HttpContext.Current.Request.Cookies[cookieName].Value ?? "";
            }
            return "";
        }

        public static void ClearCookies(CookiesType type)
        {
            if (HttpContext.Current == null) return;

            if (HttpContext.Current.Request.Cookies[GetCookiesName(type)] != null)
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Expires = DateTime.Now.AddDays(-3);
        }

        private static DateTime GetExpiresDate(bool session)
        {
            var expires = DateTime.MinValue;

            if (!session)
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId;
                expires = TenantCookieSettings.GetExpiresTime(tenant);
            }

            return expires;
        }

        public static void SetLifeTime(int lifeTime)
        {
            if (!CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID))
            {
                throw new SecurityException();
            }

            var tenant = TenantProvider.CurrentTenantID;
            var settings = TenantCookieSettings.GetForTenant(tenant);

            if (lifeTime > 0)
            {
                settings.Index = settings.Index + 1;
                settings.LifeTime = lifeTime;
            }
            else
            {
                settings.LifeTime = 0;
            }

            TenantCookieSettings.SetForTenant(tenant, settings);

            var cookie = SecurityContext.AuthenticateMe(SecurityContext.CurrentAccount.ID);

            SetCookies(CookiesType.AuthKey, cookie);
        }

        public static int GetLifeTime()
        {
            return TenantCookieSettings.GetForTenant(TenantProvider.CurrentTenantID).LifeTime;
        }

        public static void ResetUserCookie(Guid? userId = null)
        {
            var settings = TenantCookieSettings.GetForUser(userId ?? SecurityContext.CurrentAccount.ID);
            settings.Index = settings.Index + 1;
            TenantCookieSettings.SetForUser(userId ?? SecurityContext.CurrentAccount.ID, settings);

            if (!userId.HasValue)
            {
                var cookie = SecurityContext.AuthenticateMe(SecurityContext.CurrentAccount.ID);

                SetCookies(CookiesType.AuthKey, cookie);
            }
        }

        public static void ResetTenantCookie()
        {
            if (!CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID))
            {
                throw new SecurityException();
            }

            var tenant = TenantProvider.CurrentTenantID;
            var settings = TenantCookieSettings.GetForTenant(tenant);
            settings.Index = settings.Index + 1;
            TenantCookieSettings.SetForTenant(tenant, settings);

            var cookie = SecurityContext.AuthenticateMe(SecurityContext.CurrentAccount.ID);
            SetCookies(CookiesType.AuthKey, cookie);
        }
    }
}