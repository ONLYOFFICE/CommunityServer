/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using ASC.Core;
using log4net;

namespace ASC.IPSecurity
{
    public static class IPSecurity
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.IPSecurity");

        private static bool IpSecurityEnabled
        {
            get
            {
                var setting = ConfigurationManager.AppSettings["ipsecurity.enabled"];
                return !string.IsNullOrEmpty(setting) && setting == "true";
            }
        }

        public static bool Verify(int tenant)
        {
            if (!IpSecurityEnabled) return true;

            var currentTenant = CoreContext.TenantManager.GetCurrentTenant();
            if (currentTenant != null && SecurityContext.CurrentAccount.ID == currentTenant.OwnerId) return true;

            var httpContext = HttpContext.Current;
            if (httpContext == null) return true;

            var request = httpContext.Request;
            var requestIps = request.Headers["X-Forwarded-For"] ?? request.UserHostAddress;

            //for testing
            var testRequestIp = ConfigurationManager.AppSettings["ipsecurity.test"];
            if (!string.IsNullOrWhiteSpace(testRequestIp))
            {
                requestIps = testRequestIp;
            }

            try
            {
                var restrictions = IPRestrictionsService.Get(tenant);

                if (!restrictions.Any()) return true;

                var ips = string.IsNullOrWhiteSpace(requestIps)
                              ? new string[] {} :
                              requestIps.Split(new[] {",", " "}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var ip in ips)
                {
                    var requestIp = GetIpWithoutPort(ip);
                    if (restrictions.Any(restriction => MatchIPs(requestIp, restriction.Ip))) return true;
                }
            }
            catch(Exception ex)
            {
                log.Error(string.Format("Can't verify request with IP-address: {0}. Tenant: {1}. Error: {2} ", requestIps, tenant, ex));
                return false;
            }

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