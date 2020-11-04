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

namespace ASC.Core.Tenants
{
    public class TenantUtil
    {
        public static String GetBaseDomain(string hostedRegion)
        {
            var baseHost = CoreContext.Configuration.BaseDomain;

            if (string.IsNullOrEmpty(hostedRegion) || string.IsNullOrEmpty(baseHost) || !baseHost.Contains("."))
            {
                return baseHost;
            }
            var subdomain = baseHost.Remove(baseHost.IndexOf('.') + 1);
            return hostedRegion.StartsWith(subdomain) ? hostedRegion : (subdomain + hostedRegion.TrimStart('.'));
        }


        public static DateTime DateTimeFromUtc(DateTime utc)
        {
            return DateTimeFromUtc(CoreContext.TenantManager.GetCurrentTenant().TimeZone, utc);
        }

        public static DateTime DateTimeFromUtc(TimeZoneInfo timeZone, DateTime utc)
        {
            if (utc.Kind != DateTimeKind.Utc)
            {
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            }

            if (utc == DateTime.MinValue || utc == DateTime.MaxValue)
            {
                return utc;
            }

            return DateTime.SpecifyKind(TimeZoneInfo.ConvertTime(utc, TimeZoneInfo.Utc, timeZone), DateTimeKind.Local);
        }


        public static DateTime DateTimeToUtc(DateTime local)
        {
            return DateTimeToUtc(CoreContext.TenantManager.GetCurrentTenant().TimeZone, local);
        }

        public static DateTime DateTimeToUtc(TimeZoneInfo timeZone, DateTime local)
        {
            if (local.Kind == DateTimeKind.Utc || local == DateTime.MinValue || local == DateTime.MaxValue)
            {
                return local;
            }

            if (timeZone.IsInvalidTime(DateTime.SpecifyKind(local, DateTimeKind.Unspecified)))
            {
                // hack
                local = local.AddHours(1);
            }

            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), timeZone);

        }


        public static DateTime DateTimeNow()
        {
            return DateTimeNow(CoreContext.TenantManager.GetCurrentTenant().TimeZone);
        }

        public static DateTime DateTimeNow(TimeZoneInfo timeZone)
        {
            return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone), DateTimeKind.Local);
        }
    }
}