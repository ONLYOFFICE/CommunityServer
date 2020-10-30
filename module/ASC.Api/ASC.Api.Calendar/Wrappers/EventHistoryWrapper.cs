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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Calendar.BusinessObjects;
using ASC.Api.Calendar.iCalParser;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name = "eventHistory", Namespace = "")]
    public class EventHistoryWrapper
    {
        public EventHistoryWrapper(EventHistory eventHistory, bool canEdit, bool canNotify, BusinessObjects.Calendar cal, bool fullHistory = true)
        {
            CalendarId = eventHistory.CalendarId;
            EventUid = eventHistory.EventUid;
            EventId = eventHistory.EventId;

            if (fullHistory)
            {
                Ics = eventHistory.Ics;
            }
            else
            {
                var mergedCalendar = eventHistory.GetMerged();
                MergedIcs = mergedCalendar == null ? string.Empty : DDayICalParser.SerializeCalendar(mergedCalendar);
            }

            CanEdit = canEdit;
            CanNotify = canNotify;

            CalendarName = cal.Name;

            TimeZoneInfo = new TimeZoneWrapper(cal.ViewSettings.Any() && cal.ViewSettings.First().TimeZone != null
                                                   ? cal.ViewSettings.First().TimeZone
                                                   : cal.TimeZone);
        }

        [DataMember(Name = "calendarId", Order = 0)]
        public int CalendarId { get; set; }

        [DataMember(Name = "eventUid", Order = 10)]
        public string EventUid { get; set; }

        [DataMember(Name = "eventId", Order = 20)]
        public int EventId { get; set; }

        [DataMember(Name = "mergedIcs", Order = 30)]
        public string MergedIcs { get; set; }

        [DataMember(Name = "canEdit", Order = 40)]
        public bool CanEdit { get; set; }

        [DataMember(Name = "canNotify", Order = 50)]
        public bool CanNotify { get; set; }

        [DataMember(Name = "ics", Order = 60)]
        public string Ics { get; set; }

        [DataMember(Name = "timeZone", Order = 70)]
        public TimeZoneWrapper TimeZoneInfo { get; set; }

        [DataMember(Name = "calendarName", Order = 80)]
        public string CalendarName { get; set; }

        public static object GetSample()
        {
            return new
            {
                calendarId = 1,
                eventUid = "uid1@onlyoffice.com",
                eventId = 1,
                mergedIcs = "BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:onlyoffice.com\r\nBEGIN:VEVENT\r\nUID:uid1@onlyoffice.com\r\nDTSTAMP:19970714T170000Z\r\nORGANIZER;CN=John Doe:MAILTO:john.doe@example.com\r\nDTSTART:19970714T170000Z\r\nDTEND:19970715T035959Z\r\nSUMMARY:Bastille Day Party\r\nEND:VEVENT\r\nEND:VCALENDAR",
                canEdit = true,
                canNotify = true
            };
        }
    }
}
