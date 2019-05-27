/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Net;
using System.Text;
using System.Web;
using ASC.Api.Interfaces;
using ASC.Common.Logging;
using ASC.Core;


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
                SecurityContext.AuthenticateMe(u.Email, password);
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