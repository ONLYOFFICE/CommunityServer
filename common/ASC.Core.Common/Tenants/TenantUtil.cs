/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Configuration;

namespace ASC.Core.Tenants
{
    public class TenantUtil
    {
        public static String GetBaseDomain(String hostedRegion)
        {
            var baseHost = ConfigurationManager.AppSettings["core.base-domain"];

            if (String.IsNullOrEmpty(hostedRegion)) return baseHost;
            if (String.IsNullOrEmpty(baseHost)) return baseHost;
            if (baseHost.IndexOf('.') == -1) return baseHost;

            var subdomain = baseHost.Remove(baseHost.IndexOf('.'));
            return hostedRegion.StartsWith(subdomain + ".") ? hostedRegion : String.Join(".", new[] { subdomain, hostedRegion.TrimStart('.') });
        }

        public static DateTime DateTimeFromUtc(DateTime dbDateTime)
        {
            return DateTimeFromUtc(CoreContext.TenantManager.GetCurrentTenant(), dbDateTime);
        }

        public static DateTime DateTimeFromUtc(Tenant tenant, DateTime dbDateTime)
        {
            return DateTimeFromUtc(tenant.TimeZone, dbDateTime);
        }

        public static DateTime DateTimeFromUtc(TimeZoneInfo timeZone, DateTime dbDateTime)
        {
            if (dbDateTime.Kind == DateTimeKind.Local)
            {
                dbDateTime = DateTime.SpecifyKind(dbDateTime, DateTimeKind.Unspecified);
            }
            return DateTime.SpecifyKind(TimeZoneInfo.ConvertTime(dbDateTime, TimeZoneInfo.Utc, timeZone), DateTimeKind.Local);
        }

        public static DateTime DateTimeToUtc(DateTime dbDateTime)
        {
            return DateTimeToUtc(CoreContext.TenantManager.GetCurrentTenant(), dbDateTime);
        }

        public static DateTime DateTimeToUtc(Tenant tenant, DateTime dbDateTime)
        {
            return DateTimeToUtc(tenant.TimeZone, dbDateTime);
        }

        public static DateTime DateTimeToUtc(TimeZoneInfo timeZone, DateTime dbDateTime)
        {
            if (dbDateTime.Kind == DateTimeKind.Utc)
            {
                return dbDateTime;
            }
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dbDateTime, DateTimeKind.Unspecified), timeZone);

        }

        public static DateTime DateTimeNow()
        {
            return DateTimeNow(CoreContext.TenantManager.GetCurrentTenant());
        }

        public static DateTime DateTimeNow(Tenant tenant)
        {
            return DateTimeNow(tenant.TimeZone);
        }

        public static DateTime DateTimeNow(TimeZoneInfo timeZone)
        {
            return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone), DateTimeKind.Local);
        }
    }
}