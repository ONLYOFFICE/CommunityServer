/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Web;
using ASC.Api.Calendar.BusinessObjects;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Notify;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Studio.Utility;
using log4net;

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
                        var now = DateTime.UtcNow;
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
                using (var dataProvider = new DataProvider())
                {
                    foreach (var data in dataProvider.ExtractAndRecountNotifications(scheduleDate))
                    {
                        if (data.Event == null)
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

                        var r = CalendarNotifySource.Instance.GetRecipientsProvider().GetRecipient(data.UserId.ToString());
                        if (r == null)
                        {
                            continue;
                        }

                        var startDate = data.GetUtcStartDate();
                        var endDate = data.GetUtcEndDate();

                        if (!data.Event.AllDayLong)
                        {
                            startDate = startDate.Add(data.TimeZone.BaseUtcOffset);
                            endDate = (endDate == DateTime.MinValue ? DateTime.MinValue : endDate.Add(data.TimeZone.BaseUtcOffset));
                        }

                        _notifyClient.SendNoticeAsync(CalendarNotifySource.EventAlert,
                            null,
                            r,
                            true,
                            new TagValue("EventName", data.Event.Name),
                            new TagValue("EventDescription", data.Event.Description ?? ""),
                            new TagValue("EventStartDate", startDate.ToShortDateString() + " " + startDate.ToShortTimeString()),
                            new TagValue("EventEndDate", (endDate > startDate) ? (endDate.ToShortDateString() + " " + endDate.ToShortTimeString()) : ""),
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
            string port = string.Empty;
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
                port = HttpContext.Current.Request.GetUrlRewriter().IsDefaultPort ? string.Empty : ":" + HttpContext.Current.Request.GetUrlRewriter().Port;

            var result = string.Format("{0}://{1}{2}",
                   (HttpContext.Current != null && HttpContext.Current.Request != null) ? HttpContext.Current.Request.GetUrlRewriter().Scheme : Uri.UriSchemeHttp,
                   CoreContext.TenantManager.GetCurrentTenant().TenantDomain,
                   port) + ("/" + url).Replace("//", "/");

            return result;
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
