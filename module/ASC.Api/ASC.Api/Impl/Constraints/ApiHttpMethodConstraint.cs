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
using System.Web;
using System.Web.Routing;
using ASC.Api.Impl.Routing;

namespace ASC.Api.Impl.Constraints
{
    public class ApiHttpMethodConstraint : HttpMethodConstraint
    {
        private static readonly Dictionary<string, ApiHttpMethodConstraint> Methods = new Dictionary<string, ApiHttpMethodConstraint>(5);

        public static ApiHttpMethodConstraint GetInstance(string method)
        {
            method = method.ToUpperInvariant();
            if (Methods.ContainsKey(method)) return Methods[method];

            var result = new ApiHttpMethodConstraint(method);
            Methods.Add(method, result);
            return result;
        }

        public ApiHttpMethodConstraint(params string[] allowedMethods):base(allowedMethods)
        {
            
        }

        protected override bool Match(System.Web.HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var baseMatch = base.Match(httpContext, route, parameterName, values, routeDirection);
            if (!baseMatch && routeDirection==RouteDirection.IncomingRequest)
            {
                baseMatch = AllowedMethods.Any(method => string.Equals(method, httpContext.Request.RequestType, StringComparison.OrdinalIgnoreCase));
            }
            return baseMatch;
        }
    }

    public class ApiExtensionConstraint : IRouteConstraint
    {
        private readonly List<string> extensions;

        public ApiExtensionConstraint(List<string> extensions)
        {
            this.extensions = extensions;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if(!route.Url.EndsWith(ApiRouteRegistrator.ExtensionBrace)) return true;
            if (!values.Any()) return false;
            var extension = (string)values.First(r => r.Key == ApiRouteRegistrator.Extension).Value;
            extension = extension.TrimStart('.');

            if (extensions.Contains("." + extension)) return true;

            return false;
        }
    }
}