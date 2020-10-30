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
using System.IO;
using System.Linq;
using ASC.Common.Logging;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Web.Core.Calendars;


namespace ASC.Api.Calendar.iCalParser
{
    class DDayICalParser
    {
        public static Ical.Net.CalendarCollection DeserializeCalendar(string iCalCalendarString)
        {
            if (string.IsNullOrEmpty(iCalCalendarString)) return null;

            try
            {
                using (var stringReader = new StringReader(iCalCalendarString))
                {
                    return Ical.Net.CalendarCollection.Load(stringReader);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static Ical.Net.CalendarCollection DeserializeCalendar(TextReader reader)
        {
            try
            {
                return Ical.Net.CalendarCollection.Load(reader);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static string SerializeCalendar(Ical.Net.Calendar calendar)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.CalendarSerializer();
                return serializer.SerializeToString(calendar);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }



        public static Ical.Net.CalendarComponents.CalendarEvent DeserializeEvent(string iCalEventString)
        {
            if (string.IsNullOrEmpty(iCalEventString)) return null;

            try
            {
                using (var stringReader = new StringReader(iCalEventString))
                {
                    var serializer = new Ical.Net.Serialization.EventSerializer();
                    return (Ical.Net.CalendarComponents.CalendarEvent) serializer.Deserialize(stringReader);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static Ical.Net.CalendarComponents.CalendarEvent DeserializeEvent(TextReader stringReader)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.EventSerializer();
                return (Ical.Net.CalendarComponents.CalendarEvent) serializer.Deserialize(stringReader);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static string SerializeEvent(Ical.Net.CalendarComponents.CalendarEvent eventObj)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.EventSerializer();
                return serializer.SerializeToString(eventObj);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }



        public static Ical.Net.DataTypes.RecurrencePattern DeserializeRecurrencePattern(string iCalRecurrencePatternString)
        {
            if (string.IsNullOrEmpty(iCalRecurrencePatternString)) return null;

            try
            {
                using (var stringReader = new StringReader(iCalRecurrencePatternString))
                {
                    var serializer = new Ical.Net.Serialization.DataTypes.RecurrencePatternSerializer();
                    return (Ical.Net.DataTypes.RecurrencePattern) serializer.Deserialize(stringReader);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static Ical.Net.DataTypes.RecurrencePattern DeserializeRecurrencePattern(TextReader stringReader)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.DataTypes.RecurrencePatternSerializer();
                return (Ical.Net.DataTypes.RecurrencePattern)serializer.Deserialize(stringReader);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static string SerializeRecurrencePattern(Ical.Net.DataTypes.RecurrencePattern recurrencePattern)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.DataTypes.RecurrencePatternSerializer();
                return serializer.SerializeToString(recurrencePattern);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }



        public static BaseCalendar ConvertCalendar(Ical.Net.Calendar calandarObj)
        {
            if (calandarObj == null) return null;
            
            var result = new BusinessObjects.Calendar();

            result.Name = string.IsNullOrEmpty(calandarObj.Name)
                           ? calandarObj.Properties.ContainsKey("X-WR-CALNAME")
                                 ? calandarObj.Properties["X-WR-CALNAME"].Value.ToString()
                                 : string.Empty
                           : calandarObj.Name;

            result.Description = calandarObj.Properties.ContainsKey("X-WR-CALDESC")
                                     ? calandarObj.Properties["X-WR-CALDESC"].Value.ToString()
                                     : string.Empty;

            var tzids = calandarObj.TimeZones.Select(x => x.TzId).Where(x => !string.IsNullOrEmpty(x)).ToList();

            result.TimeZone = tzids.Any()
                                  ? TimeZoneConverter.GetTimeZone(tzids.First())
                                  : (calandarObj.Properties.ContainsKey("X-WR-TIMEZONE")
                                         ? TimeZoneConverter.GetTimeZone(
                                             calandarObj.Properties["X-WR-TIMEZONE"].Value.ToString())
                                         : CoreContext.TenantManager.GetCurrentTenant().TimeZone);

            return result;
        }

        public static Ical.Net.Calendar ConvertCalendar(BaseCalendar calandarObj)
        {
            if (calandarObj == null) return null;

            var result = new Ical.Net.Calendar();

            result.Method = Ical.Net.CalendarMethods.Publish;
            result.Scale = Ical.Net.CalendarScales.Gregorian;
            result.Version = Ical.Net.LibraryMetadata.Version;
            result.ProductId = "-//Ascensio System//OnlyOffice Calendar//EN";

            if (!string.IsNullOrEmpty(calandarObj.Name))
            {
                result.AddProperty("X-WR-CALNAME", calandarObj.Name);
            }

            if (!string.IsNullOrEmpty(calandarObj.Description))
            {
                result.AddProperty("X-WR-CALDESC", calandarObj.Description);
            }

            if (calandarObj.TimeZone == null)
                calandarObj.TimeZone = CoreContext.TenantManager.GetCurrentTenant().TimeZone;

            var olsonTzId = TimeZoneConverter.WindowsTzId2OlsonTzId(calandarObj.TimeZone.Id);
            var olsonTz = olsonTzId == calandarObj.TimeZone.Id
                              ? calandarObj.TimeZone
                              : TimeZoneInfo.CreateCustomTimeZone(olsonTzId,
                                                                  calandarObj.TimeZone.GetOffset(true),
                                                                  calandarObj.TimeZone.DisplayName,
                                                                  calandarObj.TimeZone.StandardName);

            result.AddTimeZone(Ical.Net.CalendarComponents.VTimeZone.FromSystemTimeZone(olsonTz));
            result.AddProperty("X-WR-TIMEZONE", olsonTzId);

            return result;
        }



        public static BaseEvent ConvertEvent(Ical.Net.CalendarComponents.CalendarEvent eventObj)
        {
            if (eventObj == null) return null;
            
            var result = new BusinessObjects.Event();

            result.Name = eventObj.Summary;

            result.Description = eventObj.Description;

            result.AllDayLong = eventObj.IsAllDay;

            result.Uid = eventObj.Uid;

            result.UtcStartDate = ToUtc(eventObj.Start);

            result.UtcEndDate = ToUtc(eventObj.End);

            result.UtcUpdateDate = ToUtc(eventObj.Created);

            var recurrenceRuleStr = string.Empty;

            if (eventObj.RecurrenceRules != null && eventObj.RecurrenceRules.Any())
            {
                var recurrenceRules = eventObj.RecurrenceRules.ToList();

                recurrenceRuleStr = SerializeRecurrencePattern(recurrenceRules.First());
            }

            result.RecurrenceRule = RecurrenceRule.Parse(recurrenceRuleStr);

            if (eventObj.ExceptionDates != null && eventObj.ExceptionDates.Any())
            {
                result.RecurrenceRule.ExDates = new List<RecurrenceRule.ExDate>();

                var exceptionDates = eventObj.ExceptionDates.ToList();

                foreach (var periodList in exceptionDates.First())
                {
                    var start = ToUtc(periodList.StartTime);

                    result.RecurrenceRule.ExDates.Add(new RecurrenceRule.ExDate
                        {
                            Date = start,
                            isDateTime = start != start.Date
                        });
                }
            }

            result.Status = ConvertEventStatus(eventObj.Status);

            return result;
        }

        public static Ical.Net.CalendarComponents.CalendarEvent ConvertEvent(BaseEvent eventObj)
        {
            if (eventObj == null) return null;

            var result = new Ical.Net.CalendarComponents.CalendarEvent();

            result.Summary = eventObj.Name;

            result.Location = string.Empty;

            result.Description = eventObj.Description;

            result.IsAllDay = eventObj.AllDayLong;

            result.Uid = eventObj.Uid;

            result.Start = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(eventObj.UtcStartDate, DateTimeKind.Utc), TimeZoneInfo.Utc.Id);

            result.End = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(eventObj.UtcEndDate, DateTimeKind.Utc), TimeZoneInfo.Utc.Id);

            result.Created = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(eventObj.UtcUpdateDate, DateTimeKind.Utc), TimeZoneInfo.Utc.Id);

            result.RecurrenceRules = new List<Ical.Net.DataTypes.RecurrencePattern>();

            var rrule = eventObj.RecurrenceRule.ToString(true);

            if (!string.IsNullOrEmpty(rrule))
            {
                result.RecurrenceRules.Add(new Ical.Net.DataTypes.RecurrencePattern(rrule));
            }

            result.Status = ConvertEventStatus(eventObj.Status);

            return result;
        }



        public static EventStatus ConvertEventStatus(string status)
        {
            switch (status)
            {
                case Ical.Net.EventStatus.Tentative:
                    return EventStatus.Tentative;
                case Ical.Net.EventStatus.Confirmed:
                    return EventStatus.Confirmed;
                case Ical.Net.EventStatus.Cancelled:
                    return EventStatus.Cancelled;
            }

            return EventStatus.Tentative;
        }

        public static string ConvertEventStatus(EventStatus status)
        {
            switch (status)
            {
                case EventStatus.Tentative:
                    return Ical.Net.EventStatus.Tentative;
                case EventStatus.Confirmed:
                    return Ical.Net.EventStatus.Confirmed;
                case EventStatus.Cancelled:
                    return Ical.Net.EventStatus.Cancelled;
            }

            return Ical.Net.EventStatus.Tentative;
        }



        public static Ical.Net.CalendarComponents.CalendarEvent CreateEvent(string name, string description, DateTime startUtcDate, DateTime endUtcDate, string repeatType, bool isAllDayLong, EventStatus status)
        {
            var evt = new Ical.Net.CalendarComponents.CalendarEvent
                {
                    Summary = name,
                    Location = string.Empty,
                    Description = description,
                    IsAllDay = isAllDayLong,
                    DtStamp = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc), TimeZoneInfo.Utc.Id),
                    Start = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(startUtcDate, DateTimeKind.Utc), TimeZoneInfo.Utc.Id),
                    End = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(endUtcDate, DateTimeKind.Utc), TimeZoneInfo.Utc.Id),
                    RecurrenceRules = new List<Ical.Net.DataTypes.RecurrencePattern>(),
                    Status = ConvertEventStatus(status)
                };

            var rrule = RecurrenceRule.Parse(repeatType).ToString(true);

            if (!string.IsNullOrEmpty(rrule))
            {
                evt.RecurrenceRules.Add(new Ical.Net.DataTypes.RecurrencePattern(rrule));
            }

            return evt;
        }


        public static DateTime ToUtc(Ical.Net.DataTypes.IDateTime dateTime)
        {
            if (dateTime.IsUtc || string.IsNullOrEmpty(dateTime.TzId) || dateTime.TzId.Equals("UTC", StringComparison.InvariantCultureIgnoreCase))
                return dateTime.Value;

            if (dateTime.AsUtc != dateTime.Value)
                return dateTime.AsUtc;

            var timeZone = TimeZoneConverter.GetTimeZone(dateTime.TzId);
            var utcOffse = timeZone.GetOffset();

            return dateTime.Value - utcOffse;
        }
    }
}
