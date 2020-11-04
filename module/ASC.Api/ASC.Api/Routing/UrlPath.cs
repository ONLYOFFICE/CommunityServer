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
using System.Linq.Expressions;
using System.Reflection;
using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.Common.Web;
using Autofac;

namespace ASC.Api.Routing
{
    public static class UrlPath
    {
        public static string ResolveUrl(Expression<Action> functionToCall)
        {
            return Resolve(functionToCall, (method, args) => ApiSetup.Builder.Resolve<IApiRouteConfigurator>().ResolveRoute(method, args).Url);
        }

        public static RouteCallInfo ResolveRouteCall(Expression<Action> functionToCall)
        {
            return Resolve(functionToCall, (method, args) => ApiSetup.Builder.Resolve<IApiRouteConfigurator>().ResolveRoute(method, args));
        }

        private static T Resolve<T>(Expression<Action> functionToCall, Func<MethodInfo, Dictionary<string, object>, T> resolver)
        {
            if (functionToCall.Body.NodeType == ExpressionType.Call)
            {
                var methodCall = (MethodCallExpression)functionToCall.Body;
                //Ensure parameter
                if (methodCall.Method.GetCustomAttributes(typeof(ApiAttribute), true).Cast<ApiAttribute>().Any())
                {
                    //It has apicall attr

                    //Build an argument list
                    var callArgs = methodCall.Arguments.Select(x => GetValue(x)).ToArray();
                    var arguments = new Dictionary<string, object>();
                    var methodParams = methodCall.Method.GetParameters();
                    for (var index = 0; index < methodParams.Length; index++)
                    {
                        var parameterInfo = methodParams[index];
                        if (index < callArgs.Length)
                        {
                            arguments.Add(parameterInfo.Name, callArgs[index]);
                        }
                    }
                    return resolver(methodCall.Method,arguments);
                }

            }
            return default(T);
        }

        private static object GetValue(Expression expression)
        {
            return GetValue(expression, null);
        }

        private static object GetValue(Expression expression, MemberExpression member)
        {
            //Resolving epressions to values
            if (expression is ConstantExpression)
            {
                if (member == null)
                    return ((ConstantExpression)expression).Value;

                //get by name
                return ((ConstantExpression)expression).Value.GetType().InvokeMember(member.Member.Name,
                                                                                      BindingFlags.Public |
                                                                                      BindingFlags.NonPublic |
                                                                                      BindingFlags.Instance | BindingFlags.GetField, null,
                                                                                      ((ConstantExpression)
                                                                                       expression).Value,
                                                                                      new object[] { });
            }
            if (expression is MemberExpression)
            {
                return GetValue(((MemberExpression)expression).Expression, (MemberExpression)expression);
            }
            if (expression is MethodCallExpression)
            {
                var methodCallExpr = ((MethodCallExpression)expression);
                return methodCallExpr.Method.Invoke(methodCallExpr.Object,
                                                    methodCallExpr.Arguments.Select(x => GetValue(x)).ToArray());
            }
            return null;
        }
    }
}