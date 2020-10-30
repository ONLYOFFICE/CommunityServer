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


namespace ASC.Specific
{

    public class AscBearerAuthorization : IApiAuthorization
    {
        private readonly ILog log;


        public AscBearerAuthorization()
        {

        }

        public AscBearerAuthorization(ILog log)
        {
            this.log = log;
        }


        public bool Authorize(HttpContextBase context)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                try
                {
                    var authorization = context.Request.Headers["Authorization"];
                    if (string.IsNullOrEmpty(authorization))
                    {
                        return false;
                    }
                    authorization = authorization.Trim();
                    if (authorization.IndexOf(',') != -1)
                        authorization = authorization.Substring(0, authorization.IndexOf(',')).Trim();

                    if (authorization.IndexOf("Bearer", 0, StringComparison.Ordinal) != 0)
                    {
                        return false;
                    }

                    SecurityContext.AuthenticateMe(authorization.Substring(7));
                }
                catch (Exception e)
                {
                    log.Error("ASC bearer auth error", e);
                }
            }
            return SecurityContext.IsAuthenticated;
        }

        public bool OnAuthorizationFailed(HttpContextBase context)
        {
            if (!string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.StatusDescription = HttpStatusCode.Unauthorized.ToString();
                var realm = String.Format("Bearer Realm=\"{0}\"", context.Request.GetUrlRewriter().Host);
                context.Response.AppendHeader("WWW-Authenticate", realm);
                return true;
            }
            return false;
        }
    }
}