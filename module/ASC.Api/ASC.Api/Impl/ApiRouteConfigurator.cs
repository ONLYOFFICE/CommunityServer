/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
using ASC.Api.Logging;
using ASC.Api.Utils;
using ASC.Common.Web;
using Microsoft.Practices.Unity;

namespace ASC.Api.Impl
{
    class ApiRouteConfigurator : IApiRouteConfigurator
    {
        [Dependency]
        public IUnityContainer Container { get; set; }

        [Dependency]
        public IApiConfiguration Config { get; set; }

        [Dependency]
        public ILog Log { get; set; }

        public void RegisterEntryPoints()
        {
            Log.Debug("configuring entry points");
            var routeMap = new List<IApiMethodCall>();
            string apiBasePathPath = Config.GetBasePath();
            var registrations = Container.Registrations.Where(x => x.RegisteredType == typeof(IApiEntryPoint)).ToList();
            //Register instances
            foreach (ApiMethodCall apiMethodCall in
                registrations.Select(apiEntryPoint => RouteEntryPoint(apiEntryPoint)).SelectMany(
                    routePaths => routePaths.Cast<ApiMethodCall>()))
            {
                apiMethodCall.FullPath = GetFullPath(apiBasePathPath, apiMethodCall);
                if (!string.IsNullOrEmpty(apiMethodCall.RoutingPollUrl))
                {
                    apiMethodCall.RoutingPollUrl = GetFullPollPath(apiBasePathPath, apiMethodCall);
                }
                if (routeMap.Contains(apiMethodCall))
                {
                    throw new ApiDuplicateRouteException(apiMethodCall, routeMap.Find(x => x.Equals(apiMethodCall)));
                }
                Log.Debug("configured {0}", apiMethodCall);
                routeMap.Add(apiMethodCall);
            }
            //Register instance to container
            Container.RegisterInstance(typeof(IEnumerable<IApiMethodCall>), routeMap,
                                       new SingletonLifetimeManager());


        }



        private IEnumerable<IApiMethodCall> RouteEntryPoint(ContainerRegistration apiEntryPoint)
        {
            try
            {
                //Get all methods
                MethodInfo[] methods = apiEntryPoint.MappedToType.GetMethods(BindingFlags.Instance | BindingFlags.Public);

                var gloabalFilters =
                    apiEntryPoint.MappedToType.GetCustomAttributes(typeof(ApiCallFilter), true).Cast<ApiCallFilter>().ToList();
                gloabalFilters.AddRange(apiEntryPoint.MappedToType.Assembly.GetCustomAttributes(typeof(ApiCallFilter), true).Cast<ApiCallFilter>());
                gloabalFilters.AddRange(Container.ResolveAll<ApiCallFilter>());//Add gloably registered filters

                return (from methodInfo in methods.Where(x => !x.IsConstructor)
                        let attr =
                            methodInfo.GetCustomAttributes(typeof(ApiAttribute), true).Cast<ApiAttribute>().FirstOrDefault()
                        let cache =
                            methodInfo.GetCustomAttributes(typeof(CacheAttribute), true).Cast<CacheAttribute>().
                            FirstOrDefault()
                        let filters = methodInfo.GetCustomAttributes(typeof(ApiCallFilter), true).Cast<ApiCallFilter>()
                        let poll = methodInfo.GetCustomAttributes(typeof(PollAttribute), true).Cast<PollAttribute>().
                            FirstOrDefault()
                        where attr != null
                        select ToApiMethodCall(methodInfo, apiEntryPoint, attr, cache, poll, filters, gloabalFilters)).ToList();
            }
            catch (Exception err)
            {
                Log.Error(err, "Could not load apiEntryPoint {0}", apiEntryPoint.Name);
                return Enumerable.Empty<IApiMethodCall>();
            }
        }

        private IApiMethodCall ToApiMethodCall(MethodInfo methodInfo, ContainerRegistration apiEntryPointType, ApiAttribute attr, CacheAttribute cache, PollAttribute poll, IEnumerable<ApiCallFilter> filters, List<ApiCallFilter> gloabalFilters)
        {
            var methodCall = Container.Resolve<IApiMethodCall>();
            methodCall.MethodCall = methodInfo;
            methodCall.Name = apiEntryPointType.Name;
            methodCall.ApiClassType = apiEntryPointType.MappedToType;
            methodCall.HttpMethod = attr.Method;
            methodCall.RoutingUrl = ExtractPath(attr.Path);
            methodCall.CacheTime = cache != null ? cache.CacheTime : 0;
            methodCall.Constraints = ExtractConstraints(attr.Path, attr.Method);
            methodCall.RequiresAuthorization = attr.RequiresAuthorization;
            methodCall.SupportsPoll = poll != null;
            methodCall.RoutingPollUrl = poll != null ? poll.PollUrl : string.Empty;

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
            rwDict.Add("method", new ApiHttpMethodConstraint(method.ToUpperInvariant()));//Adding method Constraint
            return rwDict;
        }

        private string ExtractPath(string path)
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
            var entryPoint = Container.Resolve<IEnumerable<IApiMethodCall>>().Where(x => x.MethodCall.Equals(apiCall)).SingleOrDefault();
            if (entryPoint != null)
            {
                //Yahoo
                var url = RouteReplacer.Replace(GetFullPath(Config.GetBasePath(), entryPoint), (x) =>
                                                                                                   {
                                                                                                       if (x.Success && x.Groups["route"].Success && arguments.ContainsKey(x.Groups["route"].Value))
                                                                                                       {
                                                                                                           var args = arguments[x.Groups["route"].Value];
                                                                                                           arguments.Remove(x.Groups["route"].Value);
                                                                                                           return x.Value.Replace("{" + x.Groups["route"].Value + "}", Convert.ToString(args, CultureInfo.InvariantCulture));
                                                                                                       }
                                                                                                       return x.Value;
                                                                                                   });
                return new RouteCallInfo() { Url = url, Method = entryPoint.HttpMethod, Params = arguments };
            }
            throw new ArgumentException("Api method not found or not registered");
        }

        private string GetFullPath(string apiBasePathPath, IApiMethodCall apiMethodCall)
        {
            return (apiBasePathPath + apiMethodCall.Name + Config.ApiSeparator +
                    apiMethodCall.RoutingUrl.TrimStart(Config.ApiSeparator)).TrimEnd('/');
        }

        private string GetFullPollPath(string apiBasePathPath, IApiMethodCall apiMethodCall)
        {
            return GetFullPath(apiBasePathPath, apiMethodCall) + Config.ApiSeparator + apiMethodCall.RoutingPollUrl.TrimStart(Config.ApiSeparator).TrimEnd('/');
        }
    }
}