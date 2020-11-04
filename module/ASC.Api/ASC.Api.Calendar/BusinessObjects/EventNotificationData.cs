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
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.BusinessObjects
{
    public class EventNotificationData
    {
        public int TenantId { get; set; }
        public Guid UserId { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public DateTime NotifyUtcDate { get; set; }
        public RecurrenceRule RRule { get; set; }
        public EventAlertType AlertType { get; set; }
        public TimeZoneInfo TimeZone { get; set; }

        public EventNotificationData()
        {
            RRule = new RecurrenceRule();
        }

        public DateTime GetUtcStartDate()
        {
            if(Event==null)
                return DateTime.MinValue;

            if(RRule.Freq == Frequency.Never)
                return Event.UtcStartDate;

            return NotifyUtcDate.AddMinutes((-1) * DataProvider.GetBeforeMinutes(AlertType));
            
        }

        public DateTime GetUtcEndDate()
        {
             if(Event==null)
                return DateTime.MinValue;

             if (RRule.Freq == Frequency.Never || Event.AllDayLong || Event.UtcEndDate == DateTime.MinValue)
                return Event.UtcEndDate;
            
            return GetUtcStartDate().Add(Event.UtcEndDate - Event.UtcStartDate);
        }

    }
}
