using System;
using System.Collections.Generic;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IRecurrencePattern : IEncodableDataType
    {
        FrequencyType Frequency { get; set; }
        DateTime Until { get; set; }
        int Count { get; set; }
        int Interval { get; set; }
        List<int> BySecond { get; set; }
        List<int> ByMinute { get; set; }
        List<int> ByHour { get; set; }
        List<IWeekDay> ByDay { get; set; }
        List<int> ByMonthDay { get; set; }
        List<int> ByYearDay { get; set; }
        List<int> ByWeekNo { get; set; }
        List<int> ByMonth { get; set; }
        List<int> BySetPosition { get; set; }
        DayOfWeek FirstDayOfWeek { get; set; }

        RecurrenceRestrictionType RestrictionType { get; set; }
        RecurrenceEvaluationModeType EvaluationMode { get; set; }

        //IPeriod GetNextOccurrence(IDateTime dt);
    }
}