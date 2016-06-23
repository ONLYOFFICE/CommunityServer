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
