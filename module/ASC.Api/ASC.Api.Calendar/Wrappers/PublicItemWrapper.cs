/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
                    return CoreContext.GroupManager.GetGroupInfo(base.Id).Name;                
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
                var subject = IsGroup ? (ISubject)CoreContext.GroupManager.GetGroupInfo(base.Id) : (ISubject)CoreContext.Authentication.GetAccountByID(base.Id);
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
