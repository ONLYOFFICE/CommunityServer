/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Users;
using ASC.Specific;
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name = "todo", Namespace = "")]
    public class TodoWrapper
    {        
        private TimeZoneInfo _timeZone;

        public Guid UserId { get; private set; }

        protected ITodo _baseTodo;

        private DateTime _utcStartDate = DateTime.MinValue;
        private DateTime _utcCompletedDate = DateTime.MinValue;

        private TodoWrapper(ITodo baseTodo, Guid userId, TimeZoneInfo timeZone, DateTime utcStartDate)
            :this(baseTodo, userId, timeZone)
        {
            _utcStartDate = utcStartDate;
        }

        public TodoWrapper(ITodo baseTodo, Guid userId, TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
            _baseTodo = baseTodo;
            this.UserId = userId;            
        }

        public List<TodoWrapper> GetList()
        {
            var list = new List<TodoWrapper>();

            list.Add(new TodoWrapper(_baseTodo, this.UserId, _timeZone));

            return list;
        }
        
        [DataMember(Name = "objectId", Order = 0)]
        public string Id { get { return _baseTodo.Id; } }

        [DataMember(Name = "uniqueId", Order = 140)]
        public string Uid { get { return _baseTodo.Uid; } }

        public int TenantId { get; set; }

        [DataMember(Name = "sourceId", Order = 10)]
        public string CalendarId { get { return _baseTodo.CalendarId; } }

        [DataMember(Name = "title", Order = 20)]
        public string Name { get { return _baseTodo.Name; } }

        [DataMember(Name = "description", Order = 30)]
        public string Description { get { return _baseTodo.Description; } }

        [DataMember(Name = "start", Order = 40)]
        public ApiDateTime Start
        {
            get
            {
                var startD = _utcStartDate != DateTime.MinValue ? _utcStartDate : _baseTodo.UtcStartDate;
                startD =new DateTime(startD.Ticks, DateTimeKind.Utc);

                return new ApiDateTime(startD, _timeZone);
            }
        }

        [DataMember(Name = "completed", Order = 110)]
        public ApiDateTime Completed
        {
            get
            {
                var completedD = _utcCompletedDate != DateTime.MinValue ? _utcCompletedDate : _baseTodo.Completed;
                completedD = new DateTime(completedD.Ticks, DateTimeKind.Utc);
                return new ApiDateTime(completedD, _timeZone);
            }
        }

        [DataMember(Name = "owner", Order = 120)]
        public UserParams Owner
        {
            get
            {
                var owner = new UserParams() { Id = _baseTodo.OwnerId, Name = "" };
                if (_baseTodo.OwnerId != Guid.Empty)
                    owner.Name = CoreContext.UserManager.GetUsers(_baseTodo.OwnerId).DisplayUserName();

                return owner;
            }
        }

      
        public static object GetSample()
        {
            return new
            {
                owner = UserParams.GetSample(),
                start = new ApiDateTime(DateTime.Now.AddDays(1), TimeZoneInfo.Utc),
                description = "Todo Description",
                title = "Todo Name",
                objectId = "1",
                sourceId = "calendarID",
                completed = new ApiDateTime(DateTime.MinValue, TimeZoneInfo.Utc)
            };
        }
    }
}
