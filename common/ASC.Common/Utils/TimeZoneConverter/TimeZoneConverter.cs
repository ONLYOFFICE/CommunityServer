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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

using ASC.Common.Logging;

namespace ASC.Common.Utils
{
    public class TimeZoneConverter
    {
        private static readonly ILog Log = LogManager.GetLogger("ASC.TimeZone");

        private static IEnumerable<MapZone> _mapZones;

        private static bool _customMode;

        private static bool _isMono;

        private static Dictionary<string, string> _translations;

        static TimeZoneConverter()
        {
            InitMapZones();
            InitTranslations();
        }

        private static void InitMapZones()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream("ASC.Common.Utils.TimeZoneConverter.windowsZones.xml"))
                {
                    var xml = XElement.Load(stream);
                    _mapZones = from row in xml.XPathSelectElements("*/mapTimezones/mapZone")
                                let olsonTimeZones = row.Attribute("type").Value.Split(' ')
                                from olsonTimeZone in olsonTimeZones
                                select new MapZone
                                {
                                    OlsonTimeZoneId = olsonTimeZone,
                                    WindowsTimeZoneId = row.Attribute("other").Value,
                                    Territory = row.Attribute("territory").Value
                                };
                }
            }
            catch (Exception error)
            {
                _mapZones = new MapZone[0];
                Log.Error(error);
            }
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

                    Log.InfoFormat("TimeZone {0} not found", timeZoneId);
                    return defaultIfNoMatch ? GetTimeZoneByOffset(timeZoneId) ?? TimeZoneInfo.Utc : null;
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

        private static TimeZoneInfo GetTimeZoneByOffset(string timeZoneId)
        {
            var systemTimeZones = TimeZoneInfo.GetSystemTimeZones();

            var timeZone = systemTimeZones.FirstOrDefault(tz =>
                                                          tz.DisplayName == timeZoneId ||
                                                          tz.StandardName == timeZoneId ||
                                                          tz.DaylightName == timeZoneId);

            if (timeZone != null) return timeZone;

            var regex = new Regex(@"[+-][0-9]{2}:[0-9]{2}\b");

            var offsetStr = regex.Match(timeZoneId).Value.TrimStart('+');

            if (string.IsNullOrEmpty(offsetStr)) return null;

            TimeSpan offset;

            if (!TimeSpan.TryParse(offsetStr, out offset))
                return null;

            return systemTimeZones.FirstOrDefault(tz => tz.BaseUtcOffset == offset);
        }

        private static void InitTranslations()
        {
            try
            {
                _customMode = ConfigurationManagerExtension.AppSettings["core.custom-mode"] == "true";

                _isMono = Type.GetType("Mono.Runtime") != null;

                if (!_customMode)
                {
                    _translations = new Dictionary<string, string>();
                    return;
                }

                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream("ASC.Common.Utils.TimeZoneConverter.timeZoneNames.xml"))
                {
                    var xml = XElement.Load(stream);
                    _translations = (from row in xml.XPathSelectElements("*/zone")
                                     select new KeyValuePair<string, string>(row.Attribute("type").Value, row.Value)
                                    ).ToDictionary(item => item.Key, item => item.Value);
                }
            }
            catch (Exception error)
            {
                _translations = new Dictionary<string, string>();
                Log.Error(error);
            }
        }

        public static string GetTimeZoneName(TimeZoneInfo timeZone)
        {
            if (!_customMode)
                return _isMono ? timeZone.Id : timeZone.DisplayName;

            return _translations.ContainsKey(timeZone.Id) ? _translations[timeZone.Id] : timeZone.DisplayName;
        }

        private class MapZone
        {
            public string OlsonTimeZoneId { get; set; }
            public string WindowsTimeZoneId { get; set; }
            public string Territory { get; set; }
        }
    }
}