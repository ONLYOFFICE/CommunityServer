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
using ASC.Api.Calendar.BusinessObjects;
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.Wrappers
{
    [DataContract(Name = "sharing", Namespace = "")]
    public class PublicItemCollection
    {
        public PublicItemCollection()
        {
            this.Items = new List<PublicItemWrapper>();
        }

        [DataMember(Name = "actions", Order = 10)]
        public List<AccessOption> AvailableOptions
        {
            get { return AccessOption.CalendarStandartOptions; }
            set { }
        }

        [DataMember(Name = "items", Order = 20)]
        public List<PublicItemWrapper> Items { get; set; }

        public static object GetSample()
        {
            return new {actions=new List<object>(){AccessOption.GetSample()}, items = new List<object>(){PublicItemWrapper.GetSample()}};
        }

        public static PublicItemCollection GetDefault()
        {
            var sharingOptions = new PublicItemCollection();
            sharingOptions.Items.Add(new PublicItemWrapper(
                new ASC.Web.Core.Calendars.SharingOptions.PublicItem()
                    {
                        Id = SecurityContext.CurrentAccount.ID,
                        IsGroup = false
                    },
            "0", SecurityContext.CurrentAccount.ID));
            return sharingOptions;
        }

        public static PublicItemCollection GetForCalendar(ICalendar calendar)
        {
            var sharingOptions = new PublicItemCollection();
            sharingOptions.Items.Add(new PublicItemWrapper(new ASC.Web.Core.Calendars.SharingOptions.PublicItem()
                   {
                       Id = calendar.OwnerId,
                       IsGroup = false
                   },
                  calendar.Id.ToString(), calendar.OwnerId));
            foreach (var item in calendar.SharingOptions.PublicItems)            
                sharingOptions.Items.Add(new PublicItemWrapper(item, calendar.Id.ToString(), calendar.OwnerId));
            
            return sharingOptions;
        }

        public static PublicItemCollection GetForEvent(IEvent calendarEvent)
        {
            var sharingOptions = new PublicItemCollection();
            sharingOptions.Items.Add(new PublicItemWrapper(new ASC.Web.Core.Calendars.SharingOptions.PublicItem()
            {
                Id = calendarEvent.OwnerId,
                IsGroup = false
            },

            calendarEvent.CalendarId, calendarEvent.Id, calendarEvent.OwnerId));

            foreach (var item in calendarEvent.SharingOptions.PublicItems)
                sharingOptions.Items.Add(new PublicItemWrapper(item, calendarEvent.CalendarId, calendarEvent.Id, calendarEvent.OwnerId));

            return sharingOptions;
        }

    }
}
