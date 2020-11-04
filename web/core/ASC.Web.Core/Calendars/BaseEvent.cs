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
using System.Text;

namespace ASC.Web.Core.Calendars
{
    public abstract class BaseEvent : IEvent, ICloneable
    {
        internal TimeZoneInfo TimeZone { get; set; }

        public BaseEvent()
        {
            this.Context = new EventContext();
            this.AlertType = EventAlertType.Never;
            this.SharingOptions = new SharingOptions();
            this.RecurrenceRule = new RecurrenceRule();
        }

        #region IEvent Members

        public SharingOptions SharingOptions { get; set; }

        public virtual EventAlertType AlertType { get; set; }

        public virtual bool AllDayLong { get; set; }

        public virtual string CalendarId { get; set; }

        public virtual string Description { get; set; }

        public virtual string Id { get; set; }

        public virtual string Uid { get; set; }

        public virtual string Name { get; set; }

        public virtual Guid OwnerId { get; set; }

        public virtual DateTime UtcEndDate { get; set; }

        public virtual DateTime UtcStartDate { get; set; }
        
        public virtual DateTime UtcUpdateDate { get; set; }

        public virtual EventContext Context { get; set; }

        public virtual RecurrenceRule RecurrenceRule { get; set; }

        public virtual EventStatus Status { get; set; }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            var e = (BaseEvent)this.MemberwiseClone();
            e.Context = (EventContext)this.Context.Clone();
            e.RecurrenceRule = (RecurrenceRule)this.RecurrenceRule.Clone();
            e.SharingOptions = (SharingOptions)this.SharingOptions.Clone();
            return e;
        }

        #endregion


        #region IiCalFormatView Members

        public virtual string ToiCalFormat()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine(String.Format("UID:{0}", string.IsNullOrEmpty(this.Uid) ? this.Id : this.Uid));
            sb.AppendLine(String.Format("SUMMARY:{0}", this.Name));

            if (!string.IsNullOrEmpty(this.Description))
                sb.AppendLine(String.Format("DESCRIPTION:{0}", this.Description.Replace("\n","\\n")));

            if (this.AllDayLong)
            {
                DateTime startDate = this.UtcStartDate, endDate = this.UtcEndDate;
                if (this.TimeZone != null)
                {
                    if (this.UtcStartDate != DateTime.MinValue && startDate.Kind == DateTimeKind.Utc)
                        startDate = startDate.Add(TimeZone.GetOffset());

                    if (this.UtcEndDate != DateTime.MinValue && endDate.Kind == DateTimeKind.Utc)
                        endDate = endDate.Add(TimeZone.GetOffset());
                }

                if (this.UtcStartDate != DateTime.MinValue)
                    sb.AppendLine(String.Format("DTSTART;VALUE=DATE:{0}", startDate.ToString("yyyyMMdd")));

                if (this.UtcEndDate != DateTime.MinValue)
                    sb.AppendLine(String.Format("DTEND;VALUE=DATE:{0}", endDate.AddDays(1).ToString("yyyyMMdd")));
            }
            else
            {
                if (this.UtcStartDate != DateTime.MinValue)
                    sb.AppendLine(String.Format("DTSTART:{0}", this.UtcStartDate.ToString("yyyyMMdd'T'HHmmss'Z'")));

                if (this.UtcEndDate != DateTime.MinValue)
                    sb.AppendLine(String.Format("DTEND:{0}", this.UtcEndDate.ToString("yyyyMMdd'T'HHmmss'Z'")));
            }
            

            if (this.RecurrenceRule != null)
                sb.AppendLine(this.RecurrenceRule.ToiCalFormat());

            sb.Append("END:VEVENT");
            return sb.ToString();
        }

        #endregion     
        
    }
}
