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


using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.Mail.Autoreply.AddressParsers
{
    internal abstract class AddressParser : IAddressParser
    {
        private Regex _routeRegex;
        private Regex RouteRegex
        {
            get { return _routeRegex ?? (_routeRegex = GetRouteRegex()); }
        }

        protected abstract Regex GetRouteRegex();
        protected abstract ApiRequest ParseRequestInfo(IDictionary<string, string> groups, Tenant t);

        protected bool IsLastVersion(Tenant t)
        {
            return t.Version > 1;
        }

        public ApiRequest ParseRequestInfo(string address)
        {
            try
            {
                var mailAddress = new MailAddress(address);

                var match = RouteRegex.Match(mailAddress.User);

                if (!match.Success)
                    return null;

                var tenant = CoreContext.TenantManager.GetTenant(mailAddress.Host);

                if (tenant == null)
                    return null;

                var groups = RouteRegex.GetGroupNames().ToDictionary(groupName => groupName, groupName => match.Groups[groupName].Value);
                var requestInfo = ParseRequestInfo(groups, tenant);

                requestInfo.Method = "POST";
                requestInfo.Tenant = tenant;

                if (!string.IsNullOrEmpty(requestInfo.Url))
                    requestInfo.Url = string.Format("api/2.0/{0}.json", requestInfo.Url);

                return requestInfo;
            }
            catch
            {
                return null;
            }
        }
    }
}
