using System;

namespace Ical.Net.Interfaces.Serialization
{
    public interface ISerializationSettings
    {
        Type CalendarType { get; set; }
        bool EnsureAccurateLineNumbers { get; set; }
        ParsingModeType ParsingMode { get; set; }
        bool StoreExtraSerializationData { get; set; }
    }

    public enum ParsingModeType
    {
        Strict,
        Loose
    }
}