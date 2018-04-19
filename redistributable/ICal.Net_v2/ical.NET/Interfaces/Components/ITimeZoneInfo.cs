using System.Collections.Generic;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;

namespace Ical.Net.Interfaces.Components
{
    public interface ITimeZoneInfo : ICalendarComponent, IRecurrable
    {
        string TzId { get; }
        string TimeZoneName { get; set; }
        IList<string> TimeZoneNames { get; set; }
        IUtcOffset OffsetFrom { get; set; }
        IUtcOffset OffsetTo { get; set; }
    }
}
