using System;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IPeriod : IEncodableDataType, IComparable<IPeriod>
    {
        IDateTime StartTime { get; set; }
        IDateTime EndTime { get; set; }
        TimeSpan Duration { get; set; }

        bool Contains(IDateTime dt);
        bool CollidesWith(IPeriod period);
    }
}