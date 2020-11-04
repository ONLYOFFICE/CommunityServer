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
using System.Text;
using System.Web;
using System.Web.Routing;

using ASC.Api.Batch;
using ASC.Api.Interfaces;
using ASC.Common.Logging;
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