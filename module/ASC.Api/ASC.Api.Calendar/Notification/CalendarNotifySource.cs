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
using ASC.Api.Calendar.BusinessObjects;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Notify;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Core.Calendars;
using ASC.Web.Studio.Utility;

namespace ASC.Api.Calendar.Notification
{
    public class CalendarNotifyClient
    {
        private static INotifyClient _notifyClient;

        private static string _syncName = "calendarNotifySyncName";


        static CalendarNotifyClient()
        {
            _notifyClient = ASC.Core.WorkContext.NotifyContext.NotifyService.RegisterClient(CalendarNotifySource.Instance);
        }

        private static bool _isRegistered = false;
        public static void RegisterSendMethod()
        {
            if (!_isRegistered)
            {
                lock (_syncName)
                {
                    if (!_isRegistered)
                    {
                        _notifyClient.RegisterSendMethod(NotifyAbouFutureEvent, "0 * * ? * *");

                        _isRegistered = true;
                    }
                }
            }
        }

        private static void NotifyAbouFutureEvent(DateTime scheduleDate)
        {
            try
            {
                using (var provider = new DataProvider())
                {
                    foreach (var data in provider.ExtractAndRecountNotifications(scheduleDate))
                    {
                        if (data.Event == null || data.Event.Status == EventStatus.Cancelled)
                        {
                            continue;
                        }

                        var tenant = CoreContext.TenantManager.GetTenant(data.TenantId);
                        if (tenant == null ||
                            tenant.Status != TenantStatus.Active ||
                            TariffState.NotPaid <= CoreContext.PaymentManager.GetTariff(tenant.TenantId).State)
                        {
                            continue;
                        }
                        CoreContext.TenantManager.SetCurrentTenant(tenant);

                        var r =
                            CalendarNotifySource.Instance.GetRecipientsProvider().GetRecipient(data.UserId.ToString());
                        if (r == null)
                        {
                            continue;
                        }

                        var startDate = data.GetUtcStartDate();
                        var endDate = data.GetUtcEndDate();

                        if (!data.Event.AllDayLong)
                        {
                            startDate = startDate.Add(data.TimeZone.GetOffset());
                            endDate = (endDate == DateTime.MinValue
                                ? DateTime.MinValue
                                : endDate.Add(data.TimeZone.GetOffset()));
                        }

                        _notifyClient.SendNoticeAsync(CalendarNotifySource.EventAlert,
                            null,
                            r,
                            true,
                            new TagValue("EventName", data.Event.Name),
                            new TagValue("EventDescription", data.Event.Description ?? ""),
                            new TagValue("EventStartDate",
                                startDate.ToShortDateString() + " " + startDate.ToShortTimeString()),
                            new TagValue("EventEndDate",
                                (endDate > startDate)
                                    ? (endDate.ToShortDateString() + " " + endDate.ToShortTimeString())
                                    : ""),
                            new TagValue("Priority", 1));
                    }
                }
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Notify.Calendar").Error(error);
            }
        }

        public static void NotifyAboutSharingCalendar(ASC.Api.Calendar.BusinessObjects.Calendar calendar)
        {
            NotifyAboutSharingCalendar(calendar, null);
        }

        public static void NotifyAboutSharingCalendar(ASC.Api.Calendar.BusinessObjects.Calendar calendar, ASC.Api.Calendar.BusinessObjects.Calendar oldCalendar)
        {
            var initatorInterceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), SecurityContext.CurrentAccount.Name));
            try
            {
                var usr = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var userLink = PerformUrl(CommonLinkUtility.GetUserProfile(usr.ID.ToString(), false));

                foreach (var item in calendar.SharingOptions.PublicItems)
                {
                    if (oldCalendar != null && oldCalendar.SharingOptions.PublicItems.Exists(i => i.Id.Equals(item.Id)))
                        continue;

                    var r = CalendarNotifySource.Instance.GetRecipientsProvider().GetRecipient(item.Id.ToString());
                    if (r != null)
                    {
                        _notifyClient.SendNoticeAsync(CalendarNotifySource.CalendarSharing, null, r, true,
                            new TagValue("SharingType", "calendar"),
                            new TagValue("UserName", usr.DisplayUserName()),
                            new TagValue("UserLink", userLink),
                            new TagValue("CalendarName", calendar.Name));
                    }
                }
                _notifyClient.EndSingleRecipientEvent(_syncName);
            }
            finally
            {
                _notifyClient.RemoveInterceptor(initatorInterceptor.Name);
            }
        }

        public static void NotifyAboutSharingEvent(ASC.Api.Calendar.BusinessObjects.Event calendarEvent)
        {
            NotifyAboutSharingEvent(calendarEvent, null);
        }
        public static void NotifyAboutSharingEvent(ASC.Api.Calendar.BusinessObjects.Event calendarEvent, ASC.Api.Calendar.BusinessObjects.Event oldCalendarEvent)
        {
            var initatorInterceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), SecurityContext.CurrentAccount.Name));
            try
            {
                var usr = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var userLink = PerformUrl(CommonLinkUtility.GetUserProfile(usr.ID.ToString(), false));

                foreach (var item in calendarEvent.SharingOptions.PublicItems)
                {
                    if (oldCalendarEvent != null && oldCalendarEvent.SharingOptions.PublicItems.Exists(i => i.Id.Equals(item.Id)))
                        continue;

                    var r = CalendarNotifySource.Instance.GetRecipientsProvider().GetRecipient(item.Id.ToString());
                    if (r != null)
                    {
                        _notifyClient.SendNoticeAsync(CalendarNotifySource.CalendarSharing, null, r, true,
                            new TagValue("SharingType", "event"),
                            new TagValue("UserName", usr.DisplayUserName()),
                            new TagValue("UserLink", userLink),
                            new TagValue("EventName", calendarEvent.Name));
                    }
                }
                _notifyClient.EndSingleRecipientEvent(_syncName);
            }
            finally
            {
                _notifyClient.RemoveInterceptor(initatorInterceptor.Name);
            }
        }

        private static string PerformUrl(string url)
        {
            return CommonLinkUtility.GetFullAbsolutePath(url);
        }
    }


    public class CalendarNotifySource : NotifySource
    {
        public static INotifyAction CalendarSharing = new NotifyAction("CalendarSharingPattern");
        public static INotifyAction EventAlert = new NotifyAction("EventAlertPattern");

        public static CalendarNotifySource Instance
        {
            get;
            private set;
        }

        static CalendarNotifySource()
        {
            Instance = new CalendarNotifySource();
        }

        private CalendarNotifySource()
            : base(new Guid("{40650DA3-F7C1-424c-8C89-B9C115472E08}"))
        {
        }


        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(
                CalendarSharing,
                EventAlert);
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider2(CalendarPatterns.calendar_patterns);
        }
    }
}
