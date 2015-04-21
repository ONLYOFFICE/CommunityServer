/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Collections.Generic;
using System.Linq;
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
