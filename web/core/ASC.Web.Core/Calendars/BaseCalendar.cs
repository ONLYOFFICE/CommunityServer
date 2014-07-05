/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            sb.AppendLine(String.Format("X-WR-TIMEZONE:{0}", OlsenTimeZoneConverter.TimeZoneInfo2OlsonTZId(TimeZone)));
            //tz
            sb.AppendLine("BEGIN:VTIMEZONE");
            sb.AppendLine(String.Format("TZID:{0}", OlsenTimeZoneConverter.TimeZoneInfo2OlsonTZId(TimeZone)));
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
