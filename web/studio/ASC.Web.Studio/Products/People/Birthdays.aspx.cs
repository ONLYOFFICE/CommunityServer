/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.People.Core;
using ASC.Web.People.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.Masters.MasterManagement;
using ASC.Web.Studio.Utility;

namespace ASC.Web.People
{
    public partial class Birthdays : MainPage
    {
        protected List<UserInfo> todayBirthdays;
        protected List<BirthdayWrapper> upcomingBirthdays;



        protected void Page_Load(object sender, EventArgs e)
        {
            var isBirthdaysAvailable = WebItemManager.Instance.GetSubItems(PeopleProduct.ID).Any(item => item.ID == WebItemManager.BirthdaysProductID);

            if (!isBirthdaysAvailable)
            {
                Response.Redirect(PeopleProduct.GetStartURL());
            }

            RenderScripts();

            todayBirthdays = GetTodayBirthdays();
            upcomingBirthdays = GetUpcomingBirthdays().ToList();

            if (upcomingBirthdays == null || !upcomingBirthdays.Any())
            {
                upcomingEmptyContent.Controls.Add(new EmptyScreenControl
                {
                    ImgSrc = VirtualPathUtility.ToAbsolute("~/Products/People/App_Themes/default/images/birthdayEmpScr.svg"),
                    Header = BirthdaysResource.BirthdayEmptyScreenCaption,
                    Describe = BirthdaysResource.BirthdaysEmptyModuleDescription
                });
            }

            Title = HeaderStringHelper.GetPageTitle(BirthdaysResource.BirthdaysModuleTitle);
        }

        protected void RenderScripts()
        {
            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle("~/Products/People/App_Themes/dark/dark-birthdays.less");
            }
            else
            {
                Page.RegisterStyle("~/Products/People/App_Themes/default/css/birthdays.less");
            }
            Page.RegisterBodyScripts("~/Products/People/js/birthdays.js");
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
                              .Select(x => new BirthdayWrapper { Date = x.Key, Users = x.ToList() })
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