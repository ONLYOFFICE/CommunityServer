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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Web.Studio.Utility;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Community.Birthdays.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;

namespace ASC.Web.Community.Birthdays
{
    public partial class Default : MainPage
    {

        protected List<UserInfo> todayBirthdays;
        protected List<BirthdayWrapper> upcomingBirthdays;

        protected void Page_Load(object sender, EventArgs e)
        {
            RenderScripts();

            todayBirthdays = GetTodayBirthdays();
            upcomingBirthdays = GetUpcomingBirthdays().ToList();

            if (upcomingBirthdays == null || !upcomingBirthdays.Any())
            {
                upcomingEmptyContent.Controls.Add(new EmptyScreenControl
                    {
                        Describe = BirthdaysResource.BirthdaysEmptyUpcomingTitle
                    });
            }

            Title = HeaderStringHelper.GetPageTitle(BirthdaysResource.BirthdaysModuleTitle);
        }

        protected void RenderScripts()
        {
            Page
                .RegisterStyle("~/products/community/modules/birthdays/app_themes/default/birthdays.css")
                .RegisterBodyScripts("~/products/community/modules/birthdays/js/birthdays.js");
        }

        private static List<UserInfo> GetTodayBirthdays()
        {
            var today = TenantUtil.DateTimeNow();
            return (from u in CoreContext.UserManager.GetUsers(EmployeeStatus.Active, EmployeeType.User)
                    where u.BirthDate.HasValue && u.BirthDate.Value.Month.Equals(today.Month) && u.BirthDate.Value.Day.Equals(today.Day)
                    orderby u.DisplayUserName()
                    select u)
                .ToList();
        }

        private class BirthDateComparer : IComparer<DateTime>
        {
            public int Compare(DateTime x, DateTime y)
            {
                var today = TenantUtil.DateTimeNow();
                var leap = new DateTime(2000, today.Month, today.Day);

                var dx = new DateTime(2000, x.Month, x.Day).DayOfYear - leap.DayOfYear;
                var dy = new DateTime(2000, y.Month, y.Day).DayOfYear - leap.DayOfYear;

                if (dx < 0 && dy >= 0) return 1;
                if (dx >= 0 && dy < 0) return -1;

                return dx.CompareTo(dy);
            }
        }

        private static IEnumerable<BirthdayWrapper> GetUpcomingBirthdays()
        {
            var today = TenantUtil.DateTimeNow();

            return CoreContext.UserManager.GetUsers(EmployeeStatus.Active, EmployeeType.User)
                              .Where(x => x.BirthDate.HasValue)
                              .OrderBy(x => x.BirthDate.Value, new BirthDateComparer())
                              .GroupBy(x => new DateTime(2000, x.BirthDate.Value.Month, x.BirthDate.Value.Day)) // 29 february
                              .Select(x => new BirthdayWrapper {Date = x.Key, Users = x.ToList()})
                              .SkipWhile(x => x.Date.Month.Equals(today.Month) && x.Date.Day.Equals(today.Day))
                              .Take(10);
        }

        protected bool IsInRemindList(Guid userID)
        {
            return BirthdaysNotifyClient.Instance.IsSubscribe(SecurityContext.CurrentAccount.ID, userID);
        }

        protected class BirthdayWrapper
        {
            public DateTime Date;
            public List<UserInfo> Users;
        }
    }
}