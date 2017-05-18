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