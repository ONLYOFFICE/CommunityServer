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


#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;

#endregion

namespace ASC.Projects.Core.Domain.Reports
{
    public class ReportFilter 
    {
        public ReportTimeInterval TimeInterval { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }


        public List<int> ProjectIds { get; set; }

        public int TagId { get; set; }

        public string ProjectTag { get; set; }

        public Guid UserId { get; set; }

        public Guid DepartmentId { get; set; }


        public List<ProjectStatus> ProjectStatuses { get; set; }

        public List<MilestoneStatus> MilestoneStatuses { get; set; }

        public MessageStatus? MessageStatus { get; set; }

        public List<TaskStatus> TaskStatuses { get; set; }

        public List<PaymentStatus> PaymentStatuses { get; set; }

        public int ViewType { get; set; }

        public bool NoResponsible { get; set; }


        public bool HasUserId
        {
            get { return UserId != default(Guid) || DepartmentId != default(Guid); }
        }

        public bool HasProjectIds
        {
            get { return 0 < ProjectIds.Count; }
        }

        public bool HasProjectStatuses
        {
            get { return 0 < ProjectStatuses.Count; }
        }

        public bool HasMilestoneStatuses
        {
            get { return 0 < MilestoneStatuses.Count; }
        }

        public bool HasTaskStatuses
        {
            get { return 0 < TaskStatuses.Count; }
        }


        public ReportFilter()
        {
            ToDate = DateTime.MaxValue;
            ProjectIds = new List<int>();
            ProjectStatuses = new List<ProjectStatus>();
            MilestoneStatuses = new List<MilestoneStatus>();
            TaskStatuses = new List<TaskStatus>();
            PaymentStatuses = new List<PaymentStatus>();
        }


        public void SetProjectIds(IEnumerable<int> ids)
        {
            ProjectIds.Clear();
            if (ids != null && ids.Any()) ProjectIds.AddRange(ids);
            else ProjectIds.Add(-1);
        }


        public List<string> GetUserIds()
        {
            var result = new List<string>();
            if (UserId != Guid.Empty)
            {
                result.Add(UserId.ToString());
            }
            else if (DepartmentId != Guid.Empty)
            {
                result.AddRange(CoreContext.UserManager.GetUsersByGroup(DepartmentId).Select(u => u.ID.ToString()));
            }
            return result;
        }

        public DateTime GetFromDate()
        {
            return GetFromDate(false);
        }

        public DateTime GetToDate()
        {
            return GetToDate(false);
        }

        public DateTime GetFromDate(bool toUtc)
        {
            var date = DateTime.MinValue;
            switch (TimeInterval)
            {
                case ReportTimeInterval.Absolute:
                    date = FromDate;
                    break;
                case ReportTimeInterval.Relative:
                    if (FromDate != DateTime.MinValue && FromDate != DateTime.MaxValue)
                    {
                        date = TenantUtil.DateTimeNow();
                    }
                    break;
                //Hack for Russian Standard Time
                case ReportTimeInterval.CurrYear:
                    date = GetDate(true);
                    date = date.AddHours(1);
                    break;
                default:
                    date = GetDate(true);
                    break;
            }
            if (date != DateTime.MinValue && date != DateTime.MaxValue && TimeInterval != ReportTimeInterval.CurrYear)
            {
                date = date.Date;
                if (toUtc) date = TenantUtil.DateTimeToUtc(date);
            }
            return date;
        }

        public DateTime GetToDate(bool toUtc)
        {
            var date = DateTime.MaxValue;
            switch (TimeInterval)
            {
                case ReportTimeInterval.Absolute:
                    date = ToDate;
                    break;
                case ReportTimeInterval.Relative:
                    if (FromDate != DateTime.MinValue && FromDate != DateTime.MaxValue && ToDate != DateTime.MinValue && ToDate != DateTime.MaxValue)
                    {
                        date = TenantUtil.DateTimeNow().Add(ToDate - FromDate);
                    }
                    break;
                default:
                    date = GetDate(false);
                    break;
            }
            if (date != DateTime.MinValue && date != DateTime.MaxValue)
            {
                date = date.Date.AddTicks(TimeSpan.TicksPerDay - 1);
                if (toUtc) date = TenantUtil.DateTimeToUtc(date);
            }
            return date;
        }

        private DateTime GetDate(bool start)
        {
            var date = TenantUtil.DateTimeNow();
            if (TimeInterval == ReportTimeInterval.Today)
            {
                return date;
            }
            if (TimeInterval == ReportTimeInterval.Yesterday)
            {
                return date.AddDays(-1);
            }
            if (TimeInterval == ReportTimeInterval.Tomorrow)
            {
                return date.AddDays(1);
            }
            if (TimeInterval == ReportTimeInterval.CurrWeek || TimeInterval == ReportTimeInterval.NextWeek || TimeInterval == ReportTimeInterval.PrevWeek)
            {
                var diff = CoreContext.TenantManager.GetCurrentTenant().GetCulture().DateTimeFormat.FirstDayOfWeek - date.DayOfWeek;
                if (0 < diff) diff -= 7;
                date = date.AddDays(diff);
                if (TimeInterval == ReportTimeInterval.NextWeek) date = date.AddDays(7);
                if (TimeInterval == ReportTimeInterval.PrevWeek) date = date.AddDays(-7);
                if (!start) date = date.AddDays(7).AddDays(-1);
                return date;
            }
            if (TimeInterval == ReportTimeInterval.CurrMonth || TimeInterval == ReportTimeInterval.NextMonth || TimeInterval == ReportTimeInterval.PrevMonth)
            {
                date = new DateTime(date.Year, date.Month, 1);
                if (TimeInterval == ReportTimeInterval.NextMonth) date = date.AddMonths(1);
                if (TimeInterval == ReportTimeInterval.PrevMonth) date = date.AddMonths(-1);
                if (!start) date = date.AddMonths(1).AddDays(-1);
                return date;
            }
            if (TimeInterval == ReportTimeInterval.CurrYear || TimeInterval == ReportTimeInterval.NextYear || TimeInterval == ReportTimeInterval.PrevYear)
            {
                date = new DateTime(date.Year, 1, 1);
                if (TimeInterval == ReportTimeInterval.NextYear) date = date.AddYears(1);
                if (TimeInterval == ReportTimeInterval.PrevYear) date = date.AddYears(-1);
                if (!start) date = date.AddYears(1).AddDays(-1);
                return date;
            }
            throw new ArgumentOutOfRangeException(string.Format("TimeInterval"));
        }
    }
}
