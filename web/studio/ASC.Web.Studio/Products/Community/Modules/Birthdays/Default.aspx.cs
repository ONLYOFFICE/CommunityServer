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
                        ImgSrc = VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Birthdays/App_Themes/default/images/birthday.png"),
                        Header = BirthdaysResource.BirthdayEmptyScreenCaption,
                        Describe = BirthdaysResource.BirthdaysEmptyModuleDescription
                    });
            }

            Title = HeaderStringHelper.GetPageTitle(BirthdaysResource.BirthdaysModuleTitle);
        }

        protected void RenderScripts()
        {
            Page
                .RegisterStyle("~/Products/Community/Modules/Birthdays/App_Themes/default/birthdays.css")
                .RegisterBodyScripts("~/Products/Community/Modules/Birthdays/js/birthdays.js");
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