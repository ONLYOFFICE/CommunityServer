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
using log4net;
using System;
using System.Web.Http.Filters;

namespace ASC.HealthCheck.Infrastructure
{
    public class AddAccessControlAllowOriginHeaderFilter : ActionFilterAttribute
    {
        private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        private readonly ILog log = LogManager.GetLogger(typeof(AddAccessControlAllowOriginHeaderFilter));

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                // EnableCors does not work with Mono
                // use AddAccessControlAllowOriginHeaderFilter instead
                if (WorkContext.IsMono)
                {
                    log.Debug("AddAccessControlAllowOriginHeaderFilter OnActionExecuted");
                    if (actionExecutedContext != null && WorkContext.IsMono)
                    {
                        actionExecutedContext.Response.Content.Headers.Add(AccessControlAllowOrigin, "*");
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected error on AddAccessControlAllowOriginHeaderFilter: {0} {1}",
                    ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }
        }
    }
}