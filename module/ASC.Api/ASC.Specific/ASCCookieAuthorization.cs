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
using System.Web;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Common.Logging;
using ASC.Web.Core;


namespace ASC.Specific
{
    public class AscCookieAuthorization : IApiAuthorization
    {
        private readonly ILog log;

        public AscCookieAuthorization(ILog log)
        {
            this.log = log;
        }

        public bool Authorize(HttpContextBase context)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                try
                {
                    var cookie = CookiesManager.GetRequestVar(CookiesType.AuthKey).If(x => string.IsNullOrEmpty(x), () => context.Request.Headers["Authorization"]);
                    if (cookie != null && !string.IsNullOrEmpty(cookie))
                    {

                        if (!SecurityContext.AuthenticateMe(cookie))
                        {
                            log.WarnFormat("ASC cookie auth failed with cookie={0}", cookie);
                        }
                        return SecurityContext.IsAuthenticated;

                    }
                    log.Debug("no ASC cookie");
                }
                catch (Exception e)
                {
                    log.Error("ASC cookie auth error", e);
                }
            }
            return SecurityContext.IsAuthenticated;

        }

        public bool OnAuthorizationFailed(HttpContextBase context)
        {
            return false;
        }
    }
}