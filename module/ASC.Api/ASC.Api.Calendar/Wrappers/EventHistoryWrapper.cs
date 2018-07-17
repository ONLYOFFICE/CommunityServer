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
