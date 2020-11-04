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
using System.Runtime.Serialization;
using ASC.Api.Calendar.BusinessObjects;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Calendars;
using ASC.Api.Calendar.ExternalCalendars;
using ASC.Common.Security;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name = "calendar", Namespace = "")]
    public class CalendarWrapper
    {        
        public BaseCalendar UserCalendar{get; private set;}
        protected UserViewSettings _userViewSettings;
        protected Guid _userId;

        public CalendarWrapper(BaseCalendar calendar) : this(calendar, null){}
        public CalendarWrapper(BaseCalendar calendar, UserViewSettings userViewSettings)
        {
            _userViewSettings = userViewSettings;
            if (_userViewSettings == null && calendar is ASC.Api.Calendar.BusinessObjects.Calendar)
            { 
                _userViewSettings = (calendar as ASC.Api.Calendar.BusinessObjects.Calendar)
                                    .ViewSettings.Find(s=> s.UserId == SecurityContext.CurrentAccount.ID);
            }

            if (_userViewSettings == null)
            {
                UserCalendar = calendar;
                _userId = SecurityContext.CurrentAccount.ID;
            }
            else
            {
                UserCalendar = calendar.GetUserCalendar(_userViewSettings);
                _userId = _userViewSettings.UserId;
            }
        }

        [DataMember(Name = "isSubscription", Order = 80)]
        public virtual bool IsSubscription
        {
            get
            {
                if (UserCalendar.IsiCalStream())
                    return true;

                if (UserCalendar.Id.Equals(SharedEventsCalendar.CalendarId, StringComparison.InvariantCultureIgnoreCase))
                    return true;

                if (UserCalendar.OwnerId.Equals(_userId))
                    return false;

                return true;
            }
            set { }
        }

        [DataMember(Name = "iCalUrl", Order = 230)]
        public virtual string iCalUrl
        {
            get
            {
                if (UserCalendar.IsiCalStream())
                    return (UserCalendar as BusinessObjects.Calendar).iCalUrl;

                return "";
            }
            set { }
        }

        [DataMember(Name = "isiCalStream", Order = 220)]
        public virtual bool IsiCalStream
        {
            get
            {
                if (UserCalendar.IsiCalStream())
                    return true;

                return false;
            }
            set { }
        }

        [DataMember(Name = "isHidden", Order = 50)]
        public virtual bool IsHidden
        {
            get
            {
                return _userViewSettings != null ? _userViewSettings.IsHideEvents : false;
            }
            set { }
        }

        [DataMember(Name = "canAlertModify", Order = 200)]
        public virtual bool CanAlertModify
        {
            get
            {
                return UserCalendar.Context.CanChangeAlertType;
            }
            set { }
        }


        [DataMember(Name = "isShared", Order = 60)]
        public virtual bool IsShared
        {
            get
            {
                return UserCalendar.SharingOptions.SharedForAll || UserCalendar.SharingOptions.PublicItems.Count > 0;
            }
            set { }
        }

        [DataMember(Name = "permissions", Order = 70)]
        public virtual CalendarPermissions Permissions
        {
            get
            {
                var p = new CalendarPermissions() { Data = PublicItemCollection.GetForCalendar(UserCalendar) };
                foreach (var item in UserCalendar.SharingOptions.PublicItems)
                {
                    if (item.IsGroup)
                        p.UserParams.Add(new UserParams() { Id = item.Id, Name = CoreContext.UserManager.GetGroupInfo(item.Id).Name });
                    else
                        p.UserParams.Add(new UserParams() { Id = item.Id, Name = CoreContext.UserManager.GetUsers(item.Id).DisplayUserName() });
                }
                return p;
            }
            set { }
        }

        [DataMember(Name = "isEditable", Order = 90)]
        public virtual bool IsEditable
        {
            get
            {
                if (UserCalendar.IsiCalStream())
                    return false;

                if (UserCalendar is ISecurityObject)
                    return SecurityContext.PermissionResolver.Check(CoreContext.Authentication.GetAccountByID(_userId), (ISecurityObject)UserCalendar as ISecurityObject, null, CalendarAccessRights.FullAccessAction);

                return false;
            }
            set { }
        }

        [DataMember(Name = "textColor", Order = 30)]
        public string TextColor
        {
            get
            {
                return String.IsNullOrEmpty(UserCalendar.Context.HtmlTextColor)? BusinessObjects.Calendar.DefaultTextColor:
                    UserCalendar.Context.HtmlTextColor;
            }
            set { }
        }

        [DataMember(Name = "backgroundColor", Order = 40)]
        public string BackgroundColor
        {
            get
            {
                return String.IsNullOrEmpty(UserCalendar.Context.HtmlBackgroundColor) ? BusinessObjects.Calendar.DefaultBackgroundColor :
                    UserCalendar.Context.HtmlBackgroundColor;
            }
            set { }
        }

        [DataMember(Name = "description", Order = 20)]
        public string Description { get { return UserCalendar.Description; } set { } }

        [DataMember(Name = "title", Order = 30)]
        public string Title
        {
            get{return UserCalendar.Name;}
            set{}
        } 

        [DataMember(Name = "objectId", Order = 0)]
        public string Id
        {
            get{return UserCalendar.Id;}
            set{}
        }

        [DataMember(Name = "isTodo", Order = 0)]
        public int IsTodo
        {

            get
            {
                if (UserCalendar.IsExistTodo())
                    return (UserCalendar as BusinessObjects.Calendar).IsTodo;

                return 0;
            }
            set { }
        }

        [DataMember(Name = "owner", Order = 120)]
        public UserParams Owner
        {
            get
            {
                var owner = new UserParams() { Id = UserCalendar.OwnerId , Name = ""};
                if (UserCalendar.OwnerId != Guid.Empty)
                    owner.Name = CoreContext.UserManager.GetUsers(UserCalendar.OwnerId).DisplayUserName();

                return owner;
            }
            set { }
        }

        public bool IsAcceptedSubscription
        {
            get
            {
                return  _userViewSettings == null || _userViewSettings.IsAccepted;
            }
            set { }
        }

        [DataMember(Name = "events", Order = 150)]
        public List<EventWrapper> Events { get; set; }

        [DataMember(Name = "todos", Order = 160)]
        public List<TodoWrapper> Todos { get; set; }

        [DataMember(Name = "defaultAlert", Order = 160)]
        public EventAlertWrapper DefaultAlertType
        {
            get{
                return EventAlertWrapper.ConvertToTypeSurrogated(UserCalendar.EventAlertType);            
            }
            set { }
        }
        
        [DataMember(Name = "timeZone", Order = 160)]
        public TimeZoneWrapper TimeZoneInfo
        {
            get {
                return new TimeZoneWrapper(UserCalendar.TimeZone);
            }
            set{}
        }

        [DataMember(Name = "canEditTimeZone", Order = 160)]
        public bool CanEditTimeZone
        {
            get { return UserCalendar.Context.CanChangeTimeZone;}
            set { }
        }

        public static object GetSample()
        {
            return new
            {
                canEditTimeZone = false,
                timeZone = TimeZoneWrapper.GetSample(),
                defaultAlert = EventAlertWrapper.GetSample(),
                events = new List<object>() { EventWrapper.GetSample() },
                owner = UserParams.GetSample(),
                objectId = "1",
                title = "Calendar Name",
                description = "Calendar Description",
                backgroundColor = "#000000",
                textColor = "#ffffff",
                isEditable = true,
                permissions = CalendarPermissions.GetSample(),
                isShared = true,
                canAlertModify = true,
                isHidden = false,
                isiCalStream = false,
                isSubscription = false
            };
        }

    }
}
