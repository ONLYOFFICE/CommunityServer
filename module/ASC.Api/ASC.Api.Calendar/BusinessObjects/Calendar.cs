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
using System.Text;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Web.Core.Calendars;
using ASC.Api.Calendar.Wrappers;
using ASC.Specific;
using System.Net;
using System.IO;
using ASC.Api.Calendar.iCalParser;

namespace ASC.Api.Calendar.BusinessObjects
{
    public static class CalendarExtention
    {
        public static bool IsiCalStream(this BaseCalendar calendar)
        {
            return (calendar is BusinessObjects.Calendar && !String.IsNullOrEmpty((calendar as BusinessObjects.Calendar).iCalUrl));
        }
        public static bool IsExistTodo(this BaseCalendar calendar)
        {
            return (calendar is BusinessObjects.Calendar && (calendar as BusinessObjects.Calendar).IsTodo != 0);
        }

        public static BaseCalendar GetUserCalendar(this BaseCalendar calendar, UserViewSettings userViewSettings)
        {
            var cal = (BaseCalendar)calendar.Clone();

            if (userViewSettings == null)
                return cal;
            
            //name             
            if (!String.IsNullOrEmpty(userViewSettings.Name))
                cal.Name = userViewSettings.Name;

            //backgroundColor
            if (!String.IsNullOrEmpty(userViewSettings.BackgroundColor))
                cal.Context.HtmlBackgroundColor = userViewSettings.BackgroundColor;

            //textColor
            if (!String.IsNullOrEmpty(userViewSettings.TextColor))
                cal.Context.HtmlTextColor = userViewSettings.TextColor;

            //TimeZoneInfo      
            if (userViewSettings.TimeZone!= null)
                cal.TimeZone = userViewSettings.TimeZone;

            //alert type            
            cal.EventAlertType = userViewSettings.EventAlertType;

            return cal;
        }

        public static List<EventWrapper> GetEventWrappers(this BaseCalendar calendar, Guid userId, ApiDateTime startDate, ApiDateTime endDate)
        {   
            var result = new List<EventWrapper>();
            if (calendar != null)
            {
                var events = calendar.LoadEvents(userId, startDate.UtcTime, endDate.UtcTime);
                foreach (var e in events)
                {
                    var wrapper = new EventWrapper(e, userId, calendar.TimeZone);
                    var listWrapper = wrapper.GetList(startDate.UtcTime, endDate.UtcTime);
                    result.AddRange(listWrapper);
                }
            }

            return result;
        }
        public static List<TodoWrapper> GetTodoWrappers(this BaseCalendar calendar, Guid userId, ApiDateTime startDate, ApiDateTime endDate)
        {
            var result = new List<TodoWrapper>();
            if (calendar != null)
            {
                using (var provider = new DataProvider())
                {
                    var cal = provider.GetCalendarById(Convert.ToInt32(calendar.Id));
                    if (cal != null)
                    {
                        var todos = provider.LoadTodos(Convert.ToInt32(calendar.Id), userId, cal.TenantId, startDate,endDate)
                                .Cast<ITodo>()
                                .ToList();
                        foreach (var t in todos)
                        {
                            var wrapper = new TodoWrapper(t, userId, calendar.TimeZone);
                            var listWrapper = wrapper.GetList();
                            result.AddRange(listWrapper);
                        }
                        return result;
                    }
                }
            }
            return null;
        }
    }


    [DataContract(Name = "calendar", Namespace = "")]
    public class Calendar : BaseCalendar,  ISecurityObject
    {
        public static string DefaultTextColor { get { return "#000000";} }
        public static string DefaultBackgroundColor { get { return "#9bb845"; } }
        public static string DefaultTodoBackgroundColor { get { return "#ffb45e"; } }

        public Calendar()
        {
            this.ViewSettings = new List<UserViewSettings>();
            this.Context.CanChangeAlertType = true;
            this.Context.CanChangeTimeZone = true;
        }

        public int TenantId { get; set; }
        
        public List<UserViewSettings> ViewSettings { get; set; }

        public string iCalUrl { get; set; }

        public string calDavGuid { get; set; }

        public int IsTodo { get; set; }

        #region ISecurityObjectId Members

        /// <inheritdoc/>
        public object SecurityId
        {
            get { return this.Id; }
        }

        /// <inheritdoc/>
        public Type ObjectType
        {
            get { return typeof(Calendar); }
        }

        #endregion

        #region ISecurityObjectProvider Members

        public IEnumerable<ASC.Common.Security.Authorizing.IRole> GetObjectRoles(ASC.Common.Security.Authorizing.ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            List<IRole> roles = new List<IRole>();
            if (account.ID.Equals(this.OwnerId))
                roles.Add(ASC.Common.Security.Authorizing.Constants.Owner);

            return roles;
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            throw new NotImplementedException();
        }

        public bool InheritSupported
        {
            get { return false; }
        }

        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        #endregion

        public override List<IEvent> LoadEvents(Guid userId, DateTime utcStartDate, DateTime utcEndDate)
        {
            if (!String.IsNullOrEmpty(iCalUrl))
            {
                try
                {
                    var cal = iCalendar.GetFromUrl(iCalUrl,this.Id);
                    return cal.LoadEvents(userId, utcStartDate, utcEndDate);
                }
                catch
                {
                    return new List<IEvent>();
                }
            }

            using (var provider = new DataProvider())
            {
                return provider.LoadEvents(Convert.ToInt32(this.Id), userId, TenantId, utcStartDate, utcEndDate)
                        .Cast<IEvent>()
                        .ToList();
            }
        }
    }    
}
