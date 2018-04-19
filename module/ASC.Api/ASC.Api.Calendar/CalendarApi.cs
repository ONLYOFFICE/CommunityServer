/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Calendar.BusinessObjects;
using ASC.Api.Calendar.ExternalCalendars;
using ASC.Api.Calendar.iCalParser;
using ASC.Api.Calendar.Notification;
using ASC.Api.Calendar.Wrappers;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Api.Interfaces.ResponseTypes;
using ASC.Api.Routing;
using ASC.Common.Caching;
using ASC.Common.Security;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Specific;
using ASC.Web.Core.Calendars;
using System.Configuration;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;

namespace ASC.Api.Calendar
{
    public class iCalApiContentResponse : IApiContentResponce
    {
        private System.IO.Stream _stream;
        private string _fileName;

        public iCalApiContentResponse(System.IO.Stream stream, string fileName)
        {
            _stream = stream;
            _fileName = fileName;
        }

        #region IApiContentResponce Members

        public System.Text.Encoding ContentEncoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

        public System.IO.Stream ContentStream
        {
            get { return _stream; }
        }

        public System.Net.Mime.ContentType ContentType
        {
            get { return new System.Net.Mime.ContentType("text/calendar; charset=UTF-8"); }
        }

        public System.Net.Mime.ContentDisposition ContentDisposition
        {
            get { return new System.Net.Mime.ContentDisposition() { Inline = true, FileName = _fileName }; }
        }

        #endregion
    }

    public class ExportDataCache
    {
        public static readonly ICache Cache = AscCache.Default;

        public static String GetCacheKey(string calendarId)
        {
            return String.Format("{0}_ExportCalendar_{1}", TenantProvider.CurrentTenantID, calendarId);
        }

        public static string Get(string calendarId)
        {
            return Cache.Get<string>(GetCacheKey(calendarId));
        }

        public static void Insert(string calendarId, string data)
        {
            if(string.IsNullOrEmpty(data))
                Reset(calendarId);
            else
                Cache.Insert(GetCacheKey(calendarId), data, TimeSpan.FromMinutes(5));
        }

        public static void Reset(string calendarId)
        {
            Cache.Remove(GetCacheKey(calendarId));
        }
    }

    public class CalendarApi : IApiEntryPoint, IDisposable
    {
        public static bool IsPersonal
        {
            get
            {
                return String.Equals(ConfigurationManager.AppSettings["web.personal"] ?? "false", "true");
            }
        }

        #region IApiEntryPoint Members

        public string Name
        {
            get { return "calendar"; }
        }

        #endregion

        private ApiContext _context;
        private int _monthCount = 3;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public CalendarApi(ApiContext context)
        {
            _context = context;
            CalendarManager.Instance.RegistryCalendar(new SharedEventsCalendar());

            var birthdayReminderCalendar = new BirthdayReminderCalendar();
            if (CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Core.Users.Constants.GroupVisitor.ID))
            {
                CalendarManager.Instance.UnRegistryCalendar(birthdayReminderCalendar.Id);
            }
            else
            {
                CalendarManager.Instance.RegistryCalendar(birthdayReminderCalendar);
            }

            _dataProvider = new DataProvider();
        }

        private CalendarApi()
        {
        }

        protected DataProvider _dataProvider;

        #region Calendars & Subscriptions

        /// <summary>
        /// Returns the list of all dates which contain the events from the displayed calendars
        /// </summary>
        /// <short>
        /// Calendar events
        /// </short>
        /// <param name="startDate">Period start date</param>
        /// <param name="endDate">Period end date</param>
        /// <returns>Date list</returns>
        /// <visible>false</visible>
        [Read("eventdays/{startDate}/{endDate}")]
        public List<ApiDateTime> GetEventDays(ApiDateTime startDate, ApiDateTime endDate)
        {
            var result = new List<CalendarWrapper>();
            int newCalendarsCount;
            //internal
            var calendars = _dataProvider.LoadCalendarsForUser(SecurityContext.CurrentAccount.ID, out newCalendarsCount);

            TimeZoneInfo userTimeZone = CoreContext.TenantManager.GetCurrentTenant().TimeZone;
            result.AddRange(calendars.ConvertAll<CalendarWrapper>(c => new CalendarWrapper(c)));

            if (!IsPersonal)
            {
                //external
                var extCalendars = CalendarManager.Instance.GetCalendarsForUser(SecurityContext.CurrentAccount.ID);
                var viewSettings = _dataProvider.GetUserViewSettings(SecurityContext.CurrentAccount.ID, extCalendars.ConvertAll<string>(c => c.Id));

                var extCalendarsWrappers = extCalendars.ConvertAll<CalendarWrapper>(c =>
                                        new CalendarWrapper(c, viewSettings.Find(o => o.CalendarId.Equals(c.Id, StringComparison.InvariantCultureIgnoreCase))))
                                        .FindAll(c => c.IsAcceptedSubscription);


                extCalendarsWrappers.ForEach(c => c.Events = c.UserCalendar.GetEventWrappers(SecurityContext.CurrentAccount.ID, startDate, endDate));
                var sharedEvents = extCalendarsWrappers.Find(c => String.Equals(c.Id, SharedEventsCalendar.CalendarId, StringComparison.InvariantCultureIgnoreCase));


                if (sharedEvents != null)
                    result.ForEach(c =>
                    {
                        c.Events = c.UserCalendar.GetEventWrappers(SecurityContext.CurrentAccount.ID, startDate, endDate);
                        c.Events.RemoveAll(e => sharedEvents.Events.Exists(s_ev => string.Equals(s_ev.Id, e.Id, StringComparison.InvariantCultureIgnoreCase)));
                    });
                else
                    result.ForEach(c => c.Events = c.UserCalendar.GetEventWrappers(SecurityContext.CurrentAccount.ID, startDate, endDate));

                result.AddRange(extCalendarsWrappers);
            }
            else
            {
                //remove all subscription except ical streams
                result.RemoveAll(c => c.IsSubscription && !c.IsiCalStream);

                result.ForEach(c => c.Events = c.UserCalendar.GetEventWrappers(SecurityContext.CurrentAccount.ID, startDate, endDate));
            }

            var days = new List<ApiDateTime>();
            foreach (var cal in result)
            {
                if (cal.IsHidden)
                    continue;

                foreach (var e in cal.Events)
                {
                    var d = (e.Start.UtcTime + e.Start.TimeZoneOffset).Date;
                    var dend = (e.End.UtcTime + e.End.TimeZoneOffset).Date;
                    while (d <= dend)
                    {
                        if (!days.Exists(day => day == d))
                            days.Add(new ApiDateTime(d, TimeZoneInfo.Utc));

                        d = d.AddDays(1);
                    }

                }
            }

            return days;
        }

        /// <summary>
        /// Returns the list of calendars and subscriptions with the events for the current user for the selected period
        /// </summary>
        /// <short>
        /// Calendars and subscriptions
        /// </short>
        /// <param name="startDate">Period start date</param>
        /// <param name="endDate">Period end date</param>
        /// <returns>List of calendars and subscriptions with events</returns>
        [Read("calendars/{startDate}/{endDate}")]
        public List<CalendarWrapper> LoadCalendars(ApiDateTime startDate, ApiDateTime endDate)
        {
            var result = LoadInternalCalendars();

            //external
            if (!IsPersonal)
            {
                var extCalendars = CalendarManager.Instance.GetCalendarsForUser(SecurityContext.CurrentAccount.ID);
                var viewSettings = _dataProvider.GetUserViewSettings(SecurityContext.CurrentAccount.ID, extCalendars.ConvertAll<string>(c => c.Id));

                var extCalendarsWrappers = extCalendars.ConvertAll<CalendarWrapper>(c =>
                                        new CalendarWrapper(c, viewSettings.Find(o => o.CalendarId.Equals(c.Id, StringComparison.InvariantCultureIgnoreCase))))
                                        .FindAll(c => c.IsAcceptedSubscription);


                extCalendarsWrappers.ForEach(c => c.Events = c.UserCalendar.GetEventWrappers(SecurityContext.CurrentAccount.ID, startDate, endDate));
                var sharedEvents = extCalendarsWrappers.Find(c => String.Equals(c.Id, SharedEventsCalendar.CalendarId, StringComparison.InvariantCultureIgnoreCase));
                if (sharedEvents != null)
                    result.ForEach(c =>
                    {
                        c.Events = c.UserCalendar.GetEventWrappers(SecurityContext.CurrentAccount.ID, startDate, endDate);
                        c.Events.RemoveAll(e => sharedEvents.Events.Exists(s_ev => string.Equals(s_ev.Id, e.Id, StringComparison.InvariantCultureIgnoreCase)));
                    });
                else
                    result.ForEach(c => c.Events = c.UserCalendar.GetEventWrappers(SecurityContext.CurrentAccount.ID, startDate, endDate));

                result.AddRange(extCalendarsWrappers);
            }
            else
            {
                //remove all subscription except ical streams
                result.RemoveAll(c => c.IsSubscription && !c.IsiCalStream);

                result.ForEach(c => c.Events = c.UserCalendar.GetEventWrappers(SecurityContext.CurrentAccount.ID, startDate, endDate));
            }

            return result;
        }

        private List<CalendarWrapper> LoadInternalCalendars()
        {
            var result = new List<CalendarWrapper>();
            int newCalendarsCount;
            //internal
            var calendars = _dataProvider.LoadCalendarsForUser(SecurityContext.CurrentAccount.ID, out newCalendarsCount);

            var userTimeZone = CoreContext.TenantManager.GetCurrentTenant().TimeZone;

            result.AddRange(calendars.ConvertAll(c => new CalendarWrapper(c)));
            if (!result.Exists(c => !c.IsSubscription))
            {
                //create first calendar
                var firstCal = _dataProvider.CreateCalendar(SecurityContext.CurrentAccount.ID,
                        Resources.CalendarApiResource.DefaultCalendarName, "", BusinessObjects.Calendar.DefaultTextColor, BusinessObjects.Calendar.DefaultBackgroundColor, userTimeZone, EventAlertType.FifteenMinutes, null, new List<SharingOptions.PublicItem>(), new List<UserViewSettings>());

                result.Add(new CalendarWrapper(firstCal));
            }

            return result;
        }

        /// <summary>
        /// Returns the list of all subscriptions available to the user
        /// </summary>
        /// <short>
        /// Subscription list
        /// </short>
        /// <returns>List of subscriptions</returns>
        [Read("subscriptions")]
        public List<SubscriptionWrapper> LoadSubscriptions()
        {
            var result = new List<SubscriptionWrapper>();

            if (!IsPersonal)
            {

                var calendars = _dataProvider.LoadSubscriptionsForUser(SecurityContext.CurrentAccount.ID);
                result.AddRange(calendars.FindAll(c => !c.OwnerId.Equals(SecurityContext.CurrentAccount.ID)).ConvertAll<SubscriptionWrapper>(c => new SubscriptionWrapper(c)));

                var iCalStreams = _dataProvider.LoadiCalStreamsForUser(SecurityContext.CurrentAccount.ID);
                result.AddRange(iCalStreams.ConvertAll<SubscriptionWrapper>(c => new SubscriptionWrapper(c)));


                var extCalendars = CalendarManager.Instance.GetCalendarsForUser(SecurityContext.CurrentAccount.ID);
                var viewSettings = _dataProvider.GetUserViewSettings(SecurityContext.CurrentAccount.ID, extCalendars.ConvertAll<string>(c => c.Id));

                result.AddRange(extCalendars.ConvertAll<SubscriptionWrapper>(c =>
                                        new SubscriptionWrapper(c, viewSettings.Find(o => o.CalendarId.Equals(c.Id, StringComparison.InvariantCultureIgnoreCase)))));


            }
            else
            {
                var iCalStreams = _dataProvider.LoadiCalStreamsForUser(SecurityContext.CurrentAccount.ID);
                result.AddRange(iCalStreams.ConvertAll<SubscriptionWrapper>(c => new SubscriptionWrapper(c)));
            }

            return result;
        }

        public class SubscriptionState
        {
            public string id { get; set; }
            public bool isAccepted { get; set; }
        }

        /// <summary>
        /// Updates the subscription state either subscribing or unsubscribing the user to/from it
        /// </summary>
        /// <short>
        /// Update subscription
        /// </short>
        /// <param name="states">Updated subscription states</param>
        /// <visible>false</visible>
        [Update("subscriptions/manage")]
        public void ManageSubscriptions(IEnumerable<SubscriptionState> states)
        {
            var viewSettings = _dataProvider.GetUserViewSettings(SecurityContext.CurrentAccount.ID, states.Select(s => s.id).ToList());

            var settingsCollection = new List<UserViewSettings>();
            foreach (var s in states)
            {
                var settings = viewSettings.Find(vs => vs.CalendarId.Equals(s.id, StringComparison.InvariantCultureIgnoreCase));
                if (settings == null)
                {
                    settings = new UserViewSettings()
                    {
                        CalendarId = s.id,
                        UserId = SecurityContext.CurrentAccount.ID
                    };
                }
                settings.IsAccepted = s.isAccepted;
                settingsCollection.Add(settings);

            }
            _dataProvider.UpdateCalendarUserView(settingsCollection);
        }

        /// <summary>
        /// Returns the detailed information about the calendar with the ID specified in the request
        /// </summary>
        /// <short>
        /// Calendar by ID
        /// </short>
        /// <param name="calendarId">Calendar ID</param>
        /// <returns>Calendar</returns>
        [Read("{calendarId}")]
        public CalendarWrapper GetCalendarById(string calendarId)
        {
            int calId;
            if (int.TryParse(calendarId, out calId))
            {
                var cal = _dataProvider.GetCalendarById(calId);
                return (cal != null ? new CalendarWrapper(cal) : null);
            }
            else
            {
                //external                
                var extCalendar = CalendarManager.Instance.GetCalendarForUser(SecurityContext.CurrentAccount.ID, calendarId);
                if (extCalendar != null)
                {
                    var viewSettings = _dataProvider.GetUserViewSettings(SecurityContext.CurrentAccount.ID, new List<string> { calendarId });
                    return new CalendarWrapper(extCalendar, viewSettings.FirstOrDefault());
                }
            }

            return null;
        }

        public class SharingParam : SharingOptions.PublicItem
        {
            public string actionId { get; set; }
            public Guid itemId
            {
                get { return base.Id; }
                set { base.Id = value; }
            }
            public bool isGroup
            {
                get { return base.IsGroup; }
                set { base.IsGroup = value; }
            }
        }

        /// <summary>
        /// Creates the new calendar with the parameters (name, description, color, etc.) specified in the request
        /// </summary>
        /// <short>
        /// Create calendar
        /// </short>
        /// <param name="name">Calendar name</param>
        /// <param name="description">Calendar description</param>
        /// <param name="textColor">Event text color</param>
        /// <param name="backgroundColor">Event background color</param>
        /// <param name="timeZone">Calendar time zone</param>
        /// <param name="alertType">Event alert type, in case alert type is set by default</param>
        /// <param name="sharingOptions">Calendar sharing options with other users</param>
        /// <returns>Created calendar</returns>
        [Create("")]
        public CalendarWrapper CreateCalendar(string name, string description, string textColor, string backgroundColor, string timeZone, EventAlertType alertType, List<SharingParam> sharingOptions)
        {
            var sharingOptionsList = sharingOptions ?? new List<SharingParam>();
            var timeZoneInfo = TimeZoneConverter.GetTimeZone(timeZone);

            name = (name ?? "").Trim();
            if (String.IsNullOrEmpty(name))
                throw new Exception(Resources.CalendarApiResource.ErrorEmptyName);

            description = (description ?? "").Trim();
            textColor = (textColor ?? "").Trim();
            backgroundColor = (backgroundColor ?? "").Trim();

            var cal = _dataProvider.CreateCalendar(
                        SecurityContext.CurrentAccount.ID, name, description, textColor, backgroundColor, timeZoneInfo, alertType, null,
                        sharingOptionsList.Select(o => o as SharingOptions.PublicItem).ToList(),
                        new List<UserViewSettings>());

            if (cal != null)
            {
                foreach (var opt in sharingOptionsList)
                    if (String.Equals(opt.actionId, AccessOption.FullAccessOption.Id, StringComparison.InvariantCultureIgnoreCase))
                        CoreContext.AuthorizationManager.AddAce(new AzRecord(opt.Id, CalendarAccessRights.FullAccessAction.ID, ASC.Common.Security.Authorizing.AceType.Allow, cal));

                //notify
                CalendarNotifyClient.NotifyAboutSharingCalendar(cal);

                return new CalendarWrapper(cal);
            }
            return null;
        }

        /// <summary>
        /// Updates the selected calendar with the parameters (name, description, color, etc.) specified in the request for the current user and access rights for other users
        /// </summary>
        /// <short>
        /// Update calendar
        /// </short>
        /// <param name="calendarId">Calendar ID</param>
        /// <param name="name">Calendar new name</param>
        /// <param name="description">Calendar new description</param>
        /// <param name="textColor">Event text color</param>
        /// <param name="backgroundColor">Event background color</param>
        /// <param name="timeZone">Calendar time zone</param>
        /// <param name="alertType">Event alert type, in case alert type is set by default</param>
        /// <param name="hideEvents">Display type: show or hide events in calendar</param>
        /// <param name="sharingOptions">Calendar sharing options with other users</param>
        /// <returns>Updated calendar</returns>
        [Update("{calendarId}")]
        public CalendarWrapper UpdateCalendar(string calendarId, string name, string description, string textColor, string backgroundColor, string timeZone, EventAlertType alertType, bool hideEvents, List<SharingParam> sharingOptions)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneConverter.GetTimeZone(timeZone);
            int calId;
            if (int.TryParse(calendarId, out calId))
            {
                var oldCal = _dataProvider.GetCalendarById(calId);
                if (CheckPermissions(oldCal, CalendarAccessRights.FullAccessAction, true))
                {
                    //update calendar and share options
                    var sharingOptionsList = sharingOptions ?? new List<SharingParam>();

                    name = (name ?? "").Trim();
                    if (String.IsNullOrEmpty(name))
                        throw new Exception(Resources.CalendarApiResource.ErrorEmptyName);

                    description = (description ?? "").Trim();
                    textColor = (textColor ?? "").Trim();
                    backgroundColor = (backgroundColor ?? "").Trim();


                    //view
                    var userOptions = oldCal.ViewSettings;
                    var usrOpt = userOptions.Find(o => o.UserId.Equals(SecurityContext.CurrentAccount.ID));
                    if (usrOpt == null)
                    {
                        userOptions.Add(new UserViewSettings()
                        {
                            Name = name,
                            TextColor = textColor,
                            BackgroundColor = backgroundColor,
                            EventAlertType = alertType,
                            IsAccepted = true,
                            UserId = SecurityContext.CurrentAccount.ID,
                            TimeZone = timeZoneInfo
                        });
                    }
                    else
                    {
                        usrOpt.Name = name;
                        usrOpt.TextColor = textColor;
                        usrOpt.BackgroundColor = backgroundColor;
                        usrOpt.EventAlertType = alertType;
                        usrOpt.TimeZone = timeZoneInfo;
                    }

                    userOptions.RemoveAll(o => !o.UserId.Equals(oldCal.OwnerId) && !sharingOptionsList.Exists(opt => (!opt.IsGroup && o.UserId.Equals(opt.Id))
                                                                               || opt.IsGroup && CoreContext.UserManager.IsUserInGroup(o.UserId, opt.Id)));

                    //check owner
                    if (!oldCal.OwnerId.Equals(SecurityContext.CurrentAccount.ID))
                    {
                        name = oldCal.Name;
                        description = oldCal.Description;
                    }

                    var cal = _dataProvider.UpdateCalendar(calId, name, description,
                                        sharingOptionsList.Select(o => o as SharingOptions.PublicItem).ToList(),
                                        userOptions);
                    if (cal != null)
                    {
                        //clear old rights
                        CoreContext.AuthorizationManager.RemoveAllAces(cal);

                        foreach (var opt in sharingOptionsList)
                            if (String.Equals(opt.actionId, AccessOption.FullAccessOption.Id, StringComparison.InvariantCultureIgnoreCase))
                                CoreContext.AuthorizationManager.AddAce(new AzRecord(opt.Id, CalendarAccessRights.FullAccessAction.ID, ASC.Common.Security.Authorizing.AceType.Allow, cal));

                        //notify
                        CalendarNotifyClient.NotifyAboutSharingCalendar(cal, oldCal);
                        return new CalendarWrapper(cal);
                    }
                    return null;
                }
            }

            //update view
            return UpdateCalendarView(calendarId, name, textColor, backgroundColor, timeZone, alertType, hideEvents);

        }

        /// <summary>
        /// Change the calendar display parameters specified in the request for the current user
        /// </summary>
        /// <short>
        /// Update calendar user view
        /// </short>
        /// <param name="calendarId">Calendar ID</param>
        /// <param name="name">Calendar name</param>
        /// <param name="textColor">Event text color</param>
        /// <param name="backgroundColor">Event background color</param>
        /// <param name="timeZone">Calendar time zone</param>
        /// <param name="alertType">Event alert type, in case alert type is set by default</param>
        /// <param name="hideEvents">Display type: show or hide events in calendar</param>
        /// <returns>Updated calendar</returns>
        [Update("{calendarId}/view")]
        public CalendarWrapper UpdateCalendarView(string calendarId, string name, string textColor, string backgroundColor, string timeZone, EventAlertType alertType, bool hideEvents)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneConverter.GetTimeZone(timeZone);
            name = (name ?? "").Trim();
            if (String.IsNullOrEmpty(name))
                throw new Exception(Resources.CalendarApiResource.ErrorEmptyName);

            var settings = new UserViewSettings()
            {
                BackgroundColor = backgroundColor,
                CalendarId = calendarId,
                IsHideEvents = hideEvents,
                TextColor = textColor,
                EventAlertType = alertType,
                IsAccepted = true,
                UserId = SecurityContext.CurrentAccount.ID,
                Name = name,
                TimeZone = timeZoneInfo
            };

            _dataProvider.UpdateCalendarUserView(settings);
            return GetCalendarById(calendarId);
        }

        /// <summary>
        /// Deletes the calendar with the ID specified in the request
        /// </summary>
        /// <short>
        /// Delete calendar
        /// </short>
        /// <param name="calendarId">Calendar ID</param>
        [Delete("{calendarId}")]
        public void RemoveCalendar(int calendarId)
        {
            var cal = _dataProvider.GetCalendarById(calendarId);

            //check permissions
            CheckPermissions(cal, CalendarAccessRights.FullAccessAction);
            //clear old rights
            CoreContext.AuthorizationManager.RemoveAllAces(cal);
            _dataProvider.RemoveCalendar(calendarId);

        }

        #endregion

        #region ICal/import

        /// <summary>
        /// Returns the link for the iCal associated with the calendar with the ID specified in the request
        /// </summary>
        /// <short>
        /// Get iCal link
        /// </short>
        /// <param name="calendarId">Calendar ID</param>
        /// <returns>iCal link</returns>
        [Read("{calendarId}/icalurl")]
        public string GetCalendariCalUrl(string calendarId)
        {
            var sig = Signature.Create<Guid>(SecurityContext.CurrentAccount.ID);
            var path = UrlPath.ResolveUrl(() => new CalendarApi().GetCalendariCalStream(calendarId, sig));
            return new Uri(_context.RequestContext.HttpContext.Request.GetUrlRewriter(), VirtualPathUtility.ToAbsolute("~/" + path)).ToString();
        }

        /// <summary>
        /// Returns the feed for the iCal associated with the calendar by its ID and signagure specified in the request
        /// </summary>
        /// <short>Get iCal feed</short>
        /// <param name="calendarId">Calendar ID</param>
        /// <param name="signature">Signature</param>
        /// <remarks>To get the feed you need to use the method returning the iCal feed link (it will generate the necessary signature)</remarks>
        /// <returns>Calendar iCal feed</returns>
        [Read("{calendarId}/ical/{signature}", false)] //NOTE: this method doesn't requires auth!!!
        public iCalApiContentResponse GetCalendariCalStream(string calendarId, string signature)
        {
            iCalApiContentResponse resp = null;
            var userId = Signature.Read<Guid>(signature);
            if (CoreContext.UserManager.GetUsers(userId).ID != ASC.Core.Users.Constants.LostUser.ID)
            {
                var currentUserId = Guid.Empty;
                if (SecurityContext.IsAuthenticated)
                {
                    currentUserId = SecurityContext.CurrentAccount.ID;
                    SecurityContext.Logout();
                }
                try
                {
                    SecurityContext.AuthenticateMe(userId);
                    var icalFormat = GetCalendariCalString(calendarId);
                    if (icalFormat != null)
                        resp = new iCalApiContentResponse(new MemoryStream(Encoding.UTF8.GetBytes(icalFormat)), calendarId + ".ics");
                }
                finally
                {
                    SecurityContext.Logout();
                    if (currentUserId != Guid.Empty)
                    {
                        SecurityContext.AuthenticateMe(currentUserId);
                    }
                }
            }
            return resp;
        }

        private string GetCalendariCalString(string calendarId)
        {
            log4net.LogManager.GetLogger("ASC.Calendar").Debug("GetCalendariCalString calendarId = " + calendarId);

            try
            {
                var result = ExportDataCache.Get(calendarId);

                if (!string.IsNullOrEmpty(result))
                    return result;

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                BaseCalendar icalendar;
                int calId;

                var viewSettings = _dataProvider.GetUserViewSettings(SecurityContext.CurrentAccount.ID, new List<string> { calendarId });

                if (int.TryParse(calendarId, out calId))
                {
                    icalendar = _dataProvider.GetCalendarById(calId);
                    if (icalendar != null)
                    {
                        icalendar = icalendar.GetUserCalendar(viewSettings.FirstOrDefault());
                    }
                }
                else
                {
                    //external                
                    icalendar = CalendarManager.Instance.GetCalendarForUser(SecurityContext.CurrentAccount.ID, calendarId);
                    if (icalendar != null)
                    {
                        icalendar = icalendar.GetUserCalendar(viewSettings.FirstOrDefault());
                    }
                }

                if (icalendar == null) return null;

                var ddayCalendar = DDayICalParser.ConvertCalendar(icalendar);
                ddayCalendar.Events.Clear();

                var events = icalendar.LoadEvents(SecurityContext.CurrentAccount.ID, DateTime.MinValue, DateTime.MaxValue);
                var eventIds = new List<int>();

                foreach ( var e in events)
                {
                    int evtId;

                    if (int.TryParse(e.Id, out evtId))
                        eventIds.Add(evtId);
                }

                var eventsHystory = _dataProvider.GetEventsHistory(eventIds.ToArray());

                foreach (var e in events)
                {
                    int evtId;
                    EventHistory evtHistory = null;

                    if (int.TryParse(e.Id, out evtId))
                        evtHistory = eventsHystory.FirstOrDefault(x => x.EventId == evtId);

                    if (evtHistory != null)
                    {
                        var mergedCalendar = evtHistory.GetMerged();
                        if (mergedCalendar == null || mergedCalendar.Events == null || !mergedCalendar.Events.Any())
                            continue;
                        ddayCalendar.Events.Add(mergedCalendar.Events.First());
                    }
                    else
                    {
                        var convertedEvent = DDayICalParser.ConvertEvent(e as BaseEvent);
                        if (string.IsNullOrEmpty(convertedEvent.Uid))
                            convertedEvent.Uid = DataProvider.GetEventUid(e.Uid, e.Id);
                        ddayCalendar.Events.Add(convertedEvent);
                    }
                }

                result = DDayICalParser.SerializeCalendar(ddayCalendar);

                ExportDataCache.Insert(calendarId, result);

                stopWatch.Stop();
                var timeSpan = stopWatch.Elapsed;
                var elapsedTime = String.Format("GetCalendariCalString elapsedTime = {0:00}:{1:00}:{2:00}.{3:00}",
                                                timeSpan.Hours,
                                                timeSpan.Minutes,
                                                timeSpan.Seconds,
                                                timeSpan.Milliseconds/10);

                log4net.LogManager.GetLogger("ASC.Calendar").Debug(elapsedTime);

                return result;
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Calendar").Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Imports the events from the iCal files
        /// </summary>
        /// <short>
        /// Import iCal
        /// </short>
        /// <param name="files">iCal formatted files with the events to be imported</param>
        /// <returns>Returns the number of imported events</returns>
        [Create("import")]
        public int ImportEvents(IEnumerable<System.Web.HttpPostedFileBase> files)
        {
            var calendar = LoadInternalCalendars().First(x => !x.IsSubscription);
            int calendarId;

            if (int.TryParse(calendar.Id, out calendarId))
                return ImportEvents(calendarId, files);

            throw new Exception(string.Format("Can't parse {0} to int", calendar.Id));
        }

        /// <summary>
        /// Imports the events from the iCal files to the existing calendar
        /// </summary>
        /// <short>
        /// Import iCal
        /// </short>
        /// <param name="calendarId">ID for the calendar which serves as the future storage base for the imported events</param>
        /// <param name="files">iCal formatted files with the events to be imported</param>
        /// <returns>Returns the number of imported events</returns>
        [Create("{calendarId}/import")]
        public int ImportEvents(int calendarId, IEnumerable<System.Web.HttpPostedFileBase> files)
        {
            var counter = 0;
            
            if (files != null)
            {
                foreach (var file in files)
                {
                    using (var reader = new StreamReader(file.InputStream))
                    {
                        var cals = DDayICalParser.DeserializeCalendar(reader);

                        counter = ImportEvents(calendarId, cals);
                    }
                }
            }

            return counter;
        }

        /// <summary>
        /// Imports the events from the iCal files
        /// </summary>
        /// <short>
        /// Import iCal
        /// </short>
        /// <param name="calendarId">Calendar ID</param>
        /// <param name="iCalString">iCal formatted string</param>
        /// <returns>Returns the number of imported events</returns>
        [Create("importIcs")]
        public int ImportEvents(int calendarId, string iCalString)
        {
            if (calendarId > 0)
            {
                var cals = DDayICalParser.DeserializeCalendar(iCalString);
                return ImportEvents(calendarId, cals);
            }

            var calendar = LoadInternalCalendars().First(x => !x.IsSubscription);

            if (int.TryParse(calendar.Id, out calendarId))
                return ImportEvents(calendarId, iCalString);

            throw new Exception(string.Format("Can't parse {0} to int", calendar.Id));
        }


        private int ImportEvents(int calendarId, Ical.Net.Interfaces.IICalendarCollection cals)
        {
            var counter = 0;

            CheckPermissions(_dataProvider.GetCalendarById(calendarId), CalendarAccessRights.FullAccessAction);

            if (cals == null) return counter;

            var calendars = cals.Where(x => string.IsNullOrEmpty(x.Method) ||
                                            x.Method == Ical.Net.CalendarMethods.Publish ||
                                            x.Method == Ical.Net.CalendarMethods.Request ||
                                            x.Method == Ical.Net.CalendarMethods.Reply ||
                                            x.Method == Ical.Net.CalendarMethods.Cancel).ToList();

            foreach (var calendar in calendars)
            {
                if (calendar.Events == null) continue;

                if (string.IsNullOrEmpty(calendar.Method))
                    calendar.Method = Ical.Net.CalendarMethods.Publish;

                foreach (var eventObj in calendar.Events)
                {
                    if (eventObj == null) continue;

                    var tmpCalendar = calendar.Copy<Ical.Net.Interfaces.ICalendar>();
                    tmpCalendar.Events.Clear();
                    tmpCalendar.Events.Add(eventObj);

                    var rrule = string.Empty;
                    var ics = DDayICalParser.SerializeCalendar(tmpCalendar);

                    var eventHistory = _dataProvider.GetEventHistory(eventObj.Uid);

                    if (eventHistory == null)
                    {
                        rrule = GetRRuleString(eventObj);

                        var utcStartDate = eventObj.IsAllDay ? eventObj.Start.Value : DDayICalParser.ToUtc(eventObj.Start);
                        var utcEndDate = eventObj.IsAllDay ? eventObj.End.Value : DDayICalParser.ToUtc(eventObj.End);

                        if (eventObj.IsAllDay && utcStartDate.Date < utcEndDate.Date)
                            utcEndDate = utcEndDate.AddDays(-1);

                        var result = CreateEvent(calendarId,
                                                 eventObj.Summary,
                                                 eventObj.Description,
                                                 utcStartDate,
                                                 utcEndDate,
                                                 RecurrenceRule.Parse(rrule),
                                                 EventAlertType.Default,
                                                 eventObj.IsAllDay,
                                                 null,
                                                 eventObj.Uid,
                                                 calendar.Method == Ical.Net.CalendarMethods.Cancel ? EventStatus.Cancelled : (EventStatus)eventObj.Status);

                        var eventId = result != null && result.Any() ? Int32.Parse(result.First().Id) : 0;

                        if (eventId > 0)
                        {
                            _dataProvider.AddEventHistory(calendarId, eventObj.Uid, eventId, ics);
                            counter++;
                        }
                    }
                    else
                    {
                        if (eventHistory.Contains(tmpCalendar)) continue;

                        eventHistory = _dataProvider.AddEventHistory(eventHistory.CalendarId, eventHistory.EventUid,
                                                                     eventHistory.EventId, ics);

                        var mergedCalendar = eventHistory.GetMerged();

                        if (mergedCalendar == null || mergedCalendar.Events == null || !mergedCalendar.Events.Any()) continue;

                        var mergedEvent = mergedCalendar.Events.First();

                        rrule = GetRRuleString(mergedEvent);

                        var utcStartDate = mergedEvent.IsAllDay ? mergedEvent.Start.Value : DDayICalParser.ToUtc(mergedEvent.Start);
                        var utcEndDate = mergedEvent.IsAllDay ? mergedEvent.End.Value : DDayICalParser.ToUtc(mergedEvent.End);

                        if (mergedEvent.IsAllDay && utcStartDate.Date < utcEndDate.Date)
                            utcEndDate = utcEndDate.AddDays(-1);

                        var targetEvent = _dataProvider.GetEventById(eventHistory.EventId);
                        var permissions = PublicItemCollection.GetForEvent(targetEvent);
                        var sharingOptions = permissions.Items
                            .Where(x => x.SharingOption.Id != AccessOption.OwnerOption.Id)
                            .Select(x => new SharingParam
                            {
                                Id = x.Id,
                                actionId = x.SharingOption.Id,
                                isGroup = x.IsGroup
                            }).ToList();

                        CreateEvent(eventHistory.CalendarId,
                                    mergedEvent.Summary,
                                    mergedEvent.Description,
                                    utcStartDate,
                                    utcEndDate,
                                    RecurrenceRule.Parse(rrule),
                                    EventAlertType.Default,
                                    mergedEvent.IsAllDay,
                                    sharingOptions,
                                    mergedEvent.Uid,
                                    (EventStatus)mergedEvent.Status);

                        counter++;
                    }
                }
            }

            return counter;
        }

        /// <summary>
        /// Creates a calendar by the link to the external iCal feed
        /// </summary>
        /// <short>
        /// Create calendar
        /// </short>
        /// <param name="iCalUrl">Link to the external iCal feed</param>
        /// <param name="name">Calendar name</param>
        /// <param name="textColor">Event text name</param>
        /// <param name="backgroundColor">Event background name</param>
        /// <returns>Created calendar</returns>
        [Create("calendarUrl")]
        public CalendarWrapper CreateCalendarStream(string iCalUrl, string name, string textColor, string backgroundColor)
        {
            var cal = iCalendar.GetFromUrl(iCalUrl);
            if (cal.isEmptyName)
                cal.Name = iCalUrl;

            if (String.IsNullOrEmpty(name))
                name = cal.Name;

            textColor = (textColor ?? "").Trim();
            backgroundColor = (backgroundColor ?? "").Trim();

            var calendar = _dataProvider.CreateCalendar(
                        SecurityContext.CurrentAccount.ID, name, cal.Description ?? "", textColor, backgroundColor,
                        cal.TimeZone, cal.EventAlertType, iCalUrl, null, new List<UserViewSettings>());

            if (calendar != null)
            {
                var calendarWrapperr = UpdateCalendarView(calendar.Id, calendar.Name, textColor, backgroundColor, calendar.TimeZone.Id, cal.EventAlertType, false);
                return calendarWrapperr;
            }

            return null;
        }

        #endregion

        #region Events

        /// <summary>
        /// Creates the new event in the default calendar with the parameters specified in the request
        /// </summary>
        /// <short>
        /// Create new event
        /// </short>
        /// <param name="name">Event name</param>
        /// <param name="description">Event description</param>
        /// <param name="startDate">Event start date</param>
        /// <param name="endDate">Event end date</param>
        /// <param name="repeatType">Event recurrence type (RRULE string in iCal format)</param>
        /// <param name="alertType">Event notification type</param>
        /// <param name="isAllDayLong">Event duration type: all day long or not</param>
        /// <param name="sharingOptions">Event sharing access parameters</param>
        /// <returns>Event list</returns>
        [Create("event")]
        public List<EventWrapper> AddEvent(string name, string description, ApiDateTime startDate, ApiDateTime endDate, string repeatType, EventAlertType alertType, bool isAllDayLong, List<SharingParam> sharingOptions)
        {
            var calendar = LoadInternalCalendars().First(x => !x.IsSubscription);
            int calendarId;

            if (int.TryParse(calendar.Id, out calendarId))
            {
                var cal = new Ical.Net.Calendar();
                cal.Events.Add(DDayICalParser.CreateEvent(name, description, startDate.UtcTime, endDate.UtcTime, repeatType, isAllDayLong, EventStatus.Confirmed));
                return AddEvent(calendarId, DDayICalParser.SerializeCalendar(cal), alertType, sharingOptions);
            }

            throw new Exception(string.Format("Can't parse {0} to int", calendar.Id));
        }

        /// <summary>
        /// Creates the new event in the selected calendar with the parameters specified in the request
        /// </summary>
        /// <short>
        /// Create new event
        /// </short>
        /// <param name="calendarId">ID of the calendar where the event is created</param>
        /// <param name="name">Event name</param>
        /// <param name="description">Event description</param>
        /// <param name="startDate">Event start date</param>
        /// <param name="endDate">Event end date</param>
        /// <param name="repeatType">Event recurrence type (RRULE string in iCal format)</param>
        /// <param name="alertType">Event notification type</param>
        /// <param name="isAllDayLong">Event duration type: all day long or not</param>
        /// <param name="sharingOptions">Event sharing access parameters</param>
        /// <returns>Event list</returns>
        [Create("{calendarId}/event")]
        public List<EventWrapper> AddEvent(int calendarId, string name, string description, ApiDateTime startDate, ApiDateTime endDate, string repeatType, EventAlertType alertType, bool isAllDayLong, List<SharingParam> sharingOptions)
        {
            var cal = new Ical.Net.Calendar();
            cal.Events.Add(DDayICalParser.CreateEvent(name, description, startDate.UtcTime, endDate.UtcTime, repeatType, isAllDayLong, EventStatus.Confirmed));
            return AddEvent(calendarId, DDayICalParser.SerializeCalendar(cal), alertType, sharingOptions);
        }

        private List<EventWrapper> CreateEvent(int calendarId, string name, string description, DateTime utcStartDate, DateTime utcEndDate, RecurrenceRule rrule, EventAlertType alertType, bool isAllDayLong, List<SharingParam> sharingOptions, string uid, EventStatus status)
        {
            var sharingOptionsList = sharingOptions ?? new List<SharingParam>();

            name = (name ?? "").Trim();
            description = (description ?? "").Trim();

            if (!string.IsNullOrEmpty(uid))
            {
                var existEvent = _dataProvider.GetEventByUid(uid);

                if (existEvent != null)
                {
                    return UpdateEvent(existEvent.CalendarId,
                                       int.Parse(existEvent.Id),
                                       name,
                                       description,
                                       new ApiDateTime(utcStartDate, TimeZoneInfo.Utc),
                                       new ApiDateTime(utcEndDate, TimeZoneInfo.Utc),
                                       rrule.ToString(),
                                       alertType,
                                       isAllDayLong,
                                       sharingOptions,
                                       status);
                }
            }

            CheckPermissions(_dataProvider.GetCalendarById(calendarId), CalendarAccessRights.FullAccessAction);

            var evt = _dataProvider.CreateEvent(calendarId,
                                                SecurityContext.CurrentAccount.ID,
                                                name,
                                                description,
                                                utcStartDate,
                                                utcEndDate,
                                                rrule,
                                                alertType,
                                                isAllDayLong,
                                                sharingOptionsList.Select(o => o as SharingOptions.PublicItem).ToList(),
                                                uid,
                                                status);

            if (evt != null)
            {
                foreach (var opt in sharingOptionsList)
                    if (String.Equals(opt.actionId, AccessOption.FullAccessOption.Id, StringComparison.InvariantCultureIgnoreCase))
                        CoreContext.AuthorizationManager.AddAce(new AzRecord(opt.Id, CalendarAccessRights.FullAccessAction.ID, Common.Security.Authorizing.AceType.Allow, evt));

                //notify
                CalendarNotifyClient.NotifyAboutSharingEvent(evt);

                return new EventWrapper(evt, SecurityContext.CurrentAccount.ID,
                                        _dataProvider.GetTimeZoneForCalendar(SecurityContext.CurrentAccount.ID, calendarId))
                                        .GetList(utcStartDate, utcStartDate.AddMonths(_monthCount));
            }
            return null;
        }

        /// <summary>
        /// Updates the existing event in the selected calendar with the parameters specified in the request
        /// </summary>
        /// <short>
        /// Update event
        /// </short>
        /// <param name="calendarId">ID of the calendar where the event belongs</param>
        /// <param name="eventId">Event ID</param>
        /// <param name="name">Event new name</param>
        /// <param name="description">Event new description</param>
        /// <param name="startDate">Event start date</param>
        /// <param name="endDate">Event end date</param>
        /// <param name="repeatType">Event recurrence type (RRULE string in iCal format)</param>
        /// <param name="alertType">Event notification type</param>
        /// <param name="isAllDayLong">Event duration type: all day long or not</param>
        /// <param name="sharingOptions">Event sharing access parameters</param>
        /// <param name="status">Event status</param>
        /// <returns>Updated event list</returns>
        [Update("{calendarId}/{eventId}")]
        public List<EventWrapper> Update(string calendarId, int eventId, string name, string description, ApiDateTime startDate, ApiDateTime endDate, string repeatType, EventAlertType alertType, bool isAllDayLong, List<SharingParam> sharingOptions, EventStatus status)
        {
            var cal = new Ical.Net.Calendar();
            cal.Events.Add(DDayICalParser.CreateEvent(name, description, startDate.UtcTime, endDate.UtcTime, repeatType, isAllDayLong, status));
            return UpdateEvent(eventId, calendarId, DDayICalParser.SerializeCalendar(cal), alertType, sharingOptions);
        }


        private List<EventWrapper> UpdateEvent(string calendarId, int eventId, string name, string description, ApiDateTime startDate, ApiDateTime endDate, string repeatType, EventAlertType alertType, bool isAllDayLong, List<SharingParam> sharingOptions, EventStatus status)
        {
            var sharingOptionsList = sharingOptions ?? new List<SharingParam>();

            var oldEvent = _dataProvider.GetEventById(eventId);

            if (oldEvent == null)
                throw new Exception(Resources.CalendarApiResource.ErrorItemNotFound);

            var cal = _dataProvider.GetCalendarById(Int32.Parse(oldEvent.CalendarId));

            if (!oldEvent.OwnerId.Equals(SecurityContext.CurrentAccount.ID) &&
                !CheckPermissions(oldEvent, CalendarAccessRights.FullAccessAction, true) &&
                !CheckPermissions(cal, CalendarAccessRights.FullAccessAction, true))
                throw new System.Security.SecurityException(Resources.CalendarApiResource.ErrorAccessDenied);

            name = (name ?? "").Trim();
            description = (description ?? "").Trim();

            TimeZoneInfo timeZone;

            var calId = int.Parse(oldEvent.CalendarId);

            if (!int.TryParse(calendarId, out calId))
            {
                calId = int.Parse(oldEvent.CalendarId);
                timeZone = _dataProvider.GetTimeZoneForSharedEventsCalendar(SecurityContext.CurrentAccount.ID);
            }
            else
                timeZone = _dataProvider.GetTimeZoneForCalendar(SecurityContext.CurrentAccount.ID, calId);

            var rrule = RecurrenceRule.Parse(repeatType);
            var evt = _dataProvider.UpdateEvent(eventId, calId,
                                                oldEvent.OwnerId, name, description, startDate.UtcTime, endDate.UtcTime, rrule, alertType, isAllDayLong,
                                                sharingOptionsList.Select(o => o as SharingOptions.PublicItem).ToList(), status);

            if (evt != null)
            {
                //clear old rights
                CoreContext.AuthorizationManager.RemoveAllAces(evt);

                foreach (var opt in sharingOptionsList)
                    if (String.Equals(opt.actionId, AccessOption.FullAccessOption.Id, StringComparison.InvariantCultureIgnoreCase))
                        CoreContext.AuthorizationManager.AddAce(new AzRecord(opt.Id, CalendarAccessRights.FullAccessAction.ID, Common.Security.Authorizing.AceType.Allow, evt));

                //notify
                CalendarNotifyClient.NotifyAboutSharingEvent(evt, oldEvent);

                evt.CalendarId = calendarId;
                return new EventWrapper(evt, SecurityContext.CurrentAccount.ID, timeZone).GetList(startDate.UtcTime, startDate.UtcTime.AddMonths(_monthCount));
            }
            return null;
        }



        /// <summary>
        /// Creates the new event in the selected calendar with the parameters specified in the request
        /// </summary>
        /// <short>
        /// Create new event
        /// </short>
        /// <param name="calendarId">ID of the calendar where the event is created</param>
        /// <param name="ics">Event in iCal format</param>
        /// <param name="alertType">Event notification type</param>
        /// <param name="sharingOptions">Event sharing access parameters</param>
        /// <returns>Event</returns>
        [Create("icsevent")]
        public List<EventWrapper> AddEvent(int calendarId, string ics, EventAlertType alertType, List<SharingParam> sharingOptions)
        {
            if (calendarId <= 0)
            {
                var defaultCalendar = LoadInternalCalendars().First(x => !x.IsSubscription);
                if (!int.TryParse(defaultCalendar.Id, out calendarId))
                    throw new Exception(string.Format("Can't parse {0} to int", defaultCalendar.Id));
            }
            
            var calendars = DDayICalParser.DeserializeCalendar(ics);

            if (calendars == null) return null;

            var calendar = calendars.FirstOrDefault();

            if (calendar == null || calendar.Events == null) return null;

            var eventObj = calendar.Events.FirstOrDefault();

            if (eventObj == null) return null;

            var calendarObj = _dataProvider.GetCalendarById(calendarId);
            var calendarObjViewSettings = calendarObj != null && calendarObj.ViewSettings != null ? calendarObj.ViewSettings.FirstOrDefault() : null;
            var targetCalendar = DDayICalParser.ConvertCalendar(calendarObj != null ? calendarObj.GetUserCalendar(calendarObjViewSettings) : null);

            if (targetCalendar == null) return null;

            var rrule = GetRRuleString(eventObj);

            var utcStartDate = eventObj.IsAllDay ? eventObj.Start.Value : DDayICalParser.ToUtc(eventObj.Start);
            var utcEndDate = eventObj.IsAllDay ? eventObj.End.Value : DDayICalParser.ToUtc(eventObj.End);

            if (eventObj.IsAllDay && utcStartDate.Date < utcEndDate.Date)
                utcEndDate = utcEndDate.AddDays(-1);

            var result = CreateEvent(calendarId,
                                     eventObj.Summary,
                                     eventObj.Description,
                                     utcStartDate,
                                     utcEndDate,
                                     RecurrenceRule.Parse(rrule),
                                     alertType,
                                     eventObj.IsAllDay,
                                     sharingOptions,
                                     DataProvider.GetEventUid(null),
                                     EventStatus.Confirmed);

            if (result == null || !result.Any()) return null;
            
            var evt = result.First();

            eventObj.Uid = evt.Uid;
            eventObj.Sequence = 0;
            eventObj.Status = Ical.Net.EventStatus.Confirmed;

            targetCalendar.Method = Ical.Net.CalendarMethods.Request;
            targetCalendar.Events.Clear();
            targetCalendar.Events.Add(eventObj);

            ics = DDayICalParser.SerializeCalendar(targetCalendar);

            _dataProvider.AddEventHistory(calendarId, evt.Uid, int.Parse(evt.Id), ics);

            return result;
        }

        /// <summary>
        /// Updates the existing event in the selected calendar with the parameters specified in the request
        /// </summary>
        /// <short>
        /// Update event
        /// </short>
        /// <param name="eventId">Event ID</param>
        /// <param name="calendarId">ID of the calendar where the event belongs</param>
        /// <param name="ics">Event in iCal format</param>
        /// <param name="alertType">Event notification type</param>
        /// <param name="sharingOptions">Event sharing access parameters</param>
        /// <returns>Updated event</returns>
        [Update("icsevent")]
        public List<EventWrapper> UpdateEvent(int eventId, string calendarId, string ics, EventAlertType alertType, List<SharingParam> sharingOptions)
        {
            var evt = _dataProvider.GetEventById(eventId);

            if (evt == null)
                throw new Exception(Resources.CalendarApiResource.ErrorItemNotFound);

            var cal = _dataProvider.GetCalendarById(Int32.Parse(evt.CalendarId));

            if (!evt.OwnerId.Equals(SecurityContext.CurrentAccount.ID) &&
                !CheckPermissions(evt, CalendarAccessRights.FullAccessAction, true) &&
                !CheckPermissions(cal, CalendarAccessRights.FullAccessAction, true))
                throw new System.Security.SecurityException(Resources.CalendarApiResource.ErrorAccessDenied);

            int calId;

            if (!int.TryParse(calendarId, out calId))
            {
                calId = int.Parse(evt.CalendarId);
            }

            EventHistory evtHistory = null;

            if (string.IsNullOrEmpty(evt.Uid))
            {
                evt.Uid = DataProvider.GetEventUid(evt.Uid);
                _dataProvider.SetEventUid(eventId, evt.Uid);
            }
            else
            {
                evtHistory = _dataProvider.GetEventHistory(eventId);
            }

            var sequence = 0;
            if (evtHistory != null)
            {
                var maxSequence = evtHistory.History.Select(x => x.Events.First()).Max(x => x.Sequence);

                if (evt.OwnerId == SecurityContext.CurrentAccount.ID && !CheckIsOrganizer(evtHistory))
                    sequence = maxSequence;
                else
                    sequence = maxSequence + 1;
            }

            var calendars = DDayICalParser.DeserializeCalendar(ics);

            if (calendars == null) return null;

            var calendar = calendars.FirstOrDefault();

            if (calendar == null || calendar.Events == null) return null;

            var eventObj = calendar.Events.FirstOrDefault();

            if (eventObj == null) return null;

            var calendarObj = _dataProvider.GetCalendarById(calId);
            var calendarObjViewSettings = calendarObj != null && calendarObj.ViewSettings != null ? calendarObj.ViewSettings.FirstOrDefault() : null;
            var targetCalendar = DDayICalParser.ConvertCalendar(calendarObj != null ? calendarObj.GetUserCalendar(calendarObjViewSettings) : null);

            if (targetCalendar == null) return null;

            eventObj.Uid = evt.Uid;
            eventObj.Sequence = sequence;
            eventObj.ExceptionDates.Clear();

            targetCalendar.Method = Ical.Net.CalendarMethods.Request;
            targetCalendar.Events.Clear();
            targetCalendar.Events.Add(eventObj);

            ics = (evtHistory != null ? (evtHistory.Ics + Environment.NewLine) : string.Empty) + DDayICalParser.SerializeCalendar(targetCalendar);

            _dataProvider.RemoveEventHistory(eventId);

            evtHistory = _dataProvider.AddEventHistory(calId, evt.Uid, eventId, ics);

            var mergedCalendar = evtHistory.GetMerged();

            if (mergedCalendar == null || mergedCalendar.Events == null || !mergedCalendar.Events.Any()) return null;

            var mergedEvent = mergedCalendar.Events.First();

            var rrule = GetRRuleString(mergedEvent);

            var utcStartDate = mergedEvent.IsAllDay ? mergedEvent.Start.Value : DDayICalParser.ToUtc(mergedEvent.Start);
            var utcEndDate = mergedEvent.IsAllDay ? mergedEvent.End.Value : DDayICalParser.ToUtc(mergedEvent.End);

            if (mergedEvent.IsAllDay && utcStartDate.Date < utcEndDate.Date)
                utcEndDate = utcEndDate.AddDays(-1);

            return UpdateEvent(calendarId,
                               eventId,
                               mergedEvent.Summary,
                               mergedEvent.Description,
                               new ApiDateTime(utcStartDate, TimeZoneInfo.Utc),
                               new ApiDateTime(utcEndDate, TimeZoneInfo.Utc),
                               rrule,
                               alertType,
                               mergedEvent.IsAllDay,
                               sharingOptions,
                               (EventStatus) mergedEvent.Status);
        }



        public enum EventRemoveType
        {
            Single = 0,
            AllFollowing = 1,
            AllSeries = 2
        }

        /// <summary>
        /// Deletes the whole event from the calendar (all events in the series)
        /// </summary>
        /// <short>
        /// Delete event series
        /// </short>
        /// <param name="eventId">Event ID</param>
        [Delete("events/{eventId}")]
        public void RemoveEvent(int eventId)
        {
            RemoveEvent(eventId, null, EventRemoveType.AllSeries);
        }

        /// <summary>
        /// Deletes one event from the series of recurrent events
        /// </summary>
        /// <short>
        /// Delete event
        /// </short>
        /// <param name="eventId">Event ID</param>
        /// <param name="date">Date to be deleted from the recurrent event</param>
        /// <param name="type">Recurrent event deletion type</param>
        /// <returns>Updated event series collection</returns>
        [Delete("events/{eventId}/custom")]
        public List<EventWrapper> RemoveEvent(int eventId, ApiDateTime date, EventRemoveType type)
        {
            var events = new List<EventWrapper>();
            var evt = _dataProvider.GetEventById(eventId);
            var cal = _dataProvider.GetCalendarById(Convert.ToInt32(evt.CalendarId));

            if (evt.OwnerId.Equals(SecurityContext.CurrentAccount.ID) || CheckPermissions(evt, CalendarAccessRights.FullAccessAction, true) || CheckPermissions(cal, CalendarAccessRights.FullAccessAction, true))
            {
                if (type == EventRemoveType.AllSeries || evt.RecurrenceRule.Freq == Frequency.Never)
                {
                    _dataProvider.RemoveEvent(eventId);
                    return events;
                }

                var utcDate = evt.AllDayLong
                                  ? date.UtcTime.Date
                                  : TimeZoneInfo.ConvertTime(new DateTime(date.UtcTime.Ticks),
                                                             cal.ViewSettings.Any() && cal.ViewSettings.First().TimeZone != null
                                                                 ? cal.ViewSettings.First().TimeZone
                                                                 : cal.TimeZone,
                                                             TimeZoneInfo.Utc);

                if (type == EventRemoveType.Single)
                {
                    evt.RecurrenceRule.ExDates.Add(new RecurrenceRule.ExDate
                        {
                            Date = evt.AllDayLong ? utcDate.Date : utcDate,
                            isDateTime = !evt.AllDayLong
                        });
                }
                else if (type == EventRemoveType.AllFollowing)
                {
                    var lastEventDate = evt.AllDayLong ? utcDate.Date : utcDate;
                    var dates = evt.RecurrenceRule
                        .GetDates(evt.UtcStartDate, evt.UtcStartDate, evt.UtcStartDate.AddMonths(_monthCount), int.MaxValue, false)
                        .Where(x => x < lastEventDate)
                        .ToList();

                    var untilDate = dates.Any() ? dates.Last() : evt.UtcStartDate.AddDays(-1);

                    evt.RecurrenceRule.Until = evt.AllDayLong ? untilDate.Date : untilDate;
                }

                evt = _dataProvider.UpdateEvent(int.Parse(evt.Id), int.Parse(evt.CalendarId), evt.OwnerId, evt.Name, evt.Description,
                                              evt.UtcStartDate, evt.UtcEndDate, evt.RecurrenceRule, evt.AlertType, evt.AllDayLong,
                                              evt.SharingOptions.PublicItems, evt.Status);

                if (type != EventRemoveType.AllSeries)
                {
                    var history = _dataProvider.GetEventHistory(eventId);
                    if (history != null)
                    {
                        var mergedCalendar = history.GetMerged();
                        if (mergedCalendar != null && mergedCalendar.Events != null && mergedCalendar.Events.Any())
                        {
                            if (evt.OwnerId != SecurityContext.CurrentAccount.ID || CheckIsOrganizer(history))
                            {
                                mergedCalendar.Events[0].Sequence++;
                            }

                            mergedCalendar.Events[0].RecurrenceRules.Clear();
                            
                            mergedCalendar.Events[0].RecurrenceRules.Add(DDayICalParser.DeserializeRecurrencePattern(evt.RecurrenceRule.ToString(true)));

                            mergedCalendar.Events[0].ExceptionDates.Clear();

                            foreach (var exDate in evt.RecurrenceRule.ExDates)
                            {
                                mergedCalendar.Events[0].ExceptionDates.Add(new Ical.Net.DataTypes.PeriodList
                                    {
                                        exDate.isDateTime ?
                                            new Ical.Net.DataTypes.CalDateTime(exDate.Date.Year, exDate.Date.Month, exDate.Date.Day, exDate.Date.Hour, exDate.Date.Minute, exDate.Date.Second) :
                                            new Ical.Net.DataTypes.CalDateTime(exDate.Date.Year, exDate.Date.Month, exDate.Date.Day)
                                    });
                            }

                            _dataProvider.AddEventHistory(int.Parse(evt.CalendarId), evt.Uid, int.Parse(evt.Id), DDayICalParser.SerializeCalendar(mergedCalendar));
                        }
                    }
                }

                //define timeZone
                TimeZoneInfo timeZone;
                if (!CheckPermissions(cal, CalendarAccessRights.FullAccessAction, true))
                {
                    timeZone = _dataProvider.GetTimeZoneForSharedEventsCalendar(SecurityContext.CurrentAccount.ID);
                    evt.CalendarId = SharedEventsCalendar.CalendarId;
                }
                else
                    timeZone = _dataProvider.GetTimeZoneForCalendar(SecurityContext.CurrentAccount.ID, int.Parse(evt.CalendarId));

                events = new EventWrapper(evt, SecurityContext.CurrentAccount.ID, timeZone).GetList(evt.UtcStartDate, evt.UtcStartDate.AddMonths(_monthCount));
            }
            else
                _dataProvider.UnsubscribeFromEvent(eventId, SecurityContext.CurrentAccount.ID);

            return events;
        }

        /// <summary>
        /// Unsubscribes the current user from the event with the ID specified in the request
        /// </summary>
        /// <short>
        /// Unsubscribe from event
        /// </short>
        /// <param name="eventId">Event ID</param>
        [Delete("events/{eventId}/unsubscribe")]
        public void UnsubscribeEvent(int eventId)
        {
            _dataProvider.UnsubscribeFromEvent(eventId, SecurityContext.CurrentAccount.ID);
        }

        /// <summary>
        /// Returns the event in ics format from history
        /// </summary>
        /// <short>
        /// Returns the event in ics format from history
        /// </short>
        /// <param name="eventUid">Event UID</param>
        /// <returns>Event History</returns>
        [Read("events/{eventUid}/historybyuid")]
        public EventHistoryWrapper GetEventHistoryByUid(string eventUid)
        {
            if (string.IsNullOrEmpty(eventUid))
            {
                throw new ArgumentException("eventUid");
            }
            
            var evt = _dataProvider.GetEventByUid(eventUid);

            return GetEventHistoryWrapper(evt);
        }

        /// <summary>
        /// Returns the event in ics format from history
        /// </summary>
        /// <short>
        /// Returns the event in ics format from history
        /// </short>
        /// <param name="eventId">Event ID</param>
        /// <returns>Event History</returns>
        [Read("events/{eventId}/historybyid")]
        public EventHistoryWrapper GetEventHistoryById(int eventId)
        {
            if (eventId <= 0)
            {
                throw new ArgumentException("eventId");
            }

            var evt = _dataProvider.GetEventById(eventId);

            return GetEventHistoryWrapper(evt);
        }

        #endregion

        private EventHistoryWrapper GetEventHistoryWrapper(Event evt, bool fullHistory = false)
        {
            if (evt == null) return null;

            int calId;
            BusinessObjects.Calendar cal = null;

            if (int.TryParse(evt.CalendarId, out calId))
                cal = _dataProvider.GetCalendarById(calId);

            if (cal == null) return null;

            int evtId;
            EventHistory history = null;

            if (int.TryParse(evt.Id, out evtId))
                history = _dataProvider.GetEventHistory(evtId);

            if (history == null) return null;

            return ToEventHistoryWrapper(evt, cal, history, fullHistory);
        }
        private EventHistoryWrapper ToEventHistoryWrapper(Event evt, BusinessObjects.Calendar cal, EventHistory history, bool fullHistory = false)
        {
            var canNotify = false;
            bool canEdit;

            var calIsShared = cal.SharingOptions.SharedForAll || cal.SharingOptions.PublicItems.Count > 0;
            if (calIsShared)
            {
                canEdit = canNotify = CheckPermissions(cal, CalendarAccessRights.FullAccessAction, true);
                return new EventHistoryWrapper(history, canEdit, canNotify, cal, fullHistory);
            }

            var evtIsShared = evt.SharingOptions.SharedForAll || evt.SharingOptions.PublicItems.Count > 0;
            if (evtIsShared)
            {
                canEdit = canNotify = CheckPermissions(evt, CalendarAccessRights.FullAccessAction, true);
                return new EventHistoryWrapper(history, canEdit, canNotify, cal, fullHistory);
            }

            canEdit = CheckPermissions(evt, CalendarAccessRights.FullAccessAction, true);
            if (canEdit)
            {
                canNotify = CheckIsOrganizer(history);
            }

            return new EventHistoryWrapper(history, canEdit, canNotify, cal, fullHistory);
        }
        private bool CheckIsOrganizer(EventHistory history)
        {
            var canNotify = false;
            
            var apiServer = new ApiServer();
            var apiResponse = apiServer.GetApiResponse(String.Format("{0}mail/accounts.json", SetupInfo.WebApiBaseUrl), "GET");
            var obj = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(apiResponse)));

            if (obj["response"] != null)
            {
                var accounts = (from account in JArray.Parse(obj["response"].ToString())
                                let email = account.Value<String>("email")
                                let enabled = account.Value<Boolean>("enabled")
                                let isGroup = account.Value<Boolean>("isGroup")
                                where enabled && !isGroup
                                select email).ToList();

                if (accounts.Any())
                {
                    var mergedHistory = history.GetMerged();
                    if (mergedHistory != null && mergedHistory.Events != null)
                    {
                        var eventObj = mergedHistory.Events.FirstOrDefault();
                        if (eventObj != null && eventObj.Organizer != null)
                        {
                            var organizerEmail = eventObj.Organizer.Value.ToString()
                                                         .ToLowerInvariant()
                                                         .Replace("mailto:", "");

                            canNotify = accounts.Contains(organizerEmail);
                        }
                    }
                }
            }

            return canNotify;
        }
        private string GetRRuleString(Ical.Net.Interfaces.Components.IEvent evt)
        {
            var rrule = string.Empty;

            if (evt.RecurrenceRules != null && evt.RecurrenceRules.Any())
            {
                var recurrenceRules = evt.RecurrenceRules.ToList();

                rrule = DDayICalParser.SerializeRecurrencePattern(recurrenceRules.First());

                if (evt.ExceptionDates != null && evt.ExceptionDates.Any())
                {
                    rrule += ";exdates=";

                    var exceptionDates = evt.ExceptionDates.ToList();

                    foreach (var periodList in exceptionDates)
                    {
                        var date = periodList.ToString();

                        //has time
                        if (date.ToLowerInvariant().IndexOf('t') >= 0)
                        {
                            //is utc time
                            if (date.ToLowerInvariant().IndexOf('z') >= 0)
                            {
                                rrule += date;
                            }
                            else
                            {
                                //convert to utc time
                                DateTime dt;
                                if (DateTime.TryParseExact(date.ToUpper(), "yyyyMMdd'T'HHmmssK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt))
                                {
                                    var tzid = periodList.TzId ?? evt.Start.TzId;
                                    if (!String.IsNullOrEmpty(tzid))
                                    {
                                        dt = TimeZoneInfo.ConvertTime(dt, TimeZoneConverter.GetTimeZone(tzid), TimeZoneInfo.Utc);
                                    }
                                    rrule += dt.ToString("yyyyMMdd'T'HHmmssK");
                                }
                                else
                                {
                                    rrule += date;
                                }
                            }
                        }
                        else
                        {
                            rrule += date;
                        }

                        rrule += ",";
                    }

                    rrule = rrule.TrimEnd(',');
                }
            }

            return rrule;
        }
        private void CheckPermissions(ISecurityObject securityObj, Common.Security.Authorizing.Action action)
        {
            CheckPermissions(securityObj, action, false);
        }
        private bool CheckPermissions(ISecurityObject securityObj, Common.Security.Authorizing.Action action, bool silent)
        {
            if (securityObj == null)
                throw new Exception(Resources.CalendarApiResource.ErrorItemNotFound);

            if (silent)
                return SecurityContext.CheckPermissions(securityObj, action);

            SecurityContext.DemandPermissions(securityObj, action);

            return true;
        }

        /// <summary>
        /// Returns the sharing access parameters to the calendar with the ID specified in the request
        /// </summary>
        /// <short>
        /// Get access parameters
        /// </short>
        /// <param name="calendarId">Calendar ID</param>
        /// <returns>Sharing access parameters</returns>
        [Read("{calendarId}/sharing")]
        public PublicItemCollection GetCalendarSharingOptions(int calendarId)
        {
            var cal = _dataProvider.GetCalendarById(calendarId);
            if (cal == null)
                throw new Exception(Resources.CalendarApiResource.ErrorItemNotFound);

            return PublicItemCollection.GetForCalendar(cal);
        }

        /// <summary>
        /// Returns the default values for the sharing access parameters
        /// </summary>
        /// <short>
        /// Get default access
        /// </short>
        /// <returns>Default sharing access parameters</returns>
        [Read("sharing")]
        public PublicItemCollection GetDefaultSharingOptions()
        {
            return PublicItemCollection.GetDefault();
        }

        public void Dispose()
        {
            if (_dataProvider != null)
            {
                _dataProvider.Dispose();
            }
        }
    }
}
