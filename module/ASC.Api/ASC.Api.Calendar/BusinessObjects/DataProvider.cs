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
using System.Globalization;
using System.Linq;
using System.Web;
using ASC.Api.Calendar.ExternalCalendars;
using ASC.Api.Calendar.iCalParser;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.BusinessObjects
{
    public class DataProvider:IDisposable
    {
        private IDbManager db;
        private const string DBId = "calendar";
        private const string _calendarTable = "calendar_calendars cal";
        private const string _calendarItemTable = "calendar_calendar_item cal_itm";
        private const string _calendarUserTable = "calendar_calendar_user cal_usr";
        private const string _eventTable = "calendar_events evt";
        private const string _todoTable = "calendar_todos td";
        private const string _eventItemTable = "calendar_event_item evt_itm";

        public DataProvider()
        {
            db = DbManager.FromHttpContext(DBId);
        }

        public List<UserViewSettings> GetUserViewSettings(Guid userId, List<string> calendarIds)
        {
            var cc = new ColumnCollection();

            var extCalId = cc.RegistryColumn("ext_calendar_id");
            var usrId = cc.RegistryColumn("user_id");
            var hideEvents = cc.RegistryColumn("hide_events");
            var isAccepted = cc.RegistryColumn("is_accepted");
            var textColor = cc.RegistryColumn("text_color");
            var background = cc.RegistryColumn("background_color");
            var alertType = cc.RegistryColumn("alert_type");
            var calId = cc.RegistryColumn("convert(calendar_id using utf8)");
            var calName = cc.RegistryColumn("name");
            var timeZone = cc.RegistryColumn("time_zone");

            var query = new SqlQuery("calendar_calendar_user")
                .Select(cc.SelectQuery)
                .Where((Exp.In(extCalId.Name, calendarIds) |
                        Exp.In(calId.Name, calendarIds)) &
                       Exp.Eq(usrId.Name, userId));

            var data = db.ExecuteList(query);

            var options = new List<UserViewSettings>();
            foreach (var r in data)
            {
                options.Add(new UserViewSettings()
                    {
                        CalendarId =
                            Convert.ToInt32(r[calId.Ind]) == 0
                                ? Convert.ToString(r[extCalId.Ind])
                                : Convert.ToString(r[calId.Ind]),
                        UserId = usrId.Parse<Guid>(r),
                        IsHideEvents = hideEvents.Parse<bool>(r),
                        IsAccepted = isAccepted.Parse<bool>(r),
                        TextColor = textColor.Parse<string>(r),
                        BackgroundColor = background.Parse<string>(r),
                        EventAlertType = (EventAlertType) alertType.Parse<int>(r),
                        Name = calName.Parse<string>(r),
                        TimeZone = timeZone.Parse<TimeZoneInfo>(r)
                    });
            }

            return options;
        }

        public List<Calendar> LoadTodoCalendarsForUser(Guid userId)
        {
            var groups = CoreContext.UserManager.GetUserGroups(userId).Select(g => g.ID).ToList();
            groups.AddRange(
                CoreContext.UserManager.GetUserGroups(userId, Core.Users.Constants.SysGroupCategoryId).Select(g => g.ID));
            var currentTenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var queryGetCalIds = new SqlQuery(_calendarTable)
                           .Select("cal.id")
                           .Where("cal.owner_id", userId)
                           .Where("cal.is_todo", 1)
                           .Where("cal.tenant", currentTenantId);

            var calIds = db.ExecuteList(queryGetCalIds).Select(r => r[0]);

            var cals = GetCalendarsByIds(calIds.ToArray());

            return cals;
        }
        public void RemoveTodo(int todoId)
        {
            using (var tr = db.BeginTransaction())
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId;

                db.ExecuteNonQuery(new SqlDelete("calendar_todos").Where("id", todoId).Where("tenant", tenant));
              
                tr.Commit();
            }
        }
        public List<Calendar> LoadCalendarsForUser(Guid userId, out int newCalendarsCount)
        {
            var groups = CoreContext.UserManager.GetUserGroups(userId).Select(g => g.ID).ToList();
            groups.AddRange(
                CoreContext.UserManager.GetUserGroups(userId, Core.Users.Constants.SysGroupCategoryId).Select(g => g.ID));

            var currentTenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var queryGetCalIds = new SqlQuery(_calendarItemTable)
                .Select("cal_itm.calendar_id")
                .InnerJoin(_calendarTable, Exp.EqColumns("cal.id", "cal_itm.calendar_id"))
                .Where("cal.tenant", currentTenantId)
                .Where(Exp.Eq("cal_itm.item_id", userId) |
                       (Exp.In("cal_itm.item_id", groups.ToArray()) & Exp.Eq("cal_itm.is_group", true)))
                .Union(new SqlQuery(_calendarTable)
                           .Select("cal.id")
                           .Where("cal.owner_id", userId)
                           .Where("cal.tenant", currentTenantId));

            var calIds = db.ExecuteList(queryGetCalIds).Select(r => r[0]);

            var cals = GetCalendarsByIds(calIds.ToArray());

            //filter by is_accepted field
            newCalendarsCount =
                cals.RemoveAll(
                    c =>
                    (!c.OwnerId.Equals(userId) &&
                        !c.ViewSettings.Exists(v => v.UserId.Equals(userId) && v.IsAccepted))
                    || (c.IsiCalStream() && c.ViewSettings.Exists(v => v.UserId.Equals(userId) && !v.IsAccepted)));
            return cals;
        }

        public List<Calendar> LoadiCalStreamsForUser(Guid userId)
        {
            var queryGetCalIds = new SqlQuery(_calendarTable)
                .Select("cal.id")
                .Where("cal.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .Where("cal.owner_id", userId)
                .Where(!Exp.Eq("cal.ical_url", null));

            var calIds = db.ExecuteList(queryGetCalIds).Select(r => r[0]);

            var calendars = GetCalendarsByIds(calIds.ToArray());
            return calendars;
        }

        public List<Calendar> LoadSubscriptionsForUser(Guid userId)
        {
            var groups = CoreContext.UserManager.GetUserGroups(userId).Select(g => g.ID).ToList();
            groups.AddRange(CoreContext.UserManager.GetUserGroups(userId, Core.Users.Constants.SysGroupCategoryId).Select(g => g.ID));

            var calIds = db.ExecuteList(new SqlQuery(_calendarItemTable).Select("cal_itm.calendar_id")
                                                .InnerJoin(_calendarTable, Exp.EqColumns("cal.id", "cal_itm.calendar_id"))
                                                .Where("cal.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                                                .Where(Exp.Eq("cal_itm.item_id", userId) | (Exp.In("cal_itm.item_id", groups.ToArray()) & Exp.Eq("cal_itm.is_group", true)))
                                            ).Select(r => r[0]);

            var calendars = GetCalendarsByIds(calIds.ToArray());
            return calendars;
        }

        public TimeZoneInfo GetTimeZoneForSharedEventsCalendar(Guid userId)
        {
            var q = new SqlQuery(_calendarUserTable)
                .Select("time_zone")
                .Where("ext_calendar_id", SharedEventsCalendar.CalendarId)
                .Where("user_id", userId);

            var data = db.ExecuteList(q);
            if (data.Count > 0)
                return data.Select(r => TimeZoneConverter.GetTimeZone(Convert.ToString(r[0]))).First();

            return CoreContext.TenantManager.GetCurrentTenant().TimeZone;
        }

        public TimeZoneInfo GetTimeZoneForCalendar(Guid userId, int caledarId)
        {
            return db.ExecuteList(new SqlQuery(_calendarTable).Select("cal.time_zone", "cal_usr.time_zone")
                                            .LeftOuterJoin(_calendarUserTable, Exp.EqColumns("cal.id", "cal_usr.calendar_id") & Exp.Eq("cal_usr.user_id", userId))
                                            .Where(Exp.Eq("cal.id", caledarId)))
                                            .Select(r => (r[1] == null || r[1] == DBNull.Value) ? TimeZoneConverter.GetTimeZone(Convert.ToString(r[0])) : TimeZoneConverter.GetTimeZone(Convert.ToString(r[1]))).First();
        }
        public List<object[]> GetCalendarIdByCaldavGuid(string caldavGuid)
        {
            var data = db.ExecuteList(new SqlQuery(_calendarTable).Select("id", "owner_id", "tenant").Where("caldav_guid", caldavGuid));
            return data;
        }
        public Event GetEventIdByUid(string uid, int calendarId)
        {
            var sql = new SqlQuery("calendar_events")
                .Select("id")
                .Where(Exp.Like("uid", uid))
                .Where("calendar_id", calendarId);

            var eventId = db.ExecuteScalar<int>(sql);

            return eventId == 0 ? null : GetEventById(eventId);
        }
        public Event GetEventIdOnlyByUid(string uid)
        {
            var sql = new SqlQuery("calendar_events")
                .Select("id")
                .Where(Exp.Like("uid", uid));

            var eventId = db.ExecuteScalar<int>(sql);

            return eventId == 0 ? null : GetEventById(eventId);
        }
        public List<Calendar> GetCalendarsByIds(object[] calIds)
        {
            var cc = new ColumnCollection();

            var calId = cc.RegistryColumn("cal.id");
            var calName = cc.RegistryColumn("cal.name");
            var calDescription = cc.RegistryColumn("cal.description");
            var calTenant = cc.RegistryColumn("cal.tenant");
            var calTextColor = cc.RegistryColumn("cal.text_color");
            var calBackground = cc.RegistryColumn("cal.background_color");
            var calOwner = cc.RegistryColumn("cal.owner_id");
            var calAlertType = cc.RegistryColumn("cal.alert_type");
            var calTimeZone = cc.RegistryColumn("cal.time_zone");
            var iCalUrl = cc.RegistryColumn("cal.ical_url");
            var calDavGuid = cc.RegistryColumn("cal.caldav_guid");
            var isTodo = cc.RegistryColumn("cal.is_todo");

            var usrId = cc.RegistryColumn("cal_usr.user_id");
            var usrHideEvents = cc.RegistryColumn("cal_usr.hide_events");
            var usrIsAccepted = cc.RegistryColumn("cal_usr.is_accepted");
            var usrTextColor = cc.RegistryColumn("cal_usr.text_color");
            var usrBackground = cc.RegistryColumn("cal_usr.background_color");
            var usrAlertType = cc.RegistryColumn("cal_usr.alert_type");
            var usrCalName = cc.RegistryColumn("cal_usr.name");
            var usrTimeZone = cc.RegistryColumn("cal_usr.time_zone");

            var data = db.ExecuteList(new SqlQuery(_calendarTable).Select(cc.SelectQuery)
                                                                            .LeftOuterJoin(_calendarUserTable,
                                                                                        Exp.EqColumns(calId.Name,
                                                                                                        "cal_usr.calendar_id"))
                                                                            .Where(Exp.In(calId.Name, calIds)));

            var cc1 = new ColumnCollection();

            var itemCalId = cc1.RegistryColumn("cal_itm.calendar_id");
            var itemId = cc1.RegistryColumn("cal_itm.item_id");
            var itemIsGroup = cc1.RegistryColumn("cal_itm.is_group");

            var sharingData = db.ExecuteList(new SqlQuery(_calendarItemTable).Select(cc1.SelectQuery)
                                                                                    .Where(Exp.In(itemCalId.Name,
                                                                                                    calIds)));


            //parsing
            var calendars = new List<Calendar>();
            foreach (var r in data)
            {
                var calendar =
                    calendars.Find(
                        c =>
                        string.Equals(c.Id, calId.Parse<int>(r).ToString(),
                                        StringComparison.InvariantCultureIgnoreCase));
                if (calendar == null)
                {
                    calendar = new Calendar
                        {
                            Id = calId.Parse<int>(r).ToString(),
                            Name = calName.Parse<string>(r),
                            Description = calDescription.Parse<string>(r),
                            TenantId = calTenant.Parse<int>(r),
                            OwnerId = calOwner.Parse<Guid>(r),
                            EventAlertType = (EventAlertType) calAlertType.Parse<int>(r),
                            TimeZone = calTimeZone.Parse<TimeZoneInfo>(r),
                            iCalUrl = iCalUrl.Parse<string>(r),
                            calDavGuid = calDavGuid.Parse<string>(r),
                            IsTodo = isTodo.Parse<int>(r)
                        };
                    calendar.Context.HtmlTextColor = calTextColor.Parse<string>(r);
                    calendar.Context.HtmlBackgroundColor = calBackground.Parse<string>(r);
                    if (!String.IsNullOrEmpty(calendar.iCalUrl))
                    {
                        calendar.Context.CanChangeTimeZone = false;
                        calendar.Context.CanChangeAlertType = false;
                    }

                    calendars.Add(calendar);

                    foreach (var row in sharingData)
                    {
                        var _calId = itemCalId.Parse<int>(row).ToString();
                        if (String.Equals(_calId, calendar.Id, StringComparison.InvariantCultureIgnoreCase))
                        {
                            calendar.SharingOptions.PublicItems.Add(new SharingOptions.PublicItem
                                {
                                    Id = itemId.Parse<Guid>(row),
                                    IsGroup = itemIsGroup.Parse<bool>(row)
                                });
                        }
                    }
                }

                if (!usrId.IsNull(r))
                {
                    var uvs = new UserViewSettings
                        {
                            CalendarId = calendar.Id,
                            UserId = usrId.Parse<Guid>(r),
                            IsHideEvents = usrHideEvents.Parse<bool>(r),
                            IsAccepted = usrIsAccepted.Parse<bool>(r),
                            TextColor = usrTextColor.Parse<string>(r),
                            BackgroundColor = usrBackground.Parse<string>(r),
                            EventAlertType = (EventAlertType) usrAlertType.Parse<int>(r),
                            Name = usrCalName.Parse<string>(r),
                            TimeZone = usrTimeZone.Parse<TimeZoneInfo>(r)
                        };

                    calendar.ViewSettings.Add(uvs);
                }
            }

            return calendars;
        }

        public Calendar GetCalendarById(int calendarId)
        {
            var calendars = GetCalendarsByIds(new object[] { calendarId });
            if (calendars.Count > 0)
                return calendars[0];

            return null;
        }

        public Calendar CreateCalendar(Guid ownerId, string name, string description, string textColor, string backgroundColor, TimeZoneInfo timeZone, EventAlertType eventAlertType, string iCalUrl, List<SharingOptions.PublicItem> publicItems, List<UserViewSettings> viewSettings, Guid calDavGuid, int isTodo = 0)
        {
            int calendarId;
            using (var tr = db.BeginTransaction())
            {

                calendarId = db.ExecuteScalar<int>(new SqlInsert("calendar_calendars")
                                                                .InColumnValue("id", 0)
                                                                .InColumnValue("tenant",
                                                                                CoreContext.TenantManager
                                                                                        .GetCurrentTenant().TenantId)
                                                                .InColumnValue("owner_id", ownerId)
                                                                .InColumnValue("name", name)
                                                                .InColumnValue("description", description)
                                                                .InColumnValue("text_color", textColor)
                                                                .InColumnValue("background_color", backgroundColor)
                                                                .InColumnValue("alert_type", (int) eventAlertType)
                                                                .InColumnValue("time_zone", timeZone.Id)
                                                                .InColumnValue("ical_url", iCalUrl)
                                                                .InColumnValue("caldav_guid", calDavGuid)
                                                                .InColumnValue("is_todo", isTodo)
                                                                .Identity(0, 0, true));

                if (publicItems != null)
                {
                    foreach (var item in publicItems)
                    {
                        db.ExecuteNonQuery(new SqlInsert("calendar_calendar_item")
                                                        .InColumnValue("calendar_id", calendarId)
                                                        .InColumnValue("item_id", item.Id)
                                                        .InColumnValue("is_group", item.IsGroup));
                    }
                }

                if (viewSettings != null)
                {
                    foreach (var view in viewSettings)
                    {
                        db.ExecuteNonQuery(new SqlInsert("calendar_calendar_user")
                                                        .InColumnValue("calendar_id", calendarId)
                                                        .InColumnValue("user_id", view.UserId)
                                                        .InColumnValue("hide_events", view.IsHideEvents)
                                                        .InColumnValue("is_accepted", view.IsAccepted)
                                                        .InColumnValue("text_color", view.TextColor)
                                                        .InColumnValue("background_color", view.BackgroundColor)
                                                        .InColumnValue("alert_type", (int) view.EventAlertType)
                                                        .InColumnValue("name", view.Name ?? "")
                                                        .InColumnValue("time_zone",
                                                                        view.TimeZone != null ? view.TimeZone.Id : null)
                            );

                    }
                }
                tr.Commit();
            }

            return GetCalendarById(calendarId);
        }

        public Calendar UpdateCalendarGuid(int calendarId, Guid calDavGuid)
        {
            using (var tr = db.BeginTransaction())
            {
                db.ExecuteNonQuery(new SqlUpdate("calendar_calendars")
                                                .Set("caldav_guid", calDavGuid)
                                                .Where("id", calendarId));
                tr.Commit();
            }
            return GetCalendarById(calendarId);
        }
        public Calendar UpdateCalendar(int calendarId, string name, string description, List<SharingOptions.PublicItem> publicItems, List<UserViewSettings> viewSettings)
        {
            using (var tr = db.BeginTransaction())
            {

                db.ExecuteNonQuery(new SqlUpdate("calendar_calendars")
                                                .Set("name", name)
                                                .Set("description", description)
                                                .Where("id", calendarId));

                //sharing
                var sqlQuery = new SqlQuery("calendar_calendar_item")
                    .Select("item_id", "is_group")
                    .Where(Exp.Eq("calendar_id", calendarId));
                var existsItems = db.ExecuteList(sqlQuery).ConvertAll(row => new SharingOptions.PublicItem
                {
                    Id = new Guid((string)row[0]),
                    IsGroup = Convert.ToBoolean(row[1])
                });

                foreach (var existCalendar in existsItems)
                {
                    db.ExecuteNonQuery(new SqlDelete("calendar_calendar_item")
                        .Where("calendar_id", calendarId)
                        .Where("item_id", existCalendar.Id)
                        .Where("is_group", existCalendar.IsGroup));
                }

                
                foreach (var item in publicItems)
                {
                    db.ExecuteNonQuery(new SqlInsert("calendar_calendar_item")
                                                    .InColumnValue("calendar_id", calendarId)
                                                    .InColumnValue("item_id", item.Id)
                                                    .InColumnValue("is_group", item.IsGroup));
                }

                //view
                sqlQuery = new SqlQuery("calendar_calendar_user")
                    .Select("ext_calendar_id", "user_id")
                    .Where(Exp.Eq("calendar_id", calendarId));
                var existsUsers = db.ExecuteList(sqlQuery).Select(row => new
                {
                    ExtCalendarId = (string)row[0],
                    Id = (string)row[1]
                }).ToList();

                foreach (var user in existsUsers)
                {
                    db.ExecuteNonQuery(new SqlDelete("calendar_calendar_user")
                        .Where("calendar_id", calendarId)
                        .Where("ext_calendar_id", user.ExtCalendarId)
                        .Where("user_id", user.Id));
                }

                foreach (var view in viewSettings)
                {
                    db.ExecuteNonQuery(new SqlInsert("calendar_calendar_user")
                                                    .InColumnValue("calendar_id", calendarId)
                                                    .InColumnValue("user_id", view.UserId)
                                                    .InColumnValue("hide_events", view.IsHideEvents)
                                                    .InColumnValue("is_accepted", view.IsAccepted)
                                                    .InColumnValue("text_color", view.TextColor)
                                                    .InColumnValue("background_color", view.BackgroundColor)
                                                    .InColumnValue("alert_type", (int) view.EventAlertType)
                                                    .InColumnValue("name", view.Name ?? "")
                                                    .InColumnValue("time_zone",
                                                                    view.TimeZone != null ? view.TimeZone.Id : null)
                        );

                }

                //update notifications
                var cc = new ColumnCollection();
                var eId = cc.RegistryColumn("e.id");
                var eStartDate = cc.RegistryColumn("e.start_date");
                var eAlertType = cc.RegistryColumn("e.alert_type");
                var eRRule = cc.RegistryColumn("e.rrule");
                var eIsAllDay = cc.RegistryColumn("e.all_day_long");

                var eventsData = db.ExecuteList(
                    new SqlQuery("calendar_events e")
                        .Select(cc.SelectQuery)
                        .Where("e.calendar_id", calendarId)
                        .Where("e.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId));

                foreach (var r in eventsData)
                {
                    UpdateEventNotifications(eId.Parse<int>(r), calendarId,
                                                eStartDate.Parse<DateTime>(r),
                                                (EventAlertType) eAlertType.Parse<int>(r),
                                                eRRule.Parse<RecurrenceRule>(r), null, publicItems,
                                                eIsAllDay.Parse<bool>(r));
                }

                tr.Commit();
            }

            return GetCalendarById(calendarId);
        }

        public void UpdateCalendarUserView(List<UserViewSettings> viewSettings)
        {
            using (var tr = db.BeginTransaction())
            {
                foreach (var s in viewSettings)
                    UpdateCalendarUserView(s);

                tr.Commit();
            }
        }
        public void UpdateCalendarUserView(UserViewSettings viewSettings)
        {
            var cc = new ColumnCollection();
            var eId = cc.RegistryColumn("e.id");
            var eStartDate = cc.RegistryColumn("e.start_date");
            var eAlertType = cc.RegistryColumn("e.alert_type");
            var eRRule = cc.RegistryColumn("e.rrule");
            var eCalId = cc.RegistryColumn("e.calendar_id");
            var eIsAllDay = cc.RegistryColumn("e.all_day_long");

            int calendarId;
            if (int.TryParse(viewSettings.CalendarId, out calendarId))
            {
                db.ExecuteNonQuery(new SqlInsert("calendar_calendar_user", true)
                                                .InColumnValue("calendar_id", calendarId)
                                                .InColumnValue("user_id", viewSettings.UserId)
                                                .InColumnValue("hide_events", viewSettings.IsHideEvents)
                                                .InColumnValue("text_color", viewSettings.TextColor)
                                                .InColumnValue("background_color", viewSettings.BackgroundColor)
                                                .InColumnValue("is_accepted", viewSettings.IsAccepted)
                                                .InColumnValue("alert_type", (int) viewSettings.EventAlertType)
                                                .InColumnValue("name", viewSettings.Name ?? "")
                                                .InColumnValue("time_zone",
                                                                viewSettings.TimeZone != null
                                                                    ? viewSettings.TimeZone.Id
                                                                    : null)
                    );



                //update notifications
                var eventsData = db.ExecuteList(
                    new SqlQuery("calendar_events e")
                        .Select(cc.SelectQuery)
                        .Where("e.calendar_id", calendarId)
                        .Where("e.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId));

                foreach (var r in eventsData)
                {
                    UpdateEventNotifications(eId.Parse<int>(r), calendarId,
                                                eStartDate.Parse<DateTime>(r),
                                                (EventAlertType) eAlertType.Parse<int>(r),
                                                eRRule.Parse<RecurrenceRule>(r), null, null,
                                                eIsAllDay.Parse<bool>(r));
                }

            }
            else
            {
                db.ExecuteNonQuery(new SqlInsert("calendar_calendar_user", true)
                                                .InColumnValue("ext_calendar_id", viewSettings.CalendarId)
                                                .InColumnValue("user_id", viewSettings.UserId)
                                                .InColumnValue("hide_events", viewSettings.IsHideEvents)
                                                .InColumnValue("text_color", viewSettings.TextColor)
                                                .InColumnValue("background_color", viewSettings.BackgroundColor)
                                                .InColumnValue("alert_type", (int) viewSettings.EventAlertType)
                                                .InColumnValue("is_accepted", viewSettings.IsAccepted)
                                                .InColumnValue("name", viewSettings.Name ?? "")
                                                .InColumnValue("time_zone",
                                                                viewSettings.TimeZone != null
                                                                    ? viewSettings.TimeZone.Id
                                                                    : null)
                    );

                if (String.Equals(viewSettings.CalendarId, SharedEventsCalendar.CalendarId,
                                    StringComparison.InvariantCultureIgnoreCase))
                {
                    //update notifications
                    var groups =
                        CoreContext.UserManager.GetUserGroups(viewSettings.UserId).Select(g => g.ID).ToList();
                    groups.AddRange(
                        CoreContext.UserManager.GetUserGroups(viewSettings.UserId,
                                                                Core.Users.Constants.SysGroupCategoryId)
                                    .Select(g => g.ID));

                    var q = new SqlQuery("calendar_events e")
                        .Select(cc.SelectQuery)
                        .InnerJoin("calendar_event_item ei", Exp.EqColumns("ei.event_id", eId.Name))
                        .Where("e.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                        .Where((Exp.Eq("ei.is_group", false) & Exp.Eq("ei.item_id", viewSettings.UserId)) |
                                (Exp.Eq("ei.is_group", true) & Exp.In("ei.item_id", groups.ToArray())));

                    var eventsData = db.ExecuteList(q);

                    foreach (var r in eventsData)
                    {
                        UpdateEventNotifications(eId.Parse<int>(r), eCalId.Parse<int>(r),
                                                    eStartDate.Parse<DateTime>(r),
                                                    (EventAlertType) eAlertType.Parse<int>(r),
                                                    eRRule.Parse<RecurrenceRule>(r), null, null,
                                                    eIsAllDay.Parse<bool>(r));
                    }
                }
            }
        }

        public Guid RemoveCalendar(int calendarId)
        {
            using (var tr = db.BeginTransaction())
            {
                var caldavGuid = Guid.Empty;
                try
                {
                    var dataCaldavGuid =
                        db.ExecuteList(new SqlQuery("calendar_calendars").Select("caldav_guid").Where("id", calendarId))
                          .Select(r => r[0])
                          .ToArray();
                    if (dataCaldavGuid[0] != null) caldavGuid = Guid.Parse(dataCaldavGuid[0].ToString());
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ASC.Calendar").Error(ex);
                }

                db.ExecuteNonQuery(new SqlDelete("calendar_calendars").Where("id", calendarId));
                db.ExecuteNonQuery(new SqlDelete("calendar_calendar_user").Where("calendar_id", calendarId));
                db.ExecuteNonQuery(new SqlDelete("calendar_calendar_item").Where("calendar_id", calendarId));

                var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId;

                var data = db.ExecuteList(new SqlQuery("calendar_events")
                                                .Select("id")
                                                .Where("calendar_id", calendarId)
                                                .Where("tenant", tenant))
                                .Select(r => r[0])
                                .ToArray();

                db.ExecuteNonQuery(new SqlDelete("calendar_events").Where("calendar_id", calendarId).Where("tenant", tenant));
                db.ExecuteNonQuery(new SqlDelete("calendar_event_item").Where(Exp.In("event_id", data)));
                db.ExecuteNonQuery(new SqlDelete("calendar_event_user").Where(Exp.In("event_id", data)));
                db.ExecuteNonQuery(new SqlDelete("calendar_notifications").Where(Exp.In("event_id", data)));
                db.ExecuteNonQuery(new SqlDelete("calendar_event_history").Where("tenant", tenant).Where(Exp.In("event_id", data)));

                tr.Commit();

                return caldavGuid;
            }
        }

        public void RemoveExternalCalendarData(string calendarId)
        {
            using (var tr = db.BeginTransaction())
            {
                db.ExecuteNonQuery(new SqlDelete("calendar_calendar_user").Where("ext_calendar_id",calendarId));
                tr.Commit();
            }
        }


        public Todo GetTodoByUid(string todoUid)
        {
            var sql = new SqlQuery("calendar_todos t")
                .Select("t.id")
                .InnerJoin("calendar_calendars c", Exp.EqColumns("c.id", "t.calendar_id"))
                .Where("t.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .Where(Exp.Like("t.uid", todoUid))
                .Where("t.owner_id", SecurityContext.CurrentAccount.ID)
                .Where("c.owner_id", SecurityContext.CurrentAccount.ID)
                .Where("c.ical_url", null);

            var todoId = db.ExecuteScalar<int>(sql);

            return todoId == 0 ? null : GetTodoById(todoId);
        }
        public Todo GetTodoIdByUid(string uid, int calendarId)
        {
            var sql = new SqlQuery("calendar_todos")
                .Select("id")
                .Where(Exp.Like("uid", uid))
                .Where("calendar_id", calendarId);

            var todoId = db.ExecuteScalar<int>(sql);

            return todoId == 0 ? null : GetTodoById(todoId);
        }
        public Todo UpdateTodo(string id, int calendarId, Guid ownerId, string name, string description, DateTime utcStartDate, string uid, DateTime completed)
        {
            int todoId;


            using (var tr = db.BeginTransaction())
            {

                todoId = db.ExecuteScalar<int>(new SqlUpdate("calendar_todos")
                                                            .Set("name", name)
                                                            .Set("description", description)
                                                            .Set("calendar_id", calendarId)
                                                            .Set("owner_id", ownerId)
                                                            .Set("start_date", utcStartDate == DateTime.MinValue ? null : utcStartDate.ToString("yyyy-MM-dd HH:mm:ss"))
                                                            .Set("uid", GetEventUid(uid))
                                                            .Set("completed", completed == DateTime.MinValue ? null : completed.ToString("yyyy-MM-dd HH:mm:ss"))
                                                            .Where(Exp.Eq("id", id)));


                tr.Commit();
            }

            return GetTodoById(int.Parse(id));
            
        }

        public Todo CreateTodo(int calendarId,
                                 Guid ownerId,
                                 string name,
                                 string description,
                                 DateTime utcStartDate,
                                 string uid,
                                 DateTime completed)
        {
            int todoId;
            
            
            using (var tr = db.BeginTransaction())
            {

                todoId = db.ExecuteScalar<int>(new SqlInsert("calendar_todos")
                                                            .InColumnValue("id", 0)
                                                            .InColumnValue("tenant",
                                                                            CoreContext.TenantManager.GetCurrentTenant
                                                                                ().TenantId)
                                                            .InColumnValue("name", name)
                                                            .InColumnValue("description", description)
                                                            .InColumnValue("calendar_id", calendarId)
                                                            .InColumnValue("owner_id", ownerId)
                                                            .InColumnValue("start_date", utcStartDate == DateTime.MinValue ? null : utcStartDate.ToString("yyyy-MM-dd HH:mm:ss"))
                                                            .InColumnValue("uid", GetEventUid(uid))
                                                            .InColumnValue("completed", completed == DateTime.MinValue ? null : completed.ToString("yyyy-MM-dd HH:mm:ss"))
                                                            .Identity(0, 0, true));


                tr.Commit();
            }

            return GetTodoById(todoId);
        }
        public Todo GetTodoById(int todoId)
        {
            var todos = GetTodosByIds(new object[] { todoId }, SecurityContext.CurrentAccount.ID);
            if (todos.Count > 0)
                return todos[0];

            return null;
        }

        public List<Todo> GetTodosByIds(object[] todoIds, Guid userId, int tenantId = -1)
        {
            var cc = new ColumnCollection();
            var tdId = cc.RegistryColumn("td.id");
            var tdName = cc.RegistryColumn("td.name");
            var tdDescription = cc.RegistryColumn("td.description");
            var tdTenant = cc.RegistryColumn("td.tenant");
            var tdCalId = cc.RegistryColumn("td.calendar_id");
            var tdStartDate = cc.RegistryColumn("td.start_date");
            var tdCompleted = cc.RegistryColumn("td.completed");
            var tdOwner = cc.RegistryColumn("td.owner_id");
            var tdUid = cc.RegistryColumn("td.uid");

            var data = new List<Object[]>();

            if (todoIds.Length > 0)
            {
                if (tenantId != -1)
                {
                    data = db.ExecuteList(new SqlQuery(_todoTable)

                                              .Select(cc.SelectQuery)
                                              .Where(Exp.In(tdId.Name, todoIds))
                                              .Where("tenant", tenantId));
                }
                else
                {
                    data = db.ExecuteList(new SqlQuery(_todoTable)

                                                   .Select(cc.SelectQuery)
                                                   .Where(Exp.In(tdId.Name, todoIds)));
                }
               
            }
            

            //parsing           
            var todos = new List<Todo>();

            foreach (var r in data)
            {
                var td =
                    todos.Find(
                        e => String.Equals(e.Id, tdId.Parse<string>(r), StringComparison.InvariantCultureIgnoreCase));
                if (td == null)
                {
                    td = new Todo()
                    {
                        Id = tdId.Parse<string>(r),
                        Name = tdName.Parse<string>(r),
                        Description = tdDescription.Parse<string>(r),
                        TenantId = tdTenant.Parse<int>(r),
                        CalendarId = tdCalId.Parse<string>(r),
                        UtcStartDate = tdStartDate.Parse<DateTime>(r),
                        Completed = tdCompleted.Parse<DateTime>(r),
                        OwnerId = tdOwner.Parse<Guid>(r),
                        Uid = tdUid.Parse<string>(r)
                        
                    };
                    todos.Add(td);
                }
            }
            return todos;
        }
       
        public List<Todo> LoadTodos(int calendarId, Guid userId, int tenantId, DateTime utcStartDate, DateTime utcEndDate)
        {
            var sqlQuery = new SqlQuery(_todoTable)
                .Select("td.id")
                .Where(
                    Exp.Eq("td.calendar_id", calendarId) &
                    Exp.Eq("td.tenant", tenantId)
                );

            var tdIds = db.ExecuteList(sqlQuery).Select(r => r[0]);

            return GetTodosByIds(tdIds.ToArray(), userId, tenantId);
        }
        internal List<Event> LoadSharedEvents(Guid userId, int tenantId, DateTime utcStartDate, DateTime utcEndDate)
        {
            var groups = CoreContext.UserManager.GetUserGroups(userId).Select(g => g.ID).ToList();
            groups.AddRange(CoreContext.UserManager.GetUserGroups(userId, Core.Users.Constants.SysGroupCategoryId).Select(g => g.ID));

            var evIds = db.ExecuteList(
                new SqlQuery(_eventTable).Select("evt.id")
                    .InnerJoin(_eventItemTable, Exp.EqColumns("evt_itm.event_id", "evt.id"))
                    .Where("evt.tenant", tenantId)
                    .Where(
                    (Exp.Eq("evt_itm.item_id", userId) | (Exp.In("evt_itm.item_id", groups.ToArray()) & Exp.Eq("evt_itm.is_group", true)))
                    & Exp.Eq("evt.tenant", tenantId)
                    & ((Exp.Ge("evt.start_date", utcStartDate) & Exp.Le("evt.start_date", utcEndDate) & Exp.Eq("evt.rrule", "")
                        | !Exp.Eq("evt.rrule", "")))

                    & !Exp.Eq("evt.owner_id", userId)

                    & !Exp.Exists(new SqlQuery("calendar_event_user evt_usr").Select("evt_usr.event_id")
                                                                                .Where(Exp.EqColumns("evt_usr.event_id", "evt.id")
                                                                                        & Exp.Eq("evt_usr.user_id", userId)
                                                                                        & Exp.Eq("evt_usr.is_unsubscribe", true)))
                    )).Select(r => r[0]);

            return GetEventsByIds(evIds.ToArray(), userId, tenantId);
        }

        public List<Event> LoadEvents(int calendarId, Guid userId, int tenantId, DateTime utcStartDate, DateTime utcEndDate)
        {
            var sqlQuery = new SqlQuery(_eventTable)
                .Select("evt.id")
                .Where(
                    Exp.Eq("evt.calendar_id", calendarId) &
                    Exp.Eq("evt.tenant", tenantId) &
                    (
                        !Exp.Eq("evt.rrule", "") |
                        (Exp.Eq("evt.rrule", "") &
                            (
                                    (Exp.Ge("evt.start_date", utcStartDate) & Exp.Le("evt.end_date", utcEndDate)) |
                                    (Exp.Le("evt.start_date", utcStartDate) & Exp.Ge("evt.end_date", utcStartDate)) |
                                    (Exp.Le("evt.start_date", utcEndDate) & Exp.Ge("evt.end_date", utcEndDate))
                                )
                        )
                    )
                );

            var evIds = db.ExecuteList(sqlQuery).Select(r => r[0]);

            return GetEventsByIds(evIds.ToArray(), userId, tenantId);
        }

        public Event GetEventById(int eventId)
        {
            var events = GetEventsByIds(new object[] { eventId }, SecurityContext.CurrentAccount.ID);
            if (events.Count > 0)
                return events[0];

            return null;
        }

        public List<Event> GetEventsByIds(object[] evtIds, Guid userId, int tenantId = -1)
        {
            var cc = new ColumnCollection();
            var eId = cc.RegistryColumn("evt.id");
            var eName = cc.RegistryColumn("evt.name");
            var eDescription = cc.RegistryColumn("evt.description");
            var eTenant = cc.RegistryColumn("evt.tenant");
            var eCalId = cc.RegistryColumn("evt.calendar_id");
            var eStartDate = cc.RegistryColumn("evt.start_date");
            var eEndDate = cc.RegistryColumn("evt.end_date");
            var eUpdateDate = cc.RegistryColumn("evt.update_date");
            var eIsAllDay = cc.RegistryColumn("evt.all_day_long");
            var eRRule = cc.RegistryColumn("evt.rrule");
            var eOwner = cc.RegistryColumn("evt.owner_id");
            var usrAlertType = cc.RegistryColumn("evt_usr.alert_type");
            var eAlertType = cc.RegistryColumn("evt.alert_type");
            var eUid = cc.RegistryColumn("evt.uid");
            var eStatus = cc.RegistryColumn("evt.status");

            var data = new List<Object[]>();

            if (evtIds.Length > 0)
            {
                if (tenantId != -1)
                {
                    data = db.ExecuteList(new SqlQuery(_eventTable)
                                              .LeftOuterJoin("calendar_event_user evt_usr",
                                                             Exp.EqColumns(eId.Name, "evt_usr.event_id") &
                                                             Exp.Eq("evt_usr.user_id", userId))
                                              .Select(cc.SelectQuery)
                                              .Where(Exp.In(eId.Name, evtIds))
                                              .Where("tenant", tenantId));
                }
                else
                {
                    data = db.ExecuteList(new SqlQuery(_eventTable)
                                                .LeftOuterJoin("calendar_event_user evt_usr",
                                                               Exp.EqColumns(eId.Name, "evt_usr.event_id") &
                                                               Exp.Eq("evt_usr.user_id", userId))
                                                .Select(cc.SelectQuery)
                                                .Where(Exp.In(eId.Name, evtIds)));
                    
                }
                
            }

            var cc1 = new ColumnCollection();
            var evId = cc1.RegistryColumn("evt_itm.event_id");
            var itemId = cc1.RegistryColumn("evt_itm.item_id");
            var itemIsGroup = cc1.RegistryColumn("evt_itm.is_group");

            var sharingData = new List<Object[]>();

            if (evtIds.Length > 0)
            {
                sharingData = db.ExecuteList(new SqlQuery(_eventItemTable).Select(cc1.SelectQuery)
                                                                                   .Where(Exp.In(evId.Name, evtIds)));
            }

            //parsing           
            var events = new List<Event>();

            foreach (var r in data)
            {
                var ev =
                    events.Find(
                        e => String.Equals(e.Id, eId.Parse<string>(r), StringComparison.InvariantCultureIgnoreCase));
                if (ev == null)
                {
                    ev = new Event
                        {
                            Id = eId.Parse<string>(r),
                            Name = eName.Parse<string>(r),
                            Description = eDescription.Parse<string>(r),
                            TenantId = eTenant.Parse<int>(r),
                            CalendarId = eCalId.Parse<string>(r),
                            UtcStartDate = eStartDate.Parse<DateTime>(r),
                            UtcEndDate = eEndDate.Parse<DateTime>(r),
                            UtcUpdateDate = eUpdateDate.Parse<DateTime>(r),
                            AllDayLong = eIsAllDay.Parse<bool>(r),
                            OwnerId = eOwner.Parse<Guid>(r),
                            AlertType =
                                (usrAlertType.IsNull(r))
                                    ? (EventAlertType) eAlertType.Parse<int>(r)
                                    : (EventAlertType) usrAlertType.Parse<int>(r),
                            RecurrenceRule = eRRule.Parse<RecurrenceRule>(r),
                            Uid = eUid.Parse<string>(r),
                            Status = (EventStatus) eStatus.Parse<int>(r)
                        };
                    events.Add(ev);
                }

                foreach (var row in sharingData)
                {
                    if (String.Equals(evId.Parse<string>(row), ev.Id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ev.SharingOptions.PublicItems.Add(new SharingOptions.PublicItem
                            {
                                Id = itemId.Parse<Guid>(row),
                                IsGroup = itemIsGroup.Parse<bool>(row)
                            });
                    }
                }
            }
            return events;
        }

        public Event GetEventByUid(string eventUid)
        {
            var sql = new SqlQuery("calendar_events e")
                .Select("e.id")
                .InnerJoin("calendar_calendars c", Exp.EqColumns("c.id", "e.calendar_id"))
                .Where("e.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .Where("e.uid", eventUid)
                .Where("e.owner_id", SecurityContext.CurrentAccount.ID)
                .Where("c.owner_id", SecurityContext.CurrentAccount.ID)
                .Where("c.ical_url", null);

            var eventId = db.ExecuteScalar<int>(sql);

            return eventId == 0 ? null : GetEventById(eventId);
        }
        public Event GetEventOnlyByUid(string eventUid)
        {
            var sql = new SqlQuery("calendar_events e")
                .Select("e.id")
                .Where("e.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .Where("e.uid", eventUid);

            var eventId = db.ExecuteScalar<int>(sql);

            return eventId == 0 ? null : GetEventById(eventId);
        }

        public void SetEventUid(int eventId, string uid)
        {
            using (var tr = db.BeginTransaction())
            {
                db.ExecuteNonQuery(new SqlUpdate("calendar_events")
                                                .Set("uid", uid)
                                                .Where(Exp.Eq("id", eventId)));

                tr.Commit();
            }
        }

        public void UnsubscribeFromEvent(int eventID, Guid userId)
        {
            using (var tr = db.BeginTransaction())
            {
                if (db.ExecuteNonQuery(new SqlDelete("calendar_event_item").Where(Exp.Eq("event_id", eventID)
                                                                                & Exp.Eq("item_id", userId)
                                                                                & Exp.Eq("is_group", false))) == 0)
                {
                    db.ExecuteNonQuery(new SqlInsert("calendar_event_user", true).InColumnValue("event_id", eventID)
                                                                                        .InColumnValue("user_id", userId)
                                                                                        .InColumnValue("is_unsubscribe", true));
                }

                db.ExecuteNonQuery(new SqlDelete("calendar_notifications").Where(Exp.Eq("event_id", eventID) & Exp.Eq("user_id", userId)));

                tr.Commit();
            }
        }

        public void RemoveEvent(int eventId)
        {
            using (var tr = db.BeginTransaction())
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId;

                db.ExecuteNonQuery(new SqlDelete("calendar_events").Where("id", eventId).Where("tenant", tenant));
                db.ExecuteNonQuery(new SqlDelete("calendar_event_item").Where("event_id", eventId));
                db.ExecuteNonQuery(new SqlDelete("calendar_event_user").Where("event_id", eventId));
                db.ExecuteNonQuery(new SqlDelete("calendar_notifications").Where("event_id", eventId));
                db.ExecuteNonQuery(new SqlDelete("calendar_event_history").Where("tenant", tenant).Where("event_id", eventId));

                tr.Commit();
            }
        }

        public Event CreateEvent(int calendarId,
                                 Guid ownerId,
                                 string name,
                                 string description,
                                 DateTime utcStartDate,
                                 DateTime utcEndDate,
                                 RecurrenceRule rrule,
                                 EventAlertType alertType,
                                 bool isAllDayLong,
                                 List<SharingOptions.PublicItem> publicItems,
                                 string uid,
                                 EventStatus status,
                                 DateTime createDate)
        {
            int eventId;
            using (var tr = db.BeginTransaction())
            {

                eventId = db.ExecuteScalar<int>(new SqlInsert("calendar_events")
                                                            .InColumnValue("id", 0)
                                                            .InColumnValue("tenant",
                                                                            CoreContext.TenantManager.GetCurrentTenant
                                                                                ().TenantId)
                                                            .InColumnValue("name", name)
                                                            .InColumnValue("description", description)
                                                            .InColumnValue("calendar_id", calendarId)
                                                            .InColumnValue("owner_id", ownerId)
                                                            .InColumnValue("start_date",
                                                                            utcStartDate.ToString(
                                                                                "yyyy-MM-dd HH:mm:ss"))
                                                            .InColumnValue("end_date",
                                                                            utcEndDate.ToString("yyyy-MM-dd HH:mm:ss"))
                                                                            .InColumnValue("update_date", createDate.ToString("yyyy-MM-dd HH:mm:ss"))
                                                            .InColumnValue("all_day_long", isAllDayLong)
                                                            .InColumnValue("rrule", rrule.ToString())
                                                            .InColumnValue("alert_type", (int) alertType)
                                                            .InColumnValue("uid", GetEventUid(uid))
                                                            .InColumnValue("status", (int) status)
                                                            .Identity(0, 0, true));

                foreach (var item in publicItems)
                {
                    db.ExecuteNonQuery(new SqlInsert("calendar_event_item")
                                                    .InColumnValue("event_id", eventId)
                                                    .InColumnValue("item_id", item.Id)
                                                    .InColumnValue("is_group", item.IsGroup));

                }

                //update notifications
                UpdateEventNotifications(eventId, calendarId, utcStartDate, alertType, rrule, publicItems, null, isAllDayLong);

                tr.Commit();
            }

            return GetEventById(eventId);
        }

        public Event UpdateEvent(int eventId,
            int calendarId,
            Guid ownerId,
            string name,
            string description,
            DateTime utcStartDate,
            DateTime utcEndDate,
            RecurrenceRule rrule,
            EventAlertType alertType,
            bool isAllDayLong,
            List<SharingOptions.PublicItem> publicItems,
            EventStatus status,
            DateTime createDate
            )
        {
            using (var tr = db.BeginTransaction())
            {
                var query = new SqlUpdate("calendar_events")
                    .Set("name", name)
                    .Set("description", description)
                    .Set("calendar_id", calendarId)
                    .Set("owner_id", ownerId)
                    .Set("start_date", utcStartDate.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Set("end_date", utcEndDate.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Set("update_date", createDate.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Set("all_day_long", isAllDayLong)
                    .Set("rrule", rrule.ToString())
                    .Set("status", (int) status)
                    .Where(Exp.Eq("id", eventId));

                if (ownerId.Equals(SecurityContext.CurrentAccount.ID))
                    query = query.Set("alert_type", (int) alertType);
                else
                    db.ExecuteNonQuery(new SqlInsert("calendar_event_user", true)
                                                    .InColumnValue("event_id", eventId)
                                                    .InColumnValue("user_id", SecurityContext.CurrentAccount.ID)
                                                    .InColumnValue("alert_type", alertType));


                db.ExecuteNonQuery(query);

                var userIds = db.ExecuteList(new SqlQuery("calendar_event_user")
                                                        .Select("user_id")
                                                        .Where("event_id", eventId))
                                        .Select(r => new Guid(Convert.ToString(r[0])));

                foreach (var usrId in userIds)
                {
                    if (!publicItems.Exists(i => (i.IsGroup && CoreContext.UserManager.IsUserInGroup(usrId, i.Id))
                                                    || (!i.IsGroup && i.Id.Equals(usrId))))
                    {
                        db.ExecuteNonQuery(new SqlDelete("calendar_event_user")
                                                        .Where(Exp.Eq("user_id", usrId) & Exp.Eq("event_id", eventId)));
                    }
                }

                db.ExecuteNonQuery(new SqlDelete("calendar_event_item").Where("event_id", eventId));
                foreach (var item in publicItems)
                {
                    db.ExecuteNonQuery(new SqlInsert("calendar_event_item")
                                                    .InColumnValue("event_id", eventId)
                                                    .InColumnValue("item_id", item.Id)
                                                    .InColumnValue("is_group", item.IsGroup));


                }

                //update notifications
                var baseAlertType =
                    db.ExecuteList(new SqlQuery("calendar_events").Select("alert_type").Where("id", eventId))
                                .Select(r => (EventAlertType) Convert.ToInt32(r[0])).First();
                UpdateEventNotifications(eventId, calendarId, utcStartDate, baseAlertType, rrule, publicItems, null, isAllDayLong);


                tr.Commit();
            }

            return GetEventById(eventId);
        }



        public EventHistory GetEventHistory(string eventUid)
        {           
            var sql = new SqlQuery("calendar_event_history h")
                .Select("h.calendar_id")
                .Select("h.event_uid")
                .Select("h.event_id")
                .Select("h.ics")
                .InnerJoin("calendar_events e", Exp.EqColumns("e.uid", "h.event_uid"))
                .InnerJoin("calendar_calendars c", Exp.EqColumns("c.id", "h.calendar_id"))
                .Where("h.tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .Where("h.event_uid", eventUid)
                .Where("e.owner_id", SecurityContext.CurrentAccount.ID)
                .Where("c.owner_id", SecurityContext.CurrentAccount.ID)
                .Where("c.ical_url", null);

            var items = db.ExecuteList(sql).ConvertAll(ToEventHistory);

            return items.Count > 0 ? items[0] : null;
        }

        public List<EventHistory> GetEventsHistory(int[] eventIds)
        {
            var sql = new SqlQuery("calendar_event_history")
                .Select("calendar_id")
                .Select("event_uid")
                .Select("event_id")
                .Select("ics")
                .Where("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .Where(Exp.In("event_id", eventIds));

            return db.ExecuteList(sql).ConvertAll(ToEventHistory);
        }
        
        public EventHistory GetEventHistory(int eventId)
        {
            var items = GetEventsHistory(new [] { eventId });
            return items.Count > 0 ? items[0] : null;
        }

        public EventHistory AddEventHistory(int calendarId, string eventUid, int eventId, string ics)
        {
            var icsCalendars = DDayICalParser.DeserializeCalendar(ics);
            var icsCalendar = icsCalendars == null ? null : icsCalendars.FirstOrDefault();
            var icsEvents = icsCalendar == null ? null : icsCalendar.Events;
            var icsEvent = icsEvents == null ? null : icsEvents.FirstOrDefault();

            if (icsEvent == null) return null;

            EventHistory history;
            using (var tr = db.BeginTransaction())
            {
                ISqlInstruction sql;

                history = GetEventHistory(eventId);

                if (history == null)
                {
                    history = new EventHistory(calendarId, eventUid, eventId, ics);

                    sql = new SqlInsert("calendar_event_history")
                        .InColumnValue("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                        .InColumnValue("calendar_id", calendarId)
                        .InColumnValue("event_uid", eventUid)
                        .InColumnValue("event_id", eventId)
                        .InColumnValue("ics", history.Ics);
                }
                else
                {
                    var exist = history.History
                                        .Where(x => x.Method == icsCalendar.Method)
                                        .Select(x => x.Events.FirstOrDefault())
                                        .Any(x => x.Uid == icsEvent.Uid &&
                                                    x.Sequence == icsEvent.Sequence &&
                                                    DDayICalParser.ToUtc(x.DtStamp) == DDayICalParser.ToUtc(icsEvent.DtStamp));

                    if (exist) return history;

                    history.Ics = history.Ics + Environment.NewLine + ics;

                    sql = new SqlUpdate("calendar_event_history")
                        .Set("ics", history.Ics)
                        .Where("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                        .Where("calendar_id", calendarId)
                        .Where("event_uid", eventUid);
                }

                db.ExecuteNonQuery(sql);

                tr.Commit();
            }

            return history;
        }

        public void RemoveEventHistory(int calendarId, string eventUid)
        {
            var sql = new SqlDelete("calendar_event_history")
                .Where("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .Where("calendar_id", calendarId)
                .Where("event_uid", eventUid);

            db.ExecuteNonQuery(sql);
        }

        public void RemoveEventHistory(int eventId)
        {
            var sql = new SqlDelete("calendar_event_history")
                .Where("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                .Where("event_id", eventId);

            db.ExecuteNonQuery(sql);
        }

        private static EventHistory ToEventHistory(object[] row)
        {
            return new EventHistory(Convert.ToInt32(row[0]),
                                    Convert.ToString(row[1]),
                                    Convert.ToInt32(row[2]),
                                    Convert.ToString(row[3]));
        }

        public static string GetEventUid(string uid, string id = null)
        {
            if (!string.IsNullOrEmpty(uid))
                return uid;

            return string.Format("{0}@onlyoffice.com", string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id);

        }

        #region Event Notifications

        internal static int GetBeforeMinutes(EventAlertType eventAlertType)
        {
            switch (eventAlertType)
            {
                case EventAlertType.Day:
                    return -24 * 60;
                case EventAlertType.FifteenMinutes:
                    return -15;
                case EventAlertType.FiveMinutes:
                    return -5;
                case EventAlertType.HalfHour:
                    return -30;
                case EventAlertType.Hour:
                    return -60;
                case EventAlertType.TwoHours:
                    return -120;
            }

            return 0;
        }

        private DateTime GetNextAlertDate(DateTime utcStartDate, RecurrenceRule rrule, EventAlertType eventAlertType, TimeZoneInfo timeZone, bool isAllDayLong)
        {
            if (eventAlertType == EventAlertType.Never)
                return DateTime.MinValue;

            var offset = timeZone.GetOffset();
            var localFromDate = DateTime.UtcNow.Add(offset);
            var localStartDate = isAllDayLong ? utcStartDate : utcStartDate.Add(offset);
            var dates = rrule.GetDates(localStartDate, localFromDate, 3);
            for (var i = 0; i < dates.Count; i++)
            {
                dates[i] = dates[i].Subtract(offset);
            }

            foreach (var d in dates)
            {
                var dd = d.AddMinutes(GetBeforeMinutes(eventAlertType));
                if (dd > DateTime.UtcNow)
                    return dd;
            }

            return DateTime.MinValue;
        }

        private class UserAlertType
        {
            public Guid UserId { get; set; }
            public EventAlertType AlertType { get; set; }
            public TimeZoneInfo TimeZone { get; set; }


            public UserAlertType(Guid userId, EventAlertType alertType, TimeZoneInfo timeZone)
            {
                UserId = userId;
                AlertType = alertType;
                TimeZone = timeZone;
            }
        }

        private void UpdateEventNotifications(int eventId, int calendarId, DateTime eventUtcStartDate, EventAlertType baseEventAlertType, RecurrenceRule rrule,
            IEnumerable<SharingOptions.PublicItem> eventPublicItems,
            IEnumerable<SharingOptions.PublicItem> calendarPublicItems,
            bool isAllDayLong)
        {
            var cc = new ColumnCollection();
            var userIdCol = cc.RegistryColumn("user_id");
            var alertTypeCol = cc.RegistryColumn("alert_type");
            var isUnsubscribeCol = cc.RegistryColumn("is_unsubscribe");

            var eventUsersData = db.ExecuteList(new SqlQuery("calendar_event_user").Select(cc.SelectQuery).Where(Exp.Eq("event_id", eventId)));

            var calendarData = db.ExecuteList(new SqlQuery("calendar_calendars").Select("alert_type", "owner_id", "time_zone").Where(Exp.Eq("id", calendarId)));
            var calendarAlertType = calendarData.Select(r => (EventAlertType)Convert.ToInt32(r[0])).First();
            var calendarOwner = calendarData.Select(r => new Guid(Convert.ToString(r[1]))).First();
            var calendarTimeZone = calendarData.Select(r => TimeZoneConverter.GetTimeZone(Convert.ToString(r[2]))).First();

            var eventUsers = new List<UserAlertType>();

            #region shared event's data

            if (eventPublicItems == null)
            {
                eventPublicItems = new List<SharingOptions.PublicItem>(db.ExecuteList(new SqlQuery("calendar_event_item").Select("item_id", "is_group").Where(Exp.Eq("event_id", eventId)))
                                                                        .Select(r => new SharingOptions.PublicItem { Id = new Guid(Convert.ToString(r[0])), IsGroup = Convert.ToBoolean(r[1]) }));
            }

            foreach (var item in eventPublicItems)
            {
                if (item.IsGroup)
                    eventUsers.AddRange(CoreContext.UserManager.GetUsersByGroup(item.Id).Select(u => new UserAlertType(u.ID, baseEventAlertType, calendarTimeZone)));
                else
                    eventUsers.Add(new UserAlertType(item.Id, baseEventAlertType, calendarTimeZone));
            }

            //remove calendar owner
            eventUsers.RemoveAll(u => u.UserId.Equals(calendarOwner));

            //remove unsubscribed and exec personal alert_type
            if (eventUsers.Count > 0)
            {
                foreach (var r in eventUsersData)
                {
                    if (isUnsubscribeCol.Parse<bool>(r))
                        eventUsers.RemoveAll(u => u.UserId.Equals(userIdCol.Parse<Guid>(r)));
                    else
                        eventUsers.ForEach(u =>
                        {
                            if (u.UserId.Equals(userIdCol.Parse<Guid>(r)))
                                u.AlertType = (EventAlertType)alertTypeCol.Parse<int>(r);
                        });

                }
            }

            //remove and exec sharing calendar options
            if (eventUsers.Count > 0)
            {
                var extCalendarAlertTypes = db.ExecuteList(new SqlQuery("calendar_calendar_user cu")
                                                        .Select("cu.user_id", "cu.alert_type", "cu.is_accepted", "cu.time_zone")
                                                        .Where(Exp.Eq("cu.ext_calendar_id", SharedEventsCalendar.CalendarId) & Exp.In("cu.user_id", eventUsers.Select(u => u.UserId).ToArray())));

                foreach (var r in extCalendarAlertTypes)
                {
                    if (!Convert.ToBoolean(r[2]))
                    {
                        //remove unsubscribed from shared events calendar
                        eventUsers.RemoveAll(u => u.UserId.Equals(new Guid(Convert.ToString(r[0]))));
                        continue;
                    }
                    eventUsers.ForEach(u =>
                    {
                        if (u.UserId.Equals(new Guid(Convert.ToString(r[0]))))
                            u.TimeZone = ((r[3] == null || r[3] == DBNull.Value) ? calendarTimeZone : TimeZoneConverter.GetTimeZone(Convert.ToString(r[3])));

                        if (u.AlertType == EventAlertType.Default && u.UserId.Equals(new Guid(Convert.ToString(r[0]))))
                            u.AlertType = (EventAlertType)Convert.ToInt32(r[1]);
                    });
                }

                eventUsers.ForEach(u =>
                {
                    if (u.AlertType == EventAlertType.Default)
                        u.AlertType = EventAlertType.Hour;
                });

            }
            #endregion

            #region calendar's data

            if (calendarPublicItems == null)
            {
                calendarPublicItems = new List<SharingOptions.PublicItem>(db.ExecuteList(new SqlQuery("calendar_calendar_item").Select("item_id", "is_group").Where(Exp.Eq("calendar_id", calendarId)))
                                                                        .Select(r => new SharingOptions.PublicItem { Id = new Guid(Convert.ToString(r[0])), IsGroup = Convert.ToBoolean(r[1]) }));
            }

            //calendar users
            var calendarUsers = new List<UserAlertType>();
            foreach (var item in eventPublicItems)
            {
                if (item.IsGroup)
                    calendarUsers.AddRange(CoreContext.UserManager.GetUsersByGroup(item.Id).Select(u => new UserAlertType(u.ID, baseEventAlertType, calendarTimeZone)));
                else
                    calendarUsers.Add(new UserAlertType(item.Id, baseEventAlertType, calendarTimeZone));
            }

            calendarUsers.Add(new UserAlertType(calendarOwner, baseEventAlertType, calendarTimeZone));

            //remove event's users
            calendarUsers.RemoveAll(u => eventUsers.Exists(eu => eu.UserId.Equals(u.UserId)));

            //calendar options            
            if (calendarUsers.Count > 0)
            {
                //set personal alert_type
                foreach (var r in eventUsersData)
                {
                    eventUsers.ForEach(u =>
                    {
                        if (u.UserId.Equals(new Guid(Convert.ToString(r[0]))))
                            u.AlertType = (EventAlertType)(Convert.ToInt32(r[1]));
                    });

                }

                var calendarAlertTypes = db.ExecuteList(new SqlQuery("calendar_calendar_user")
                                                        .Select("user_id", "alert_type", "is_accepted", "time_zone")
                                                        .Where(Exp.Eq("calendar_id", calendarId) & Exp.In("user_id", calendarUsers.Select(u => u.UserId).ToArray())));

                foreach (var r in calendarAlertTypes)
                {
                    if (!Convert.ToBoolean(r[2]))
                    {
                        //remove unsubscribed
                        calendarUsers.RemoveAll(u => u.UserId.Equals(new Guid(Convert.ToString(r[0]))));
                        continue;
                    }
                    calendarUsers.ForEach(u =>
                    {
                        if (u.UserId.Equals(new Guid(Convert.ToString(r[0]))))
                            u.TimeZone = ((r[3] == null || r[3] == DBNull.Value) ? calendarTimeZone : TimeZoneConverter.GetTimeZone(Convert.ToString(r[3])));

                        if (u.AlertType == EventAlertType.Default && u.UserId.Equals(new Guid(Convert.ToString(r[0]))))
                            u.AlertType = (EventAlertType)Convert.ToInt32(r[1]);
                    });
                }

                calendarUsers.ForEach(u =>
                {
                    if (u.AlertType == EventAlertType.Default)
                        u.AlertType = calendarAlertType;
                });
            }

            #endregion


            //clear notifications
            db.ExecuteNonQuery(new SqlDelete("calendar_notifications").Where("event_id", eventId));

            eventUsers.AddRange(calendarUsers);

            foreach (var u in eventUsers)
            {
                //todo: recount
                var alertDate = GetNextAlertDate(eventUtcStartDate, rrule, u.AlertType, u.TimeZone, isAllDayLong);
                if (!alertDate.Equals(DateTime.MinValue))
                {
                    db.ExecuteNonQuery(new SqlInsert("calendar_notifications", true).InColumnValue("user_id", u.UserId)
                                                                                        .InColumnValue("event_id", eventId)
                                                                                        .InColumnValue("rrule", rrule.ToString())
                                                                                        .InColumnValue("alert_type", (int)u.AlertType)
                                                                                        .InColumnValue("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                                                                                        .InColumnValue("notify_date", alertDate)
                                                                                        .InColumnValue("time_zone", u.TimeZone.Id));
                }
            }
        }

        public List<EventNotificationData> ExtractAndRecountNotifications(DateTime utcDate)
        {
            List<EventNotificationData> data;
            using (var tr = db.BeginTransaction())
            {
                var cc = new ColumnCollection();
                var userIdCol = cc.RegistryColumn("user_id");
                var tenantCol = cc.RegistryColumn("tenant");
                var eventIdCol = cc.RegistryColumn("event_id");
                var notifyDateCol = cc.RegistryColumn("notify_date");
                var rruleCol = cc.RegistryColumn("rrule");
                var alertTypeCol = cc.RegistryColumn("alert_type");
                var timeZoneCol = cc.RegistryColumn("time_zone");

                data = new List<EventNotificationData>(db.ExecuteList(new SqlQuery("calendar_notifications").Select(cc.SelectQuery)
                                        .Where(Exp.Le(notifyDateCol.Name, utcDate)))
                                        .Select(r => new EventNotificationData
                                        {
                                            UserId = userIdCol.Parse<Guid>(r),
                                            TenantId = tenantCol.Parse<int>(r),
                                            EventId = eventIdCol.Parse<int>(r),
                                            NotifyUtcDate = notifyDateCol.Parse<DateTime>(r),
                                            RRule = rruleCol.Parse<RecurrenceRule>(r),
                                            AlertType = (EventAlertType)alertTypeCol.Parse<int>(r),
                                            TimeZone = timeZoneCol.Parse<TimeZoneInfo>(r)
                                        }));


                var events = GetEventsByIds(data.Select(d => (object)d.EventId).Distinct().ToArray(), Guid.Empty);
                data.ForEach(d => d.Event = events.Find(e => String.Equals(e.Id, d.EventId.ToString(CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase)));

                foreach (var d in data)   
                {
                    if (d.RRule.Freq == Frequency.Never)
                        db.ExecuteNonQuery(new SqlDelete("calendar_notifications").Where(Exp.Eq("user_id", d.UserId) & Exp.Eq("event_id", d.EventId)));
                    else
                    {
                        var alertDate = GetNextAlertDate(d.Event.UtcStartDate, d.RRule, d.AlertType, d.TimeZone, d.Event.AllDayLong);
                        if (!alertDate.Equals(DateTime.MinValue))
                        {
                            db.ExecuteNonQuery(new SqlInsert("calendar_notifications", true).InColumnValue("user_id", d.UserId)
                                                                                                .InColumnValue("event_id", d.EventId)
                                                                                                .InColumnValue("rrule", d.RRule.ToString())
                                                                                                .InColumnValue("alert_type", (int)d.AlertType)
                                                                                                .InColumnValue("tenant", d.TenantId)
                                                                                                .InColumnValue("notify_date", alertDate)
                                                                                                .InColumnValue("time_zone", d.TimeZone.Id));
                        }
                        else
                            db.ExecuteNonQuery(new SqlDelete("calendar_notifications").Where(Exp.Eq("user_id", d.UserId) & Exp.Eq("event_id", d.EventId)));
                    }
                }

                tr.Commit();
            }

            return data;
        }

        #endregion

        public void Dispose()
        {
            if (HttpContext.Current == null && db != null)
            {
                db.Dispose();
            }
        }
    }
}
