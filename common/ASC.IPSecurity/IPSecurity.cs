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


using ASC.Core;
using ASC.Core.Tenants;
using log4net;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;

namespace ASC.IPSecurity
{
    public static class IPSecurity
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC.IPSecurity");

        public static bool IpSecurityEnabled { get; private set; }

        static IPSecurity()
        {
            var hideSettings = (ConfigurationManager.AppSettings["web.hide-settings"] ?? "").Split(new[] {',', ';', ' '});
            IpSecurityEnabled = !hideSettings.Contains("IpSecurity", StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool Verify(Tenant tenant)
        {
            if (!IpSecurityEnabled) return true;

            var httpContext = HttpContext.Current;
            if (httpContext == null) return true;

            if (tenant == null || SecurityContext.CurrentAccount.ID == tenant.OwnerId) return true;

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
                var restrictions = IPRestrictionsService.Get(tenant.TenantId).ToList();

                if (!restrictions.Any()) return true;

                var ips = string.IsNullOrWhiteSpace(requestIps)
                              ? new string[] {}
                              : requestIps.Split(new[] {",", " "}, StringSplitOptions.RemoveEmptyEntries);

                if (ips.Select(GetIpWithoutPort)
                       .Any(requestIp => restrictions.Any(restriction => MatchIPs(requestIp, restriction.Ip))))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Can't verify request with IP-address: {0}. Tenant: {1}. Error: {2} ", requestIps, tenant, ex));
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