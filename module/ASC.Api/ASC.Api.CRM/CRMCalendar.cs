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