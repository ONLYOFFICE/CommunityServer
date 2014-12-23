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
using ASC.Web.Core.Calendars;
using ASC.Api.Calendar.BusinessObjects;
using System.Collections.Generic;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name = "subscription", Namespace = "")]
    public class SubscriptionWrapper : CalendarWrapper
    {
        public SubscriptionWrapper(BaseCalendar calendar)
            : base(calendar) { }
        public SubscriptionWrapper(BaseCalendar calendar, UserViewSettings userViewSettings)
            : base(calendar, userViewSettings) { }


        [DataMember(Name = "isSubscribed", Order = 100)]
        public bool IsAccepted
        {
            get
            {
                if(UserCalendar is Calendar.BusinessObjects.Calendar)
                    return _userViewSettings != null && _userViewSettings.IsAccepted;

                return this.IsAcceptedSubscription;
            }
            set { }
        }


        [DataMember(Name = "isNew", Order = 140)]
        public bool IsNew
        {
            get
            {
                return _userViewSettings==null;
            }
            set { }
        }

        [DataMember(Name = "group", Order = 130)]
        public string Group 
        {
            get {

                if(UserCalendar.IsiCalStream())
                    return Resources.CalendarApiResource.iCalCalendarsGroup;

                return String.IsNullOrEmpty(UserCalendar.Context.Group) ? Resources.CalendarApiResource.SharedCalendarsGroup : UserCalendar.Context.Group;
            }
            set { }
        }

        [DataMember(IsRequired=false)]
        public override CalendarPermissions Permissions{get; set;}

        public new static object GetSample()
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
                isSubscription = false,
                group = "Personal Calendars",
                isNew = true,
                isSubscribed = false,

            };
        }
    }
}
