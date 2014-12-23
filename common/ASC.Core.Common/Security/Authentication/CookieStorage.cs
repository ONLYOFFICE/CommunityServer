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

using System;
using System.Web;
using ASC.Core.Security.Authentication;
using ASC.Security.Cryptography;
using ASC.Core.Tenants;
using log4net;

namespace ASC.Core.Security.Authentication
{
    class CookieStorage
    {
        public static bool DecryptCookie(string cookie, out int tenant, out Guid userid, out string login, out string password)
        {
            tenant = Tenant.DEFAULT_TENANT;
            userid = Guid.Empty;
            login = null;
            password = null;

            if (string.IsNullOrEmpty(cookie))
            {
                return false;
            }

            try
            {
                cookie = HttpUtility.UrlDecode(cookie).Replace(' ', '+');
                var s = InstanceCrypto.Decrypt(cookie).Split('$');

                if (0 < s.Length) login = s[0];
                if (1 < s.Length) tenant = int.Parse(s[1]);
                if (2 < s.Length) password = s[2];
                if (4 < s.Length) userid = new Guid(s[4]);
                return true;
            }
            catch(Exception err)
            {
                LogManager.GetLogger("ASC.Core").ErrorFormat("Authenticate error: cookie {0}, tenant {1}, userid {2}, login {3}, pass {4}: {5}",
                    cookie, tenant, userid, login, password, err);
            }
            return false;
        }

        public static string EncryptCookie(int tenant, Guid userid, string login, string password)
        {
            var s = string.Format("{0}${1}${2}${3}${4}",
                (login ?? string.Empty).ToLowerInvariant(),
                tenant,
                password,
                GetUserDepenencySalt(),
                userid.ToString("N"));
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