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

namespace ASC.Web.Core.Calendars
{
    public class CalendarContext : ICloneable
    {
        public delegate string GetString();
        public GetString GetGroupMethod { get; set; }
        public string Group { get { return GetGroupMethod != null ? GetGroupMethod() : ""; } }
        public string HtmlTextColor { get; set; }
        public string HtmlBackgroundColor { get; set; }
        public bool CanChangeTimeZone { get; set; }
        public bool CanChangeAlertType { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public interface ICalendar: IiCalFormatView
    {
        string Id { get;}
        string Name { get; }
        string Description { get; }        
        Guid OwnerId { get; }
        EventAlertType EventAlertType { get; }
        List<IEvent> LoadEvents(Guid userId, DateTime utcStartDate, DateTime utcEndDate);
        SharingOptions SharingOptions { get; }
        TimeZoneInfo TimeZone { get; }

        CalendarContext Context {get;}
    }
}
