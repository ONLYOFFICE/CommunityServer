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
using System.Text;
using ASC.Common.Utils;
using ASC.Core;

namespace ASC.Web.Core.Calendars
{
    public abstract class BaseCalendar : ICalendar, ICloneable
    {
        public BaseCalendar()
        {
            this.Context = new CalendarContext();
            this.SharingOptions = new SharingOptions();
        }

        #region ICalendar Members

        public virtual string Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public virtual Guid OwnerId { get; set; }

        public virtual EventAlertType EventAlertType { get; set; }

        public abstract List<IEvent> LoadEvents(Guid userId, DateTime utcStartDate, DateTime utcEndDate);

        public virtual SharingOptions SharingOptions { get; set; }

        public virtual TimeZoneInfo TimeZone { get; set; }

        public virtual CalendarContext Context { get; set; }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            var cal = (BaseCalendar)this.MemberwiseClone();
            cal.Context = (CalendarContext)this.Context.Clone();
            cal.SharingOptions = (SharingOptions)this.SharingOptions.Clone();
            return cal;
        }

        #endregion

        #region IiCalFormatView Members

        public string ToiCalFormat()
        {
            var sb = new StringBuilder();

            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("PRODID:TeamLab Calendar");
            sb.AppendLine("VERSION:2.0");
            
            sb.AppendLine("METHOD:PUBLISH");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine(String.Format("X-WR-CALNAME:{0}", Name));
            sb.AppendLine(String.Format("X-WR-TIMEZONE:{0}", TimeZoneConverter.WindowsTzId2OlsonTzId(TimeZone.Id)));
            //tz
            sb.AppendLine("BEGIN:VTIMEZONE");
            sb.AppendLine(String.Format("TZID:{0}", TimeZoneConverter.WindowsTzId2OlsonTzId(TimeZone.Id)));
            sb.AppendLine("END:VTIMEZONE");

            //events
            foreach (var e in LoadEvents(SecurityContext.CurrentAccount.ID, DateTime.MinValue, DateTime.MaxValue))
            {
                if (e is BaseEvent && e.GetType().GetCustomAttributes(typeof(AllDayLongUTCAttribute),true).Length==0)
                    (e as BaseEvent).TimeZone = TimeZone;

                sb.AppendLine(e.ToiCalFormat());
            }

            sb.Append("END:VCALENDAR");

            return sb.ToString();
        }

        #endregion
    }
}
