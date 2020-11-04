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

        public virtual bool HasTaskStatuses
        {
            get { return 0 < TaskStatuses.Count && Substatus.HasValue; }
        }

        public int? Substatus { get; set; }

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
