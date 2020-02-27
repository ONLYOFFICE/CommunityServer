/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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