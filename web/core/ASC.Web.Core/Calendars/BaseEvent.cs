/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
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

        public virtual string Name { get; set; }

        public virtual Guid OwnerId { get; set; }

        public virtual DateTime UtcEndDate { get; set; }

        public virtual DateTime UtcStartDate { get; set; }

        public virtual EventContext Context { get; set; }

        public virtual RecurrenceRule RecurrenceRule { get; set; }

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
            sb.AppendLine(String.Format("UID:{0}", this.Id));
            sb.AppendLine(String.Format("SUMMARY:{0}", this.Name));

            if (!string.IsNullOrEmpty(this.Description))
                sb.AppendLine(String.Format("DESCRIPTION:{0}", this.Description.Replace("\n","\\n")));

            if (this.AllDayLong)
            {
                DateTime startDate = this.UtcStartDate, endDate = this.UtcEndDate;
                if (this.TimeZone != null)
                {
                    if (this.UtcStartDate != DateTime.MinValue && startDate.Kind == DateTimeKind.Utc)
                        startDate = startDate + TimeZone.BaseUtcOffset;

                    if (this.UtcEndDate != DateTime.MinValue && startDate.Kind == DateTimeKind.Utc)
                        endDate = endDate + TimeZone.BaseUtcOffset;
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
