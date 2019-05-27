/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
