/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

using ASC.Common.Logging;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;

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

        private static readonly string MyNetworks = ConfigurationManagerExtension.AppSettings["ipsecurity.mynetworks"];

        public static bool Verify(Tenant tenant, string login = null)
        {
            if (!IpSecurityEnabled) return true;

            var httpContext = HttpContext.Current;
            if (httpContext == null) return true;

            if (tenant == null || SecurityContext.CurrentAccount.ID == tenant.OwnerId) return true;

            string address = null;
            try
            {
                var restrictions = IPRestrictionsService.Get(tenant.TenantId).ToList();

                if (!restrictions.Any()) return true;

                if (string.IsNullOrWhiteSpace(address = CurrentIpForTest))
                {
                    var request = httpContext.Request;
                    address = request.Headers["X-Forwarded-For"] ?? request.UserHostAddress;
                }

                var ips = IpAddressParser.ParseAddress(address).Select(IpAddressParser.GetIpWithoutPort);

                var currentUserId = SecurityContext.CurrentAccount.ID;
                bool isAdmin;

                if (!string.IsNullOrEmpty(login) && currentUserId == ASC.Core.Configuration.Constants.Guest.ID)
                {
                    var currentUser = CoreContext.UserManager.GetUserByEmail(login);
                    isAdmin = currentUser.IsAdmin();
                }
                else
                {
                    isAdmin = CoreContext.UserManager.IsUserInGroup(currentUserId, Constants.GroupAdmin.ID);
                }

                if (ips.Any(requestIp => restrictions.Any(restriction => (restriction.ForAdmin ? isAdmin : true) && MatchIPs(requestIp, restriction.Ip))))
                {
                    return true;
                }

                if (IsMyNetwork(ips))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Can't verify request with IP-address: {0}. Tenant: {1}. Error: {2} ", address ?? "", tenant, ex);
                return false;
            }

            Log.InfoFormat("Restricted from IP-address: {0}. Tenant: {1}. Request to: {2}", address ?? "", tenant, httpContext.Request.Url);
            return false;
        }

        public static bool MatchIPs(string requestIp, string restrictionIp)
        {
            var dividerIdx = restrictionIp.IndexOf('-');
            if (restrictionIp.IndexOf('-') > 0)
            {
                var lower = IPAddress.Parse(restrictionIp.Substring(0, dividerIdx).Trim());
                var upper = IPAddress.Parse(restrictionIp.Substring(dividerIdx + 1).Trim());

                var range = new IPAddressRange(lower, upper);
                return range.IsInRange(IPAddress.Parse(requestIp));
            }

            if (restrictionIp.IndexOf('/') > 0)
            {
                return IPAddressRange.IsInRange(requestIp, restrictionIp);
            }

            return requestIp == restrictionIp;
        }

        private static bool IsMyNetwork(IEnumerable<string> ips)
        {
            try
            {
                if (!string.IsNullOrEmpty(MyNetworks))
                {
                    var myNetworkIps = MyNetworks.Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

                    if (ips.Any(requestIp => myNetworkIps.Any(ipAddress => MatchIPs(requestIp, ipAddress))))
                    {
                        return true;
                    }
                }

                var hostName = Dns.GetHostName();
                var hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());

                var localIPs = new List<IPAddress> { IPAddress.IPv6Loopback, IPAddress.Loopback };

                localIPs.AddRange(hostAddresses.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork || ip.AddressFamily == AddressFamily.InterNetworkV6));

                foreach (var ipAddress in localIPs)
                {
                    if (ips.Contains(ipAddress.ToString()))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Can't verify local network from request with IP-address: {0}", string.Join(",", ips), ex);
            }

            return false;
        }
    }
}