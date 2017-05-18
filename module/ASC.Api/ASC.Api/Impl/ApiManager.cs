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


#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Routing;
using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Interfaces;
using ASC.Api.Logging;
using ASC.Api.Routing;
using ASC.Api.Utils;
using ASC.Common.Web;
using Microsoft.Practices.Unity;
using System.Globalization;

#endregion

namespace ASC.Api.Impl
{

    public class ApiManager : IApiManager
    {
        private readonly IUnityContainer _container;
        private readonly IApiMethodInvoker _invoker;
        private readonly List<IApiParamInspector> _paramInspectors;
        private readonly IEnumerable<IApiMethodCall> _methods;

        [Dependency]
        public IApiConfiguration Config { get; set; }

        [Dependency]
        public IApiArgumentBuilder ArgumentBuilder { get; set; }

        [Dependency]
        public ILog Log { get; set; }

        #region IApiManager Members

        public ApiManager(IUnityContainer container, IApiMethodInvoker invoker)
        {
            _container = container;
            _invoker = invoker;
            _paramInspectors = _container.ResolveAll<IApiParamInspector>().ToList();
            _methods = _container.Resolve<IEnumerable<IApiMethodCall>>();

        }

        public object InvokeMethod(IApiMethodCall methodToCall, ApiContext apicontext)
        {
            if (apicontext == null) throw new ArgumentNullException("apicontext");

            if (methodToCall != null)
            {
                var context = apicontext.RequestContext;

                Log.Debug("Method to call={0}", methodToCall);
                object instance = _container.Resolve(methodToCall.ApiClassType, new DependencyOverride(typeof(ApiContext), apicontext));

                //try convert params
                var callArg = ArgumentBuilder.BuildCallingArguments(context, methodToCall);
                if (_paramInspectors.Any())
                {
                    callArg = _paramInspectors.Aggregate(callArg,
                                                   (current, apiParamInspector) =>
                                                   apiParamInspector.InspectParams(current));
                }

                Log.Debug("Arguments count: {0}", callArg == null ? "empty" : callArg.Count().ToString());


                try
                {
                    //Pre call filter
                    methodToCall.Filters.ToList().ForEach(x => x.PreMethodCall(methodToCall, apicontext, callArg));
                    if (apicontext.RequestContext.HttpContext.Response.StatusCode / 100 != 2)
                    {
                        return new HttpException(apicontext.RequestContext.HttpContext.Response.StatusCode, apicontext.RequestContext.HttpContext.Response.StatusDescription);
                    }

                    object result = _invoker.InvokeMethod(methodToCall, instance, callArg, apicontext);
                    //Post call filter
                    methodToCall.Filters.ToList().ForEach(x => x.PostMethodCall(methodToCall, apicontext, result));
                    return result;
                }
                catch (Exception e)
                {
                    methodToCall.Filters.ToList().ForEach(x => x.ErrorMethodCall(methodToCall, apicontext, e));
                    throw;
                }
            }
            throw new ApiBadHttpMethodException();
        }

        public IApiMethodCall GetMethod(string routeUrl, string httpMethod)
        {
            var basePath = StringUtils.TrimExtension(routeUrl, Config.GetBasePath().Length);
            return _methods.SingleOrDefault(x => x.FullPath == basePath && x.HttpMethod.Equals(httpMethod, StringComparison.OrdinalIgnoreCase));
        }


        #endregion

    }
}