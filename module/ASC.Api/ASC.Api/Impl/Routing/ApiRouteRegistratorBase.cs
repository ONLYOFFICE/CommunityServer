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
using ASC.Api.Interfaces;
using ASC.Common.Logging;
using Autofac;

namespace ASC.Api.Impl.Routing
{
    internal abstract class ApiRouteRegistratorBase : IApiRouteRegistrator
    {
        public IComponentContext Container { get; set; }

        public IApiConfiguration Config { get; set; }

        public ILog Log { get; set; }

        public void RegisterRoutes(RouteCollection routes)
        {
            var entryPoints = Container.Resolve<IEnumerable<IApiMethodCall>>();
            var extensions = new List<string>();
            foreach (var apiSerializer in Container.Resolve<IEnumerable<IApiResponder>>())
            {
                extensions.AddRange(apiSerializer.GetSupportedExtensions().Select(x => x.StartsWith(".") ? x : "." + x));
            }
            RegisterEntryPoints(routes, entryPoints, extensions);
        }

        protected abstract void RegisterEntryPoints(RouteCollection routes, IEnumerable<IApiMethodCall> entryPoints, List<string> extensions);
    }
}