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
using System.Reflection;
using System.Web.Routing;
using ASC.Api.Attributes;

namespace ASC.Api.Interfaces
{
    public interface IApiMethodCall : IEquatable<IApiMethodCall>
    {
        string RoutingUrl { get; set; }
        Type ApiClassType { get; set; }
        MethodInfo MethodCall { get; set; }
        string HttpMethod { get; set; }
        string Name { get; set; }
        long CacheTime { get; set; }
        bool ShouldCache { get; }
        void SetParams(ParameterInfo[] value);
        ParameterInfo[] GetParams();
        string FullPath { get; set; }
        RouteValueDictionary Constraints { get; set; }
        bool RequiresAuthorization { get; set; }
        bool CheckPayment { get; set; }
        object Invoke(object instance, object[] args);
        IEnumerable<ApiCallFilter> Filters { get; set; }
        ICollection<IApiResponder> Responders { get; set; } 
    }
}