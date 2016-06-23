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
        public static DDay.iCal.IICalendarCollection DeserializeCalendar(string iCalCalendarString)
        {
            if (string.IsNullOrEmpty(iCalCalendarString)) return null;

            try
            {
                using (var stringReader = new StringReader(iCalCalendarString))
                {
                    return DDay.iCal.iCalendar.LoadFromStream(stringReader);
                }
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }
        
        public static DDay.iCal.IICalendarCollection DeserializeCalendar(TextReader reader)
        {
            try
            {
                return DDay.iCal.iCalendar.LoadFromStream(reader);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static DDay.iCal.IICalendarCollection DeserializeCalendar(Uri uri)
        {
            try
            {
                return DDay.iCal.iCalendar.LoadFromUri(uri);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static string SerializeCalendar(DDay.iCal.IICalendar calendar)
        {
            try
            {
                var context = new DDay.iCal.Serialization.SerializationContext();
                var factory = new DDay.iCal.Serialization.iCalendar.SerializerFactory();
                var serializer = factory.Build(calendar.GetType(), context) as DDay.iCal.Serialization.IStringSerializer;
                return serializer != null ? serializer.SerializeToString(calendar) : null;
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }



        public static DDay.iCal.IEvent DeserializeEvent(string iCalEventString)
        {
            if (string.IsNullOrEmpty(iCalEventString)) return null;

            try
            {
                using (var stringReader = new StringReader(iCalEventString))
                {
                    var serializer = new DDay.iCal.Serialization.iCalendar.EventSerializer();
                    return (DDay.iCal.IEvent) serializer.Deserialize(stringReader);
                }
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static DDay.iCal.IEvent DeserializeEvent(TextReader stringReader)
        {
            try
            {
                var serializer = new DDay.iCal.Serialization.iCalendar.EventSerializer();
                return (DDay.iCal.IEvent)serializer.Deserialize(stringReader);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static string SerializeEvent(DDay.iCal.IEvent eventObj)
        {
            try
            {
                var context = new DDay.iCal.Serialization.SerializationContext();
                var factory = new DDay.iCal.Serialization.iCalendar.SerializerFactory();
                var serializer = factory.Build(eventObj.GetType(), context) as DDay.iCal.Serialization.IStringSerializer;
                return serializer != null ? serializer.SerializeToString(eventObj) : null;
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }



        public static DDay.iCal.IRecurrencePattern DeserializeRecurrencePattern(string iCalRecurrencePatternString)
        {
            if (string.IsNullOrEmpty(iCalRecurrencePatternString)) return null;

            try
            {
                using (var stringReader = new StringReader(iCalRecurrencePatternString))
                {
                    var serializer = new DDay.iCal.Serialization.iCalendar.RecurrencePatternSerializer();
                    return (DDay.iCal.RecurrencePattern) serializer.Deserialize(stringReader);
                }
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static DDay.iCal.IRecurrencePattern DeserializeRecurrencePattern(TextReader stringReader)
        {
            try
            {
                var serializer = new DDay.iCal.Serialization.iCalendar.RecurrencePatternSerializer();
                return (DDay.iCal.RecurrencePattern)serializer.Deserialize(stringReader);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        public static string SerializeRecurrencePattern(DDay.iCal.IRecurrencePattern recurrencePattern)
        {
            try
            {
                var serializer = new DDay.iCal.Serialization.iCalendar.RecurrencePatternSerializer();
                return serializer.SerializeToString(recurrencePattern);
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }



        public static BaseCalendar ConvertCalendar(DDay.iCal.IICalendar calandarObj)
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

            var tzids = calandarObj.TimeZones.Select(x => x.TZID).Where(x => !string.IsNullOrEmpty(x)).ToList();

            result.TimeZone = tzids.Any()
                                  ? TimeZoneConverter.GetTimeZone(tzids.First())
                                  : (calandarObj.Properties.ContainsKey("X-WR-TIMEZONE")
                                         ? TimeZoneConverter.GetTimeZone(
                                             calandarObj.Properties["X-WR-TIMEZONE"].Value.ToString())
                                         : CoreContext.TenantManager.GetCurrentTenant().TimeZone);

            return result;
        }

        public static DDay.iCal.IICalendar ConvertCalendar(BaseCalendar calandarObj)
        {
            if (calandarObj == null) return null;
            
            var result = new DDay.iCal.iCalendar();

            result.Method = DDay.iCal.CalendarMethods.Publish;
            result.Scale = DDay.iCal.CalendarScales.Gregorian;
            result.Version = DDay.iCal.CalendarVersions.v2_0;
            result.ProductID = "-//Ascensio System//OnlyOffice Calendar//EN";

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

            result.AddTimeZone(olsonTz);
            result.AddProperty("X-WR-TIMEZONE", olsonTzId);

            return result;
        }



        public static BaseEvent ConvertEvent(DDay.iCal.IEvent eventObj)
        {
            if (eventObj == null) return null;
            
            var result = new BusinessObjects.Event();

            result.Name = eventObj.Summary;

            result.Description = eventObj.Description;

            result.AllDayLong = eventObj.IsAllDay;

            result.Uid = eventObj.UID;

            result.UtcStartDate = eventObj.Start.UTC;

            result.UtcEndDate = eventObj.End.UTC;

            result.RecurrenceRule = RecurrenceRule.Parse(eventObj.RecurrenceRules.Any()
                                                         ? SerializeRecurrencePattern(eventObj.RecurrenceRules.First())
                                                         : string.Empty);
            if (eventObj.ExceptionDates.Any())
            {
                result.RecurrenceRule.ExDates = new List<RecurrenceRule.ExDate>();
                foreach (var periodList in eventObj.ExceptionDates.First())
                {
                    result.RecurrenceRule.ExDates.Add(new RecurrenceRule.ExDate
                        {
                            Date = periodList.StartTime.UTC,
                            isDateTime = periodList.StartTime.UTC != periodList.StartTime.UTC.Date
                        });
                }
            }

            result.Status = (EventStatus) eventObj.Status;

            return result;
        }

        public static DDay.iCal.IEvent ConvertEvent(BaseEvent eventObj)
        {
            if (eventObj == null) return null;

            var result = new DDay.iCal.Event();

            result.Summary = eventObj.Name;

            result.Location = string.Empty;

            result.Description = eventObj.Description;

            result.IsAllDay = eventObj.AllDayLong;

            result.UID = eventObj.Uid;

            result.Start = new DDay.iCal.iCalDateTime(eventObj.UtcStartDate, TimeZoneInfo.Utc.Id);

            result.End = new DDay.iCal.iCalDateTime(eventObj.UtcEndDate, TimeZoneInfo.Utc.Id);

            result.RecurrenceRules = new List<DDay.iCal.IRecurrencePattern>();

            var rrule = eventObj.RecurrenceRule.ToString(true);

            if (!string.IsNullOrEmpty(rrule))
            {
                result.RecurrenceRules.Add(new DDay.iCal.RecurrencePattern(rrule));
            }

            result.Status = (DDay.iCal.EventStatus) eventObj.Status;

            return result;
        }

    }
}
