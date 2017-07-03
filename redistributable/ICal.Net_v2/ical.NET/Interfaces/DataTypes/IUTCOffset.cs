using System;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IUtcOffset : IEncodableDataType
    {
        TimeSpan Offset { get; }
        bool Positive { get; }
        int Hours { get; }
        int Minutes { get; }
        int Seconds { get; }

        DateTime ToUtc(DateTime dt);
        DateTime ToLocal(DateTime dt);
    }
}