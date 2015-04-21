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
using System.Globalization;
using ASC.Web.Core.Calendars;
using ASC.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core;
using ASC.Web.Core;

namespace ASC.Api.CRM
{
    public sealed class CRMCalendar : BaseCalendar
    {
        private class Event : BaseEvent
        {
        }

        private readonly DaoFactory _daoFactory;

        public CRMCalendar(DaoFactory daoFactory, Guid userId)
        {
            _daoFactory = daoFactory;

            Context.HtmlBackgroundColor = "";
            Context.HtmlTextColor = "";
            Context.CanChangeAlertType = false;
            Context.CanChangeTimeZone = false;
            Context.GetGroupMethod = () => Web.CRM.Resources.CRMCommonResource.ProductName;
            Id = "crm_calendar";
            EventAlertType = EventAlertType.Never;
            Name = Web.CRM.Resources.CRMCommonResource.ProductName;
            Description = "";
            SharingOptions = new SharingOptions();
            SharingOptions.PublicItems.Add(new SharingOptions.PublicItem {Id = userId, IsGroup = false});
        }

        public override List<IEvent> LoadEvents(Guid userId, DateTime startDate, DateTime endDate)
        {
            var events = new List<IEvent>();

            if (!WebItemSecurity.IsAvailableForUser(WebItemManager.CRMProductID.ToString(), SecurityContext.CurrentAccount.ID))
            {
                return events;
            }

            var tasks = _daoFactory.GetTaskDao().GetTasks(String.Empty, userId, 0, false, DateTime.MinValue, DateTime.MinValue, EntityType.Any, 0, 0, 0, null);

            foreach (var t in tasks)
            {
                if (t.DeadLine == DateTime.MinValue) continue;

                var e = new Event
                    {
                        AlertType = EventAlertType.Never,
                        CalendarId = Id,
                        UtcStartDate = t.DeadLine,
                        UtcEndDate = t.DeadLine,
                        Id = t.ID.ToString(CultureInfo.InvariantCulture),
                        Name = Web.CRM.Resources.CRMCommonResource.ProductName + ": " + t.Title,
                        Description = t.Description
                    };

                if (t.DeadLine.Hour == 0 && t.DeadLine.Minute == 0)
                {
                    e.AllDayLong = true;
                }

                events.Add(e);
            }

            return events;
        }

        public override TimeZoneInfo TimeZone
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TimeZone; }
        }
    }
}