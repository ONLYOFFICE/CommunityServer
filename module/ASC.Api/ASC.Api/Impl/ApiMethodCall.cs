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
using System.Reflection;
using System.Web.Routing;
using ASC.Api.Attributes;
using ASC.Api.Interfaces;

namespace ASC.Api.Impl
{
    public class ApiMethodCall : IApiMethodCall
    {
        private MethodInfo _methodCall;

        #region IApiMethodCall Members

        public string RoutingUrl { get; set; }
        public Type ApiClassType { get; set; }

        public MethodInfo MethodCall
        {
            get { return _methodCall; }
            set
            {
                _methodCall = value;
                SetParams(_methodCall.GetParameters());
            }
        }

        public string HttpMethod { get; set; }
        public string Name { get; set; }
        public long CacheTime { get; set; }

        public bool ShouldCache
        {
            get { return CacheTime > 0; }
        }

        private ParameterInfo[] _params;

        public void SetParams(ParameterInfo[] value)
        {
            _params = value;
        }

        public ParameterInfo[] GetParams()
        {
            return _params;
        }

        public string FullPath { get; set; }

        public RouteValueDictionary Constraints { get; set; }

        public bool RequiresAuthorization { get; set; }
        public bool CheckPayment { get; set; }

        public bool SupportsPoll { get; set; }
        public string RoutingPollUrl { get; set; }

        public bool Equals(IApiMethodCall other)
        {
            return Equals(other as ApiMethodCall);
        }

        public object Invoke(object instance, object[] args)
        {
            return MethodCall.Invoke(instance, args);
        }

        public IEnumerable<ApiCallFilter> Filters { get; set; }

        private ICollection<IApiResponder> _responders = new List<IApiResponder>();

        public ICollection<IApiResponder> Responders
        {
            get { return _responders; }
            set { _responders = value; }
        }

        #endregion

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == typeof (ApiMethodCall) && Equals((ApiMethodCall)obj));
        }

        public bool Equals(ApiMethodCall other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.FullPath, FullPath) && Equals(other.HttpMethod, HttpMethod);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FullPath != null ? FullPath.GetHashCode() : 0)*397) ^
                       (HttpMethod != null ? HttpMethod.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}.{3}({4})",
                                 RoutingUrl,
                                 HttpMethod,
                                 MethodCall.DeclaringType.FullName,
                                 MethodCall.Name,
                                 string.Join(",", GetParams().Select(x => x.Name).ToArray()));
        }
    }
}