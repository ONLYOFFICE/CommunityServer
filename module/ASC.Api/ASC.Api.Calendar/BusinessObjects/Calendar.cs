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
    }


    [DataContract(Name = "calendar", Namespace = "")]
    public class Calendar : BaseCalendar,  ISecurityObject
    {
        public static string DefaultTextColor { get { return "#000000";} }
        public static string DefaultBackgroundColor { get { return "#9bb845";} }

        public Calendar()
        {
            this.ViewSettings = new List<UserViewSettings>();
            this.Context.CanChangeAlertType = true;
            this.Context.CanChangeTimeZone = true;
        }

        public int TenantId { get; set; }
        
        public List<UserViewSettings> ViewSettings { get; set; }

        public string iCalUrl { get; set; }

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
