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
using System.Net;
using System.Text;
using System.Web;
using ASC.Api.Interfaces;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Security.Cryptography;

namespace ASC.Specific
{
    public class AscBasicTestAuthorization : AscBasicAuthorization
    {
        protected override void Authentificate(string username, string password)
        {
            //set tennant to 0 for testing
            CoreContext.TenantManager.SetCurrentTenant(0);
            base.Authentificate(username, password);
        }
    }

    public class AscBasicAuthorization : IApiAuthorization
    {
        private readonly ILog log;


        public AscBasicAuthorization()
        {

        }

        public AscBasicAuthorization(ILog log)
        {
            this.log = log;
        }

        
        public bool Authorize(HttpContextBase context)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                try
                {
                    //Try basic
                    var authorization = context.Request.Headers["Authorization"];
                    if (string.IsNullOrEmpty(authorization))
                    {
                        return false;
                    }
                    authorization = authorization.Trim();
                    if (authorization.IndexOf(',') != -1)
                        authorization = authorization.Substring(0, authorization.IndexOf(',')).Trim();

                    if (authorization.IndexOf("Basic", 0) != 0)
                    {
                        return false;
                    }

                    // cut the word "basic" and decode from base64
                    // get "username:password"
                    var tempConverted = Convert.FromBase64String(authorization.Substring(6));
                    var user = new ASCIIEncoding().GetString(tempConverted);

                    // get "username"
                    // get "password"
                    var usernamePassword = user.Split(new[] { ':' });
                    var username = usernamePassword[0];
                    var password = usernamePassword[1];
                    log.DebugFormat("Basic Authorizing {0}", username);
                    Authentificate(username, password);
                }
                catch (Exception) { }
            }
            return SecurityContext.IsAuthenticated;
        }

        protected virtual void Authentificate(string username, string password)
        {
            var u = CoreContext.UserManager.GetUserByEmail(username) ?? CoreContext.UserManager.GetUserByUserName(username) ?? CoreContext.UserManager.GetUsers(new Guid(username));
            if (!Core.Users.Constants.LostUser.Equals(u))
            {
                var passwordHash = PasswordHasher.GetClientPassword(password);
                SecurityContext.AuthenticateMe(u.Email, passwordHash);
            }
        }

        public bool OnAuthorizationFailed(HttpContextBase context)
        {
            if (!string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.StatusDescription = HttpStatusCode.Unauthorized.ToString();
                string realm = String.Format("Basic Realm=\"{0}\"", context.Request.GetUrlRewriter().Host);
                context.Response.AppendHeader("WWW-Authenticate", realm);
                return true;
            }
            return false;
        }
    }
}