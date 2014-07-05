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
        private TimeZoneInfo _timeZone;
        public Guid UserId { get; private set; }

        protected IEvent _baseEvent;

        private DateTime _utcStartDate = DateTime.MinValue;
        private DateTime _utcEndDate= DateTime.MinValue;

        private EventWrapper(IEvent baseEvent, Guid userId, TimeZoneInfo timeZone, DateTime utcStartDate, DateTime utcEndDate)
            :this(baseEvent, userId, timeZone)
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

            var localStartDate = _baseEvent.UtcStartDate.Kind == DateTimeKind.Local ? _baseEvent.UtcStartDate : _baseEvent.UtcStartDate.Add(_timeZone.BaseUtcOffset);

            var recurenceDates = _baseEvent.RecurrenceRule.GetDates(localStartDate, utcStartDate.Add(_timeZone.BaseUtcOffset), utcEndDate.Add(_timeZone.BaseUtcOffset));

            if(recurenceDates.Count==0)
                recurenceDates.Add(localStartDate);

            foreach (var d in recurenceDates)
            {
                var utcD = d.AddMinutes((-1) * (int)_timeZone.BaseUtcOffset.TotalMinutes);
                var endDate = _baseEvent.UtcEndDate;
                if (!_baseEvent.UtcEndDate.Equals(DateTime.MinValue))
                    endDate = utcD + difference;

                list.Add(new EventWrapper(_baseEvent, this.UserId, _timeZone, utcD, endDate));
            }

            return list;
        }
        
        [DataMember(Name = "objectId", Order = 0)]
        public string Id { get { return _baseEvent.Id; } set { } }

        public int TenantId { get; set; }

        [DataMember(Name = "sourceId", Order = 10)]
        public string CalendarId { get { return _baseEvent.CalendarId; } set { } }

        [DataMember(Name = "title", Order = 20)]
        public string Name { get { return _baseEvent.Name; } set { } }

        [DataMember(Name = "description", Order = 30)]
        public string Description { get { return _baseEvent.Description; } set { } }

        [DataMember(Name = "allDay", Order = 60)]
        public bool AllDayLong { get { return _baseEvent.AllDayLong; } set { } }

        [DataMember(Name = "start", Order = 40)]
        public ApiDateTime Start
        {
            get
            {
                var startD = _utcStartDate != DateTime.MinValue ? _utcStartDate : _baseEvent.UtcStartDate;
                startD =new DateTime(startD.Ticks, DateTimeKind.Utc);

                if (_baseEvent.AllDayLong && _baseEvent.GetType().GetCustomAttributes(typeof(AllDayLongUTCAttribute), true).Length > 0)
                    return new ApiDateTime(startD, TimeZoneInfo.Utc);

                if(_baseEvent is iCalParser.iCalEvent)
                    if (_baseEvent.AllDayLong)
                        return new ApiDateTime(startD, TimeZoneInfo.Utc);
                    else
                        return new ApiDateTime(startD, CoreContext.TenantManager.GetCurrentTenant().TimeZone);

                return new ApiDateTime(startD, _timeZone.BaseUtcOffset);
            }
            set { }
        }

        [DataMember(Name = "end", Order = 50)]
        public ApiDateTime End
        {
            get
            {
                var endD = _utcEndDate!= DateTime.MinValue? _utcEndDate : _baseEvent.UtcEndDate;
                endD = new DateTime(endD.Ticks, DateTimeKind.Utc);

                if (_baseEvent.AllDayLong && _baseEvent.GetType().GetCustomAttributes(typeof(AllDayLongUTCAttribute), true).Length > 0)
                    return new ApiDateTime(endD, TimeZoneInfo.Utc);

                if (_baseEvent is iCalParser.iCalEvent)
                    if (_baseEvent.AllDayLong)
                        return new ApiDateTime(endD, TimeZoneInfo.Utc);
                    else
                        return new ApiDateTime(endD, CoreContext.TenantManager.GetCurrentTenant().TimeZone);

                return new ApiDateTime(endD, _timeZone.BaseUtcOffset);
            }
            set { }
        }

        [DataMember(Name = "repeatRule", Order = 70)]
        public string RepeatRule
        {
            get
            {
                return _baseEvent.RecurrenceRule.ToString();
            }
            set { }
        }

        [DataMember(Name = "alert", Order = 110)]
        public EventAlertWrapper Alert
        {
            get
            {
                return EventAlertWrapper.ConvertToTypeSurrogated(_baseEvent.AlertType);
            }
            set { }
        }

        [DataMember(Name = "isShared", Order = 80)]
        public bool IsShared
        {
            get
            {
                return _baseEvent.SharingOptions.SharedForAll || _baseEvent.SharingOptions.PublicItems.Count > 0;
            }
            set { }
        }

        [DataMember(Name = "canUnsubscribe", Order = 180)]
        public bool CanUnsubscribe
        {
            get
            {
                return String.Equals(_baseEvent.CalendarId, SharedEventsCalendar.CalendarId, StringComparison.InvariantCultureIgnoreCase);
            }
            set { }
        }

        [DataMember(Name = "isEditable", Order = 100)]
        public virtual bool IsEditable
        {
            get
            {
                if (_baseEvent is ISecurityObject)
                    return SecurityContext.PermissionResolver.Check(CoreContext.Authentication.GetAccountByID(this.UserId), (ISecurityObject)_baseEvent, null, CalendarAccessRights.FullAccessAction);

                return false;
            }
            set { }
        }

        [DataMember(Name = "permissions", Order = 90)]
        public Permissions Permissions
        {
            get
            {
                var p = new CalendarPermissions() { Data = PublicItemCollection.GetForEvent(_baseEvent) };
                foreach (var item in _baseEvent.SharingOptions.PublicItems)
                {
                    if (item.IsGroup)
                        p.UserParams.Add(new UserParams() { Id = item.Id, Name = CoreContext.GroupManager.GetGroupInfo(item.Id).Name });
                    else
                        p.UserParams.Add(new UserParams() { Id = item.Id, Name = CoreContext.UserManager.GetUsers(item.Id).DisplayUserName() });
                }
                return p;
            }
            set { }
        }

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
            set { }
        }

        public static object GetSample()
        {
            return new
            {
                owner = UserParams.GetSample(),
                permissions = Permissions.GetSample(),
                isEditable = false,
                CanUnsubscribe = true,
                isShared = true,
                alert = EventAlertWrapper.GetSample(),
                repeatRule = "",
                start = new ApiDateTime(DateTime.Now.AddDays(1), TimeZoneInfo.Utc),
                end = new ApiDateTime(DateTime.Now.AddDays(1), TimeZoneInfo.Utc),
                allDay = false,
                description = "Event Description",
                title = "Event Name",
                objectId = "1",
                sourceId = "calendarID"

            };
        }
    }
}
