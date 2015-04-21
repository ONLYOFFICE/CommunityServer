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
using System.Web;
using ASC.Web.Studio.Utility;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Community.Birthdays.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;

namespace ASC.Web.Community.Birthdays
{
    [AjaxNamespace("AjaxPro.Birthdays")]
    public partial class Default : MainPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RenderScripts();
            Utility.RegisterTypeForAjax(GetType(), this);

            var today = GetTodayBirthdays();
            var upcoming = GetUpcomingBirthdays();

            if (today != null && today.Count > 0)
            {
                todayRpt.DataSource = today;
                todayRpt.DataBind();
            }

            if (upcoming == null || !upcoming.Any())
            {
                upcomingEmptyContent.Controls.Add(new EmptyScreenControl
                    {
                        Describe = BirthdaysResource.BirthdaysEmptyUpcomingTitle
                    });
            }
            else
            {
                upcomingRpt.DataSource = upcoming;
                upcomingRpt.DataBind();
            }

            Title = HeaderStringHelper.GetPageTitle(BirthdaysResource.BirthdaysModuleTitle);
        }

        protected void RenderScripts()
        {
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/products/community/modules/birthdays/app_themes/default/birthdays.css"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/products/community/modules/birthdays/js/birthdays.js"));
        }

        private List<UserInfo> GetTodayBirthdays()
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
                var now = TenantUtil.DateTimeNow();

                var xTicks = x.Subtract(new DateTime(x.Year, now.Month, now.Day)).Ticks;
                var yTicks = y.Subtract(new DateTime(y.Year, now.Month, now.Day)).Ticks;

                int sing;

                if (xTicks < 0 && yTicks < 0)
                    sing = 1;
                else if (xTicks < 0 && yTicks >= 0)
                    return 1;
                else if (xTicks >= 0 && yTicks < 0)
                    return -1;
                else
                    sing = 1;

                return sing*xTicks.CompareTo(yTicks);
            }
        }

        private IEnumerable<BirthdayWrapper> GetUpcomingBirthdays()
        {
            var now = TenantUtil.DateTimeNow().Date;

            return CoreContext.UserManager.GetUsers(EmployeeStatus.Active, EmployeeType.User)
                              .Where(x => x.BirthDate.HasValue && TrueDate(x.BirthDate.Value))
                              .OrderBy(x => x.BirthDate.Value, new BirthDateComparer())
                              .GroupBy(x => new DateTime(now.Year, x.BirthDate.Value.Month, x.BirthDate.Value.Day))
                              .Select(x => new BirthdayWrapper { Date = x.Key, Users = x.ToList() })
                              .SkipWhile(x => x.Date.CompareTo(now) == 0)
                              .Take(10);
        }

        protected bool TrueDate(DateTime dateTime)
        {
            var now = TenantUtil.DateTimeNow();
            var daysInMonth = DateTime.DaysInMonth(now.Year, dateTime.Month);
            if (daysInMonth < dateTime.Day)
            {
                return false;
            }
            return true;
        }

        protected bool IsInRemindList(Guid userID)
        {
            return BirthdaysNotifyClient.Instance.IsSubscribe(SecurityContext.CurrentAccount.ID, userID);
        }

        [AjaxMethod]
        public bool RemindAboutBirthday(Guid userID, bool onRemind)
        {
            BirthdaysNotifyClient.Instance.SetSubscription(SecurityContext.CurrentAccount.ID, userID, onRemind);
            return onRemind;
        }

        protected class BirthdayWrapper
        {
            public DateTime Date;
            public List<UserInfo> Users;
        }
    }
}