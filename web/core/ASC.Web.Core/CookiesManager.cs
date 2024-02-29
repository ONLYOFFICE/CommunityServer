/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using ASC.Core.Data;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core.Client;
using ASC.Web.Studio.Utility;

using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Core
{
    public enum CookiesType
    {
        AuthKey,
        SocketIO,
        ShareLink,
        ComebackAuthKey,
        CurrentShareLink,
        AnonymousSessionKey,
        ModeThemeKey
    }

    public class CookiesManager
    {
        private const string AuthCookiesName = "asc_auth_key";
        private const string SocketIOCookiesName = "socketio.sid";
        private const string ShareLinkCookiesName = "sharelink";
        private const string ComebackAuthKeyCookiesName = "comeback_auth_key";
        private const string AnonymousSessionKeyCookiesName = "anonymous_session_key";
        private const string ModeThemeKeyCookiesName = "mode_theme_key";

        private static string GetCookiesName(CookiesType type)
        {
            switch (type)
            {
                case CookiesType.AuthKey: return AuthCookiesName;
                case CookiesType.SocketIO: return SocketIOCookiesName;
                case CookiesType.ShareLink: return ShareLinkCookiesName;
                case CookiesType.ComebackAuthKey: return ComebackAuthKeyCookiesName;
                case CookiesType.AnonymousSessionKey:return AnonymousSessionKeyCookiesName;
                case CookiesType.ModeThemeKey: return ModeThemeKeyCookiesName;
            }

            return string.Empty;
        }

        public static string GetCookiesName(CookiesType type, string itemId)
        {
            var cookieName = GetCookiesName(type);

            if (!string.IsNullOrEmpty(itemId))
            {
                cookieName += itemId;
            }

            return cookieName;
        }

        public static string GetRequestVar(CookiesType type)
        {
            if (HttpContext.Current == null) return "";

            var cookieName = GetCookiesName(type);

            var cookie = HttpContext.Current.Request.QueryString[cookieName] ?? HttpContext.Current.Request.Form[cookieName];

            return string.IsNullOrEmpty(cookie) ? GetCookies(type) : cookie;
        }

        public static void SetCookies(CookiesType type, string value, bool session = false)
        {
            SetCookies(type, null, value, null, session, type != CookiesType.SocketIO);
        }

        public static void SetCookies(CookiesType type, string value, string domain, bool session = false)
        {
            SetCookies(type, null, value, domain, session, type != CookiesType.SocketIO);
        }

        public static void SetCookies(CookiesType type, string itemId, string value, string domain, bool session = false, bool httpOnly = false)
        {
            if (HttpContext.Current == null) return;

            var cookieName = GetCookiesName(type, itemId);

            HttpContext.Current.Response.Cookies[cookieName].Value = value;
            HttpContext.Current.Response.Cookies[cookieName].Expires = GetExpiresDate(session);

            if (!string.IsNullOrEmpty(domain))
            {
                HttpContext.Current.Response.Cookies[cookieName].Domain = domain;
            }

            if (httpOnly)
            {
                HttpContext.Current.Response.Cookies[cookieName].HttpOnly = true;

                if (HttpContext.Current.Request.GetUrlRewriter().Scheme == "https")
                {
                    HttpContext.Current.Response.Cookies[cookieName].Secure = true;

                    if (ClientSettings.SameSiteCookieEnabled)
                    {
                        // SameSite is not support by Mono yet (https://github.com/mono/mono/issues/18711)
                        // HttpContext.Current.Response.Cookies[cookieName].SameSite = SameSiteMode.None;

                        var cookies = HttpContext.Current.Response.Cookies[cookieName];

                        var property = cookies.GetType().GetProperty("SameSite");

                        if (property != null)
                        {
                            property.SetValue(cookies, 0);
                        }
                        else
                        {
                            cookies.Path = "/; SameSite=None";
                        }
                    }
                }
            }
        }

        public static HttpCookie GetCookie(CookiesType type, string itemId)
        {
            if (HttpContext.Current == null)
            {
                return null;
            }

            var cookieName = GetCookiesName(type, itemId);

            return HttpContext.Current.Request.Cookies[cookieName];
        }

        public static string GetCookies(CookiesType type)
        {
            return GetCookies(type, null);
        }

        public static string GetCookies(CookiesType type, string itemId, bool allowHeader = false)
        {
            if (HttpContext.Current != null)
            {
                var cookieName = GetCookiesName(type, itemId);

                var cookie = HttpContext.Current.Request.Cookies[cookieName];
                if (cookie != null)
                {
                    return cookie.Value ?? "";
                }

                if (allowHeader)
                {
                    return HttpContext.Current.Request.Headers[cookieName] ?? "";
                }
            }
            return "";
        }

        public static void ClearCookies(CookiesType type)
        {
            ClearCookies(type, null);
        }

        public static void ClearCookies(CookiesType type, string itemId)
        {
            if (HttpContext.Current == null) return;

            var cookieName = GetCookiesName(type, itemId);

            if (HttpContext.Current.Request.Cookies[cookieName] != null)
                HttpContext.Current.Response.Cookies[cookieName].Expires = DateTime.Now.AddDays(-3);
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

            if (lifeTime > 0)
            {
                DbLoginEventsManager.LogOutAllActiveConnectionsForTenant(tenant);
            }

            var userId = SecurityContext.CurrentAccount.ID;
            AuthenticateMeAndSetCookies(tenant, userId, MessageAction.LoginSuccess);
        }

        public static int GetLifeTime()
        {
            return TenantCookieSettings.GetForTenant(TenantProvider.CurrentTenantID).LifeTime;
        }

        public static void ResetUserCookie(Guid? userId = null)
        {
            var targetUserId = userId ?? SecurityContext.CurrentAccount.ID;
            var tenant = TenantProvider.CurrentTenantID;
            var settings = TenantCookieSettings.GetForUser(targetUserId);
            settings.Index = settings.Index + 1;
            TenantCookieSettings.SetForUser(targetUserId, settings);

            DbLoginEventsManager.LogOutAllActiveConnections(tenant, targetUserId);

            if (targetUserId == SecurityContext.CurrentAccount.ID)
            {
                AuthenticateMeAndSetCookies(tenant, targetUserId, MessageAction.LoginSuccess);
            }
        }

        public static void ResetTenantCookie()
        {
            var userId = SecurityContext.CurrentAccount.ID;

            if (!CoreContext.UserManager.IsUserInGroup(userId, Constants.GroupAdmin.ID))
            {
                throw new SecurityException();
            }

            var tenant = TenantProvider.CurrentTenantID;
            var settings = TenantCookieSettings.GetForTenant(tenant);
            settings.Index = settings.Index + 1;
            TenantCookieSettings.SetForTenant(tenant, settings);

            DbLoginEventsManager.LogOutAllActiveConnectionsForTenant(tenant);

            AuthenticateMeAndSetCookies(tenant, userId, MessageAction.LoginSuccess);
        }

        public static string AuthenticateMeAndSetCookies(int tenantId, Guid userId, MessageAction action, bool session = false)
        {
            bool isSuccess = true;
            var cookies = string.Empty;
            Func<int> funcLoginEvent = () => { return GetLoginEventId(action); };

            try
            {
                cookies = SecurityContext.AuthenticateMe(userId, funcLoginEvent);
            }
            catch (Exception)
            {
                isSuccess = false;
                throw;
            }
            finally
            {
                if (isSuccess)
                {
                    SetCookies(CookiesType.AuthKey, cookies, session);
                    DbLoginEventsManager.ResetCache(tenantId, userId);
                }
            }

            return cookies;
        }

        public static void AuthenticateMeAndSetCookies(string login, string passwordHash, MessageAction action, bool session = false)
        {
            bool isSuccess = true;
            var cookies = string.Empty;
            Func<int> funcLoginEvent = () => { return GetLoginEventId(action); };

            try
            {
                cookies = SecurityContext.AuthenticateMe(login, passwordHash, funcLoginEvent);
            }
            catch (Exception)
            {
                isSuccess = false;
                throw;
            }
            finally
            {
                if (isSuccess)
                {
                    SetCookies(CookiesType.AuthKey, cookies, session);
                    DbLoginEventsManager.ResetCache();
                }
            }
        }

        public static int GetLoginEventId(MessageAction action)
        {
            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var userId = SecurityContext.CurrentAccount.ID;
            var data = new MessageUserData(tenantId, userId);

            return MessageService.SendLoginMessage(HttpContext.Current == null ? null : HttpContext.Current.Request, data, action);
        }
    }
}