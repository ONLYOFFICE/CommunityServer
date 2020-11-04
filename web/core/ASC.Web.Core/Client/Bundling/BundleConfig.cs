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


using System.Web.Optimization;
using System.Web.Routing;

namespace ASC.Web.Core.Client.Bundling
{
    public static class BundleConfig
    {
        public static void Configure()
        {
            RouteTable.Routes.Add(new Route(BundleHelper.CLIENT_SCRIPT_VPATH.TrimStart('/') + "{path}.js", new ClientScriptRouteHandler()));
            BundleTable.Bundles.UseCdn = ClientSettings.StoreBundles;
            BundleTable.EnableOptimizations = true;
            PreApplicationStartCode.Start();
        }
    }
}
