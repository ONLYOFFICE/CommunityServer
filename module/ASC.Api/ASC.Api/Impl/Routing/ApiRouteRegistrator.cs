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


using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using ASC.Api.Impl.Constraints;
using ASC.Api.Interfaces;
using Autofac;

namespace ASC.Api.Impl.Routing
{
    class ApiRouteRegistrator : ApiRouteRegistratorBase
    {
        public const string Extension = "extension";
        public const string ExtensionBrace = "{" + Extension + "}";

        protected override void RegisterEntryPoints(RouteCollection routes, IEnumerable<IApiMethodCall> entryPoints, List<string> extensions)
        {
            var routeHandler = Container.Resolve<IApiRouteHandler>();
            var defaults = new RouteValueDictionary { { Extension, ".json" } };
            var apiExtensionConstraint = new ApiExtensionConstraint(extensions);

            foreach (var apiMethodCall in entryPoints.OrderBy(x => x.RoutingUrl.IndexOf('{')).ThenBy(x => x.RoutingUrl.LastIndexOf('}')))
            {
                var dataTokens = GetDataTokens(apiMethodCall);
                var url = apiMethodCall.FullPath + (apiMethodCall.FullPath.EndsWith("}") ? "." : "") +  ExtensionBrace;

                apiMethodCall.Constraints.Add(Extension, apiExtensionConstraint);

                routes.Add(new Route(url, defaults, apiMethodCall.Constraints, dataTokens, routeHandler));
                routes.Add(new Route(apiMethodCall.FullPath, defaults, apiMethodCall.Constraints, dataTokens, routeHandler));
            }
        }

        private static RouteValueDictionary GetDataTokens(IApiMethodCall method)
        {
            if (method.RequiresAuthorization && method.CheckPayment) return null;

            var dataTokens = new RouteValueDictionary();
            if (!method.RequiresAuthorization)
            {
                dataTokens.Add(DataTokenConstants.RequiresAuthorization, false);
            }

            if (!method.CheckPayment)
            {
                dataTokens.Add(DataTokenConstants.CheckPayment, false);
            }

            return dataTokens;
        }
    }
}