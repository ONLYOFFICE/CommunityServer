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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Routing;

using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Impl.Constraints;
using ASC.Api.Interfaces;
using ASC.Common.Logging;
using ASC.Common.Web;

using Autofac;
using Autofac.Core;

namespace ASC.Api.Impl
{
    class ApiRouteConfigurator : IApiRouteConfigurator
    {
        public IComponentContext Container { get; set; }

        public IApiConfiguration Config { get; set; }

        public ILog Log { get; set; }

        public IEnumerable<IApiMethodCall> RegisterEntryPoints()
        {
            Log.Debug("configuring entry points");
            var routeMap = new List<IApiMethodCall>();
            var registrations = Container.ComponentRegistry.Registrations
                .Where(x => typeof(IApiEntryPoint).IsAssignableFrom(x.Activator.LimitType)).ToList();
            //Register instances
            foreach (var apiMethodCall in registrations.Select(RouteEntryPoint).SelectMany(routePaths => routePaths.Cast<ApiMethodCall>()))
            {
                apiMethodCall.FullPath = GetFullPath(apiMethodCall);
                if (routeMap.Contains(apiMethodCall))
                {
                    throw new ApiDuplicateRouteException(apiMethodCall, routeMap.Find(x => x.Equals(apiMethodCall)));
                }
                Log.DebugFormat("configured {0}", apiMethodCall);
                routeMap.Add(apiMethodCall);
            }

            return routeMap;
        }


        private IEnumerable<IApiMethodCall> RouteEntryPoint(IComponentRegistration apiEntryPoint)
        {
            try
            {
                //Get all methods
                var methods = apiEntryPoint.Activator.LimitType.GetMethods(BindingFlags.Instance | BindingFlags.Public);

                var gloabalFilters = apiEntryPoint.Activator.LimitType.GetCustomAttributes(typeof (ApiCallFilter), true).Cast<ApiCallFilter>().ToList();
                gloabalFilters.AddRange(apiEntryPoint.Activator.LimitType.Assembly.GetCustomAttributes(typeof (ApiCallFilter), true).Cast<ApiCallFilter>());
                gloabalFilters.AddRange(Container.Resolve<IEnumerable<ApiCallFilter>>()); //Add gloably registered filters

                return (from methodInfo in methods.Where(x => !x.IsConstructor)
                        let attr = methodInfo.GetCustomAttributes(typeof (ApiAttribute), true).Cast<ApiAttribute>().FirstOrDefault()
                        let cache = methodInfo.GetCustomAttributes(typeof (CacheAttribute), true).Cast<CacheAttribute>().FirstOrDefault()
                        let filters = methodInfo.GetCustomAttributes(typeof (ApiCallFilter), true).Cast<ApiCallFilter>()
                        where attr != null
                        select ToApiMethodCall(methodInfo, apiEntryPoint, attr, cache, filters, gloabalFilters)).ToList();
            }
            catch (Exception err)
            {
                Log.Error(string.Format("Could not load apiEntryPoint {0}", apiEntryPoint.Activator.LimitType), err);
                return Enumerable.Empty<IApiMethodCall>();
            }
        }

        private IApiMethodCall ToApiMethodCall(MethodInfo methodInfo, IComponentRegistration apiEntryPointType, ApiAttribute attr, CacheAttribute cache, IEnumerable<ApiCallFilter> filters, List<ApiCallFilter> gloabalFilters)
        {
            var methodCall = Container.Resolve<IApiMethodCall>();
            methodCall.MethodCall = methodInfo;
            methodCall.Name = apiEntryPointType.Services.OfType<KeyedService>().First().ServiceKey.ToString();
            methodCall.ApiClassType = apiEntryPointType.Activator.LimitType;
            methodCall.HttpMethod = attr.Method;
            methodCall.RoutingUrl = ExtractPath(attr.Path);
            methodCall.CacheTime = cache != null ? cache.CacheTime : 0;
            methodCall.Constraints = ExtractConstraints(attr.Path, attr.Method);
            methodCall.RequiresAuthorization = attr.RequiresAuthorization;
            methodCall.CheckPayment = attr.CheckPayment;

            //Add filters
            gloabalFilters.AddRange(filters);
            methodCall.Filters = gloabalFilters;

            return methodCall;
        }

        private static readonly Regex RouteParser = new Regex(@"\{(?'route'[^\}:]+)(?'constraint'\:[^\}]+)}", RegexOptions.Compiled);
        private static readonly Regex RouteReplacer = new Regex(@"\{(?'route'[^\}]+)}", RegexOptions.Compiled);

        private RouteValueDictionary ExtractConstraints(string path, string method)
        {
            var rwDict = new RouteValueDictionary();
            var dictionary = RouteParser.Matches(path).Cast<Match>()
                                        .Where(match => match.Success && match.Groups["constraint"].Success && match.Groups["route"].Success)
                                        .ToDictionary(match => match.Groups["route"].Value,
                                                      match => match.Groups["constraint"].Value.TrimStart(':'));
            if (dictionary.Count > 0)
            {
                foreach (var constraint in dictionary)
                {
                    rwDict.Add(constraint.Key, constraint.Value);
                }
            }

            rwDict.Add("method", ApiHttpMethodConstraint.GetInstance(method)); //Adding method Constraint
            return rwDict;
        }

        private static string ExtractPath(string path)
        {
            return RouteParser.Replace(path, EvaluteRoute);
        }

        private static string EvaluteRoute(Match match)
        {
            if (match.Success && match.Groups["constraint"].Success)
            {
                return match.Value.Replace(match.Groups["constraint"].Value, "");
            }
            return match.Value;
        }

        public RouteCallInfo ResolveRoute(MethodInfo apiCall, Dictionary<string, object> arguments)
        {
            //Iterate throug all points and find needed one
            var entryPoint = Container.Resolve<IEnumerable<IApiMethodCall>>().SingleOrDefault(x => x.MethodCall.Equals(apiCall));
            if (entryPoint != null)
            {
                //Yahoo
                var url = RouteReplacer.Replace(GetFullPath(entryPoint),
                                                x =>
                                                    {
                                                        if (x.Success && x.Groups["route"].Success && arguments.ContainsKey(x.Groups["route"].Value))
                                                        {
                                                            var args = arguments[x.Groups["route"].Value];
                                                            arguments.Remove(x.Groups["route"].Value);
                                                            return x.Value.Replace("{" + x.Groups["route"].Value + "}",
                                                                                   Convert.ToString(args, CultureInfo.InvariantCulture));
                                                        }
                                                        return x.Value;
                                                    });
                return new RouteCallInfo
                    {
                        Url = url,
                        Method = entryPoint.HttpMethod,
                        Params = arguments
                    };
            }
            throw new ArgumentException("Api method not found or not registered");
        }

        private string GetFullPath(IApiMethodCall apiMethodCall)
        {
            return (Config.GetBasePath() + apiMethodCall.Name + Config.ApiSeparator +
                    apiMethodCall.RoutingUrl.TrimStart(Config.ApiSeparator)).TrimEnd('/');
        }
    }
}