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
using System.Runtime.Serialization;

using ASC.Api.Calendar.ExternalCalendars;
using ASC.Common.Security;
using ASC.Core;
using ASC.Core.Users;
using ASC.Specific;
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name = "event", Namespace = "")]
    public class EventWrapper
    {
        private readonly TimeZoneInfo _timeZone;
        public Guid UserId { get; private set; }

        protected IEvent _baseEvent;

        private readonly DateTime _utcStartDate = DateTime.MinValue;
        private readonly DateTime _utcEndDate = DateTime.MinValue;

        private EventWrapper(IEvent baseEvent, Guid userId, TimeZoneInfo timeZone, DateTime utcStartDate, DateTime utcEndDate)
            : this(baseEvent, userId, timeZone)
        {
            _utcStartDate = utcStartDate;
            _utcEndDate = utcEndDate;
        }

        public EventWrapper(IEvent baseEvent, Guid userId, TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
            _baseEvent = baseEvent;
            this.UserId = userId;
        }

        public List<EventWrapper> GetList(DateTime utcStartDate, DateTime utcEndDate)
        {
            var list = new List<EventWrapper>();

            if (_baseEvent.UtcStartDate == DateTime.MinValue)
                return list;

            var difference = _baseEvent.UtcEndDate - _baseEvent.UtcStartDate;

            var recurenceDates = new List<DateTime>();

            if (_baseEvent.RecurrenceRule.Freq == Frequency.Never)
            {
                if ((_baseEvent.UtcStartDate <= utcStartDate && _baseEvent.UtcEndDate >= utcStartDate) ||
                    (_baseEvent.UtcStartDate <= utcEndDate && _baseEvent.UtcEndDate >= utcEndDate) ||
                    (_baseEvent.UtcStartDate >= utcStartDate && _baseEvent.UtcEndDate <= utcEndDate))
                    recurenceDates.Add(_baseEvent.UtcStartDate);
            }
            else
            {
                recurenceDates = _baseEvent.RecurrenceRule.GetDates(_baseEvent.UtcStartDate, _baseEvent.TimeZone ?? _timeZone, _baseEvent.AllDayLong, utcStartDate, utcEndDate);
            }

            foreach (var d in recurenceDates)
            {
                var endDate = _baseEvent.UtcEndDate;
                if (!_baseEvent.UtcEndDate.Equals(DateTime.MinValue))
                    endDate = d + difference;

                list.Add(new EventWrapper(_baseEvent, this.UserId, _timeZone, d, endDate));
            }

            return list;
        }

        ///<example name="objectId">1</example>
        ///<order>0</order>
        [DataMember(Name = "objectId", Order = 0)]
        public string Id { get { return _baseEvent.Id; } }

        ///<example name="uniqueId">1234wda</example>
        ///<order>140</order>
        [DataMember(Name = "uniqueId", Order = 140)]
        public string Uid { get { return _baseEvent.Uid; } }

        ///<example>1</example>
        public int TenantId { get; set; }

        ///<example>true</example>
        public bool Todo { get; set; }

        ///<example name="sourceId">calendarID</example>
        ///<order>10</order>
        [DataMember(Name = "sourceId", Order = 10)]
        public string CalendarId { get { return _baseEvent.CalendarId; } }

        ///<example name="title">Event Name</example>
        ///<order>20</order>
        [DataMember(Name = "title", Order = 20)]
        public string Name { get { return _baseEvent.Name; } }

        ///<example name="description">Event Description</example>
        ///<order>30</order>
        [DataMember(Name = "description", Order = 30)]
        public string Description { get { return _baseEvent.Description; } }

        ///<example name="allDay">false</example>
        ///<order>60</order>
        [DataMember(Name = "allDay", Order = 60)]
        public bool AllDayLong { get { return _baseEvent.AllDayLong; } }

        ///<example name="start">2020-12-01T06:36:10.8645482Z</example>
        ///<order>40</order>
        [DataMember(Name = "start", Order = 40)]
        public ApiDateTime Start
        {
            get
            {
                var startD = _utcStartDate != DateTime.MinValue ? _utcStartDate : _baseEvent.UtcStartDate;
                startD = new DateTime(startD.Ticks, DateTimeKind.Utc);

                if (_baseEvent.AllDayLong && _baseEvent.GetType().GetCustomAttributes(typeof(AllDayLongUTCAttribute), true).Length > 0)
                    return new ApiDateTime(startD, TimeZoneInfo.Utc);

                if (_baseEvent.AllDayLong && _baseEvent is iCalParser.iCalEvent)
                    return new ApiDateTime(startD, TimeZoneInfo.Utc);

                return new ApiDateTime(startD, _timeZone);
            }
        }

        ///<example name="end">2020-12-01T06:36:10.8645482Z</example>
        ///<order>50</order>
        [DataMember(Name = "end", Order = 50)]
        public ApiDateTime End
        {
            get
            {
                var endD = _utcEndDate != DateTime.MinValue ? _utcEndDate : _baseEvent.UtcEndDate;
                endD = new DateTime(endD.Ticks, DateTimeKind.Utc);

                if (_baseEvent.AllDayLong && _baseEvent.GetType().GetCustomAttributes(typeof(AllDayLongUTCAttribute), true).Length > 0)
                    return new ApiDateTime(endD, TimeZoneInfo.Utc);

                if (_baseEvent.AllDayLong && _baseEvent is iCalParser.iCalEvent)
                    return new ApiDateTime(endD, TimeZoneInfo.Utc);

                return new ApiDateTime(endD, _timeZone);
            }
        }

        ///<example name="repeatRule"></example>
        ///<order>70</order>
        [DataMember(Name = "repeatRule", Order = 70)]
        public string RepeatRule
        {
            get
            {
                return _baseEvent.RecurrenceRule.ToString();
            }
        }

        ///<type name="alert">ASC.Api.Calendar.Wrappers.EventAlertWrapper, ASC.Api.Calendar</type>
        ///<order>110</order>
        [DataMember(Name = "alert", Order = 110)]
        public EventAlertWrapper Alert
        {
            get
            {
                return EventAlertWrapper.ConvertToTypeSurrogated(_baseEvent.AlertType);
            }
        }

        ///<example name="isShared">true</example>
        ///<order>80</order>
        [DataMember(Name = "isShared", Order = 80)]
        public bool IsShared
        {
            get
            {
                return _baseEvent.SharingOptions.SharedForAll || _baseEvent.SharingOptions.PublicItems.Count > 0;
            }
        }

        ///<example name="canUnsubscribe">true</example>
        ///<order>130</order>
        [DataMember(Name = "canUnsubscribe", Order = 130)]
        public bool CanUnsubscribe
        {
            get
            {
                return String.Equals(_baseEvent.CalendarId, SharedEventsCalendar.CalendarId, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        ///<example name="isEditable">false</example>
        ///<order>100</order>
        [DataMember(Name = "isEditable", Order = 100)]
        public virtual bool IsEditable
        {
            get
            {
                if (_baseEvent is ISecurityObject)
                    return SecurityContext.PermissionResolver.Check(CoreContext.Authentication.GetAccountByID(this.UserId), (ISecurityObject)_baseEvent, null, CalendarAccessRights.FullAccessAction);

                return false;
            }
        }

        ///<type name="permissions">ASC.Api.Calendar.Wrappers.CalendarPermissions, ASC.Api.Calendar</type>
        ///<order>90</order>
        [DataMember(Name = "permissions", Order = 90)]
        public Permissions Permissions
        {
            get
            {
                var p = new CalendarPermissions() { Data = PublicItemCollection.GetForEvent(_baseEvent) };
                foreach (var item in _baseEvent.SharingOptions.PublicItems)
                {
                    if (item.IsGroup)
                        p.UserParams.Add(new UserParams() { Id = item.Id, Name = CoreContext.UserManager.GetGroupInfo(item.Id).Name });
                    else
                        p.UserParams.Add(new UserParams() { Id = item.Id, Name = CoreContext.UserManager.GetUsers(item.Id).DisplayUserName() });
                }
                return p;
            }
        }
        ///<type name="owner">ASC.Api.Calendar.Wrappers.UserParams, ASC.Api.Calendar</type>
        ///<order>120</order>
        [DataMember(Name = "owner", Order = 120)]
        public UserParams Owner
        {
            get
            {
                var owner = new UserParams() { Id = _baseEvent.OwnerId, Name = "" };
                if (_baseEvent.OwnerId != Guid.Empty)
                    owner.Name = CoreContext.UserManager.GetUsers(_baseEvent.OwnerId).DisplayUserName();

                return owner;
            }
        }

        ///<example name="status" type="int">0</example>
        ///<order>150</order>
        [DataMember(Name = "status", Order = 150)]
        public EventStatus Status
        {
            get
            {
                return _baseEvent.Status;
            }
        }

        [DataMember(Name = "hasAttachments", Order = 160)]
        public bool HasAttachments
        {
            get
            {
                return _baseEvent.HasAttachments;
            }
        }


        public static object GetSample()
        {
            return new
            {
                owner = UserParams.GetSample(),
                permissions = Permissions.GetSample(),
                isEditable = false,
                сanUnsubscribe = true,
                isShared = true,
                alert = EventAlertWrapper.GetSample(),
                repeatRule = "",
                start = new ApiDateTime(DateTime.Now.AddDays(1), TimeZoneInfo.Utc),
                end = new ApiDateTime(DateTime.Now.AddDays(1), TimeZoneInfo.Utc),
                allDay = false,
                description = "Event Description",
                title = "Event Name",
                objectId = "1",
                sourceId = "calendarID",
                status = (int)EventStatus.Tentative,
                hasAttachments = false
            };
        }
    }
}
