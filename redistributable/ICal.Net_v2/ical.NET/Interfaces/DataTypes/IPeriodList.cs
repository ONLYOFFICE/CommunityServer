using System.Collections.Generic;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IPeriodList : IEncodableDataType, IList<IPeriod>
    {
        string TzId { get; }
        void Add(IDateTime dt);
    }
}