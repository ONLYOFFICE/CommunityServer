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
using ASC.Api.Batch;
using ASC.Api.Impl.Constraints;
using ASC.Api.Interfaces;
using Autofac;

namespace ASC.Api.Impl.Routing
{
    public class ApiBatchRouteRegitrator : IApiRouteRegistrator
    {
        public IComponentContext Container { get; set; }

        public IApiConfiguration Config { get; set; }

        public void RegisterRoutes(RouteCollection routes)
        {
            var constrasints = new RouteValueDictionary {{"method", new ApiHttpMethodConstraint("POST", "GET")}};
            var basePath = Config.GetBasePath();
            foreach (var extension in Container.Resolve<IEnumerable<IApiResponder>>().ToList().SelectMany(apiSerializer => apiSerializer.GetSupportedExtensions().Select(x => x.StartsWith(".") ? x : "." + x)))
            {
                routes.Add(new Route(basePath + "batch" + extension, null, constrasints, null, Container.Resolve<ApiBatchRouteHandler>()));
            }
            routes.Add(new Route(basePath + "batch", null, constrasints, null, Container.Resolve<ApiBatchRouteHandler>()));
        }
    }
}