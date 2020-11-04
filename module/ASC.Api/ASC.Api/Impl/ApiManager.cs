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
using System.Web;
using ASC.Api.Exceptions;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Common.Logging;
using Autofac;

namespace ASC.Api.Impl
{

    public class ApiManager : IApiManager
    {
        private readonly IApiMethodInvoker _invoker;
        private readonly List<IApiParamInspector> _paramInspectors;
        private readonly IEnumerable<IApiMethodCall> _methods;

        public IApiConfiguration Config { get; set; }

        public IApiArgumentBuilder ArgumentBuilder { get; set; }

        public ILog Log { get; set; }

        #region IApiManager Members

        public ApiManager(IComponentContext container, IApiMethodInvoker invoker)
        {
            _invoker = invoker;
            _paramInspectors = container.Resolve<IEnumerable<IApiParamInspector>>().ToList();
            _methods = container.Resolve<IEnumerable<IApiMethodCall>>();

        }

        public object InvokeMethod(IApiMethodCall methodToCall, ApiContext apicontext, object instance)
        {
            if (apicontext == null) throw new ArgumentNullException("apicontext");

            if (methodToCall != null)
            {
                var context = apicontext.RequestContext;

                Log.DebugFormat("Method to call={0}", methodToCall);

                //try convert params
                var callArg = ArgumentBuilder.BuildCallingArguments(context, methodToCall);
                if (_paramInspectors.Any())
                {
                    callArg = _paramInspectors.Aggregate(callArg,
                        (current, apiParamInspector) =>
                            apiParamInspector.InspectParams(current));
                }

                Log.DebugFormat("Arguments count: {0}", callArg == null ? "empty" : callArg.Count().ToString());


                try
                {
                    //Pre call filter
                    methodToCall.Filters.ToList().ForEach(x => x.PreMethodCall(methodToCall, apicontext, callArg));
                    if (apicontext.RequestContext.HttpContext.Response.StatusCode/100 != 2)
                    {
                        return new HttpException(apicontext.RequestContext.HttpContext.Response.StatusCode,
                            apicontext.RequestContext.HttpContext.Response.StatusDescription);
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