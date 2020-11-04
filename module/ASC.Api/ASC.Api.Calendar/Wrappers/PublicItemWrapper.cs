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
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Users;
using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name ="publicItem")]
    public class PublicItemWrapper : ASC.Web.Core.Calendars.SharingOptions.PublicItem
    {
        private Guid _owner;
        private string _calendarId;
        private string _eventId;
        private bool _isCalendar;


        public PublicItemWrapper( ASC.Web.Core.Calendars.SharingOptions.PublicItem publicItem, string calendartId, Guid owner)
        {
            base.Id = publicItem.Id;
            base.IsGroup = publicItem.IsGroup;

            _owner = owner;
            _calendarId = calendartId;
            _isCalendar = true;
        }

        public PublicItemWrapper(ASC.Web.Core.Calendars.SharingOptions.PublicItem publicItem, string calendarId, string eventId, Guid owner)
        {
            base.Id = publicItem.Id;
            base.IsGroup = publicItem.IsGroup;

            _owner = owner;
            _calendarId = calendarId;
            _eventId = eventId;
            _isCalendar = false;
        }

        [DataMember(Name = "id", Order = 10)]
        public string ItemId
        {
            get
            {
                return base.Id.ToString();
            }
            set{}
        }

        [DataMember(Name = "name", Order = 20)]
        public string ItemName
        {
            get
            {
                if(this.IsGroup)                
                    return CoreContext.UserManager.GetGroupInfo(base.Id).Name;                
                else
                    return CoreContext.UserManager.GetUsers(base.Id).DisplayUserName();                
            }
            set{}
        }

        [DataMember(Name = "isGroup", Order = 30)]
        public new bool IsGroup
        {
            get
            {
                return base.IsGroup;
            }
            set { }
        }

        [DataMember(Name = "canEdit", Order = 40)]
        public bool CanEdit
        {
            get
            {
                return !base.Id.Equals(_owner); 
            }
            set { }
        }

        [DataMember(Name = "selectedAction", Order = 50)]
        public AccessOption SharingOption
        {
            get {
                if (base.Id.Equals(_owner))
                {
                    return AccessOption.OwnerOption;
                }
                var subject = IsGroup ? (ISubject)CoreContext.UserManager.GetGroupInfo(base.Id) : (ISubject)CoreContext.Authentication.GetAccountByID(base.Id);
                int calId;
                if (_isCalendar && int.TryParse(_calendarId,out calId))
                {
                    var obj = new ASC.Api.Calendar.BusinessObjects.Calendar() { Id = _calendarId };
                    if (SecurityContext.PermissionResolver.Check(subject, obj, null, CalendarAccessRights.FullAccessAction))
                        return AccessOption.FullAccessOption;
                }
                else if(!_isCalendar)
                {
                    var obj = new ASC.Api.Calendar.BusinessObjects.Event() { Id = _eventId, CalendarId = _calendarId};
                    if (SecurityContext.PermissionResolver.Check(subject, obj, null, CalendarAccessRights.FullAccessAction))
                        return AccessOption.FullAccessOption;
                }

                return AccessOption.ReadOption;
            }
            set { }
        }

        public static object GetSample()
        {
            return new { selectedAction = AccessOption.GetSample(), canEdit = true, isGroup = true, 
                         name = "Everyone", id = "2fdfe577-3c26-4736-9df9-b5a683bb8520" };
        }
    }
}
