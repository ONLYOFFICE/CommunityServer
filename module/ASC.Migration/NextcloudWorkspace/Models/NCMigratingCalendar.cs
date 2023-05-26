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


using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Migration.Core.Models;
using ASC.Migration.Resources;

using Ical.Net;

namespace ASC.Migration.NextcloudWorkspace.Models.Parse
{
    public class NCMigratingCalendar : MigratingCalendar
    {
        public override int CalendarsCount => calendars.Count;

        public override int EventsCount => calendars.Count>0 && calendars.Values.First()!=null ? calendars.Values.SelectMany(c => c.SelectMany(x => x.Events)).Count(): calendars.Count;
        public override string ModuleName => MigrationResource.ModuleNameCalendar;

        private List<NCCalendars> userCalendars;
        private Dictionary<string, CalendarCollection> calendars = new Dictionary<string, CalendarCollection>();
        public NCMigratingCalendar(List<NCCalendars> calendars, Action<string, Exception> log) : base(log)
        {
            this.userCalendars = calendars;
        }
        
        public override void Parse()
        {
            //foreach(var calendar in this.userCalendars)
            //{
            //    var calendarString = "";
            //    foreach (var calendarEvent in calendar.CalendarObject)
            //    {
            //        string calendarEventByteToString = Encoding.Default.GetString(calendarEvent.CalendarData);
            //        if (calendarString != "")
            //        {
            //            int start = calendarEventByteToString.IndexOf("BEGIN:VEVENT");                        
            //            int end = calendarEventByteToString.IndexOf("END:VEVENT\r\n") + "END:VEVENT\r\n".Length;
            //            string newEvent = calendarEventByteToString.Substring(start, end - start);

            //            int insertionPoint = calendarString.LastIndexOf("END:VEVENT\r\n") + "END:VEVENT\r\n".Length;
            //            calendarString = calendarString.Insert(insertionPoint, newEvent);
            //        }
            //        else
            //        {
            //            calendarString = calendarEventByteToString;
            //        }
            //    }
            //    var events = DDayICalParser.DeserializeCalendar(calendarString);
            //    calendars.Add(calendar.DisplayName, events);
            //}
        }

        public override void Migrate()
        {
            if (!ShouldImport) return;
            throw new NotImplementedException();
        }
    }
}
