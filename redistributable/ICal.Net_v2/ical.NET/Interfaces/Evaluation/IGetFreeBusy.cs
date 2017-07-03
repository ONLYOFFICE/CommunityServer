using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Interfaces.Evaluation
{
    public interface IGetFreeBusy
    {
        IFreeBusy GetFreeBusy(IFreeBusy freeBusyRequest);
        IFreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive);
        IFreeBusy GetFreeBusy(IOrganizer organizer, IAttendee[] contacts, IDateTime fromInclusive, IDateTime toExclusive);
    }
}