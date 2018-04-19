using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Interfaces.Components
{
    public interface ITimeZone : ICalendarComponent
    {
        string TzId { get; set; }

        IDateTime LastModified { get; set; }
        Uri Url { get; set; }
        HashSet<ITimeZoneInfo> TimeZoneInfos { get; set; }
    }
}