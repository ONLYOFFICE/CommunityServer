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


using System.Collections.Generic;

namespace ASC.Web.Core.Calendars
{
    public class CalendarColors
    {
        public string BackgroudColor { get; set; }
        public string TextColor { get; set; }
        public static List<CalendarColors> BaseColors
        {
            get {
                return new List<CalendarColors>(){
                    new CalendarColors(){ BackgroudColor = "#e34603", TextColor="#ffffff"},
                    new CalendarColors(){ BackgroudColor = "#f88e14", TextColor="#000000"},
                    new CalendarColors(){ BackgroudColor = "#ffb403", TextColor="#000000"},
                    new CalendarColors(){ BackgroudColor = "#9fbb4c", TextColor="#000000"},
                    new CalendarColors(){ BackgroudColor = "#288e31", TextColor="#ffffff"},
                    new CalendarColors(){ BackgroudColor = "#4cbb78", TextColor="#ffffff"},
                    new CalendarColors(){ BackgroudColor = "#0797ba", TextColor="#ffffff"},
                    new CalendarColors(){ BackgroudColor = "#1d5f99", TextColor="#ffffff"},
                    new CalendarColors(){ BackgroudColor = "#4c76bb", TextColor="#ffffff"},
                    new CalendarColors(){ BackgroudColor = "#3552d2", TextColor="#ffffff"},
                    new CalendarColors(){ BackgroudColor = "#473388", TextColor="#ffffff"},
                    new CalendarColors(){ BackgroudColor = "#884cbb", TextColor="#ffffff"},
                    new CalendarColors(){ BackgroudColor = "#cb59ba", TextColor="#000000"},
                    new CalendarColors(){ BackgroudColor = "#ca3083", TextColor="#ffffff"},
                    new CalendarColors(){ BackgroudColor = "#e24e78", TextColor="#000000"},
                    new CalendarColors(){ BackgroudColor = "#bf0036", TextColor="#ffffff"}
                };
            }
        }

    }
}
