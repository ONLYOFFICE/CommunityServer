/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
                var url = string.Format("{0}{1}{2}", apiMethodCall.FullPath, apiMethodCall.FullPath.EndsWith("}") ? "." : "", ExtensionBrace);

                apiMethodCall.Constraints.Add(Extension, apiExtensionConstraint);

                routes.Add(new Route(url, defaults, apiMethodCall.Constraints, dataTokens, routeHandler));
                routes.Add(new Route(apiMethodCall.FullPath, defaults, apiMethodCall.Constraints, dataTokens, routeHandler));
            }
        }

        private static RouteValueDictionary GetDataTokens(IApiMethodCall method)
        {
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