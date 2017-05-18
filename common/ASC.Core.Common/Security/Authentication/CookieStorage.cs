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


using System;
using System.Globalization;
using System.Web;
using ASC.Security.Cryptography;
using ASC.Core.Tenants;
using log4net;

namespace ASC.Core.Security.Authentication
{
    class CookieStorage
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss,fff";

        public static bool DecryptCookie(string cookie, out int tenant, out Guid userid, out string login, out string password, out int indexTenant, out DateTime expire, out int indexUser)
        {
            tenant = Tenant.DEFAULT_TENANT;
            userid = Guid.Empty;
            login = null;
            password = null;
            indexTenant = 0;
            expire = DateTime.MaxValue;
            indexUser = 0;

            if (string.IsNullOrEmpty(cookie))
            {
                return false;
            }

            try
            {
                cookie = (HttpUtility.UrlDecode(cookie) ?? "").Replace(' ', '+');
                var s = InstanceCrypto.Decrypt(cookie).Split('$');

                if (0 < s.Length) login = s[0];
                if (1 < s.Length) tenant = int.Parse(s[1]);
                if (2 < s.Length) password = s[2];
                if (4 < s.Length) userid = new Guid(s[4]);
                if (5 < s.Length) indexTenant = int.Parse(s[5]);
                if (6 < s.Length) expire = DateTime.ParseExact(s[6], DateTimeFormat, CultureInfo.InvariantCulture);
                if (7 < s.Length) indexUser = int.Parse(s[7]);

                return true;
            }
            catch(Exception err)
            {
                LogManager.GetLogger("ASC.Core").ErrorFormat("Authenticate error: cookie {0}, tenant {1}, userid {2}, login {3}, pass {4}, indexTenant {5}, expire {6}: {7}",
                    cookie, tenant, userid, login, password, indexTenant, expire.ToString(DateTimeFormat), err);
            }
            return false;
        }


        public static string EncryptCookie(int tenant, Guid userid, string login = null, string password = null)
        {
            var settingsTenant = TenantCookieSettings.GetForTenant(tenant);
            var expires = settingsTenant.IsDefault() ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMinutes(settingsTenant.LifeTime);
            var settingsUser = TenantCookieSettings.GetForUser(userid);
            return EncryptCookie(tenant, userid, login, password, settingsTenant.Index, expires, settingsUser.Index);
        }

        public static string EncryptCookie(int tenant, Guid userid, string login, string password, int indexTenant, DateTime expires, int indexUser)
        {
            var s = string.Format("{0}${1}${2}${3}${4}${5}${6}${7}",
                (login ?? string.Empty).ToLowerInvariant(),
                tenant,
                password,
                GetUserDepenencySalt(),
                userid.ToString("N"),
                indexTenant,
                expires.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                indexUser);

            return InstanceCrypto.Encrypt(s);
        }


        private static string GetUserDepenencySalt()
        {
            var data = string.Empty;
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    var forwarded = HttpContext.Current.Request.Headers["X-Forwarded-For"];
                    data = string.IsNullOrEmpty(forwarded) ? HttpContext.Current.Request.UserHostAddress : forwarded.Split(':')[0];
                }
            }
            catch { }
            return Hasher.Base64Hash(data ?? string.Empty, HashAlg.SHA256);
        }
    }
}