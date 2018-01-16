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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

using ASC.Api.Batch;
using ASC.Api.Interfaces;
using ASC.Api.Logging;

using Autofac;

namespace ASC.Api
{
    public class ApiServer
    {
        private static bool? available = null;
        private readonly HttpContextBase _context;
        private readonly ApiBatchHttpHandler _batchHandler;
        private readonly IComponentContext container;


        public static bool Available
        {
            get
            {
                if (!available.HasValue)
                {
                    available = ApiSetup.Builder.IsRegistered<ILog>();
                }
                return available.Value;
            }
        }

        public ApiServer()
            : this(HttpContext.Current)
        {
        }

        public ApiServer(HttpContext context)
            : this(new HttpContextWrapper(context))
        { }

        public ApiServer(HttpContextBase context)
        {
            _context = context;
            container = ApiSetup.Builder;
            var config = container.Resolve<IApiConfiguration>();
            var route = RouteTable.Routes.OfType<Route>().First(r=> r.Url.EndsWith(config.GetBasePath() + "batch"));
            if (route == null)
            {
                throw new ArgumentException("Couldn't resolve api");
            }
            var routeHandler = route.RouteHandler as ApiBatchRouteHandler;
            if (routeHandler == null)
            {
                throw new ArgumentException("Couldn't resolve api");
            }
            
            var requestContext = new RequestContext(context, new RouteData(new Route("batch", routeHandler), routeHandler));
            _batchHandler = routeHandler.GetHttpHandler(requestContext) as ApiBatchHttpHandler;
            if (_batchHandler == null)
            {
                throw new ArgumentException("Couldn't resolve api");
            }
        }

        public string GetApiResponse(string apiUrl)
        {
            return GetApiResponse(apiUrl, null);
        }

        public string GetApiResponse(string apiUrl, string httpMethod)
        {
            return GetApiResponse(apiUrl, httpMethod, null);
        }

        public string GetApiResponse(string apiUrl, string httpMethod, string body)
        {
            return GetApiResponse(new ApiBatchRequest() { Method = httpMethod, RelativeUrl = apiUrl, Body = body });
        }

        public string GetApiResponse(ApiBatchRequest request)
        {
            var response = CallApiMethod(request);
            return response != null ? response.Data : null;
        }

        public ApiBatchResponse CallApiMethod(string apiUrl)
        {
            return CallApiMethod(apiUrl, "GET");
        }

        public ApiBatchResponse CallApiMethod(string apiUrl, string httpMethod)
        {
            return CallApiMethod(apiUrl, httpMethod, null);
        }

        public ApiBatchResponse CallApiMethod(string apiUrl, string httpMethod, string body)
        {
            return CallApiMethod(new ApiBatchRequest { Method = httpMethod, RelativeUrl = apiUrl, Body = body });
        }

        public ApiBatchResponse CallApiMethod(ApiBatchRequest request)
        {
            return CallApiMethod(request, true);
        }

        public ApiBatchResponse CallApiMethod(ApiBatchRequest request, bool encode)
        {
            var response = _batchHandler.ProcessBatchRequest(_context, request);
            if (encode && response != null && response.Data != null)
                response.Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(response.Data));
            return response;
        }


        public IEnumerable<ApiBatchResponse> CallApiMethods(IEnumerable<ApiBatchRequest> requests)
        {
            return requests.Select(request => CallApiMethod(request));
        }
    }
}