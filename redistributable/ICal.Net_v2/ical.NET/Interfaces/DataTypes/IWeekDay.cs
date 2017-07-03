using System;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IWeekDay : IEncodableDataType, IComparable
    {
        int Offset { get; set; }
        DayOfWeek DayOfWeek { get; set; }
    }
}