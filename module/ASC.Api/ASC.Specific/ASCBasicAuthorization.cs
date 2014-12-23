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
using System.Net;
using System.Text;
using System.Web;
using ASC.Api.Interfaces;
using ASC.Api.Logging;
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
        private readonly ILog _log;


        public AscBasicAuthorization()
        {

        }

        public AscBasicAuthorization(ILog log)
        {
            _log = log;
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
                    _log.Debug("Basic Authorizing {0}", username);
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