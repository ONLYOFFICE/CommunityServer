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


using System.Web;
using System.Web.Routing;
using ASC.Api.Impl.Constraints;
using ASC.Api.Interfaces;

namespace ASC.Api.Impl.Routing
{
    public class ApiAccessControlRouteRegistrator : IApiRouteRegistrator
    {
        public IApiConfiguration Config { get; set; }

        public void RegisterRoutes(RouteCollection routes)
        {
            //Register 1 route
            var basePath = Config.GetBasePath();
            var constrasints = new RouteValueDictionary { { "method", ApiHttpMethodConstraint.GetInstance("OPTIONS") } };
            routes.Add(new Route(basePath + "{*path}", null, constrasints, new ApiAccessRouteHandler()));
        }

        public class ApiAccessRouteHandler : IRouteHandler
        {
            public IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                return new ApiAccessHttpHandler();
            }

            public class ApiAccessHttpHandler : IHttpHandler
            {
                public void ProcessRequest(HttpContext context)
                {
                    //Set access headers
                    //Access-Control-Allow-Origin: http://foo.example  
                    //Access-Control-Allow-Methods: POST, GET, OPTIONS  
                    //Access-Control-Allow-Headers: X-PINGOTHER  
                    //Access-Control-Max-Age: 1728000  
                    //context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                    context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE";
                    context.Response.Headers["Access-Control-Allow-Headers"] = "origin, authorization, accept, content-type";
                    context.Response.Headers["Access-Control-Max-Age"] = "1728000";

                }

                public bool IsReusable
                {
                    get { return false; }
                }
            }
        }
    }
}