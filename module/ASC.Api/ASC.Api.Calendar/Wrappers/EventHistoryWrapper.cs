/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

        ///<example type="int" name="calendarId">1</example>
        ///<order>0</order>
        [DataMember(Name = "calendarId", Order = 0)]
        public int CalendarId { get; set; }

        ///<example name="eventUid">uid1@onlyoffice.com</example>
        ///<order>10</order>
        [DataMember(Name = "eventUid", Order = 10)]
        public string EventUid { get; set; }

        ///<example type="int" name="eventId">1</example>
        ///<order>20</order>
        [DataMember(Name = "eventId", Order = 20)]
        public int EventId { get; set; }

        ///<example name="mergedIcs">BEGIN:VCALENDAR
        ///VERSION:2.0
        ///PRODID:onlyoffice.com
        ///BEGIN:VEVENT
        ///UID:uid1@onlyoffice.com
        ///DTSTAMP:19970714T170000Z
        ///ORGANIZER;CN=John Doe:MAILTO:john.doe@example.com
        ///DTSTART:19970714T170000Z
        ///DTEND:19970715T035959Z
        ///SUMMARY:Bastille Day Party
        ///END:VEVENT
        ///END:VCALENDAR</example>
        ///<order>30</order>
        [DataMember(Name = "mergedIcs", Order = 30)]
        public string MergedIcs { get; set; }
        //
        ///<example name="canEdit">true</example>
        ///<order>40</order>
        [DataMember(Name = "canEdit", Order = 40)]
        public bool CanEdit { get; set; }

        ///<example name="canNotify">true</example>
        ///<order>50</order>
        [DataMember(Name = "canNotify", Order = 50)]
        public bool CanNotify { get; set; }

        ///<example name="ics">some text</example>
        ///<order>60</order>
        [DataMember(Name = "ics", Order = 60)]
        public string Ics { get; set; }

        ///<type name="timeZone">ASC.Api.Calendar.Wrappers.TimeZoneWrapper, ASC.Api.Calendar</type>
        ///<order>70</order>
        [DataMember(Name = "timeZone", Order = 70)]
        public TimeZoneWrapper TimeZoneInfo { get; set; }

        ///<example name="calendarName">Calendar name</example>
        ///<order>80</order>
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
