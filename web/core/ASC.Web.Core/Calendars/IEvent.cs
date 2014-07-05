/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASC.Web.Core.Calendars
{
    [System.AttributeUsage(System.AttributeTargets.Class,
                   AllowMultiple = false,
                   Inherited = true)]
    public class AllDayLongUTCAttribute : Attribute
    { }

    public enum EventRepeatType
    {
        Never = 0,
        EveryDay = 3,
        EveryWeek = 4,
        EveryMonth = 5,
        EveryYear = 6
    }

    public enum EventAlertType
    {
        Default = -1,
        Never = 0,
        FiveMinutes = 1,
        FifteenMinutes = 2,
        HalfHour = 3,
        Hour = 4,
        TwoHours = 5,
        Day = 6
    }
    

    public class EventContext : ICloneable
    {
        //public EventRepeatType RepeatType { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public interface IEvent : IiCalFormatView
    {
        string Id { get; }
        string CalendarId { get; }
        string Name { get; }
        string Description { get; }
        Guid OwnerId { get; }
        DateTime UtcStartDate { get; }
        DateTime UtcEndDate { get; }        
        EventAlertType AlertType { get; }
        bool AllDayLong { get; }
        RecurrenceRule RecurrenceRule { get; }
        EventContext Context { get; }
        SharingOptions SharingOptions { get;}
    }
}
