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
