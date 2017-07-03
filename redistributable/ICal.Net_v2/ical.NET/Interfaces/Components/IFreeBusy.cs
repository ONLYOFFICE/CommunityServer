using System.Collections.Generic;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net.Interfaces.Components
{
    public interface IFreeBusy : IUniqueComponent, IMergeable
    {
        IList<IFreeBusyEntry> Entries { get; set; }
        IDateTime DtStart { get; set; }
        IDateTime DtEnd { get; set; }
        IDateTime Start { get; set; }
        IDateTime End { get; set; }

        FreeBusyStatus GetFreeBusyStatus(IPeriod period);
        FreeBusyStatus GetFreeBusyStatus(IDateTime dt);
    }
}