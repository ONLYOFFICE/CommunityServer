/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
