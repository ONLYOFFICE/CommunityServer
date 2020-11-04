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


using ASC.Core;
using ASC.Security.Cryptography;
using System;
using System.Text;
using System.Web;

namespace ASC.Web.Core.Helpers
{
    public class AuthorizationHelper
    {
        public static bool ProcessBasicAuthorization(HttpContext context)
        {
            string authCookie;
            return ProcessBasicAuthorization(context, out authCookie);
        }

        public static bool ProcessBasicAuthorization(HttpContext context, out string authCookie)
        {
            authCookie = null;
            try
            {
                //Try basic
                var authorization = context.Request.Headers["Authorization"];
                if (string.IsNullOrEmpty(authorization))
                {
                    return false;
                }

                authorization = authorization.Trim();
                if (0 <= authorization.IndexOf("Basic", 0))
                {
                    var arr = Encoding.ASCII.GetString(Convert.FromBase64String(authorization.Substring(6))).Split(new[] { ':' });
                    var username = arr[0];
                    var password = arr[1];
                    var u = CoreContext.UserManager.GetUserByEmail(username);
                    if (u != null && u.ID != ASC.Core.Users.Constants.LostUser.ID)
                    {
                        var passwordHash = PasswordHasher.GetClientPassword(password);
                        authCookie = SecurityContext.AuthenticateMe(u.Email, passwordHash);
                    }
                }
                else if (0 <= authorization.IndexOf("Bearer", 0))
                {
                    authorization = authorization.Substring("Bearer ".Length);
                    if (SecurityContext.AuthenticateMe(authorization))
                    {
                        authCookie = authorization;
                    }
                }
                else
                {
                    if (SecurityContext.AuthenticateMe(authorization))
                    {
                        authCookie = authorization;
                    }
                }
            }
            catch (Exception) { }
            return SecurityContext.IsAuthenticated;
        }
    }
}