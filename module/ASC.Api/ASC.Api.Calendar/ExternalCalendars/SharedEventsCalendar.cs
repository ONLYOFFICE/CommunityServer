/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Api.Calendar.BusinessObjects;
using ASC.Core;
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.ExternalCalendars
{
    public sealed class SharedEventsCalendar : BaseCalendar
    {
        public static string CalendarId { get { return "shared_events"; } }

        public SharedEventsCalendar()
        {
            Id = CalendarId;
            Context.HtmlBackgroundColor = "#0797ba";
            Context.HtmlTextColor = "#000000";
            Context.GetGroupMethod = () => Resources.CalendarApiResource.PersonalCalendarsGroup;
            Context.CanChangeTimeZone = true;
            Context.CanChangeAlertType = true;
            EventAlertType = EventAlertType.Hour;
            SharingOptions.SharedForAll = true;
        }

        public override List<IEvent> LoadEvents(Guid userId, DateTime utcStartDate, DateTime utcEndDate)
        {
            using (var provider = new DataProvider())
            {
                var events = provider.LoadSharedEvents(userId, CoreContext.TenantManager.GetCurrentTenant().TenantId, utcStartDate, utcEndDate);
                events.ForEach(e => e.CalendarId = Id);
                var ievents = new List<IEvent>(events.Select(e => (IEvent)e));
                return ievents;
            }
        }

        private string _name;
        public override string Name
        {
            get
            {
                return string.IsNullOrEmpty(_name) ? Resources.CalendarApiResource.SharedEventsCalendarName : _name;
            }
            set { _name = value; }
        }

        public override string Description
        {
            get { return Resources.CalendarApiResource.SharedEventsCalendarDescription; }
        }

        public override Guid OwnerId
        {
            get
            {
                return SecurityContext.CurrentAccount.ID;
            }
        }

        private TimeZoneInfo _timeZone;
        public override TimeZoneInfo TimeZone
        {
            get
            {
                return _timeZone ?? CoreContext.TenantManager.GetCurrentTenant().TimeZone;
            }
            set { _timeZone = value; }
        }
    }
}