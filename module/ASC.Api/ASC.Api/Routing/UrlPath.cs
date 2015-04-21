/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.Common.Web;
using Microsoft.Practices.ServiceLocation;

namespace ASC.Api.Routing
{
    public static class UrlPath
    {
        public static string ResolveUrl(Expression<Action> functionToCall)
        {
            return Resolve(functionToCall, (method, args) => ServiceLocator.Current.GetInstance<IApiRouteConfigurator>().ResolveRoute(method, args).Url);
        }

        public static RouteCallInfo ResolveRouteCall(Expression<Action> functionToCall)
        {
            return Resolve(functionToCall, (method, args) => ServiceLocator.Current.GetInstance<IApiRouteConfigurator>().ResolveRoute(method, args));
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