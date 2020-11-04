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

namespace ASC.Web.Core.Calendars
{
    public delegate List<BaseCalendar> GetCalendarForUser(Guid userId);

    public class CalendarManager
    {   
        public static CalendarManager Instance
        {
            get;
            private set;
        }

        private List<GetCalendarForUser> _calendarProviders;
        private List<BaseCalendar> _calendars;

        static CalendarManager()
        {
            Instance = new CalendarManager();
        }

        private CalendarManager()
        {
            _calendars = new List<BaseCalendar>();
            _calendarProviders = new List<GetCalendarForUser>();
        }

        public void RegistryCalendar(BaseCalendar calendar)
        { 
            lock(this._calendars)
            {
                if (!this._calendars.Exists(c => String.Equals(c.Id, calendar.Id, StringComparison.InvariantCultureIgnoreCase)))
                    this._calendars.Add(calendar);
            }
        }

        public void UnRegistryCalendar(string calendarId)
        {
            lock (this._calendars)
            {
                this._calendars.RemoveAll(c => String.Equals(c.Id, calendarId, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public void RegistryCalendarProvider(GetCalendarForUser provider)
        {
            lock (this._calendarProviders)
            {
                if (!this._calendarProviders.Exists(p => p.Equals(provider)))
                    this._calendarProviders.Add(provider);
            }
        }

        public void UnRegistryCalendarProvider(GetCalendarForUser provider)
        {
            lock (this._calendarProviders)
            {
                this._calendarProviders.RemoveAll(p => p.Equals(provider));
            }
        }

        public BaseCalendar GetCalendarForUser(Guid userId, string calendarId)
        {
            return GetCalendarsForUser(userId).Find(c=> String.Equals(c.Id, calendarId, StringComparison.InvariantCultureIgnoreCase));
        }

        public List<BaseCalendar> GetCalendarsForUser(Guid userId)
        {
            var cals = new List<BaseCalendar>();
            foreach (var h in _calendarProviders)
            {
                var list =  h(userId);
                if(list!=null)
                    cals.AddRange(list.FindAll(c => c.SharingOptions.PublicForItem(userId)));
            }

            cals.AddRange(_calendars.FindAll(c => c.SharingOptions.PublicForItem(userId)));
            return cals;
        }

    }
}
