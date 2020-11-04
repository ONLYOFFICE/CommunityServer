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
using ASC.Web.Core.Calendars;

namespace ASC.Api.Calendar.BusinessObjects
{
    public class UserViewSettings
    { 
        public virtual string CalendarId { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }      
        public bool IsHideEvents { get; set; }
        public string TextColor { get; set; }
        public string BackgroundColor { get; set; }
        public bool IsAccepted { get; set; }        
        public EventAlertType EventAlertType { get; set; }
        public TimeZoneInfo TimeZone { get; set; }

        public UserViewSettings()
        {
            this.TextColor = String.Empty;
            this.BackgroundColor = String.Empty;
        }
    }
}
