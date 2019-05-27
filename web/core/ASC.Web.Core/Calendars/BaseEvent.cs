/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
