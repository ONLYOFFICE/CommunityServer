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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Common.Logging;
using Autofac;

namespace ASC.Api.Impl
{
    public class ApiRouteHandler : IApiRouteHandler
    {
        public ILifetimeScope Container { get; set; }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var authorizations = Container.Resolve<IEnumerable<IApiAuthorization>>().ToList();
            var log = Container.Resolve<ILog>();

            //Authorize request first
            log.DebugFormat("Authorizing {0}", requestContext.HttpContext.Request.Url);


            if (requestContext.RouteData.DataTokens.ContainsKey(DataTokenConstants.RequiresAuthorization)
                && !(bool)requestContext.RouteData.DataTokens[DataTokenConstants.RequiresAuthorization])
            {
                //Authorization is not required for method
                log.Debug("Authorization is not required");
                return GetHandler(requestContext);
            }

            foreach (var apiAuthorization in authorizations)
            {
                log.DebugFormat("Authorizing with:{0}", apiAuthorization.GetType().ToString());
                if (apiAuthorization.Authorize(requestContext.HttpContext))
                {
                    return GetHandler(requestContext);
                }
            }

            if (authorizations.Any(apiAuthorization => apiAuthorization.OnAuthorizationFailed(requestContext.HttpContext)))
            {
                log.Debug("Unauthorized");
                return new ErrorHttpHandler(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized.ToString());
            }
            log.Debug("Forbidden");

            return new ErrorHttpHandler(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized.ToString());
        }

        public virtual IHttpHandler GetHandler(RequestContext requestContext)
        {
            return Container.BeginLifetimeScope().Resolve<IApiHttpHandler>(new TypedParameter(typeof(RouteData), requestContext.RouteData));
        }
    }

    class ApiAsyncRouteHandler : ApiRouteHandler
    {
        public override IHttpHandler GetHandler(RequestContext requestContext)
        {
            throw new NotImplementedException("This handler is not yet implemented");
        }
    }
}