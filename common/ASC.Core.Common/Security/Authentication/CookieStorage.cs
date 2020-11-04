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
using System.Globalization;
using System.Web;
using ASC.Common.Logging;
using ASC.Security.Cryptography;
using ASC.Core.Tenants;

namespace ASC.Core.Security.Authentication
{
    public class CookieStorage
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss,fff";

        public static bool DecryptCookie(string cookie, out int tenant, out Guid userid, out int indexTenant, out DateTime expire, out int indexUser)
        {
            tenant = Tenant.DEFAULT_TENANT;
            userid = Guid.Empty;
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

                if (1 < s.Length) tenant = int.Parse(s[1]);
                if (4 < s.Length) userid = new Guid(s[4]);
                if (5 < s.Length) indexTenant = int.Parse(s[5]);
                if (6 < s.Length) expire = DateTime.ParseExact(s[6], DateTimeFormat, CultureInfo.InvariantCulture);
                if (7 < s.Length) indexUser = int.Parse(s[7]);

                return true;
            }
            catch(Exception err)
            {
                LogManager.GetLogger("ASC.Core").ErrorFormat("Authenticate error: cookie {0}, tenant {1}, userid {2}, indexTenant {3}, expire {4}: {5}",
                    cookie, tenant, userid, indexTenant, expire.ToString(DateTimeFormat), err);
            }
            return false;
        }


        public static string EncryptCookie(int tenant, Guid userid)
        {
            var settingsTenant = TenantCookieSettings.GetForTenant(tenant);
            var expires = TenantCookieSettings.GetExpiresTime(tenant);
            var settingsUser = TenantCookieSettings.GetForUser(tenant, userid);
            return EncryptCookie(tenant, userid, settingsTenant.Index, expires, settingsUser.Index);
        }

        public static string EncryptCookie(int tenant, Guid userid, int indexTenant, DateTime expires, int indexUser)
        {
            var s = string.Format("{0}${1}${2}${3}${4}${5}${6}${7}",
                string.Empty, //login
                tenant,
                string.Empty, // password
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