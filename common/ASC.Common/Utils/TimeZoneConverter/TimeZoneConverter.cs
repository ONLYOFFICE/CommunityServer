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
using System.Xml.Linq;
using System.Xml.XPath;
using log4net;

namespace ASC.Common.Utils
{
    public class TimeZoneConverter
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC.TimeZone");

        private static IEnumerable<MapZone> _mapZones;

        static TimeZoneConverter()
        {
            try
            {
                InitFromFile();
            }
            catch (Exception error)
            {
                Log.Error(error);
                DefaultInit();
            }
        }

        private static void InitFromFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("ASC.Common.Utils.TimeZoneConverter.windowsZones.xml"))
            {
                var xml = XElement.Load(stream);
                _mapZones = from row in xml.XPathSelectElements("*/mapTimezones/mapZone")
                            select new MapZone
                            {
                                OlsonTimeZoneId = row.Attribute("type").Value,
                                WindowsTimeZoneId = row.Attribute("other").Value,
                                Territory = row.Attribute("territory").Value
                            };
            }
        }

        private static void DefaultInit()
        {
            var olsonWindowsTimes = new Dictionary<string, string>
                {
                    {"Africa/Abidjan", "W. Europe Standard Time"},
                    {"Africa/Accra", "W. Europe Standard Time"},
                    {"Africa/Addis_Ababa", "Namibia Standard Time"},
                    {"Africa/Algiers", "W. Central Africa Standard Time"},
                    {"Africa/Asmara", "Namibia Standard Time"},
                    {"Africa/Bamako", "W. Europe Standard Time"},
                    {"Africa/Bangui", "W. Central Africa Standard Time"},
                    {"Africa/Banjul", "W. Europe Standard Time"},
                    {"Africa/Bissau", "W. Europe Standard Time"},
                    {"Africa/Blantyre", "South Africa Standard Time"},
                    {"Africa/Brazzaville", "W. Central Africa Standard Time"},
                    {"Africa/Bujumbura", "South Africa Standard Time"},
                    {"Africa/Cairo", "Egypt Standard Time"},
                    {"Africa/Casablanca", "Morocco Standard Time"},
                    {"Africa/Johannesburg", "South Africa Standard Time"},
                    {"Africa/Lagos", "W. Central Africa Standard Time"},
                    {"Africa/Nairobi", "E. Africa Standard Time"},
                    {"Africa/Windhoek", "Namibia Standard Time"},
                    {"America/Anchorage", "Alaskan Standard Time"},
                    {"America/Asuncion", "Paraguay Standard Time"},
                    {"America/Bogota", "SA Pacific Standard Time"},
                    {"America/Buenos_Aires", "Argentina Standard Time"},
                    {"America/Caracas", "Venezuela Standard Time"},
                    {"America/Cayenne", "SA Eastern Standard Time"},
                    {"America/Chicago", "Central Standard Time"},
                    {"America/Chihuahua", "Mountain Standard Time (Mexico)"},
                    {"America/Cuiaba", "Central Brazilian Standard Time"},
                    {"America/Denver", "Mountain Standard Time"},
                    {"America/Godthab", "Greenland Standard Time"},
                    {"America/Guatemala", "Central America Standard Time"},
                    {"America/Halifax", "Atlantic Standard Time"},
                    {"America/Indianapolis", "US Eastern Standard Time"},
                    {"America/La_Paz", "SA Western Standard Time"},
                    {"America/Los_Angeles", "Pacific Standard Time"},
                    {"America/Mexico_City", "Mexico Standard Time"},
                    {"America/Montevideo", "Montevideo Standard Time"},
                    {"America/New_York", "Eastern Standard Time"},
                    {"America/Phoenix", "US Mountain Standard Time"},
                    {"America/Regina", "Canada Central Standard Time"},
                    {"America/Santa_Isabel", "Pacific Standard Time (Mexico)"},
                    {"America/Santiago", "Pacific SA Standard Time"},
                    {"America/Sao_Paulo", "E. South America Standard Time"},
                    {"America/St_Johns", "Newfoundland Standard Time"},
                    {"Asia/Almaty", "Central Asia Standard Time"},
                    {"Asia/Amman", "Jordan Standard Time"},
                    {"Asia/Baghdad", "Arabic Standard Time"},
                    {"Asia/Baku", "Azerbaijan Standard Time"},
                    {"Asia/Bangkok", "SE Asia Standard Time"},
                    {"Asia/Beirut", "Middle East Standard Time"},
                    {"Asia/Calcutta", "India Standard Time"},
                    {"Asia/Colombo", "Sri Lanka Standard Time"},
                    {"Asia/Damascus", "Syria Standard Time"},
                    {"Asia/Dhaka", "Bangladesh Standard Time"},
                    {"Asia/Dubai", "Arabian Standard Time"},
                    {"Asia/Irkutsk", "North Asia East Standard Time"},
                    {"Asia/Jerusalem", "Israel Standard Time"},
                    {"Asia/Kabul", "Afghanistan Standard Time"},
                    {"Asia/Kamchatka", "Kamchatka Standard Time"},
                    {"Asia/Karachi", "Pakistan Standard Time"},
                    {"Asia/Katmandu", "Nepal Standard Time"},
                    {"Asia/Krasnoyarsk", "North Asia Standard Time"},
                    {"Asia/Magadan", "Magadan Standard Time"},
                    {"Asia/Novosibirsk", "N. Central Asia Standard Time"},
                    {"Asia/Rangoon", "Myanmar Standard Time"},
                    {"Asia/Riyadh", "Arab Standard Time"},
                    {"Asia/Seoul", "Korea Standard Time"},
                    {"Asia/Shanghai", "China Standard Time"},
                    {"Asia/Singapore", "Singapore Standard Time"},
                    {"Asia/Taipei", "Taipei Standard Time"},
                    {"Asia/Tashkent", "West Asia Standard Time"},
                    {"Asia/Tbilisi", "Georgian Standard Time"},
                    {"Asia/Tehran", "Iran Standard Time"},
                    {"Asia/Tokyo", "Tokyo Standard Time"},
                    {"Asia/Ulaanbaatar", "Ulaanbaatar Standard Time"},
                    {"Asia/Vladivostok", "Vladivostok Standard Time"},
                    {"Asia/Yakutsk", "Yakutsk Standard Time"},
                    {"Asia/Yekaterinburg", "Ekaterinburg Standard Time"},
                    {"Asia/Yerevan", "Armenian Standard Time"},
                    {"Atlantic/Azores", "Azores Standard Time"},
                    {"Atlantic/Cape_Verde", "Cape Verde Standard Time"},
                    {"Atlantic/Reykjavik", "Greenwich Standard Time"},
                    {"Australia/Adelaide", "Cen. Australia Standard Time"},
                    {"Australia/Brisbane", "E. Australia Standard Time"},
                    {"Australia/Darwin", "AUS Central Standard Time"},
                    {"Australia/Hobart", "Tasmania Standard Time"},
                    {"Australia/Perth", "W. Australia Standard Time"},
                    {"Australia/Sydney", "AUS Eastern Standard Time"},
                    {"Etc/GMT", "UTC"},
                    {"Etc/GMT+11", "UTC-11"},
                    {"Etc/GMT+12", "Dateline Standard Time"},
                    {"Etc/GMT+2", "UTC-02"},
                    {"Etc/GMT-12", "UTC+12"},
                    {"Europe/Rome", "Central Europe Standard Time"},
                    {"Europe/Berlin", "W. Europe Standard Time"},
                    {"Europe/Budapest", "Central Europe Standard Time"},
                    {"Europe/Istanbul", "GTB Standard Time"},
                    {"Europe/Kiev", "FLE Standard Time"},
                    {"Europe/London", "GMT Standard Time"},
                    {"Europe/Minsk", "E. Europe Standard Time"},
                    {"Europe/Moscow", "Russian Standard Time"},
                    {"Europe/Paris", "Romance Standard Time"},
                    {"Europe/Warsaw", "Central European Standard Time"},
                    {"Indian/Mauritius", "Mauritius Standard Time"},
                    {"Pacific/Apia", "Samoa Standard Time"},
                    {"Pacific/Auckland", "New Zealand Standard Time"},
                    {"Pacific/Fiji", "Fiji Standard Time"},
                    {"Pacific/Guadalcanal", "Central Pacific Standard Time"},
                    {"Pacific/Honolulu", "Hawaiian Standard Time"},
                    {"Pacific/Port_Moresby", "West Pacific Standard Time"},
                    {"Pacific/Tongatapu", "Tonga Standard Time"}
                };

            _mapZones = from row in olsonWindowsTimes
                        select new MapZone
                        {
                            OlsonTimeZoneId = row.Key,
                            WindowsTimeZoneId = row.Value,
                            Territory = "001"
                        };
        }

        public static string OlsonTzId2WindowsTzId(string olsonTimeZoneId, bool defaultIfNoMatch = true)
        {
            var mapZone = GetMapZoneByWindowsTzId(olsonTimeZoneId);

            if (mapZone != null)
                return olsonTimeZoneId; //already Windows

            mapZone = GetMapZoneByOlsonTzId(olsonTimeZoneId);

            if (mapZone != null)
                return mapZone.WindowsTimeZoneId;

            Log.Error(string.Format("OlsonTimeZone {0} not found", olsonTimeZoneId));

            return defaultIfNoMatch ? "UTC" : null;
        }

        public static string WindowsTzId2OlsonTzId(string windowsTimeZoneId, bool defaultIfNoMatch = true)
        {
            var mapZone = GetMapZoneByOlsonTzId(windowsTimeZoneId);

            if (mapZone != null)
                return windowsTimeZoneId; //already Olson

            mapZone = GetMapZoneByWindowsTzId(windowsTimeZoneId);

            if (mapZone != null)
                return mapZone.OlsonTimeZoneId;

            Log.Error(string.Format("WindowsTimeZone {0} not found", windowsTimeZoneId));

            return defaultIfNoMatch ? "Etc/GMT" : null;
        }

        public static TimeZoneInfo GetTimeZone(string timeZoneId, bool defaultIfNoMatch = true)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    var mapZone = GetMapZoneByOlsonTzId(timeZoneId);

                    if (mapZone != null)
                    {
                        return TimeZoneInfo.FindSystemTimeZoneById(mapZone.WindowsTimeZoneId);
                    }

                    mapZone = GetMapZoneByWindowsTzId(timeZoneId);

                    if (mapZone != null)
                    {
                        return TimeZoneInfo.FindSystemTimeZoneById(mapZone.OlsonTimeZoneId);
                    }

                    Log.Error(string.Format("TimeZone {0} not found", timeZoneId));
                    return defaultIfNoMatch ? TimeZoneInfo.Utc : null;
                }
                catch (Exception error)
                {
                    Log.Error(error);
                    return defaultIfNoMatch ? TimeZoneInfo.Utc : null;
                }
            }
            catch (Exception error)
            {
                Log.Error(error);
                return defaultIfNoMatch ? TimeZoneInfo.Utc : null;
            }
        }

        private static MapZone GetMapZoneByOlsonTzId(string olsonTimeZoneId)
        {
            return _mapZones.FirstOrDefault(x =>
                x.OlsonTimeZoneId.Equals(olsonTimeZoneId, StringComparison.CurrentCultureIgnoreCase));
        }

        private static MapZone GetMapZoneByWindowsTzId(string windowsTimeZoneId)
        {
            return _mapZones.FirstOrDefault(x =>
                x.WindowsTimeZoneId.Equals(windowsTimeZoneId, StringComparison.CurrentCultureIgnoreCase) &&
                x.Territory.Equals("001", StringComparison.CurrentCultureIgnoreCase));
        }

        private class MapZone
        {
            public string OlsonTimeZoneId { get; set; }
            public string WindowsTimeZoneId { get; set; }
            public string Territory { get; set; }
        }
    }
}
