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
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.IPSecurity
{
    public static class IPSecurity
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC.IPSecurity");

        private static bool? _ipSecurityEnabled;

        public static bool IpSecurityEnabled
        {
            get
            {
                if (_ipSecurityEnabled.HasValue) return _ipSecurityEnabled.Value;
                var hideSettings = (ConfigurationManagerExtension.AppSettings["web.hide-settings"] ?? "").Split(new[] { ',', ';', ' ' });
                return (_ipSecurityEnabled = !hideSettings.Contains("IpSecurity", StringComparer.CurrentCultureIgnoreCase)).Value;
            }
        }

        private static readonly string CurrentIpForTest = ConfigurationManagerExtension.AppSettings["ipsecurity.test"];

        public static bool Verify(Tenant tenant)
        {
            if (!IpSecurityEnabled) return true;

            var httpContext = HttpContext.Current;
            if (httpContext == null) return true;

            if (tenant == null || SecurityContext.CurrentAccount.ID == tenant.OwnerId) return true;

            string requestIps = null;
            try
            {
                var restrictions = IPRestrictionsService.Get(tenant.TenantId).ToList();

                if (!restrictions.Any()) return true;

                if (string.IsNullOrWhiteSpace(requestIps = CurrentIpForTest))
                {
                    var request = httpContext.Request;
                    requestIps = request.Headers["X-Forwarded-For"] ?? request.UserHostAddress;
                }

                var ips = string.IsNullOrWhiteSpace(requestIps)
                              ? new string[] { }
                              : requestIps.Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

                if (ips.Any(requestIp => restrictions.Any(restriction => MatchIPs(GetIpWithoutPort(requestIp), restriction.Ip))))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Can't verify request with IP-address: {0}. Tenant: {1}. Error: {2} ", requestIps ?? "", tenant, ex);
                return false;
            }

            Log.InfoFormat("Restricted from IP-address: {0}. Tenant: {1}. Request to: {2}", requestIps ?? "", tenant, httpContext.Request.Url);
            return false;
        }

        private static bool MatchIPs(string requestIp, string restrictionIp)
        {
            var dividerIdx = restrictionIp.IndexOf('-');
            if (restrictionIp.IndexOf('-') > 0)
            {
                var lower = IPAddress.Parse(restrictionIp.Substring(0, dividerIdx).Trim());
                var upper = IPAddress.Parse(restrictionIp.Substring(dividerIdx + 1).Trim());

                var range = new IPAddressRange(lower, upper);
                return range.IsInRange(IPAddress.Parse(requestIp));
            }

            return requestIp == restrictionIp;
        }

        private static string GetIpWithoutPort(string ip)
        {
            var portIdx = ip.IndexOf(':');
            return portIdx > 0 ? ip.Substring(0, portIdx) : ip;
        }
    }
}