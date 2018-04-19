using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Web.Core.Calendars;


namespace ASC.Api.Calendar.iCalParser
{
    class DDayICalParser
    {
        public static Ical.Net.Interfaces.IICalendarCollection DeserializeCalendar(string iCalCalendarString)
        {
            if (string.IsNullOrEmpty(iCalCalendarString)) return null;

            try
            {
                using (var stringReader = new StringReader(iCalCalendarString))
                {
                    return Ical.Net.Calendar.LoadFromStream(stringReader);
                }
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static Ical.Net.Interfaces.IICalendarCollection DeserializeCalendar(TextReader reader)
        {
            try
            {
                return Ical.Net.Calendar.LoadFromStream(reader);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static string SerializeCalendar(Ical.Net.Interfaces.ICalendar calendar)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.iCalendar.Serializers.CalendarSerializer();
                return serializer.SerializeToString(calendar);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }



        public static Ical.Net.Interfaces.Components.IEvent DeserializeEvent(string iCalEventString)
        {
            if (string.IsNullOrEmpty(iCalEventString)) return null;

            try
            {
                using (var stringReader = new StringReader(iCalEventString))
                {
                    var serializer = new Ical.Net.Serialization.iCalendar.Serializers.CalendarSerializer();
                    return (Ical.Net.Interfaces.Components.IEvent) serializer.Deserialize(stringReader);
                }
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static Ical.Net.Interfaces.Components.IEvent DeserializeEvent(TextReader stringReader)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.iCalendar.Serializers.CalendarSerializer();
                return (Ical.Net.Interfaces.Components.IEvent) serializer.Deserialize(stringReader);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static string SerializeEvent(Ical.Net.Interfaces.Components.IEvent eventObj)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.iCalendar.Serializers.CalendarSerializer();
                return serializer.SerializeToString(eventObj);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }



        public static Ical.Net.Interfaces.DataTypes.IRecurrencePattern DeserializeRecurrencePattern(string iCalRecurrencePatternString)
        {
            if (string.IsNullOrEmpty(iCalRecurrencePatternString)) return null;

            try
            {
                using (var stringReader = new StringReader(iCalRecurrencePatternString))
                {
                    var serializer = new Ical.Net.Serialization.iCalendar.Serializers.DataTypes.RecurrencePatternSerializer();
                    return (Ical.Net.Interfaces.DataTypes.IRecurrencePattern) serializer.Deserialize(stringReader);
                }
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static Ical.Net.Interfaces.DataTypes.IRecurrencePattern DeserializeRecurrencePattern(TextReader stringReader)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.iCalendar.Serializers.DataTypes.RecurrencePatternSerializer();
                return (Ical.Net.Interfaces.DataTypes.IRecurrencePattern) serializer.Deserialize(stringReader);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static string SerializeRecurrencePattern(Ical.Net.Interfaces.DataTypes.IRecurrencePattern recurrencePattern)
        {
            try
            {
                var serializer = new Ical.Net.Serialization.iCalendar.Serializers.DataTypes.RecurrencePatternSerializer();
                return serializer.SerializeToString(recurrencePattern);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
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
            result.Version = Ical.Net.CalendarVersions.Latest;
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
                                                                  calandarObj.TimeZone.BaseUtcOffset,
                                                                  calandarObj.TimeZone.DisplayName,
                                                                  calandarObj.TimeZone.StandardName);

            result.AddTimeZone(Ical.Net.VTimeZone.FromSystemTimeZone(olsonTz));
            result.AddProperty("X-WR-TIMEZONE", olsonTzId);

            return result;
        }



        public static BaseEvent ConvertEvent(Ical.Net.Interfaces.Components.IEvent eventObj)
        {
            if (eventObj == null) return null;
            
            var result = new BusinessObjects.Event();

            result.Name = eventObj.Summary;

            result.Description = eventObj.Description;

            result.AllDayLong = eventObj.IsAllDay;

            result.Uid = eventObj.Uid;

            result.UtcStartDate = ToUtc(eventObj.Start);

            result.UtcEndDate = ToUtc(eventObj.End);

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

            result.Status = (EventStatus) eventObj.Status;

            return result;
        }

        public static Ical.Net.Interfaces.Components.IEvent ConvertEvent(BaseEvent eventObj)
        {
            if (eventObj == null) return null;

            var result = new Ical.Net.Event();

            result.Summary = eventObj.Name;

            result.Location = string.Empty;

            result.Description = eventObj.Description;

            result.IsAllDay = eventObj.AllDayLong;

            result.Uid = eventObj.Uid;

            result.Start = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(eventObj.UtcStartDate, DateTimeKind.Utc), TimeZoneInfo.Utc.Id);

            result.End = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(eventObj.UtcEndDate, DateTimeKind.Utc), TimeZoneInfo.Utc.Id);

            result.RecurrenceRules = new List<Ical.Net.Interfaces.DataTypes.IRecurrencePattern>();

            var rrule = eventObj.RecurrenceRule.ToString(true);

            if (!string.IsNullOrEmpty(rrule))
            {
                result.RecurrenceRules.Add(new Ical.Net.DataTypes.RecurrencePattern(rrule));
            }

            result.Status = (Ical.Net.EventStatus)eventObj.Status;

            return result;
        }


        public static Ical.Net.Interfaces.Components.IEvent CreateEvent(string name, string description, DateTime startUtcDate, DateTime endUtcDate, string repeatType, bool isAllDayLong, EventStatus status)
        {
            var evt = new Ical.Net.Event
                {
                    Summary = name,
                    Location = string.Empty,
                    Description = description,
                    IsAllDay = isAllDayLong,
                    DtStamp = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc), TimeZoneInfo.Utc.Id),
                    Start = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(startUtcDate, DateTimeKind.Utc), TimeZoneInfo.Utc.Id),
                    End = new Ical.Net.DataTypes.CalDateTime(DateTime.SpecifyKind(endUtcDate, DateTimeKind.Utc), TimeZoneInfo.Utc.Id),
                    RecurrenceRules = new List<Ical.Net.Interfaces.DataTypes.IRecurrencePattern>(),
                    Status = (Ical.Net.EventStatus)status
                };

            var rrule = RecurrenceRule.Parse(repeatType).ToString(true);

            if (!string.IsNullOrEmpty(rrule))
            {
                evt.RecurrenceRules.Add(new Ical.Net.DataTypes.RecurrencePattern(rrule));
            }

            return evt;
        }


        public static DateTime ToUtc(Ical.Net.Interfaces.DataTypes.IDateTime dateTime)
        {
            if (dateTime.IsUniversalTime || dateTime.TzId.Equals("UTC", StringComparison.InvariantCultureIgnoreCase))
                return dateTime.Value;

            if (dateTime.AsUtc != dateTime.Value)
                return dateTime.AsUtc;

            var timeZone = TimeZoneConverter.GetTimeZone(dateTime.TzId);
            var utcOffse = timeZone.GetUtcOffset(dateTime.Value);

            return dateTime.Value - utcOffse;
        }
    }
}
