using System;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface ITrigger : IEncodableDataType
    {
        IDateTime DateTime { get; set; }
        TimeSpan? Duration { get; set; }
        TriggerRelation Related { get; set; }
        bool IsRelative { get; }
    }
}