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

    public enum EventStatus
    {
        Tentative = 0,
        Confirmed = 1,
        Cancelled = 2
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
        string Uid { get; }
        string CalendarId { get; }
        string Name { get; }
        string Description { get; }
        Guid OwnerId { get; }
        DateTime UtcStartDate { get; }
        DateTime UtcEndDate { get; }
        DateTime UtcUpdateDate { get; }
        EventAlertType AlertType { get; }
        bool AllDayLong { get; }
        RecurrenceRule RecurrenceRule { get; }
        EventContext Context { get; }
        SharingOptions SharingOptions { get;}
        EventStatus Status { get; }
    }
}
